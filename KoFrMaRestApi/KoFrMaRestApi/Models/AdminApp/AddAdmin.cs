﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KoFrMaRestApi.Models.AdminApp
{
    public class AddAdmin
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public int[] Permissions { get; set; }
    }
}