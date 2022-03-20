namespace CommonAPI
{
    /// <summary>
    /// Static container for logger interface to appease Unit testing assembly...
    /// </summary>
    public class CommonLogger
    {
        public static ICommonLogger logger;

        public static void SetLogger(ICommonLogger _logger)
        {
            logger = _logger;
        }
    }
}