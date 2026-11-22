using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	public class InstantHireTests
	{
		[Theory]
		[InlineData(" designer ", Employee.EmployeeRole.Designer)]
		[InlineData("SERVICE", Employee.EmployeeRole.Service)]
		public void TryParseRoleAcceptsTheGameRoleNames(string text, Employee.EmployeeRole expected)
		{
			Employee.EmployeeRole role;
			Assert.True(InstantHire.TryParseRole(text, out role));
			Assert.Equal(expected, role);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("Coder")]
		public void TryParseRoleRejectsAnythingElse(string text)
		{
			Employee.EmployeeRole role;
			Assert.False(InstantHire.TryParseRole(text, out role));
		}
	}
}
