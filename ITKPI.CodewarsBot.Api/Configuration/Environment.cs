namespace ITKPI.CodewarsBot.Api.Configuration
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
