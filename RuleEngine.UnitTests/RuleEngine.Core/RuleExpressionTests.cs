using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class RuleExpressionTests
    {
        private Dictionary<string, Type> _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Dictionary<string, Type>();
            _context.Add("Person", typeof(TestPerson));
            _context.Add("SomeString", typeof(string));
            _context.Add("SomeInt", typeof(int));
            _context.Add("SomeDouble", typeof(double));
            _context.Add("SomeDecimal", typeof(decimal));
            _context.Add("SomeBool", typeof(bool));
            _context.Add("SomeDate", typeof(DateTime));
        }

        #region validations

        [Test]
        public void AssignedSimpleTargetIsValid()
        {
            var re = new RuleExpression("SomeString");
            Assert.IsTrue(re.IsValid(_context));
        }

        [Test]
        public void UnassignedSimpleTargetIsInvalid()
        {
            var re = new RuleExpression("Whatever");
            Assert.IsFalse(re.IsValid(_context));
        }

        [Test]
        public void EmptyExpressionIsInvalid()
        {
            var re = new RuleExpression { };
            Assert.IsFalse(re.IsValid(_context));
        }

        [Test]
        public void AssignedTargetPropertyIsValid()
        {
            var re = new RuleExpression("Person.LastName");
            Assert.IsTrue(re.IsValid(_context));
        }

        [Test]
        public void AssignedTargetMethodIsValid()
        {
            var re = new RuleExpression("Person.IsMinor");
            Assert.IsTrue(re.IsValid(_context));
        }

        [Test]
        public void UnassignedTargetIsInvalid()
        {
            var re = new RuleExpression("Patient.LastName");
            Assert.IsFalse(re.IsValid(_context));
        }

        [Test]
        public void InvalidMemberIsInvalid()
        {
            var re = new RuleExpression("Person.Whatever");
            Assert.IsFalse(re.IsValid(_context));
        }



        #endregion

        #region get info

        [Test]
        public void TypeOfSimpleTarget()
        {
            var re = new RuleExpression("SomeString");
            Assert.AreEqual(typeof(string), re.GetExpressionType(_context));
        }

        [Test]
        public void TypeOfProperty()
        {
            var re = new RuleExpression("Person.IsPhysician");
            Assert.AreEqual(typeof(bool), re.GetExpressionType(_context));
        }

        [Test]
        public void TypeOfMethod()
        {
            var re = new RuleExpression("Person.Age");
            Assert.AreEqual(typeof(int), re.GetExpressionType(_context));
        }

        [Test]
        public void SimpleTargetIsAssignable()
        {
            var re = new RuleExpression("SomeString");
            Assert.IsTrue(re.IsAssignable(_context));
        }

        [Test]
        public void WritablePropertyIsAssignable()
        {
            var re = new RuleExpression("Person.Salutation");
            Assert.IsTrue(re.IsAssignable(_context));
        }

        [Test]
        public void ReadOnlyPropertyIsNotAssignable()
        {
            var re = new RuleExpression("Person.MedicalRecordNumber");
            Assert.IsTrue(re.IsValid(_context));
            Assert.IsFalse(re.IsAssignable(_context));
        }

        [Test]
        public void MethodIsNotAssignable()
        {
            var re = new RuleExpression("Person.Age");
            Assert.IsTrue(re.IsValid(_context));
            Assert.IsFalse(re.IsAssignable(_context));
        }

        #endregion
    }
}
