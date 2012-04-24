using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class RuleSetTests
    {
        [Test]
        public void RuleSetWithoutNameIsInvalid()
        {
            var rs = new RuleSet();
            Assert.IsFalse(rs.IsValid());
        }

        [Test]
        public void EmptyRuleSetIsValid()
        {
            var rs = new RuleSet { Name = "Whatever" };
            Assert.IsTrue(rs.IsValid());
        }

        [Test]
        public void CanAddInputsToRuleSet()
        {
            var rs = new RuleSet { Name = "Whatever" };
            Assert.DoesNotThrow(delegate { rs.AddInput("Person", typeof(TestPerson)); });
        }

        [Test]
        public void CanAddRulesToRuleSet()
        {
            var rs = new RuleSet { Name = "Whatever" };
            var rule = new Rule { TopCondition = new LogicCondition { Operator = "all" } };
            Assert.DoesNotThrow(delegate { rs.AddRule(rule); });
        }

        [Test]
        public void RuleSetIsValidIfRulesAreValid()
        {
            var rule = new Rule { Name = "First Rule", TopCondition = new LogicCondition { Operator = "all" } }; 
            var action1 = new AssignAction { LeftHandSide = "Person.Salutation", Value = "Mr." };
            var action2 = new AssignAction { LeftHandSide = "Person.Greeting", Value = "Hello" };
            rule.AddAction(action1);
            rule.AddAction(action2);

            var rs = new RuleSet { Name = "Whatever" };
            rs.AddInput("Person", typeof(TestPerson));
            rs.AddRule(rule);

            Assert.IsTrue(rs.IsValid());
        }


        [Test]
        public void RuleSetIsInvalidIfAnyRuleIsInvalid()
        {
            var rule = new Rule { Name = "First Rule", TopCondition = new LogicCondition { Operator = "all" } };
            var action1 = new AssignAction { LeftHandSide = "Person.Salutation", Value = "Mr." };
            var action2 = new AssignAction { LeftHandSide = "Patient.Greeting", Value = "Hello" };
            rule.AddAction(action1);
            rule.AddAction(action2);

            var rs = new RuleSet { Name = "Whatever" };
            rs.AddInput("Person", typeof(TestPerson));
            rs.AddRule(rule);

            Assert.IsFalse(rs.IsValid());
        }
 
    }
}
