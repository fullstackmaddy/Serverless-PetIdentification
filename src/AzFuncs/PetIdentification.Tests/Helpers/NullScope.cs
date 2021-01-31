using System;

namespace PetIdentification.Tests.Helpers
{
    /// <summary>
    /// Required by the ListLogger as Ilogger needs a scope.
    /// </summary>
    public class NullScope : IDisposable
    {
        public static NullScope Instance {get;} = new NullScope();

        private NullScope(){}
        public void Dispose(){}
    }
}