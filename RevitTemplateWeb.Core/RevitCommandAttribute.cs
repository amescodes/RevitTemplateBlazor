using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplateWeb.Core
{
    public class RevitCommandAttribute : Attribute
    {
        public Commands CommandToRun { get; }
        public Type[] InputVariableTypes { get; }
        public Type OutputVariableType { get; }

        public RevitCommandAttribute(Commands commandToRun, Type outputVariableType, params Type[] inputVariableTypes)
        {
            CommandToRun = commandToRun;
            OutputVariableType = outputVariableType;
            InputVariableTypes = inputVariableTypes;
        }
    }
}
