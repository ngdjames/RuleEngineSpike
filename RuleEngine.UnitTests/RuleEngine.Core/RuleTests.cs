using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class RuleTests
    {
        private Dictionary<string, Type> _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Dictionary<string,Type>();
            _context.Add("Person", typeof(TestPerson));
        }

        [Test]
        public void EmptyRuleIsInvalid()
        {
            var rule = new Rule();
            Assert.IsFalse(rule.IsValid(_context));
        }

        [Test]
        public void RuleWithoutANameIsInvalid()
        {
            var rule = new Rule
            {
                TopCondition = new LogicCondition { Operator = "all" }
            };
            Assert.IsFalse(rule.IsValid(_context));
        }

        [Test]
        public void RuleWithAnyConditionAndNameIsValid()
        {
            var rule = MakeAnyRule("Whatever");
            Assert.IsTrue(rule.IsValid(_context));
        }

        [Test]
        public void RuleWithAllConditionAndNameIsValid()
        {
            var rule = MakeAllRule("Whatever");
            Assert.IsTrue(rule.IsValid(_context));
        }
        
        [Test]
        public void MayHaveManyActions()
        {
            var rule = MakeAllRule("Whatever");
            var action1 = new AssignAction { LeftHandSide = "Person.Salutation", Value = "Mr." };
            var action2 = new AssignAction { LeftHandSide = "Person.Greeting", Value = "Hello" };
            rule.AddAction(action1);
            rule.AddAction(action2);
            Assert.IsTrue(rule.IsValid(_context));
        }

        [Test]
        public void MayHaveADescription()
        {
            var rule = MakeAllRule("Whatever");
            rule.Description = "This is just a rule for testing purposes.";
            Assert.IsTrue(rule.IsValid(_context));

        }

        [Test]
        public void IsInvalidIfAnyActionIsInvalid()
        {
            var rule = MakeAllRule("Whatever");
            var action1 = new AssignAction { LeftHandSide = "Person.Salutation", Value = "Mr." };
            var action2 = new AssignAction { LeftHandSide = "Whatever", Value = "Hello" };
            rule.AddAction(action1);
            rule.AddAction(action2);
            Assert.IsFalse(rule.IsValid(_context));
        }

        [Test]
        public void IsInvalidIfConditionIsInvalid()
        {
            var rule = MakeAllRule("Whatever");
            var badCondition = new ExpressionCondition("Whatever", "is", "uhoh");
            rule.TopCondition.AddCondition(badCondition);
            Assert.IsFalse(rule.IsValid(_context));
        }

        [Test]
        public void MayHaveInvokeActions()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void MayHaveStopActions()
        {
            Assert.Inconclusive();
        }

        #region private helper methods

        private Rule MakeAllRule(string name)
        {
            return new Rule { Name = name, TopCondition = new LogicCondition { Operator = "all" } };
        }

        private Rule MakeAnyRule(string name)
        {
            return new Rule { Name = name, TopCondition = new LogicCondition { Operator = "any" } };
        }

        #endregion
    }
}
