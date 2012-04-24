using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class LogicConditionTests
    {
        private Dictionary<string, Type> _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Dictionary<string, Type>();
            _context.Add("Person", typeof(TestPerson));
        }

        [Test]
        public void EmptyConditionIsValid()
        {
            LogicCondition lc = new LogicCondition { Operator = "any" };
            Assert.IsTrue(lc.IsValid(_context));
        }

        [Test]
        public void CanAddExpressionConditions()
        {
            LogicCondition lc = new LogicCondition { Operator = "any" };
            var ec1 = new ExpressionCondition { LeftHandSide="Person.Age", Operator="is", RightHandSide=new List<string> { "18" } };
            var ec2 = new ExpressionCondition { LeftHandSide="Person.Sex", Operator="is", RightHandSide=new List<string> { "Male" } };
            lc.AddCondition(ec1);
            lc.AddCondition(ec2);
            Assert.IsTrue(lc.IsValid(_context));
        }

        [Test]
        public void CanAddNestedLogicConditions()
        {
            LogicCondition lc1 = new LogicCondition { Operator = "any" };
            LogicCondition lc2 = new LogicCondition { Operator = "all" };
            var ec = new ExpressionCondition { LeftHandSide="Person.Age", Operator="is", RightHandSide=new List<string> { "18" } };
            lc1.AddCondition(ec);
            lc1.AddCondition(lc2);
            Assert.IsTrue(lc1.IsValid(_context));
        }

        [Test]
        public void IsInvalidIfAnyChildIsInvalid()
        {
            LogicCondition lc = new LogicCondition { Operator = "all" };
            var ec1 = new ExpressionCondition { LeftHandSide = "Person.Age", Operator = "is", RightHandSide = new List<string> { "18" } };
            var ec2 = new ExpressionCondition { LeftHandSide = "Whatever", Operator = "is", RightHandSide = new List<string> { "18" } };
            lc.AddCondition(ec1);
            lc.AddCondition(ec2);
            Assert.IsFalse(lc.IsValid(_context));
        }

        [Test]
        public void IsInvalidIfDeeplyNestedChildIsInvalid()
        {
            LogicCondition lc1 = new LogicCondition { Operator = "all" };
            LogicCondition lc2 = new LogicCondition { Operator = "any" };
            LogicCondition lc3 = new LogicCondition { Operator = "any" };
            var ok = new ExpressionCondition { LeftHandSide = "Person.Age", Operator = "is", RightHandSide = new List<string> { "18" } };
            var bad = new ExpressionCondition { LeftHandSide = "Whatever", Operator = "is", RightHandSide = new List<string> { "18" } };
            lc1.AddCondition(ok);
            lc1.AddCondition(lc2);
            lc2.AddCondition(ok);
            lc1.AddCondition(lc3);
            lc3.AddCondition(bad);
            Assert.IsFalse(lc1.IsValid(_context));
        }

        [Test]
        public void IsInvalidIfCircularNestingIsDetected()
        {
            Assert.Inconclusive();
        }

    }
}
