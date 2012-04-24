using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public class RuleSet
    {
        public string Name { get; set; }
        public Dictionary<string, Type> Inputs { get; private set; }
        public List<Rule> Rules { get; private set; }
        public List<string> ValidationErrors { get; private set; }

        public RuleSet()
        {
            Inputs = new Dictionary<string, Type>();
            Rules = new List<Rule>();
        }

        public void AddInput(string name, Type type)
        {
            Inputs.Add(name, type);
        }

        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }

        public bool IsValid()
        {
            Validate();
            return (ValidationErrors.Count == 0);
        }

        public void Validate()
        {
            ValidationErrors = new List<string>();

            if ((Name == null) || (Name == ""))
            {
                ValidationErrors.Add("RuleSet must have a name");
            }

            foreach (var rule in Rules)
            {
                rule.Validate(Inputs);
                ValidationErrors.AddRange(rule.ValidationErrors);
            }

            return;
        }
    }
}
