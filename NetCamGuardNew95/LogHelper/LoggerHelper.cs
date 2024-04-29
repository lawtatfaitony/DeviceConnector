using System;
using System.IO;
using log4net;
using log4net.Config;

namespace LogUtility
{
    public class LogHelper
    {
        private static readonly ILog logger;
        static LogHelper()
        {
            if (logger == null)
            {
                var repository = LogManager.CreateRepository("NETCoreRepository");

                XmlConfigurator.Configure(repository, new FileInfo("log4net.config")); 
                //logger = LogManager.GetLogger(typeof(LogHelper));
                logger = LogManager.GetLogger("logger");
            }
        }

        /// <summary>
        /// Info log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Info(message);
            else
                logger.Info(message, exception);
        }

        /// <summary>
        /// warning log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Warn(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Warn(message);
            else
                logger.Warn(message, exception);
        }

        /// <summary>
        /// error log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Error(message);
            else
                logger.Error(message, exception);
        }

        /// <summary>
        /// error log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Debug(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Debug(message);
            else
                logger.Debug(message, exception);
        }

        /// <summary>
        /// error log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Fatal(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Fatal(message);
            else
                logger.Fatal(message, exception);
        }
    }
}
