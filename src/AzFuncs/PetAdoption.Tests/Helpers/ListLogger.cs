using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace PetAdoption.Tests.Helpers
{
    public class ListLogger : ILogger
    {
        public IList<string> Logs;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel loglevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
         TState state, Exception exception, 
         Func<TState, Exception, string> formatter)
        {
            this.Logs.Add(
                formatter(state, exception)
            );
        }

        public ListLogger()
        {
            this.Logs = new List<string>();
        }
    }
}