﻿using System.Collections.Generic;

namespace KoFrMaDaemon.Backup
{
    public class BackupJournalObject:ISource
    {
        public string RelativePath { get; set; }
        public List<FileInfoObject> BackupJournalFiles { get; set; }

        public List<string> BackupJournalFilesDelete { get; set; }
        public List<FolderObject> BackupJournalFolders { get; set; }

        public List<string> BackupJournalFoldersDelete { get; set; }
    }
}
