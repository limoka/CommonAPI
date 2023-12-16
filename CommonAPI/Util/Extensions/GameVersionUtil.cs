namespace CommonAPI
{
    public static class GameVersionUtil
    {
        public static Version GetVersion(int major, int minor, int release, int build)
        {
            return new Version(major, minor, release)
            {
                Build = build
            };
        }
        
        public static bool CompatibleWith(this Version first, Version other)
        {
            return first.Major == other.Major &&
                   first.Minor == other.Minor &&
                   first.Release == other.Release;
        }
    }
}