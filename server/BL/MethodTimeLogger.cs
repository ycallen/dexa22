using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Fluent;

namespace BL
{
    public static class MethodTimeLogger
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {            
            logger.Log(NLog.LogLevel.Info, String.Format("[method : {0}] [execution_time : {1:#,##0}]", methodBase.Name, milliseconds));            
        }
    }
}
