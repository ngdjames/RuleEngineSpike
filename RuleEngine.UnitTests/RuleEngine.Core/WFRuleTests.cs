using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.VisualBasic.Activities;
using System.Activities.Statements;
using System.Activities;
using System.Collections;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    [TestFixture]
    public class when_using_wf_rules
    {
        private Sequence sequence;
        private DynamicActivity activity;
        private Dictionary<string, object> workingMemory;

        [SetUp]
        public void SetUp()
        {
            sequence = new Sequence();
            activity = CreateActivityForTestPerson();
            workingMemory = new Dictionary<string, object>();
        }

        #region general capabilities

        [Test] 
        public void it_can_be_invoked_outside_workflow()
        {
            Assert.DoesNotThrow(
                delegate { WorkflowInvoker.Invoke(activity, workingMemory); });
        }

        [Test] 
        public void it_can_read_and_write_properties()
        {
            var if1 = CreateAssignFor("Person.IsPhysician", "Person.Salutation", "Dr.");
            sequence.Activities.Add(if1);

            var person = new TestPerson { IsPhysician = true };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Dr.", person.Salutation);
        }

        [Test] 
        public void it_can_read_methods()
        {
            var if1 = CreateAssignFor("Person.IsMinor()", "Person.Salutation", "To the parents of");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Today };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("To the parents of", person.Salutation);
        }

        [Test] 
        public void it_can_invoke_methods()
        {
            var condition1 = new VisualBasicValue<bool>("Person.IsMinor()");
            var if1 = new If(new InArgument<bool>(condition1));

            if1.Then = new InvokeMethod
            {
                MethodName = "CancelSubscriptions",
                TargetObject = new InArgument<TestPerson>(new VisualBasicValue<TestPerson>("Person"))
            };
            sequence.Activities.Add(if1);

            var person = new TestPerson 
            { 
                Subscriptions = new ArrayList(),
                BirthDate = DateTime.Today
            };
            person.Subscriptions.Add("Newsweek");
            person.Subscriptions.Add("Time");
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual(0, person.Subscriptions.Count);
        }

        [Test]
        public void it_can_invoke_methods_with_parameters()
        {
            Assert.Inconclusive();
        }

        [Test] 
        public void it_can_return_string_values()
        {
            var outProperty = new DynamicActivityProperty
            {
                Name = "TicketPrice",
                Type = typeof(OutArgument<string>)
            };
            activity.Properties.Add(outProperty);

            var if1 = CreateAssignFor("Person.Age < 12", "TicketPrice", "5.00");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Today.AddYears(-5) };
            workingMemory.Add("Person", person);

            var outputs = WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("5.00", outputs["TicketPrice"]);
        }

        [Test] 
        public void it_can_return_numeric_values()
        {
            var outProperty = new DynamicActivityProperty
            {
                Name = "TicketPrice",
                Type = typeof(OutArgument<decimal>)
            };
            activity.Properties.Add(outProperty);

            var if1 = CreateAssignFor("Person.Age < 12", "TicketPrice", 5.01M);
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Today.AddYears(-5) };
            workingMemory.Add("Person", person);

            var outputs = WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual(5.01M, outputs["TicketPrice"]);
        }

        [Test]
        public void it_can_match_multiple_rules()
        {
            var if1 = CreateAssignFor("Person.Sex = \"Male\"", "Person.Salutation", "Mr.");
            sequence.Activities.Add(if1);

            var if2 = CreateAssignFor("Person.Age > 12 AND Person.Age < 25", "Person.Greeting", "Dude!");
            sequence.Activities.Add(if2);

            var person = new TestPerson { Sex = "Male", BirthDate = DateTime.Today.AddYears(-15) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Mr.", person.Salutation);
            Assert.AreEqual("Dude!", person.Greeting);
        }

        [Test]
        public void it_can_stop_after_first_match()
        {
            var condition1 = new VisualBasicValue<bool>("Person.Sex = \"Male\"");
            var if1 = new If(new InArgument<bool>(condition1));
            var thenSeq = new Sequence();
            thenSeq.Activities.Add( new Assign<string>
            {
                To = new OutArgument<string>(new VisualBasicReference<string>("Person.Salutation")),
                Value = new InArgument<string>("Mr.")
            });
            thenSeq.Activities.Add(new TerminateWorkflow
                {
                    Reason = "Cancel"
                });

            if1.Then = thenSeq;
            sequence.Activities.Add(if1);

            var if2 = CreateAssignFor("Person.Sex = \"Male\"", "Person.Salutation", "Mister");
            sequence.Activities.Add(if2);

            var person = new TestPerson { Sex = "Male" };
            workingMemory.Add("Person", person);
            try
            {
                WorkflowInvoker.Invoke(activity, workingMemory);
            }
            catch (WorkflowTerminatedException)
            {
            }

            Assert.AreEqual("Mr.", person.Salutation);
 
        }

        [Test]
        public void it_can_perform_multiple_actions_for_a_match()
        {
            var condition1 = new VisualBasicValue<bool>("Person.Sex = \"Male\"");
            var if1 = new If(new InArgument<bool>(condition1));
            var thenSeq = new Sequence();
            thenSeq.Activities.Add( new Assign<string>
            {
                To = new OutArgument<string>(new VisualBasicReference<string>("Person.Salutation")),
                Value = new InArgument<string>("Mr.")
            });
            thenSeq.Activities.Add( new Assign<string>
            {
                To = new OutArgument<string>(new VisualBasicReference<string>("Person.Greeting")),
                Value = new InArgument<string>("Hello, Mr.")
            });
            if1.Then = thenSeq;

            sequence.Activities.Add(if1);

            var person = new TestPerson { Sex = "Male" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Mr.", person.Salutation);
            Assert.AreEqual("Hello, Mr.", person.Greeting);
       }

        [Test]
        public void it_can_express_and()
        {
            var if1 = CreateAssignFor("Person.Age > 12 AND Person.Sex = \"Male\"", "Person.Greeting", "Dude!");
            sequence.Activities.Add(if1);

            var person = new TestPerson { Sex = "Male", BirthDate = DateTime.Today.AddYears(-15) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Dude!", person.Greeting);
        }

        [Test]
        public void it_can_express_or()
        {
            var if1 = CreateAssignFor("Person.IsPhysician OR Person.LastName = \"Holliday\"", "Person.Greeting", "Doc");
            sequence.Activities.Add(if1);

            var person = new TestPerson { LastName = "Holliday" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Doc", person.Greeting);
        }

        [Test]
        public void it_can_accept_multiple_inputs()
        {
            var inProperty = new DynamicActivityProperty
            {
                Name = "AnotherPerson",
                Type = typeof(InArgument<TestPerson>)
            };
            activity.Properties.Add(inProperty);

            var outProperty = new DynamicActivityProperty
            {
                Name = "AreRelated",
                Type = typeof(OutArgument<string>)
            };
            activity.Properties.Add(outProperty);

            var if1 = CreateAssignFor("Person.LastName = AnotherPerson.LastName", "AreRelated", "Same last name");
            sequence.Activities.Add(if1);

            var person = new TestPerson { LastName = "Smith" };
            workingMemory.Add("Person", person);

            var anotherPerson = new TestPerson { LastName = "Smith" };
            workingMemory.Add("AnotherPerson", anotherPerson);

            var outputs = WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Same last name", outputs["AreRelated"]);
       }

        [Test]
        public void it_can_access_child_objects()
        {
            var if1 = CreateAssignFor("Person.Spouse.LastName <> Person.LastName", "Person.Greeting", "Very modern");
            sequence.Activities.Add(if1);

            var person = new TestPerson
            {
                LastName = "Smith",
                Spouse = new TestPerson
                {
                    LastName = "Jones"
                }
            };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Very modern", person.Greeting);
        }

        [Test]
        public void it_can_handle_null_child_objects()
        {
            var if1 = CreateAssignFor("Not Person.Spouse Is Nothing AndAlso Person.Spouse.LastName <> Person.LastName", "Person.Greeting", "Very modern");
            sequence.Activities.Add(if1);

            var person = new TestPerson
            {
                LastName = "Smith",
            };
            workingMemory.Add("Person", person);
            
            Assert.DoesNotThrow(
                delegate { WorkflowInvoker.Invoke(activity, workingMemory); });
        }

        #endregion

        #region string comparisons

        [Test]
        public void it_can_express_string_is()
        {
            var if1 = CreateAssignFor("Person.Sex = \"Male\"", "Person.Salutation", "Mr.");
            sequence.Activities.Add(if1);

            var person = new TestPerson { Sex = "Male" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Mr.", person.Salutation);
        }

        [Test]
        public void it_can_express_string_is_not()
        {
            var if1 = CreateAssignFor("Person.Sex <> \"Male\"", "Person.Salutation", "Ms.");
            sequence.Activities.Add(if1);

            var person = new TestPerson { Sex = "Female" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Ms.", person.Salutation);
        }

        [Test]
        public void it_can_express_string_contains()
        {
            var if1 = CreateAssignFor("Person.Employer Like \"*NextGen*\"", "Person.Salutation", "Comrade");
            sequence.Activities.Add(if1);

            var person = new TestPerson { Employer = "NGIS - NextGen Healthcare" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Comrade", person.Salutation);
        }

        [Test]
        public void it_can_express_string_does_not_contain()
        {
            var if1 = CreateAssignFor("Not Person.Employer Like \"*NextGen*\"", "Person.Salutation", "Dear Sir");
            sequence.Activities.Add(if1);

            var person = new TestPerson { Employer = "Some other company, Inc." };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Dear Sir", person.Salutation);
        }

        [Test]
        public void it_can_express_string_begins_with()
        {
            var if1 = CreateAssignFor("Person.LastName Like \"Mac*\"", "Person.Salutation", "Laddy");
            sequence.Activities.Add(if1);

            var person = new TestPerson { LastName = "MacDougall" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Laddy", person.Salutation);
        }

        [Test]
        public void it_can_express_string_ends_with()
        {
            var if1 = CreateAssignFor("Person.LastName Like \"*ski\"", "Person.Salutation", "Pan");
            sequence.Activities.Add(if1);

            var person = new TestPerson { LastName = "Kowalski" };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Pan", person.Salutation);
        }
        #endregion

        #region numeric comparisons
        [Test]
        public void it_can_express_number_is() 
        { 
            var if1 = CreateAssignFor("Person.Age = 16", "Person.Greeting", "Dude");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Now.AddYears(-16) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Dude", person.Greeting);
        }

        [Test]
        public void it_can_express_number_is_not()
        {
            var if1 = CreateAssignFor("Person.Age <> 16", "Person.Greeting", "Hello");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Now.AddYears(-17) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Hello", person.Greeting);
        }

        [Test]
        public void it_can_express_number_is_greater_than()
        {
            var if1 = CreateAssignFor("Person.Age > 25", "Person.Greeting", "Hello");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Now.AddYears(-26) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Hello", person.Greeting);
        }

        [Test]
        public void it_can_express_number_is_less_than()
        {
            var if1 = CreateAssignFor("Person.Age < 12", "Person.Greeting", "Hi");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Now.AddYears(-11) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Hi", person.Greeting);
        }

        [Test]
        public void it_can_express_number_is_in_the_range()
        {
            var if1 = CreateAssignFor("Person.Age > 11 AND Person.Age < 26", "Person.Greeting", "Dude");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Now.AddYears(-17) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Dude", person.Greeting);
        }

        #endregion

        #region date comparisons
        [Test]
        public void it_can_express_date_is()
        {
            var if1 = CreateAssignFor("Person.BirthDate = new DateTime(2011,1,1)", "Person.Greeting", "All ones!");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = new DateTime(2011,1,1) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("All ones!", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_not()
        {
            var if1 = CreateAssignFor("Person.BirthDate <> new DateTime(2011,1,1)", "Person.Greeting", "Too bad");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = new DateTime(2012,1,1) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Too bad", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_after()
        {
            var if1 = CreateAssignFor("Person.BirthDate > new DateTime(1999,12,31)", "Person.Greeting", "Child of 21st Century");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Today };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Child of 21st Century", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_before()
        {
            var if1 = CreateAssignFor("Person.BirthDate < new DateTime(1900,1,1)", "Person.Greeting", "Child of 18th Century");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = new DateTime(1861,4,12) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Child of 18th Century", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_in_the_last()
        {
            // Person.BirthDate is in the last 2 years
            var if1 = CreateAssignFor("Person.BirthDate > DateTime.Today.AddYears(-2)", "Person.Greeting", "Baby");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = DateTime.Today.AddYears(-1) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Baby", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_not_in_the_last()
        {
            var if1 = CreateAssignFor("Person.BirthDate < DateTime.Today.AddYears(-70)", "Person.Greeting", "Sir");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = new DateTime(1861,4,12) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Sir", person.Greeting);
        }

        [Test]
        public void it_can_express_date_is_in_the_range()
        {
            var if1 = CreateAssignFor("Person.BirthDate > new DateTime(1979,12,31) AND Person.BirthDate < new DateTime(1990,1,1)", "Person.Greeting", "Child of the 80's");
            sequence.Activities.Add(if1);

            var person = new TestPerson { BirthDate = new DateTime(1985,6,6) };
            workingMemory.Add("Person", person);

            WorkflowInvoker.Invoke(activity, workingMemory);
            Assert.AreEqual("Child of the 80's", person.Greeting);
        }

        #endregion

        #region private helper methods
        private DynamicActivity CreateActivityShell()
        {
            activity = new DynamicActivity();
            activity.Implementation = () => sequence;
            return activity;
        }

        private DynamicActivity CreateActivityForTestPerson()
        {
            activity = CreateActivityShell();
            var inProperty = new DynamicActivityProperty
            {
                Name = "Person",
                Type = typeof(InArgument<TestPerson>)
            };
            activity.Properties.Add(inProperty);
            var settings = new VisualBasicSettings
            {
                ImportReferences = 
                {
                    new VisualBasicImportReference
                    {
                        Assembly = typeof(TestPerson).Assembly.GetName().Name,
                        Import = typeof(TestPerson).Namespace
                    }
                }
            };
            VisualBasic.SetSettings(activity, settings);

            return activity;
        }

        private static If CreateAssignFor(string condition, string property, string value)
        {
            var condition1 = new VisualBasicValue<bool>(condition);
            var if1 = new If(new InArgument<bool>(condition1));
            if1.Then = new Assign<string>
            {
                To = new OutArgument<string>(new VisualBasicReference<string>(property)),
                Value = new InArgument<string>(value)
            };
            return if1;
        }

        private static If CreateAssignFor(string condition, string property, decimal value)
        {
            var condition1 = new VisualBasicValue<bool>(condition);
            var if1 = new If(new InArgument<bool>(condition1));
            if1.Then = new Assign<decimal>
            {
                To = new OutArgument<decimal>(new VisualBasicReference<decimal>(property)),
                Value = new InArgument<decimal>(value)
            };
            return if1;
        }

        private static If CreateInvokeFor(string condition, string method)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
