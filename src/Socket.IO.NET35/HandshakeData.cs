﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socket.IO.NET35
{
    public class HandshakeData
    {
        public string Sid;
        public List<string> Upgrades = new List<string>();
        public long PingInterval;
        public long PingTimeout;

        public HandshakeData(string data)
            : this(JObject.Parse(data))
        {
        }

        public HandshakeData(JObject data)
        {
            var upgrades = data.GetValue("upgrades");

            foreach (var e in upgrades)
            {
                Upgrades.Add(e.ToString());
            }

            Sid = data.GetValue("sid").Value<string>();
            PingInterval = data.GetValue("pingInterval").Value<long>();
            PingTimeout = data.GetValue("pingTimeout").Value<long>();
        }
    }
}
