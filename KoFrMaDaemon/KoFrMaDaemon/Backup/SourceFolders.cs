﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoFrMaDaemon.Backup
{
    class SourceFolders:ISource
    {
        public List<string> Paths { get; set; }
    }
}