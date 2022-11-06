using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;

using GrpcRevitRunner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevitTemplateWeb.Core;
using RevitTemplateWebShared.Services;

namespace RevitTemplateWeb.Services
{
    public class RevitCommandRunnerService : RevitRunner.RevitRunnerBase
    {
        private RevitCommandRunner commandRunner;

        public Dictionary<Commands,MethodInfo> RevitRunnerCommandDictionary { get; } =
            new Dictionary<Commands, MethodInfo>();

        public RevitCommandRunnerService(RevitCommandRunner commandRunner)
        {
            this.commandRunner = commandRunner;
        }

        public override Task<CommandOutput> RunCommand(CommandRequest request, ServerCallContext context)
        {
            if (!Enum.TryParse(request.CommandEnum, out Commands commandEnum))
            {
                throw new RevitCommandRunnerException($"Cannot determine command to run from {nameof(request.CommandEnum)}.");
            }

            MethodInfo method = RevitRunnerCommandDictionary[commandEnum];
            if (method.GetCustomAttribute<RevitCommandAttribute>() is not RevitCommandAttribute revitCommandAttribute)
            {
                throw new RevitCommandRunnerException("Command registered without RevitCommandAttribute!");
            }

            Type[] inputVariableTypes = revitCommandAttribute.InputVariableTypes;
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != inputVariableTypes.Length)
            {
                throw new RevitCommandRunnerException(
                    $"Number of input arguments ({inputVariableTypes.Length}) from the attribute on method {method.Name} does not match the number needed by the method ({method.GetGenericArguments().Length}).");
            }
            
            object[] commandInputsAsArray = JsonConvert.DeserializeObject<object[]>(request.CommandInputJson);
            
            object output = method.Invoke(commandRunner, commandInputsAsArray);
            string jsonOutput = JsonConvert.SerializeObject(output);
            
            return Task.FromResult(new CommandOutput()
            {
                CommandOutputJson = jsonOutput
            });
        }
    }
}