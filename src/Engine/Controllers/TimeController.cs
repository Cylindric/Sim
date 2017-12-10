using System;

namespace Engine.Controllers
{
    public class TimeController : IController
    {
        #region Singleton
        private static readonly Lazy<TimeController> _instance = new Lazy<TimeController>(() => new TimeController());

        public static TimeController Instance { get { return _instance.Value; } }

        private TimeController()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private DateTime _lastFrameTime;

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public float DeltaTime { get; private set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Start()
        {
            _lastFrameTime = DateTime.Now;
        }

        public void Update()
        {
            DeltaTime = (float)(DateTime.Now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = DateTime.Now;
        }

        public void Render() {}
    }
}
