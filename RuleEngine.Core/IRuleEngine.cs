using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleEngine.Core
{
    public interface IRuleEngine
    {
        void Invoke(Dictionary<string, object> workingMemory);
    }
}
