using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Activities.Statements;

namespace RuleEngine.Core
{
    public class WFRuleEngine : IRuleEngine
    {
        private DynamicActivity _activity;
        static private readonly Dictionary<string, string> _stringOperators;
        static private readonly Dictionary<string, string> _numberOperators;
        static private readonly Dictionary<string, string> _booleanOperators;
        static private readonly Dictionary<string, string> _dateOperators;

        static WFRuleEngine()
        {
            _stringOperators = new Dictionary<string, string>();
            _stringOperators.Add("is", "{0} = \"{1}\"");
            _stringOperators.Add("is not", "{0} <> \"{1}\"");
            _stringOperators.Add("contains", "{0} Like \"*{1}*\"");
            _stringOperators.Add("does not contain", "Not {0} Like \"*{1}*\"");
            _stringOperators.Add("begins with", "{0} Like \"{1}*\"");
            _stringOperators.Add("ends with", "{0} Like \"*{1}\"");

            _numberOperators = new Dictionary<string, string>();
            _numberOperators.Add("is", "{0} = {1}");
            _numberOperators.Add("is not", "{0} <> {1}");
            _numberOperators.Add("is less than", "{0} < {1}");
            _numberOperators.Add("is greater than", "{0} > {1}");
            _numberOperators.Add("is in the range", "{0} > {1} AND {0} < {2}");

            _booleanOperators = new Dictionary<string, string>();
            _booleanOperators.Add("is true", "{0}");
            _booleanOperators.Add("is false", "Not {0}");

            _dateOperators = new Dictionary<string, string>();
            _dateOperators.Add("is", "{0} = new DateTime({1},{2},{3})");
            _dateOperators.Add("is not", "{0} <> new DateTime({1},{2},{3})");
            _dateOperators.Add("is after", "{0} > new DateTime({1},{2},{3})");
            _dateOperators.Add("is before", "{0} < new DateTime({1},{2},{3})");
            _dateOperators.Add("is in the last", "{0} > DateTime.Today.Add{2}(-{3})");
            _dateOperators.Add("is not in the last", "{0} < DateTime.Today.Add{2}(-{3})");
            _dateOperators.Add("is in the range", "{0} > new DateTime({1},{2},{3}) AND {0} < new DateTime({4},{5},{6})");
        }

        public static WFRuleEngine Compile(RuleSet ruleSet)
        {
            if (ruleSet.IsValid())
            {
                var activity = new DynamicActivity();
                CompileInputs(activity, ruleSet);
                var implementation = CompileRules(ruleSet);
                activity.Implementation = () => implementation;
                return new WFRuleEngine(activity);
            }
            else
            {
                throw new Exception("RuleSet cannot be compiled.");
            }
        }

        private WFRuleEngine(DynamicActivity activity)
        {
            _activity = activity;
        }

        public void Invoke(Dictionary<string, object> workingMemory)
        {
            WorkflowInvoker.Invoke(_activity, workingMemory);
        }

        #region private methods

        private static void CompileInputs(DynamicActivity activity, RuleSet ruleSet)
        {
            var settings = new VisualBasicSettings();
            foreach (var input in ruleSet.Inputs)
            {
                var inProperty = new DynamicActivityProperty
                {
                    Name = input.Key,
                    Type = typeof(InArgument<>).MakeGenericType(input.Value)
                };
                activity.Properties.Add(inProperty);

                settings.ImportReferences.Add(new VisualBasicImportReference
                    {
                        Assembly = input.Value.Assembly.GetName().Name,
                        Import = input.Value.Namespace
                    });
            }
            VisualBasic.SetSettings(activity, settings);
        }

        private static Sequence CompileRules(RuleSet ruleSet)
        {
            var sequence = new Sequence();
            foreach (var rule in ruleSet.Rules)
            {
                var condition = CompileConditions(rule.TopCondition, ruleSet.Inputs);
                condition.Then = CompileActions(rule, ruleSet.Inputs);
                sequence.Activities.Add(condition);
            }
            return sequence;
        }

        private static If CompileConditions(LogicCondition condition, Dictionary<string, Type> inputs)
        {
            var vbExpression = GetVBExpression(condition, inputs);
            var vbCondition = new VisualBasicValue<bool>(vbExpression);
            return new If(new InArgument<bool>(vbCondition));
        }

        private static string GetVBExpression(IRuleCondition condition, Dictionary<string, Type> inputs)
        {
            string vbExpression = "";

            if (condition.GetType().Name == "LogicCondition")
            {
                LogicCondition logicCondition = condition as LogicCondition;

                var subconditionExpressions = new List<string>();
                foreach (var subcondition in logicCondition.Conditions)
                {
                    subconditionExpressions.Add(GetVBExpression(subcondition, inputs));
                }

                if (logicCondition.Operator == "all")
                {
                    vbExpression = "(" + String.Join(" AndAlso ", subconditionExpressions) + ")";
                }
                else
                {
                    vbExpression = "(" + String.Join(" OrElse ", subconditionExpressions) + ")";
                }
            }
            else
            {
                ExpressionCondition expressionCondition = condition as ExpressionCondition;

                var type = expressionCondition.GetExpressionType(inputs).Name;

                switch (type)
                {
                    case "String":
                        vbExpression = String.Format(_stringOperators[expressionCondition.Operator], new object[] { expressionCondition.LeftHandSide, expressionCondition.RightHandSide[0] });
                        break;
                    case "Int32":
                    case "Double":
                    case "Decimal":
                        break;
                    case "Boolean":
                        vbExpression = String.Format(_booleanOperators[expressionCondition.Operator], new object[] { expressionCondition.LeftHandSide });
                        break;
                    case "DateTime":
                        break;
                    default:
                        break;
                }
            }
            return vbExpression;
        }

        private static Sequence CompileActions(Rule rule, Dictionary<string, Type> inputs)
        {
            var sequence = new Sequence();
            foreach (var action in rule.Actions)
            {
                Activity activity;
                switch (action.GetType().Name)
                {
                    case "AssignAction":
                        activity = new Assign<string>
                            {
                                To = new OutArgument<string>(new VisualBasicReference<string>(action.LeftHandSide)),
                                Value = new InArgument<string>(action.Value)
                            };
                        break;
                    default:
                        throw new Exception("The action type '" + action.GetType().Name + "' has not been implemented");
                }
                sequence.Activities.Add(activity);
            }
            return sequence;
        }

        #endregion
    }
}
