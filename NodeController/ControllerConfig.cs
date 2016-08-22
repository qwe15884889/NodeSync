﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Xml.Serialization;

using EncodingTools;

namespace NodeController
{
    public class ControllerConfig
    {
        public List<string> ListenPrefixes = new List<string>();
        public bool DecodeKeys = false;

        public class HostConnection
        {
            public string Name = string.Empty;
            public string PublicKey = string.Empty;
            public string APIKey = string.Empty;
        }
        public List<HostConnection> Hosts = new List<HostConnection>();

        public HostConnection FindHost(string hostName)
        {
            return Hosts.Find(x => x.Name == hostName);
        }

        public void InitCrypto(string hotName, RSACryptoServiceProvider rsa)
        {
            var host = FindHost(hotName);
            if (host == null)
                return;

            rsa.ImportCspBlob(Convert.FromBase64String(DecodeKeys ? host.PublicKey.Unprotect() : host.PublicKey));
        }

        public string RootTempFolder = string.Empty;

        public int TokenKeyValidationRange = 0;
        
        public static ControllerConfig ReadConfig(string path)
        {
            FileInfo file = new FileInfo(path);

            XmlSerializer xml = new XmlSerializer(typeof(ControllerConfig));
            ControllerConfig cfg = new ControllerConfig();

            if (!file.Exists)
            {
                cfg.ListenPrefixes.Add("http://localhost:80");
                HostConnection c = new HostConnection();
                c.Name = "A host";
                c.PublicKey = "A key";
                cfg.Hosts.Add(c);

                cfg.RootTempFolder = Path.GetTempPath();

                var fs = file.OpenWrite();
                xml.Serialize(fs, cfg);
                fs.Close();
         
            }
            else
            {
                var fs = file.OpenText();
                cfg = xml.Deserialize(fs) as ControllerConfig;
                fs.Close();
            }

            return cfg;
        }
    }
}