using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static Common.CommonBase;
  
namespace Common
{
    public class Logger {
        public LoggerMode loggerMode { get; set; } = LoggerMode.INFO;
        public string LoggerLine { get; set; } = string.Empty;
    }

    public class LoggerQueueHelper
    {
        #region  
        private static object lockObj1 = new object();
        private static void Log(Logger logger)
        {
            lock (lockObj1)
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Log");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string filename = string.Format("LoggerInfo{0:yyyyMMdd}.log", DateTime.Now);
                string file = Path.Combine(filePath, filename);

                switch (logger.loggerMode)
                { 
                    case LoggerMode.DEBUG:
                        filename = string.Format("LoggerDebug{0:yyyyMMdd}.log", DateTime.Now);
                        file = Path.Combine(filePath, filename);
                        break;
                    case LoggerMode.ERROR: 
                        filename = string.Format("LoggerError{0:yyyyMMdd}.log", DateTime.Now);
                        file = Path.Combine(filePath, filename);
                        break;
                    case LoggerMode.FATAL:
                        filename = string.Format("LoggerFatal{0:yyyyMMdd}.log", DateTime.Now);
                        file = Path.Combine(filePath, filename);
                        break;
                    case LoggerMode.WARNNING: 
                        filename = string.Format("LoggerWarnning{0:yyyyMMdd}.log", DateTime.Now);
                        file = Path.Combine(filePath, filename);
                        break;
                    default: 
                       break;
                } 
                using (StreamWriter w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    w.WriteLine(logger.LoggerLine);
                    w.Close();
                }
            }
        }
        #endregion
        private static object lockObj2 = new object();
        private static void ErrorLog(string msg)
        {
            lock (lockObj2)
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string file = Path.Combine(filePath , "LogError.log"); 
                using (StreamWriter w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    w.WriteLine(msg);
                    w.Close();
                }
            }
        }
         
        public bool AnalysisStatus = false;

        public static LoggerQueueHelper instance = new LoggerQueueHelper();
        private LoggerQueueHelper()
        {
        }

        private ConcurrentQueue<Logger> t = new ConcurrentQueue<Logger>();
        /// <summary>
        /// add to queue
        /// </summary>
        /// <param name="tip"></param>
        public int AddData(Logger tip)
        {
            t.Enqueue(tip);
            return t.Count;
        }

        /// <summary>
        /// get the the first queue
        /// </summary>
        /// <returns></returns>
        public Logger Get()
        {
            //如果当前队列为空，返回false，否则返回队列的第一个元素。
            Logger logger = new Logger(); 
            return t.TryPeek(out logger) ? logger : null;
        }

        /// <summary>
        ///get from queue and calc
        /// </summary>
        public void PostAnalysis()
        {
            while (t.Count > 0)
            {
                try
                {
                    AnalysisStatus = true;

                    Logger outresult = new Logger();
                      
                    if (t.TryDequeue(out outresult))
                    {
                        Log(outresult);
                    }
                    else
                    {
                        ErrorLog(string.Format("[{0:yyyy-MM-dd HH:mm:ss}][ERROR (TryDequeue error)]", DateTime.Now));
                    } 
                }
                catch (Exception ex)
                {
                    AnalysisStatus = false;
                    ErrorLog(string.Format("[{0:yyyy-MM-dd HH:mm:ss}][EXCEPTION] [{1}]", DateTime.Now, ex.Message));
                    throw;
                }
            }
            AnalysisStatus = false;
        }

        /// <summary>
        /// get the queue total count
        /// </summary>
        /// <returns></returns>
        public int Getcount()
        {
            return t.Count;
        }
    }
} 