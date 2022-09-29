using System;
using System.Collections.Generic;
using System.Linq;


namespace Trainer_v5.SDK
{
	public static class EmployeeHelper
	{
		#region Traits

		/// <summary>
		/// All traits
		/// </summary>
		public static IEnumerable<Employee.Trait> Traits { get; }
			= Enum.GetValues(typeof(Employee.Trait)).Cast<Employee.Trait>();

		public static IEnumerable<Employee.Trait> GoodTraits { get; }
			= Traits.Where(t => 0 != (Employee.GoodTraits & t));

		public static IEnumerable<Employee.Trait> NeutralTraits { get; }
			= Traits.Where(t => 0 != (Employee.NeutralTraits & t));

		public static IEnumerable<Employee.Trait> BadTraits { get; }
			= Traits.Where(t => 0 != (Employee.BadTraits & t));


		public static bool IsGood(this Employee.Trait self)
		{
			return (self & Employee.GoodTraits) > 0;
		}

		public static bool IsNeutral(this Employee.Trait self)
		{
			return (self & Employee.NeutralTraits) > 0;
		}

		public static bool IsBad(this Employee.Trait self)
		{
			return (self & Employee.BadTraits) > 0;
		}

		#endregion


		#region LeadDesign Demands

		/// <summary>
		/// All demands
		/// </summary>
		public static IEnumerable<LeadDesignDemands.Demand> Demands { get; }
			= Enum.GetValues(typeof(LeadDesignDemands.Demand)).Cast<LeadDesignDemands.Demand>();

		#endregion
	}
}