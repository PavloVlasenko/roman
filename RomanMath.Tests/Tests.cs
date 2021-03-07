using NUnit.Framework;
using RomanMath.Impl;

namespace RomanMath.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TestSingle()
		{
			Assert.AreEqual(2, Service.Evaluate("II"));
			Assert.AreEqual(3, Service.Evaluate("III"));
			Assert.AreEqual(4, Service.Evaluate("IV"));
			Assert.AreEqual(80, Service.Evaluate("LXXX"));
		}

		[Test]
		public void TestExpression()
		{
			Assert.AreEqual(3, Service.Evaluate("I+II"));
			Assert.AreEqual(5, Service.Evaluate("X-V"));
			Assert.AreEqual(25, Service.Evaluate("X-V+V*IV"));
			Assert.AreEqual(40, Service.Evaluate("(X-V+V)*IV"));
			
			Assert.AreEqual(5, Service.Evaluate("X - V"));
			Assert.AreEqual(25, Service.Evaluate("X-V+V* IV"));
			Assert.AreEqual(40, Service.Evaluate("(X-V +V )*IV"));
		}
	}
}