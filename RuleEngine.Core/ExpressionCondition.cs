using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RuleEngine.Core
{
    public class ExpressionCondition : IRuleCondition
    {
        private RuleExpression _leftHandSide;

        public string LeftHandSide 
        {
            get { return _leftHandSide.Expression; }
            set { _leftHandSide = new RuleExpression(value); }
        }

        public Type GetExpressionType(Dictionary<string, Type> context)
        {
            return _leftHandSide.GetExpressionType(context);
        }

        public string Operator { get; set; }
        public List<string> RightHandSide = new List<string>();
        public List<string> ValidationErrors { get; private set; }

        static private List<string> _legalStringOperators;
        static private List<string> _legalNumberOperators;
        static private List<string> _legalBooleanOperators;
        static private List<string> _legalDateOperators;
        static private List<string> _legalDateUnits;
        static private Dictionary<Type, string> _types;

        static ExpressionCondition()
        {
            _legalStringOperators = new List<string>(new string[] 
                { 
                    "is", "is not", "contains", "does not contain", "begins with", "ends with" 
                });

            _legalNumberOperators = new List<string>(new string[]
                {
                    "is", "is not", "is less than", "is greater than", "is in the range"
                });

            _legalBooleanOperators = new List<string>(new string[]
                {
                    "is true", "is false"
                });

            _legalDateOperators = new List<string>(new string[]
                {
                    "is", "is not", "is before", "is after", "is in the last", "is not in the last", "is in the range"
                });

            _legalDateUnits = new List<string>(new string[]
                {
                    "days", "weeks", "months", "years"
                });

            _types = new Dictionary<Type, string>();
            _types.Add(typeof(Int32), "Int32");
            _types.Add(typeof(Double), "Double");
            _types.Add(typeof(Decimal), "Decimal");
            _types.Add(typeof(DateTime), "Date");
            _types.Add(typeof(String), "String");
            _types.Add(typeof(Boolean), "Boolean");
        }

        public ExpressionCondition() { }

        public ExpressionCondition(string leftHandSide, string oper)
        {
            LeftHandSide = leftHandSide;
            Operator = oper;
        }

        public ExpressionCondition(string leftHandSide, string oper, string rightHandSide)
        {
            LeftHandSide = leftHandSide;
            Operator = oper;
            RightHandSide = new List<string> { rightHandSide };
        }

        public ExpressionCondition(string leftHandSide, string oper, string rightHandSide1, string rightHandSide2)
        {
            LeftHandSide = leftHandSide;
            Operator = oper;
            RightHandSide = new List<string> { rightHandSide1, rightHandSide2 };
        }

        public bool IsValid(Dictionary<string, Type> context)
        {
            Validate(context);
            return (ValidationErrors.Count == 0);
        }

        public void Validate(Dictionary<string, Type> context)
        {
            ValidationErrors = new List<string>();
            var stillValid = _leftHandSide.IsValid(context);
            ValidationErrors.AddRange(_leftHandSide.ValidationErrors);

            if (stillValid)
            {
                var memberType = _leftHandSide.GetExpressionType(context).Name;
                switch (memberType)
                {
                    case "String":
                        ValidateStringCondition();
                        break;
                    case "Int32":
                    case "Double":
                    case "Decimal":
                        ValidateNumberCondition(memberType);
                        break;
                    case "Boolean":
                        ValidateBooleanCondition();
                        break;
                    case "DateTime":
                        ValidateDateCondition();
                        break;
                    default:
                        ValidationErrors.Add("'" + memberType + "' is not supported");
                        break;
                }
            }
            return;
        }

        private bool ValidateDateCondition()
        {
            if (!_legalDateOperators.Contains(Operator))
            {
                ValidationErrors.Add("\"" + Operator + "\" is not a valid date operator");
            }

            if (Operator == "is in the range")
            {
                if (RightHandSide.Count != 2)
                {
                    ValidationErrors.Add("Date \"is in the range\" requires exactly two arguments");
                    return false;
                }

                try
                {
                    DateTime.Parse(RightHandSide[0]);
                    DateTime.Parse(RightHandSide[1]);
                }
                catch (FormatException e)
                {
                    ValidationErrors.Add("Date \"is in the range\" arguments must be dates");
                    return false;
                }
            }
            else if (Operator == "is in the last" || Operator == "is not in the last")
            {
                if (RightHandSide.Count != 2)
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" requires exactly two arguments");
                    return false;
                }

                try
                {
                    Convert.ToUInt32(RightHandSide[0]);
                }
                catch (FormatException)
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" requires a positive integer as the first argument");
                }
                catch (OverflowException)
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" requires a positive integer as the first argument");
                }

                if (!_legalDateUnits.Contains(RightHandSide[1]))
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" requires " + _legalDateUnits + "as ther second argument");
                }
            }
            else
            {
                if (RightHandSide.Count != 1)
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" requires exactly one argument");
                    return false;
                }

                try
                {
                    DateTime.Parse(RightHandSide[0]);
                }
                catch (FormatException e)
                {
                    ValidationErrors.Add("Date \"" + Operator + "\" argument must be a date");
                }
            }
            return (ValidationErrors.Count == 0);
        }

        private bool ValidateBooleanCondition()
        {
            if (!_legalBooleanOperators.Contains(Operator))
            {
                ValidationErrors.Add("\"" + Operator + "\" is not a valid boolean operator");
            }

            if (RightHandSide.Count != 0)
            {
                ValidationErrors.Add("\"Boolean " + Operator + "\" accepts no parameters");
            }

            return (ValidationErrors.Count == 0);
        }

        private bool ValidateNumberCondition(string type)
        {
            if (!_legalNumberOperators.Contains(Operator))
            {
                ValidationErrors.Add("\"" + Operator + "\" is not a valid numeric operator");
            }

            if (Operator == "is in the range" || Operator == "is not in the range")
            {
                if (RightHandSide.Count != 2)
                {
                    ValidationErrors.Add("\"Number " + Operator + "\" requires two parameters");
                }
            }
            else if (RightHandSide.Count != 1)
            {
                ValidationErrors.Add("\"Number " + Operator + "\" requires exactly one parameter");
            }

            // validate the RHS
            foreach (var value in RightHandSide)
            {
                try
                {
                    switch (type)
                    {
                        case "Int32":
                            Convert.ToInt32(value);
                            break;
                        case "Decimal":
                            Convert.ToDecimal(value);
                            break;
                        case "Double":
                            Convert.ToDouble(value);
                            break;
                        default:
                            return false;
                    }
                }
                catch (Exception e)
                {
                    ValidationErrors.Add("Right hand side failed validation: " + e.Message);
                }
            }
            return (ValidationErrors.Count == 0);
        }

        private bool ValidateStringCondition()
        {
            if (!_legalStringOperators.Contains(Operator))
            {
                ValidationErrors.Add("\"" + Operator + "\" is not a valid string operator");
            }
            if (RightHandSide.Count != 1)
            {
                ValidationErrors.Add("\"String " + Operator + "\" only takes a single value");
            }

            return (ValidationErrors.Count == 0);
        }

    }
}
