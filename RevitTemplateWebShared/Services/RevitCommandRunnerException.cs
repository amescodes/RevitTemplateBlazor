using System;
using System.Collections.Generic;
using System.Text;

namespace RevitTemplateWebShared.Services
{
    public class RevitCommandRunnerException : Exception
    {
        public string Message { get; } = "";
        public Exception InnerException { get; }

        public RevitCommandRunnerException(string message)
        {
            Message = message;
        }

        public RevitCommandRunnerException(string message, Exception innerException)
        {
            Message = message;
            InnerException = innerException;
        }
    }
}
