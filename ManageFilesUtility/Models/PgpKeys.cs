﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Models
{
    public class PgpKeys
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string PassPhrase { get; set; }
    }
}
