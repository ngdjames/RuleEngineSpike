using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public interface IRuleCondition
    {
        List<string> ValidationErrors { get; }
        void Validate(Dictionary<string, Type> context);
        bool IsValid(Dictionary<string, Type> context);
    }
}
