using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public class RuleExpression // TODO: not the perfect name yet
    {
        public string Expression { get; private set; }
        public List<string> ValidationErrors;

        private string[] _expressionParts;

        public RuleExpression(string expression)
        {
            Expression = expression;
            _expressionParts = expression.Split(new char[] { '.' });
        }

        public RuleExpression()
        {
        }

        public bool IsValid(Dictionary<string, Type> context)
        {
            Validate(context);
            return (ValidationErrors.Count == 0);
        }

        public void Validate(Dictionary<string, Type> context)
        {
            ValidationErrors = new List<string>();
            if ((Expression == null) || (Expression == ""))
            {
                ValidationErrors.Add("Expression is empty.");
                return;
            }

            var target = _expressionParts[0];

            if (_expressionParts.Length > 2)
            {
                ValidationErrors.Add("Expression '" + Expression + "' has too many segments.");
                return;
            }

            if (! context.ContainsKey(target))
            {
                ValidationErrors.Add("'" + target + "' is unassigned in this context.");
                return;
            }

            if (_expressionParts.Length == 2)
            {
                var memberName = _expressionParts[1];
                var matchingMember = context[target].GetMember(memberName);
                if (matchingMember.Length == 0)
                {
                    ValidationErrors.Add("'" + memberName + "' is not an attribute of '" + target + "'.");
                    return;
                }
            }
            return;
        }

        public Type GetExpressionType(Dictionary<string, Type> context)
        {
            if (IsValid(context))
            {
                var target = _expressionParts[0];
                if (_expressionParts.Length == 1)
                {
                    return context[target];
                }
                else
                {
                    var memberName = _expressionParts[1];
                    var matchingProperty = context[target].GetProperty(memberName);
                    if (matchingProperty != null)
                    {
                        return matchingProperty.PropertyType;
                    }
                    else
                    {
                        var matchingMethod = context[target].GetMethod(memberName);
                        if (matchingMethod != null)
                        {
                            return matchingMethod.ReturnType;
                        }
                    }
                }
            }
            return null;
        }

        public bool IsAssignable(Dictionary<string, Type> context)
        {
            if (IsValid(context))
            {
                if (_expressionParts.Length == 1)
                {
                    return true;
                }
                else
                {
                    var memberName = _expressionParts[1];
                    var matchingProperty = context[_expressionParts[0]].GetProperty(memberName);
                    if ((matchingProperty != null) && (matchingProperty.CanWrite))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
