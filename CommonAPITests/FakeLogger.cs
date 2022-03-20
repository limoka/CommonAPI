using System;
using CommonAPI;

namespace CommonAPITests
{
    public class FakeLogger : ICommonLogger
    {
        public void LogFatal(object data)
        {
            Console.WriteLine(data);
        }

        public void LogError(object data)
        {
            Console.WriteLine(data);
        }

        public void LogWarning(object data)
        {
            Console.WriteLine(data);
        }

        public void LogMessage(object data)
        {
            Console.WriteLine(data);
        }

        public void LogInfo(object data)
        {
            Console.WriteLine(data);
        }

        public void LogDebug(object data)
        {
            Console.WriteLine(data);
        }
    }
}