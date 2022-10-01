using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Logging.LogTargets;

namespace financing_api.Logging
{
    public static class Logger
    {
        private static LogBase logger = null;
        public static void Log(LogTarget target, string message)
        {
            switch (target)
            {
                case LogTarget.Database:
                    logger = new DBLogger();
                    logger.Log(message);
                    break;
                case LogTarget.EventLog:
                    logger = new EventLogger();
                    logger.Log(message);
                    break;
                default:
                    return;
            }
        }
    }
}