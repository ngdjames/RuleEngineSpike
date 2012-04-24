using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class ExpressionConditionTests
    {
        private Dictionary<string, Type> _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Dictionary<string, Type>();
            _context.Add("Person", typeof(TestPerson));
            _context.Add("CompanyName", typeof(string));
        }

        #region basic validations

        [Test]
        public void ConditionWithSimpleValueIsValid()
        {
            var condition = new ExpressionCondition("CompanyName", "is", "NextGen");
            Assert.IsTrue(condition.IsValid(_context));
        }
        
        [Test]
        public void ConditionWithPropertyOnTargetIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "is", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }
        
        [Test]
        public void ConditionWithMethodOnTargetIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is",  "18");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void ConditionWithInvalidTargetIsInvalid()
        {
            var condition = new ExpressionCondition("Patient.Sex", "is", "Male");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void ConditionWithInvalidTargetMemberIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Gender", "is", "Male");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void LeftHandSideWithTooManySegmentsIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Spouse.Employer", "is", "NextGen");
            Assert.IsFalse(condition.IsValid(_context));
        }
        
        #endregion

        #region string conditions

        [Test]
        public void StringIsConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "is", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void StringIsNotConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "is not", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void StringContainsConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "contains", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }

         [Test]
        public void StringDoesNotContainConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "does not contain", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }

         [Test]
        public void StringBeginsWithConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "begins with", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void StringEndsWithConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Sex", "ends with", "Male");
            Assert.IsTrue(condition.IsValid(_context));
        }
        
        [Test]
        public void StringWhateverConditionIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Sex", "whatever", "Male");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void StringConditionWithEmptyRHSIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Sex", "is");
            Assert.IsFalse(condition.IsValid(_context));
        }        

        [Test]
        public void StringConditionWithMoreThanOneRHSEntryIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Sex", "is", "Male", "Female");
            Assert.IsFalse(condition.IsValid(_context));
        }        

        #endregion

        #region number conditions

        [Test]
        public void NumberIsConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is", "18");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void NumberIsNotConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is not", "18");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void NumberIsLessThanConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is less than", "18");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void NumberIsGreaterThanConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is greater than", "18");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void NumberIsInTheRangeConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.Age", "is in the range", "13", "19");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void NumberWhateverConditionIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Age", "whatever", "18");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void NumberConditionWithEmptyRHSIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Age", "is");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void NumberRangeConditionWithOneRHSElementIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Age", "is in the range", "18");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void OtherNumberConditionWithMoreThanOneRHSElementIsInvalid()
        {
            var condition = new ExpressionCondition("Person.Age", "is", "18", "19");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void RHSOfNumberConditionMustBeANumber()
        {
            var condition = new ExpressionCondition("Person.Age", "is", "whatever");
            Assert.IsFalse(condition.IsValid(_context));
        }

        #endregion

        #region boolean conditions

        [Test]
        public void BooleanIsTrueConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.IsMinor", "is true");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void BooleanIsFalseConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.IsMinor", "is false");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void BooleanWhateverConditionIsInvalid()
        {
            var condition = new ExpressionCondition("Person.IsMinor", "whatever");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void BooleanConditionWithRHSIsInvalid()
        {
            var condition = new ExpressionCondition("Person.IsMinor", "is true", "whatever");
            Assert.IsFalse(condition.IsValid(_context));
        }
        
        #endregion

        #region date conditions

        [Test]
        public void DateIsConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is", "1/1/2001");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void DateIsNotConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is not", "1/1/2001");
            Assert.IsTrue(condition.IsValid(_context));
        }
        
        [Test]
        public void DateIsBeforeConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is before", "1/1/2001");
            Assert.IsTrue(condition.IsValid(_context));
        }
        
        [Test]
        public void DateIsAfterConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is after", "1/1/2001");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void DateIsConditionRequiresValidDate()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is", "2/29/2011");
            Assert.IsFalse(condition.IsValid(_context));
        }

        [Test]
        public void DateIsInTheRangeConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is in the range", "1/1/1980", "12/31/1989");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void DateIsInTheRangeConditionRequiresTwoDates()
        {
            var condition1 = new ExpressionCondition("Person.BirthDate", "is in the range", "1/1/2001");
            Assert.IsFalse(condition1.IsValid(_context));

            var condition2 = new ExpressionCondition("Person.BirthDate", "is in the range", "1/1/2001", "2/29/2001");
            Assert.IsFalse(condition2.IsValid(_context));
        }

        [Test]
        public void DateIsInTheLastConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is in the last", "2", "weeks");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void DateIsNotInTheLastConditionIsValid()
        {
            var condition = new ExpressionCondition("Person.BirthDate", "is not in the last", "2", "months");
            Assert.IsTrue(condition.IsValid(_context));
        }

        [Test]
        public void DateIsInTheLastConditionRequiresNumber()
        {
            var condition1 = new ExpressionCondition("Person.BirthDate", "is in the last", "whatever", "weeks");
            Assert.IsFalse(condition1.IsValid(_context));

            var condition2 = new ExpressionCondition("Person.BirthDate", "is in the last", "-7", "days");
            Assert.IsFalse(condition2.IsValid(_context));

            var condition3 = new ExpressionCondition("Person.BirthDate", "is in the last", "3.5", "days");
            Assert.IsFalse(condition3.IsValid(_context));
        }

        [Test]
        public void DateIsInTheLastConditionRequiresValidUnits()
        {
            var condition1 = new ExpressionCondition("Person.BirthDate", "is in the last", "5", "days");
            Assert.IsTrue(condition1.IsValid(_context));

            var condition2 = new ExpressionCondition("Person.BirthDate", "is in the last", "5", "weeks");
            Assert.IsTrue(condition2.IsValid(_context));

            var condition3 = new ExpressionCondition("Person.BirthDate", "is in the last", "5", "months");
            Assert.IsTrue(condition3.IsValid(_context));

            var condition4 = new ExpressionCondition("Person.BirthDate", "is in the last", "5", "years");
            Assert.IsTrue(condition4.IsValid(_context));

            var condition5 = new ExpressionCondition("Person.BirthDate", "is in the last", "5", "decades");
            Assert.IsFalse(condition5.IsValid(_context));
        }

        #endregion

    }
}
