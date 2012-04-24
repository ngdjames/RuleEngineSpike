using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class AssignActionTests
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
            _context.Add("SomeDate", typeof(DateTime));        }

        #region general assignment rules
        [Test]
        public void AssignToSimpleVariableIsValid()
        {
            var aa = MakeAssignAction("SomeDecimal","5.00");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void AssignToNonexistentVariableIsInvalid()
        {
            var aa = MakeAssignAction("CandyPrice","1.00");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void AssignToPropertyIsValid()
        {
            var aa = MakeAssignAction("Person.Salutation", "Mr.");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void AssignToPropertyofNonexistentTargetIsInvalid()
        {
            var aa = MakeAssignAction("Whatever.Salutation", "Mr.");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void AssignToNonexistentMemberIsInvalid()
        {
            var aa = MakeAssignAction("Person.Whatever", "Mr.");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void AssignToMethodIsInvalid()
        {
            var aa = MakeAssignAction("Person.Age", "18");
            Assert.IsFalse(aa.IsValid(_context));
        }
        #endregion

        #region legal value rules
        [Test]
        public void CanAssignDateToDate()
        {
            var aa = MakeAssignAction("SomeDate", "1/1/2001");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignTextToDate()
        {
            var aa = MakeAssignAction("SomeDate", "whatever");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignNumberToDate()
        {
            var aa = MakeAssignAction("SomeDate", "132");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CanAssignNumberToDecimal()
        {
            var aa = MakeAssignAction("SomeDecimal", "12.5");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignTextToDecimal()
        {
            var aa = MakeAssignAction("SomeDecimal", "whatever");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignDateToDecimal()
        {
            var aa = MakeAssignAction("SomeDecimal", "1/1/2001");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CanAssignNumberToDouble()
        {
            var aa = MakeAssignAction("SomeDouble", "12.5");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignTextToDouble()
        {
            var aa = MakeAssignAction("SomeDouble", "whatever");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignDateToDouble()
        {
            var aa = MakeAssignAction("SomeDouble", "1/1/2001");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CanAssignIntegerToInteger()
        {
            var aa = MakeAssignAction("SomeInt", "12");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignDecimalToInteger()
        {
            var aa = MakeAssignAction("SomeInt", "12.5");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignTextToInteger()
        {
            var aa = MakeAssignAction("SomeInt", "whatever");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignDateToInteger()
        {
            var aa = MakeAssignAction("SomeInt", "1/1/2001");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CanAssignBooleanToBoolean()
        {
            var aa = MakeAssignAction("SomeBool", "false");
            Assert.IsTrue(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignNumberToBoolean()
        {
            var aa = MakeAssignAction("SomeBoolean", "0");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignTextToBoolean()
        {
            var aa = MakeAssignAction("SomeBoolean", "whatever");
            Assert.IsFalse(aa.IsValid(_context));
        }

        [Test]
        public void CannotAssignDateToBoolean()
        {
            var aa = MakeAssignAction("SomeBoolean", "1/1/2001");
            Assert.IsFalse(aa.IsValid(_context));
        }

        #endregion

        #region private helper methods

        private AssignAction MakeAssignAction(string lhs, string value)
        {
            return new AssignAction
            {
                LeftHandSide = lhs,
                Value = value
            };
        }

        #endregion

    }
}
