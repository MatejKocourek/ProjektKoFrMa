﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KoFrMaDaemon
{
    public class DaemonSettings
    {
        public string ServerIP;
        public string Password;
        public string LocalLogPath;
        public bool WindowsLog;

        private StreamReader r;

        public DaemonSettings()
        {
            this.WindowsLog = false;
            this.Password = "";
            try
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\KoFrMa\config.ini"))
                {
                    r = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\KoFrMa\config.ini");
                    while (!r.EndOfStream)
                    {
                        string tmpRow = r.ReadLine();
                       if (tmpRow.StartsWith("ServerIP="))
                       {
                            string tmpRowSubstring = tmpRow.Substring(9);
                            if (tmpRowSubstring != "")
                            {
                                this.ServerIP = tmpRowSubstring;
                            }
                       }
                       else if (tmpRow.StartsWith("Password="))
                       {
                           this.Password = tmpRow.Substring(9);
                       }
                       else if (tmpRow.StartsWith("LocalLogPath="))
                       {
                            string tmpRowSubstring = tmpRow.Substring(13);
                            if (tmpRowSubstring!="")
                            {
                                this.LocalLogPath = tmpRowSubstring;
                            }

                       }
                        else if (tmpRow.StartsWith("WindowsLog="))
                        {
                            string tmp = tmpRow.Substring(11);
                            if (tmp == "0")
                            {
                                this.WindowsLog = false;
                            }
                            else if (tmp == "1")
                            {
                                this.WindowsLog = true;
                            }
                        }
                    }
                r.Close();
                r.Dispose();
                }
                
            }
            catch (Exception)
            {

            }
        }
    }
}
