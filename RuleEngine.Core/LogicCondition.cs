using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public class LogicCondition : IRuleCondition
    {
        public List<IRuleCondition> Conditions { get; private set; }
        public List<string> ValidationErrors { get; private set; }
        public string Operator { get; set; }

        public LogicCondition()
        {
            Conditions = new List<IRuleCondition>();
        }

        public void AddCondition(IRuleCondition condition)
        {
            Conditions.Add(condition);
        }

        public bool IsValid(Dictionary<string, Type> context)
        {
            Validate(context);
            return (ValidationErrors.Count == 0);
        }

        public void Validate(Dictionary<string, Type> context)
        {
            ValidationErrors = new List<string>();

            if ((Operator != "all") && (Operator != "any"))
            {
                ValidationErrors.Add("'" + Operator + "' is not a valid logical operator");
            }

            foreach (var condition in Conditions)
            {
                condition.Validate(context);
                ValidationErrors.AddRange(condition.ValidationErrors);
            }
            return;
        }

    }
}
