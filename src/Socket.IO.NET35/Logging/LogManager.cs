﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Socket.IO.NET35.Logging
{
    public class LogManager
    {
        private const string myFileName = "SocketTrace.txt";
        private string MyType;
        private static readonly LogManager EmptyLogger = new LogManager(null);

        private static System.IO.StreamWriter file;

        public static bool Enabled = true;

        #region Statics

        public static void SetupLogManager()
        {


        }

        public static LogManager GetLogger(string type)
        {
            var result = new LogManager(type);
            return result;
        }

        public static LogManager GetLogger(Type type)
        {
            return GetLogger(type.ToString());
        }

        public static LogManager GetLogger(System.Reflection.MethodBase methodBase)
        {
#if DEBUG
            var type = methodBase.DeclaringType == null ? "" : methodBase.DeclaringType.ToString();
            var type1 = string.Format("{0}#{1}", type, methodBase.Name);
            return GetLogger(type1);
#else
            return EmptyLogger;
#endif
        }

        #endregion

        public LogManager(string type)
        {
            this.MyType = type;
        }

        [Conditional("DEBUG")]
        public void Info(string msg)
        {
            //Trace.WriteLine(string.Format("{0} [{3}] {1} - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), MyType, msg, System.Threading.Thread.CurrentThread.ManagedThreadId));
            if (!Enabled)
            {
                return;
            }

            if (LogManager.file == null)
            {
                LogManager.file = new System.IO.StreamWriter(myFileName, true);
                LogManager.file.AutoFlush = true;
            }

            msg = GlobalHelper.StripInvalidUnicodeCharacters(msg);
            var msg1 = string.Format("{0} [{3}] {1} - {2}", 
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), 
                MyType,
                msg,
                System.Threading.Thread.CurrentThread.ManagedThreadId);
            LogManager.file.WriteLine(msg1);
        }

        [Conditional("DEBUG")]
        public void Error(string p, Exception exception)
        {
            this.Info(string.Format("ERROR {0} {1} {2}", p, exception.Message, exception.StackTrace));
            if (exception.InnerException != null)
            {
                this.Info(string.Format("ERROR exception.InnerException {0} {1} {2}", p, exception.InnerException.Message, exception.InnerException.StackTrace));
            }

        }


        [Conditional("DEBUG")]
        internal void Error(Exception e)
        {
            this.Error("", e);
        }
    }
}
