﻿using Socket.IO.NET35.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socket.IO.NET35
{
    public abstract class Transport : Emitter
    {
        protected enum ReadyStateEnum
        {
            OPENING,
            OPEN,
            CLOSED,
            PAUSED
        }

        public static readonly string EVENT_OPEN = "open";
        public static readonly string EVENT_CLOSE = "close";
        public static readonly string EVENT_PACKET = "packet";
        public static readonly string EVENT_DRAIN = "drain";
        public static readonly string EVENT_ERROR = "error";
        public static readonly string EVENT_SUCCESS = "success";
        public static readonly string EVENT_DATA = "data";
        public static readonly string EVENT_REQUEST_HEADERS = "requestHeaders";
        public static readonly string EVENT_RESPONSE_HEADERS = "responseHeaders";

        protected static int Timestamps = 0;

        private bool _writeable;
        public bool Writable
        {
            get { return _writeable; }
            set
            {
                var log = LogManager.GetLogger(GlobalHelper.CallerName());
                log.Info(string.Format("Writable: {0} sid={1}", value, this.Socket.Id));
                _writeable = value;
            }
        }

        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }

        public string Name;
        public Dictionary<string, string> Query;

        protected bool Secure;
        protected bool TimestampRequests;
        protected int Port;
        protected string Path;
        protected string Hostname;
        protected string TimestampParam;
        protected SocketEngine Socket;
        protected bool Agent = false;
        protected bool ForceBase64 = false;
        protected bool ForceJsonp = false;
        protected string Cookie;


        protected ReadyStateEnum ReadyState = ReadyStateEnum.CLOSED;

        protected Transport(Options options)
        {
            this.Path = options.Path;
            this.Hostname = options.Hostname;
            this.Port = options.Port;
            this.Secure = options.Secure;
            this.Query = options.Query;
            this.TimestampParam = options.TimestampParam;
            this.TimestampRequests = options.TimestampRequests;
            this.Socket = options.Socket;
            this.Agent = options.Agent;
            this.ForceBase64 = options.ForceBase64;
            this.ForceJsonp = options.ForceJsonp;
            this.Cookie = options.GetCookiesAsString();
        }

        protected Transport OnError(string message, Exception exception)
        {
            Exception err = new SocketEngineException(message, exception);
            this.Emit(EVENT_ERROR, err);

            var log = LogManager.GetLogger(GlobalHelper.CallerName());
            log.Info("Transport Error: " + exception.ToString());

            return this;
        }

        protected void OnOpen()
        {
            ReadyState = ReadyStateEnum.OPEN;
            Writable = true;
            Emit(EVENT_OPEN);
        }

        protected void OnClose()
        {
            ReadyState = ReadyStateEnum.CLOSED;

            if (this.Socket != null && this.Socket.TasksQueue != null)
            {
                var item = this.Socket.TasksQueue.FirstOrDefault();
                if (item != null)
                    item.CancelAll(this.Socket.TasksQueue);
            }

            Emit(EVENT_CLOSE);
        }


        protected virtual void OnData(string data)
        {
            this.OnPacket(EngineParser.DecodePacket(data));
        }

        protected virtual void OnData(byte[] data)
        {
            this.OnPacket(EngineParser.DecodePacket(data));
        }

        protected void OnPacket(EnginePacket packet)
        {
            this.Emit(EVENT_PACKET, packet);
        }

        public Transport Open()
        {
            if (ReadyState == ReadyStateEnum.CLOSED)
            {
                ReadyState = ReadyStateEnum.OPENING;
                DoOpen();
            }
            return this;
        }

        public Transport Close()
        {
            if (ReadyState == ReadyStateEnum.OPENING || ReadyState == ReadyStateEnum.OPEN)
            {
                DoClose();
                OnClose();
            }
            return this;
        }

        public Transport Send(List<EnginePacket> packets)
        {
            var log = LogManager.GetLogger(GlobalHelper.CallerName());
            log.Info("Send called with packets.Count: " + packets.Count);
            var count = packets.Count;
            if (ReadyState == ReadyStateEnum.OPEN)
            {
                //PollTasks.Exec((n) =>
                //{
                Write(packets);
                //});
            }
            else
            {
                log.Info("Transport not open, throwing exception.");
                throw new SocketEngineException("Transport not open");
            }
            return this;
        }

        protected abstract void DoOpen();

        protected abstract void DoClose();

        protected abstract void Write(List<EnginePacket> packets);

        public class Options
        {
            public bool Agent = false;
            public bool ForceBase64 = false;
            public bool ForceJsonp = false;
            public string Hostname;
            public string Path;
            public string TimestampParam;
            public bool Secure = false;
            public bool TimestampRequests = true;
            public int Port;
            public int PolicyPort;
            public Dictionary<string, string> Query;
            public bool IgnoreServerCertificateValidation = false;
            internal SocketEngine Socket;
            public Dictionary<string, string> Cookies = new Dictionary<string, string>();

            public string GetCookiesAsString()
            {
                var result = new StringBuilder();
                var first = true;
                foreach (var item in Cookies)
                {
                    if (!first)
                    {
                        result.Append("; ");
                    }
                    result.Append(string.Format("{0}={1}", item.Key, item.Value));
                    first = false;
                }
                return result.ToString();
            }
        }
    }
}
