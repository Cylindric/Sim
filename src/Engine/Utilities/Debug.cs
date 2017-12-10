namespace Engine.Utilities
{
    public static class Debug
    {
        public static void LogError(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }
    }
}
