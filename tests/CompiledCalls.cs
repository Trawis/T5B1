using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xunit;
using Xunit.Sdk;

namespace Trainer_v5.Tests
{
	// Reads what a compiled trainer method does, so wiring tests do not depend on formatting, comments or local names.
	// A method becomes an ordered list of steps: "call Type.Method" (call, callvirt, newobj), "ldstr text", "ldc N",
	// "ldc.r4 N", "ldfld/stfld/ldsfld/stsfld Type.Field" and "box Type", with type names unqualified. A lambda or
	// closure body (a compiler-generated method loaded with ldftn) is inlined where it is referenced.
	// Names come from the metadata of Trainer_v5.dll, so no game type is loaded (many need Unity modules the tests lack).
	// "A before B" is IL order, like the text-order checks it replaces; it is not control-flow analysis.
	internal static class CompiledCalls
	{
		private static readonly PEReader Image = new PEReader(ImmutableArray.Create(File.ReadAllBytes(typeof(Helpers).Assembly.Location)));
		private static readonly MetadataReader Reader = Image.GetMetadataReader();

		public static List<string> Steps(Type type, string methodName)
		{
			var methods = new List<MethodDefinitionHandle>();
			foreach (var handle in Reader.MethodDefinitions)
			{
				var method = Reader.GetMethodDefinition(handle);
				if (Reader.GetString(method.Name) == methodName && Matches(method.GetDeclaringType(), type))
				{
					methods.Add(handle);
				}
			}

			if (methods.Count != 1)
			{
				throw new XunitException(type.Name + "." + methodName + ": expected one compiled method with that name, found " + methods.Count + ". It was renamed, removed or overloaded; update the test with it.");
			}

			var steps = new List<string>();
			Collect(methods[0], steps, new HashSet<MethodDefinitionHandle>(), followLambdas: true);
			return steps;
		}

		public static void Calls(Type type, string methodName, string step)
		{
			Assert.True(Steps(type, methodName).Contains(step), type.Name + "." + methodName + " does not contain \"" + step + "\".");
		}

		public static void NeverCalls(Type type, string methodName, string step)
		{
			Assert.False(Steps(type, methodName).Contains(step), type.Name + "." + methodName + " must not contain \"" + step + "\".");
		}

		public static void InOrder(Type type, string methodName, params string[] ordered)
		{
			var steps = Steps(type, methodName);
			var from = 0;
			for (var i = 0; i < ordered.Length; i++)
			{
				var found = steps.IndexOf(ordered[i], from);
				Assert.True(found >= 0, type.Name + "." + methodName + ": \"" + ordered[i] + "\" is missing" + (i > 0 ? " after \"" + ordered[i - 1] + "\"" : "") + ".");
				from = found + 1;
			}
		}

		// The steps appear next to each other, in this order.
		public static void Adjacent(Type type, string methodName, params string[] run)
		{
			var steps = Steps(type, methodName);
			for (var i = 0; i + run.Length <= steps.Count; i++)
			{
				var matches = true;
				for (var j = 0; j < run.Length && matches; j++)
				{
					matches = steps[i + j] == run[j];
				}

				if (matches)
				{
					return;
				}
			}

			throw new XunitException(type.Name + "." + methodName + " has no consecutive steps \"" + string.Join("\", \"", run) + "\".");
		}

		public static void Count(Type type, string methodName, string step, int expected)
		{
			var found = 0;
			foreach (var item in Steps(type, methodName))
			{
				if (item == step)
				{
					found++;
				}
			}

			Assert.True(found == expected, type.Name + "." + methodName + " has \"" + step + "\" " + found + " time(s), expected " + expected + ".");
		}

		// Allowed places are "Type" or "Type.Method"; a lambda body counts for the method and top-level type that own it.
		public static void OnlyIn(string step, params string[] allowed)
		{
			var offenders = Where(steps => steps.Contains(step), allowed);
			Assert.True(offenders.Count == 0, "\"" + step + "\" is only allowed in " + (allowed.Length == 0 ? "no method" : string.Join(", ", allowed)) + "; found in " + string.Join(", ", offenders) + ".");
		}

		// Every trainer method, as "Type.Method", whose own steps (lambda bodies counted separately) match.
		public static SortedSet<string> Where(Func<List<string>, bool> matches, params string[] allowed)
		{
			var offenders = new SortedSet<string>(StringComparer.Ordinal);
			foreach (var method in AllMethods())
			{
				var owner = method.Key.Substring(0, method.Key.IndexOf('.'));
				if (matches(method.Value) && Array.IndexOf(allowed, owner) < 0 && Array.IndexOf(allowed, method.Key) < 0)
				{
					offenders.Add(method.Key);
				}
			}

			return offenders;
		}

		// Each trainer method body as "TopLevelType.Method" and its own steps; lambda bodies are separate entries named
		// after the method that owns them.
		public static List<KeyValuePair<string, List<string>>> AllMethods()
		{
			var methods = new List<KeyValuePair<string, List<string>>>();
			foreach (var handle in Reader.MethodDefinitions)
			{
				var method = Reader.GetMethodDefinition(handle);
				if (method.RelativeVirtualAddress == 0)
				{
					continue;
				}

				var steps = new List<string>();
				Collect(handle, steps, new HashSet<MethodDefinitionHandle>(), followLambdas: false);
				var place = Outermost(method.GetDeclaringType()) + "." + OwningMethod(Reader.GetString(method.Name));
				methods.Add(new KeyValuePair<string, List<string>>(place, steps));
			}

			return methods;
		}

		// True when the steps hold this run of consecutive steps; a null entry matches any step.
		public static bool HasRun(List<string> steps, params Func<string, bool>[] run)
		{
			for (var i = 0; i + run.Length <= steps.Count; i++)
			{
				var matches = true;
				for (var j = 0; j < run.Length && matches; j++)
				{
					matches = run[j] == null || run[j](steps[i + j]);
				}

				if (matches)
				{
					return true;
				}
			}

			return false;
		}

		// "<Update>b__29_0" is a lambda of Update.
		private static string OwningMethod(string name)
		{
			if (name.StartsWith("<", StringComparison.Ordinal))
			{
				var end = name.IndexOf('>');
				if (end > 1)
				{
					return name.Substring(1, end - 1);
				}
			}

			return name;
		}

		private static bool Matches(TypeDefinitionHandle handle, Type type)
		{
			var definition = Reader.GetTypeDefinition(handle);
			if (Reader.GetString(definition.Name) != type.Name)
			{
				return false;
			}

			var outer = definition.GetDeclaringType();
			if (type.DeclaringType != null)
			{
				return !outer.IsNil && Matches(outer, type.DeclaringType);
			}

			return outer.IsNil && Reader.GetString(definition.Namespace) == (type.Namespace ?? string.Empty);
		}

		private static string Outermost(TypeDefinitionHandle handle)
		{
			var definition = Reader.GetTypeDefinition(handle);
			while (!definition.GetDeclaringType().IsNil)
			{
				definition = Reader.GetTypeDefinition(definition.GetDeclaringType());
			}

			return Reader.GetString(definition.Name);
		}

		private static void Collect(MethodDefinitionHandle handle, List<string> steps, HashSet<MethodDefinitionHandle> visited, bool followLambdas)
		{
			if (!visited.Add(handle))
			{
				return;
			}

			var il = Image.GetMethodBody(Reader.GetMethodDefinition(handle).RelativeVirtualAddress).GetILBytes();
			IlReader.Walk(il, (offset, code, operandOffset) =>
			{
				var token = code.OperandType == OperandType.InlineMethod || code.OperandType == OperandType.InlineField || code.OperandType == OperandType.InlineType
					|| code.OperandType == OperandType.InlineString || code.OperandType == OperandType.InlineTok ? BitConverter.ToInt32(il, operandOffset) : 0;
				if (code == OpCodes.Call || code == OpCodes.Callvirt || code == OpCodes.Newobj)
				{
					steps.Add("call " + MemberName(MetadataTokens.EntityHandle(token)));
				}
				else if (code == OpCodes.Ldftn || code == OpCodes.Ldvirtftn)
				{
					var target = MetadataTokens.EntityHandle(token);
					if (followLambdas && target.Kind == HandleKind.MethodDefinition && IsCompilerGenerated((MethodDefinitionHandle)target))
					{
						Collect((MethodDefinitionHandle)target, steps, visited, true);
					}
					else
					{
						steps.Add("ldftn " + MemberName(target));
					}
				}
				else if (code == OpCodes.Ldstr)
				{
					steps.Add("ldstr " + Reader.GetUserString(MetadataTokens.UserStringHandle(token & 0xFFFFFF)));
				}
				else if (code == OpCodes.Ldfld || code == OpCodes.Stfld || code == OpCodes.Ldsfld || code == OpCodes.Stsfld)
				{
					steps.Add(code.Name + " " + MemberName(MetadataTokens.EntityHandle(token)));
				}
				else if (code == OpCodes.Box)
				{
					steps.Add("box " + TypeName(MetadataTokens.EntityHandle(token)));
				}
				else if (code == OpCodes.Ldc_R4)
				{
					steps.Add("ldc.r4 " + BitConverter.ToSingle(il, operandOffset).ToString(System.Globalization.CultureInfo.InvariantCulture));
				}
				else
				{
					int value;
					if (TryConstant(code, il, operandOffset, out value))
					{
						steps.Add("ldc " + value);
					}
				}
			});
		}

		private static bool IsCompilerGenerated(MethodDefinitionHandle handle)
		{
			var method = Reader.GetMethodDefinition(handle);
			return Reader.GetString(method.Name).StartsWith("<", StringComparison.Ordinal)
				|| Reader.GetString(Reader.GetTypeDefinition(method.GetDeclaringType()).Name).StartsWith("<", StringComparison.Ordinal);
		}

		// "Type.Member" for a method, field or member reference.
		private static string MemberName(EntityHandle handle)
		{
			switch (handle.Kind)
			{
				case HandleKind.MethodDefinition:
					var method = Reader.GetMethodDefinition((MethodDefinitionHandle)handle);
					return TypeName(method.GetDeclaringType()) + "." + Reader.GetString(method.Name);
				case HandleKind.FieldDefinition:
					var field = Reader.GetFieldDefinition((FieldDefinitionHandle)handle);
					return TypeName(field.GetDeclaringType()) + "." + Reader.GetString(field.Name);
				case HandleKind.MemberReference:
					var member = Reader.GetMemberReference((MemberReferenceHandle)handle);
					return TypeName(member.Parent) + "." + Reader.GetString(member.Name);
				case HandleKind.MethodSpecification:
					return MemberName(Reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
				default:
					return handle.Kind.ToString();
			}
		}

		private static string TypeName(EntityHandle handle)
		{
			switch (handle.Kind)
			{
				case HandleKind.TypeDefinition:
					return Reader.GetString(Reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name);
				case HandleKind.TypeReference:
					return Reader.GetString(Reader.GetTypeReference((TypeReferenceHandle)handle).Name);
				case HandleKind.TypeSpecification:
					// A generic instantiation: GENERICINST, CLASS or VALUETYPE, then the generic type itself.
					var blob = Reader.GetBlobReader(Reader.GetTypeSpecification((TypeSpecificationHandle)handle).Signature);
					if (blob.ReadByte() == 0x15)
					{
						blob.ReadByte();
						return TypeName(blob.ReadTypeHandle());
					}

					return "TypeSpec";
				default:
					return handle.Kind.ToString();
			}
		}

		private static bool TryConstant(OpCode code, byte[] il, int operandOffset, out int value)
		{
			if (code == OpCodes.Ldc_I4)
			{
				value = BitConverter.ToInt32(il, operandOffset);
				return true;
			}

			if (code == OpCodes.Ldc_I4_S)
			{
				value = (sbyte)il[operandOffset];
				return true;
			}

			if (code == OpCodes.Ldc_I4_M1)
			{
				value = -1;
				return true;
			}

			value = Array.IndexOf(ShortConstants, code);
			return value >= 0;
		}

		private static readonly OpCode[] ShortConstants =
		{
			OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4,
			OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8,
		};
	}
}
