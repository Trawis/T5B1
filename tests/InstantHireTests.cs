using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	public class InstantHireTests
	{
		[Theory]
		[InlineData("Programmer", Employee.EmployeeRole.Programmer)]
		[InlineData(" designer ", Employee.EmployeeRole.Designer)]
		[InlineData("SERVICE", Employee.EmployeeRole.Service)]
		[InlineData("lead", Employee.EmployeeRole.Lead)]
		public void TryParseRoleAcceptsTheGameRoleNames(string text, Employee.EmployeeRole expected)
		{
			Employee.EmployeeRole role;
			Assert.True(InstantHire.TryParseRole(text, out role));
			Assert.Equal(expected, role);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("Coder")]
		public void TryParseRoleRejectsAnythingElse(string text)
		{
			Employee.EmployeeRole role;
			Assert.False(InstantHire.TryParseRole(text, out role));
		}
	}
}
