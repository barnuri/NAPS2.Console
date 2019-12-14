﻿using NAPS2.ImportExport.Pdf;
using NAPS2.Scan.Wia;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject.Modules;

namespace NAPS2.DI
{
    public class WinFormsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPdfPasswordProvider>().To<WinFormsPdfPasswordProvider>();
            Bind<IWiaTransfer>().To<WinFormsWiaTransfer>();
            Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
            Bind<IOverwritePrompt>().To<WinFormsOverwritePrompt>();
        }
    }
}