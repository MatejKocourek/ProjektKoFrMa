﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//using System.IO;
//using System.Net;
using KoFrMaDaemon.ConnectionToServer;
using System.Management;
using KoFrMaDaemon.Backup;

namespace KoFrMaDaemon
{
    public partial class ServiceKoFrMa : ServiceBase
    {
        //private const string servicePrefixName = "KoFrMa";

        private bool inProgress;

        private Timer timer;

        private string logPath;

        private DebugLog debugLog;

        private List<Tasks> ScheduledTasks;

        Connection connection = new Connection();
        DaemonInfo daemon = DaemonInfo.Instance;
        //Naplánované úlohy přijaté od serveru se budou přidávat do tohoto listu

        public ServiceKoFrMa()
        {
            InitializeComponent();
            ScheduledTasks = new List<Tasks>();
            timer = new Timer(5000);
            timer.Elapsed += new ElapsedEventHandler(OnTimerTick);
            inProgress = false;
            this.logPath = @"D:\Matej\Data\Visual Studio\DebugServiceLog.log";
            debugLog = new DebugLog(this.logPath, 8);
            daemon.Version = 101;
            //ziskavat informace z pocitace
            ConnectionInfo.ServerURL = @"http://localhost:50576";
            daemon.OS = System.Environment.OSVersion.VersionString;
            daemon.PC_Unique = this.GetSerNumBIOS();

            timer.AutoReset = true;
            //this.serverURL = @"http://localhost:50576/";
        }

        protected override void OnStart(string[] args)
        {
            debugLog.WriteToLog("Service started", 4);
            timer.Start();
            
            //a.BackupFullFolder(@"d:\Users\Matej\Desktop\KoFrMaBackup\BackupThisFolder\", @"d:\Users\Matej\Desktop\KoFrMaBackup\BackupGoesHere\", debugLog);
            //a.BackupDifferential(@"d:\Users\Matej\Desktop\KoFrMaBackup\BackupGoesHere\", @"d:\Users\Matej\Desktop\KoFrMaBackup\BackupGoesHere\KoFrMaBackup_2018_02_18_20_34_42_Full\KoFrMaBackup.dat", debugLog);
            //a.BackupDifferential(@"d:\tmp\testBackup\BackupGoesHere\", @"d:\tmp\testBackup\BackupGoesHere\KoFrMaBackup_2018_02_18_13_58_48_Full\KoFrMaBackup.dat", debugLog);
        }

        protected override void OnStop()
        {
            this.inProgress = true;
            debugLog.WriteToLog("Service stopped", 4);
        }



        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            debugLog.WriteToLog("Timer tick", 7);
            if (!this.inProgress) //Pokud se service zrovna nevypíná, třeba aby při vypínání Windows nezačala běžet nová úloha nebo pokud se zrovna neprohledává seznam úloh (běží asynchonně)
            {
                this.inProgress = true;
                debugLog.WriteToLog("Updating list of scheduled tasks from the server...", 5);
                this.GetTasks();
                debugLog.WriteToLog("List of scheduled tasks now contains " + this.ScheduledTasks.Count + " tasks", 6);
                this.inProgress = false;
                if (this.ScheduledTasks.Count>0)
                {
                    debugLog.WriteToLog("Tasks found, starting to check if the time has come for each of the tasks", 5);

                    foreach (Tasks item in ScheduledTasks)
                    {
                        debugLog.WriteToLog("Checking if the task should be started for task with ID " + item.IDTask, 7);
                        if (item.TimeToBackup.CompareTo(DateTime.Now) >= 0&&item.InProgress == false) //Pokud čas úlohy už uběhl nebo zrovna neběží
                        {
                            debugLog.WriteToLog("Task " + item.IDTask +" should be running because it was planned to run in " + item.TimeToBackup.ToString() + ", starting the inicialization now...", 6);
                            BackupSwitch backupInstance = new BackupSwitch();
                            try
                            {
                                item.InProgress = true;
                                backupInstance.Backup(item.SourceOfBackup, item.WhereToBackup,item.CompressionLevel, debugLog);
                                connection.TaskCompleted(item, debugLog, true);
                            }
                            catch (Exception ex)
                            {
                                debugLog.WriteToLog("Task failed with fatal error " + ex.Message, 2);
                                connection.TaskCompleted(item, debugLog, false);
                            }
                            finally
                            {
                                item.InProgress = false;
                            }
                        }
                        else
                        {
                            debugLog.WriteToLog("Task " + item.IDTask + " is skipped because " + item.TimeToBackup.ToString() + " is in the future", 6);
                        }
                    }
                }
                else
                {
                    debugLog.WriteToLog("No other tasks planned, service will check again after " + timer.Interval / 1000 + 's', 5);
                }
                

            }
            else
            {
                debugLog.WriteToLog("Service is in the process of stopping, skipping regular timer action...", 5);
            }

        }


        private void GetTasks()
        {
            ScheduledTasks.AddRange(connection.PostRequest());

            //log.WriteToLog("InGetTasks 1");

            //WebRequest request = WebRequest.Create(this.serverURL);
            //request.Method = "POST";
            //request.ContentType = "multipart/form-data"; // ideální pro Upload souborů
            //request.ContentLength = 4;
            //log.WriteToLog("InGetTasks 2");

            //WebResponse response = request.GetResponse();
            //log.WriteToLog("InGetTasks 3");

            //string statusDescr = "StatusDescription = " + ((HttpWebResponse)response).StatusDescription;

            //debugLog.WriteToLog(statusDescr, 5);
            /*
             // Get the stream containing content returned by the server.
             dataStream = response.GetResponseStream();
             byte[] buffer = new byte[16000];
             int ReadCount = dataStream.Read(buffer, 0, buffer.Length);

             dataStream.Close();
             response.Close();
  */
        }

        private string GetSerNumBIOS()
        {
            string lcPopis = "";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    lcPopis = wmi.GetPropertyValue("SerialNumber").ToString().Trim();
                }
                catch { }
            }
            searcher.Dispose();

            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    lcPopis = lcPopis + wmi.GetPropertyValue("SerialNumber").ToString().Trim();
                }
                catch { }
            }
            searcher.Dispose();

            return lcPopis;
        }

    }
}
