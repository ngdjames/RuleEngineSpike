using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public class Rule
    {
        public string Name;
        public string Description;
        public LogicCondition TopCondition;
        public List<AssignAction> Actions;
        public List<string> ValidationErrors { get; private set; }

        public Rule()
        {
            Actions = new List<AssignAction>();
        }

        public bool IsValid(Dictionary<string, Type> _context)
        {
            Validate(_context);
            return (ValidationErrors.Count == 0);
        }

        public void Validate(Dictionary<string, Type> _context)
        {
            ValidationErrors = new List<string>();
            if (TopCondition == null)
            {
                ValidationErrors.Add("A Rule must have a top level Any or All condition");
            }

            if (Name == null)
            {
                ValidationErrors.Add("A Rule must have a name.");
            }

            foreach (var action in Actions)
            {
                action.Validate(_context);
                ValidationErrors.AddRange(action.ValidationErrors);
            }

            TopCondition.Validate(_context);
            ValidationErrors.AddRange(TopCondition.ValidationErrors);

            return;
        }

        public void AddAction(AssignAction action)
        {
            Actions.Add(action);
        }
    }
}
