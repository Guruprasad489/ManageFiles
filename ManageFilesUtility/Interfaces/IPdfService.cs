﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Interfaces
{
    public interface IPdfService
    {
        MemoryStream GeneratePdf();
    }
}
