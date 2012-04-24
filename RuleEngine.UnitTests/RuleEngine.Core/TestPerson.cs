using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace RuleEngine.UnitTests.RuleEngine.Core
{
    public class TestPerson
    {
        public string Sex { get; set; }
        public string Salutation { get; set; }
        public bool IsPhysician { get; set; }
        public DateTime BirthDate { get; set; }
        public ArrayList Subscriptions { get; set; }
        public string Employer { get; set; }
        public string LastName { get; set; }
        public string Greeting { get; set; }
        public TestPerson Spouse { get; set; }
        private string _medicalRecordNumber;
        public string MedicalRecordNumber
        {
            get { return _medicalRecordNumber; }
        }

        public int Age()
        {
            return CalculateAge();
        }

        public bool IsMinor()
        {
            return Age() < 18;
        }

        public void CancelSubscriptions()
        {
            Subscriptions.Clear();
        }

        private int CalculateAge()
        {
            DateTime now = DateTime.Today;
            int years = now.Year - BirthDate.Year;
            if ((now.Month < BirthDate.Month) ||
                (now.Month == BirthDate.Month && now.Day < BirthDate.Day))
            {
                years--;
            }
            return years;
        }

    }
}
