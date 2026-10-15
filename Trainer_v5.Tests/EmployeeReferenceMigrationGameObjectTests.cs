using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Creativity migration run against real game objects (created without their constructors, which need the
	// running game): each holder of the old employee must point at the new one, and planning must change nothing (#183).
	public class EmployeeReferenceMigrationGameObjectTests
	{
		[Fact]
		public void FriendshipsMoveToTheNewEmployeeAndOthersAreLeftAlone()
		{
			Employee.SetAllFriendships(new Dictionary<Employee.FriendKey, float>());
			var original = Named("original");
			var replacement = Named("replacement");
			var friend = Named("friend");
			var other = Named("other");
			var strangerA = Named("strangerA");
			var strangerB = Named("strangerB");
			Employee.SetFriendship(original, friend, 0.5f);
			Employee.SetFriendship(other, original, 0.25f);
			Employee.SetFriendship(strangerA, strangerB, 1f);
			var plan = new List<System.Action>();

			EmployeeReferenceMigration.PlanFriendships(plan, original, replacement);

			Assert.Equal(0.5f, Amount(original, friend));
			EmployeeReferenceMigration.Apply(plan);

			Assert.Equal(0.5f, Amount(replacement, friend));
			Assert.Equal(0.25f, Amount(replacement, other));
			Assert.Equal(0f, Amount(original, friend));
			Assert.Equal(0f, Amount(original, other));
			Assert.Equal(1f, Amount(strangerA, strangerB));
			Assert.Equal(3, Employee.GetAllFriendships().Count);
		}

		[Fact]
		public void LeadWorkAndLeadDesignerFollowTheNewEmployee()
		{
			var original = Make<Employee>();
			var replacement = Make<Employee>();
			var bystander = Make<Employee>();
			var design = Make<DesignDocument>();
			design.LeadWork = new Dictionary<Employee, float> { { original, 0.4f }, { bystander, 0.1f } };
			design.LeadDesigner = original;
			var plan = new List<System.Action>();

			EmployeeReferenceMigration.PlanWork(plan, design, original, replacement);

			Assert.True(design.LeadWork.ContainsKey(original));
			Assert.Same(original, design.LeadDesigner);
			EmployeeReferenceMigration.Apply(plan);

			Assert.False(design.LeadWork.ContainsKey(original));
			Assert.Equal(0.4f, design.LeadWork[replacement]);
			Assert.Equal(0.1f, design.LeadWork[bystander]);
			Assert.Same(replacement, design.LeadDesigner);
		}

		[Fact]
		public void CompaniesAndProductsPointAtTheNewLeadDesigner()
		{
			var original = Make<Employee>();
			var replacement = Make<Employee>();
			var simulated = Make<SimulatedCompany>();
			simulated.LeadDesigner = original;
			var other = Make<SimulatedCompany>();
			other.LeadDesigner = Make<Employee>();
			var product = Make<SoftwareProduct>();
			product.LeadDesigner = original;
			var plan = new List<System.Action>();

			EmployeeReferenceMigration.PlanCompany(plan, simulated, original, replacement);
			EmployeeReferenceMigration.PlanCompany(plan, other, original, replacement);
			EmployeeReferenceMigration.PlanProduct(plan, product, original, replacement);
			EmployeeReferenceMigration.Apply(plan);

			Assert.Same(replacement, simulated.LeadDesigner);
			Assert.Same(replacement, product.LeadDesigner);
			Assert.NotSame(replacement, other.LeadDesigner);
			Assert.Equal(2, plan.Count);
		}

		[Fact]
		public void FreeLeadsAndInteractionMemoryAreSwapped()
		{
			var original = Make<Employee>();
			var replacement = Make<Employee>();
			var other = Make<Employee>();
			var freeLeads = new HashSet<Employee> { original, other };
			var plan = new List<System.Action>();

			EmployeeReferenceMigration.PlanSet(plan, freeLeads, original, replacement);
			EmployeeReferenceMigration.PlanSet(plan, new HashSet<Employee> { other }, original, replacement);
			EmployeeReferenceMigration.PlanSet(plan, (HashSet<Employee>)null, original, replacement);

			Assert.Single(plan);
			Assert.Contains(original, freeLeads);
			EmployeeReferenceMigration.Apply(plan);

			Assert.DoesNotContain(original, freeLeads);
			Assert.Contains(replacement, freeLeads);
			Assert.Contains(other, freeLeads);
		}

		[Fact]
		public void TheGuardRefusesFoundersAndEmployeesWithoutAnActor()
		{
			var founder = Make<Employee>();
			founder.Founder = true;
			var homeless = Make<Employee>();

			Assert.Contains("founder", CreativityEditGuard.FindBlockingReference(founder));
			Assert.Contains("no actor", CreativityEditGuard.FindBlockingReference(homeless));
		}

		private static T Make<T>()
		{
			return (T)FormatterServices.GetUninitializedObject(typeof(T));
		}

		// The game's own GetFriendship needs its static helper class, which cannot start outside the game.
		private static float Amount(Employee first, Employee second)
		{
			float amount;
			return Employee.GetAllFriendships().TryGetValue(new Employee.FriendKey(first, second), out amount) ? amount : 0f;
		}

		// Friendship keys hash by name, which the constructor normally sets.
		private static Employee Named(string name)
		{
			var employee = Make<Employee>();
			typeof(Employee).GetField("Name").SetValue(employee, name);
			return employee;
		}
	}
}
