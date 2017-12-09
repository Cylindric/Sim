using System;

namespace Engine.Utilities
{
    public class Log
    {
        #region Singleton
        private static readonly Lazy<Log> _instance = new Lazy<Log>(() => new Log());

        public static Log Instance { get { return _instance.Value; } }

        private Log()
        {
        }
        #endregion

        public void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
}
