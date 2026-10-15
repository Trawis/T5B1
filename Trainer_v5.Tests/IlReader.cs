using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Trainer_v5.Tests
{
	// Minimal IL walker, so compiled output is inspected rather than executed.
	internal static class IlReader
	{
		private static readonly Dictionary<short, OpCode> OpCodesByValue = BuildOpCodeTable();

		public static IList<string> InvalidBranchTargets(MethodBody body)
		{
			var il = body.GetILAsByteArray();
			var starts = new HashSet<int>();
			var branches = new List<KeyValuePair<int, int>>();
			Walk(il, (offset, code, operandOffset, length) =>
			{
				starts.Add(offset);
				if (code.OperandType == OperandType.ShortInlineBrTarget)
				{
					branches.Add(new KeyValuePair<int, int>(offset, operandOffset + 1 + (sbyte)il[operandOffset]));
				}
				else if (code.OperandType == OperandType.InlineBrTarget)
				{
					branches.Add(new KeyValuePair<int, int>(offset, operandOffset + 4 + BitConverter.ToInt32(il, operandOffset)));
				}
				else if (code.OperandType == OperandType.InlineSwitch)
				{
					var count = BitConverter.ToInt32(il, operandOffset);
					var end = operandOffset + 4 + 4 * count;
					for (var i = 0; i < count; i++)
					{
						branches.Add(new KeyValuePair<int, int>(offset, end + BitConverter.ToInt32(il, operandOffset + 4 + 4 * i)));
					}
				}
			});

			var invalid = new List<string>();
			foreach (var branch in branches)
			{
				if (!starts.Contains(branch.Value))
				{
					invalid.Add("IL_" + branch.Key.ToString("x4") + " -> IL_" + branch.Value.ToString("x4") + " (method body is " + il.Length + " bytes)");
				}
			}
			return invalid;
		}

		public static ISet<string> CalledMethods(MethodBase method)
		{
			var il = method.GetMethodBody().GetILAsByteArray();
			var module = method.Module;
			var called = new HashSet<string>();
			Walk(il, (offset, code, operandOffset, length) =>
			{
				if (code != OpCodes.Call && code != OpCodes.Callvirt && code != OpCodes.Newobj)
				{
					return;
				}

				var target = module.ResolveMethod(BitConverter.ToInt32(il, operandOffset));
				called.Add(target.DeclaringType.FullName + "." + target.Name);
			});
			return called;
		}

		private delegate void Visitor(int offset, OpCode code, int operandOffset, int length);

		private static void Walk(byte[] il, Visitor visit)
		{
			var position = 0;
			while (position < il.Length)
			{
				var start = position;
				var value = (short)il[position++];
				if (value == 0xFE)
				{
					value = (short)(0xFE00 | il[position++]);
				}

				OpCode code;
				if (!OpCodesByValue.TryGetValue(value, out code))
				{
					throw new InvalidOperationException("Unknown IL opcode 0x" + value.ToString("x") + " at IL_" + start.ToString("x4"));
				}

				var operandOffset = position;
				position += OperandSize(code, il, position);
				visit(start, code, operandOffset, position - start);
			}
		}

		private static int OperandSize(OpCode code, byte[] il, int operandOffset)
		{
			switch (code.OperandType)
			{
				case OperandType.InlineNone:
					return 0;
				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineVar:
					return 1;
				case OperandType.InlineVar:
					return 2;
				case OperandType.InlineI8:
				case OperandType.InlineR:
					return 8;
				case OperandType.InlineSwitch:
					return 4 + 4 * BitConverter.ToInt32(il, operandOffset);
				default:
					return 4;
			}
		}

		private static Dictionary<short, OpCode> BuildOpCodeTable()
		{
			var table = new Dictionary<short, OpCode>();
			foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType == typeof(OpCode))
				{
					var code = (OpCode)field.GetValue(null);
					table[code.Value] = code;
				}
			}
			return table;
		}
	}
}
