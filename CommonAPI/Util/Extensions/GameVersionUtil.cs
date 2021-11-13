namespace CommonAPI
{
    public class GameVersionUtil
    {
        public static Version GetVersion(int major, int minor, int release, int build)
        {
            return new Version(major, minor, release)
            {
                Build = build
            };
        }
    }
}