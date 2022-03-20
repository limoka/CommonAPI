namespace CommonAPI
{
    /// <summary>
    /// Logger interface to appease Unit testing assembly...
    /// </summary>
    public interface ICommonLogger
    {
        public void LogFatal(object data);

        public void LogError(object data);

        public void LogWarning(object data);

        public void LogMessage(object data);

        public void LogInfo(object data);

        public void LogDebug(object data);
    }
}