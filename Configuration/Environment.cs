namespace Codewars_Bot.Configuration
{
    public static class Environment
    {
        public static string Name()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
