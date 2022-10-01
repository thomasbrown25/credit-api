using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Logging.LogTargets
{
    public class DBLogger : LogBase
    {
        string connectionString = string.Empty;
        public override void Log(string message)
        {
            lock (lockObj)
            {
                //Code to log data to the database
            }
        }
    }
}