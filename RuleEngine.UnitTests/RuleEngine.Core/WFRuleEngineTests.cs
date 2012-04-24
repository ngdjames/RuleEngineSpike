using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;
using System.Activities;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class WFRuleEngineTests
    {
        private Dictionary<string, object> _workingMemory;
        private RuleSet _ruleSet;

        [SetUp]
        public void SetUp()
        {
            _workingMemory = new Dictionary<string, object>();

            _ruleSet = new RuleSet { Name = "Test RuleSet" };
            _ruleSet.AddInput("Person", typeof(TestPerson));        }

        [Test]
        public void CompilingAValidRuleSetReturnsARuleEngine()
        {
            var rs = new RuleSet { Name = "Whatever" };
            Assert.DoesNotThrow(delegate
            {
                IRuleEngine engine = WFRuleEngine.Compile(rs);
                engine.Invoke(_workingMemory);
            });
        }

        [Test]
        public void CompilingAnInvalidRuleSetThrowsAnException()
        {
            var rs = new RuleSet();
            Assert.Throws(typeof(Exception), delegate
            {
                IRuleEngine engine = WFRuleEngine.Compile(rs);
            });
        }

        [Test]
        public void InvokingWithIncorrectWorkingMemoryThrowsAnException()
        {
            var rule = new Rule { Name = "First Rule", TopCondition = new LogicCondition { Operator = "all" } }; 
            var action1 = new AssignAction { LeftHandSide = "Person.Salutation", Value = "Mr." };
            rule.AddAction(action1);

            var rs = new RuleSet { Name = "Whatever" };
            rs.AddInput("Person", typeof(TestPerson));
            rs.AddRule(rule);

            IRuleEngine engine = WFRuleEngine.Compile(rs);

            Assert.Throws(typeof(InvalidWorkflowException), delegate
            {
                engine.Invoke(_workingMemory);
            });
        }

        [Test]
        public void CanReadAndWriteProperties()
        {
            var condition = new ExpressionCondition("Person.Sex", "is", "Male");
            var action = new AssignAction("Person.Salutation", "Mr.");
            Rule rule = CreateTestRule(condition, action);
            _ruleSet.AddRule(rule);

            var person = new TestPerson { Sex = "Male" };

            
            var engine = WFRuleEngine.Compile(_ruleSet);
            _workingMemory.Add("Person", person);
            engine.Invoke(_workingMemory);
            
            Assert.AreEqual("Mr.", person.Salutation);
        }

        [Test]
        public void CanReadMethods()
        {
            var condition = new ExpressionCondition("Person.IsMinor", "is true");
            var action = new AssignAction("Person.Salutation", "To the parents of");
            var rule = CreateTestRule(condition, action);
            _ruleSet.AddRule(rule);
            var engine = WFRuleEngine.Compile(_ruleSet);
            var person = new TestPerson { BirthDate = DateTime.Today };
            _workingMemory.Add("Person", person);
            engine.Invoke(_workingMemory);
            Assert.AreEqual("To the parents of", person.Salutation);
        }

        [Test]
        public void CanInvokeMethods()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void CanInvokeMethodsWithParameters()
        {
            Assert.Inconclusive();
        }

        // return string value
        // return numeric value
        // return date value
        // return boolean value

        // match multiple rules
        // match first
        // multiple actions for a match
        // and multiple conditions
        // or multiple conditions
        // nest and/or
        // multiple inputs
        // string
            // is
            // is not
            // contains
            // does not contain
            // begins with
            // ends with
        // number
            // is
            // is not
            // is greater than
            // is less than
            // is in the range
        // date
            // is
            // is not
            // is before
            // is after
            // is in the last
            // is not in the last
            // is in the range
        // boolean
            // is true
            // is not true



        #region private convenience methods

        private Rule CreateTestRule(ExpressionCondition condition, AssignAction action)
        {
            var rule = new Rule { Name = "Rule 1", TopCondition = new LogicCondition { Operator = "all" } };
            rule.TopCondition.AddCondition(condition);
            rule.AddAction(action);
            return rule;
        }

        #endregion
    }
}
