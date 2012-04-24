using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public class AssignAction
    {
        private RuleExpression _leftHandSide;
        public string LeftHandSide
        { 
            get { return _leftHandSide.Expression; }
            set { _leftHandSide = new RuleExpression(value); }
        }
        public string Value { get; set; }
        public List<string> ValidationErrors { get; private set; }

        public AssignAction() { }

        public AssignAction(string leftHandSide, string value)
        {
            LeftHandSide = leftHandSide;
            Value = value;
        }

        public bool IsValid(Dictionary<string, Type> context)
        {
            Validate(context);
            return (ValidationErrors.Count == 0);
        }

        public void Validate(Dictionary<string, Type> context)
        {
            ValidationErrors = new List<string>();

            var stillValid = _leftHandSide.IsAssignable(context);
            ValidationErrors.AddRange(_leftHandSide.ValidationErrors);

            if (stillValid)
            {
                // if it's a literal
                var memberType = _leftHandSide.GetExpressionType(context).Name;
                try
                {
                    switch (memberType)
                    {
                        case "String":
                            break;
                        case "Int32":
                            Convert.ToInt32(Value);
                            break;
                        case "Double":
                            Convert.ToDouble(Value);
                            break;
                        case "Decimal":
                            Convert.ToDecimal(Value);
                            break;
                        case "Boolean":
                            Convert.ToBoolean(Value);
                            break;
                        case "DateTime":
                            DateTime.Parse(Value);
                            break;
                        default:
                            stillValid = false;
                            break;
                    }
                }
                catch (Exception e)
                {
                    ValidationErrors.Add("Literal value '" + Value + "' is not a valid value for '" + LeftHandSide + "'.");
                }
            }
            else
            {
                ValidationErrors.Add("'" + LeftHandSide + "' cannot be assigned a value.");
            }
            return;
        }

    }
}
