/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/

    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NAPS2.Scan
{
    public abstract class ScanDriverBase : IScanDriver
    {
        public abstract string DriverName { get; }

        public ExtendedScanSettings ScanSettings { get; set; }

        public ScanDevice ScanDevice { get; set; }

        public IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling PromptForDevice().");
            }
            try
            {
                return PromptForDeviceInternal();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected abstract ScanDevice PromptForDeviceInternal();

        public IEnumerable<IScannedImage> Scan()
        {
            if (ScanSettings == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanSettings must be specified before calling Scan().");
            }
            if (ScanDevice == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanDevice must be specified before calling Scan().");
            }
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            try
            {
                return ScanInternal();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected abstract IEnumerable<IScannedImage> ScanInternal();
    }
}