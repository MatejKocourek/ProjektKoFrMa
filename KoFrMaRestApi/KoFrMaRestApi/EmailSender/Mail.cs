﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Text;
using System.Net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using KoFrMaRestApi.Models.Daemon;
using KoFrMaRestApi.Models;
using KoFrMaRestApi.Models.Daemon.Task.BackupJournal;

namespace KoFrMaRestApi.EmailSender
{
    public class Mail
    {
        string smtpAddress = "smtp.gmail.com";
        int portNumber = 587;
        string SemailFrom = "kofrmabackup@gmail.com";
        string Spassword = "KoFrMa123456";
        string Ssubject = "Test";
        string Sbody = "";

        string SemailTo = "machpetr@sssvt.cz";

        public void SendEmail()
        {
            List<Exception> exceptions = new List<Exception>();
            List<TaskComplete> completedTasks = new List<TaskComplete>() { };
            using (MySqlConnection connection = WebApiConfig.Connection())
            using (MySqlCommand command = new MySqlCommand("select * from RestApiExceptions where AdminNotified = 0", connection))
            {
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exceptions.Add(JsonConvert.DeserializeObject<Exception>((string)reader["RestApiExceptions"]));
                    }
                }
                command.CommandText = "SELECT * FROM `tbTasksCompleted` WHERE `AdminNotified` = 0";
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        completedTasks.Add(new TaskComplete() {
                            DatFile = JsonSerializationUtility.Deserialize<BackupJournalObject>((string)reader["BackupJournal"]),
                            IDTask = (int)reader["Id"],
                            DaemonInfo = new DaemonInfo() { Id = (int)reader["Id"] },
                            TimeOfCompletition = (DateTime)reader["TimeOfCompletition"],
                            DebugLog = new List<string>() { (string)reader["DebugLog"] },
                            IsSuccessfull = (bool)reader["IsSuccessfull"]
                        });
                    }
                }
                command.CommandText = "UPDATE `RestApiExceptions` SET `AdminNotified`=1 WHERE `AdminNotified`= 0";
                command.ExecuteNonQuery();
            }
            if (exceptions.Count != 0)
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(SemailFrom);
                mail.To.Add(SemailTo);
                mail.Subject = Ssubject;
                mail.Body = "< div style='border: medium solid grey; width: 500px; height: 266px;font-family: arial,sans-serif; font-size: 17px;'>";
                Sbody += "<h3 style='background-color: blueviolet; margin-top:0px;'>KOFRMA backup agency</h3>";
                Sbody += "<br />";
                Sbody += "Dear " + SemailTo + ",";
                Sbody += "<br />";
                if (exceptions.Count > 0)
                {
                    Sbody += $"Since last time, there has been {exceptions.Count} errors on KoFrMaRestApi server, here are the messages:";
                    foreach (var item in exceptions)
                    {
                        Sbody += item.Message;
                        Sbody += "<br/>";
                    }
                }
                if (completedTasks.Count > 0)
                {
                    Sbody += $"Since last time, {completedTasks.Count} were completed, here is more info:";
                    foreach (var item in completedTasks)
                    {
                        Sbody += item.IDTask;
                        Sbody += $"<a href=\"{WebApiConfig.WebServerURL}\">More info</a>";
                        Sbody += "<br/>";
                    }
                }
                Sbody += "<p>";
                Sbody += "Thank you for using our backup </p>";
                Sbody += " <br />";
                Sbody += "Thanks,";
                Sbody += "<br />";
                Sbody += "<b>The Team</b>";
                Sbody += "</div>";
                SmtpClient smt = new SmtpClient();
                mail.IsBodyHtml = true;
                smt.Host = smtpAddress;
                smt.Port = portNumber;
                smt.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential(SemailFrom, Spassword);
                //smtp.UseDefaultCredentials = true;
                smt.Credentials = nc;
                smt.Send(mail);
            }
        }
    }
}