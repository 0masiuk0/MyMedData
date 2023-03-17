using System.Reflection.Metadata;
using MyMedData;

namespace MyMedTest
{
	[TestFixture]
	public class PasswordsTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TestCase("1234")]
		[TestCase("abcd")]
		[TestCase("рсту")]
		public void PasswordHashAndDehash(string password)
		{
			User user = new User();
			user.SetPassword(password);
			var hash = user.PasswordHash;

			bool ok = user.CheckPassword(password);
			Assert.IsTrue(ok);
		}
	}
}