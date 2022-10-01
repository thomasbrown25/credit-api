using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Logging.LogTargets
{
    public class EventLogger : LogBase
    {
        public override void Log(string message)
        {
            lock (lockObj)
            {
                EventLog m_EventLog = new EventLog();
                m_EventLog.Source = "EventLog";
                m_EventLog.WriteEntry(message);
            }
        }
    }
}