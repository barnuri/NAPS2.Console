/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/

    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Phil Walter
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
using System;
using System.Runtime.InteropServices;
using WIA;

namespace NAPS2.Scan.Wia
{
    internal static class WiaApi
    {
        #region WIA Constants

        public static class DeviceProperties
        {
            public const int DEVICE_NAME = 7;
            public const int HORIZONTAL_FEED_SIZE = 3076;
            public const int VERTICAL_FEED_SIZE = 3077;
            public const int HORIZONTAL_BED_SIZE = 3074;
            public const int VERTICAL_BED_SIZE = 3075;
            public const int PAPER_SOURCE = 3088;
            public const int DOCUMENT_HANDLING_CAPABILITIES = 3086;
            public const int DOCUMENT_HANDLING_STATUS = 3087;
            public const int PAGES = 3096;
        }

        public static class ItemProperties
        {
            public const int DATA_TYPE = 4103;
            public const int VERTICAL_RESOLUTION = 6148;
            public const int HORIZONTAL_RESOLUTION = 6147;
            public const int HORIZONTAL_EXTENT = 6151;
            public const int VERTICAL_EXTENT = 6152;
            public const int BRIGHTNESS = 6154;
            public const int CONTRAST = 6155;
            public const int HORIZONTAL_START = 6149;
        }

        public static class Errors
        {
            public const uint OUT_OF_PAPER = 0x80210003;
            public const uint NO_DEVICE_FOUND = 0x80210015;
            public const uint OFFLINE = 0x80210005;
            public const uint PAPER_JAM = 0x8021000A;

            public const uint UI_CANCELED = 0x80210064;
        }

        public static class Source
        {
            public const int FEEDER = 1;
            public const int FLATBED = 2;
            public const int DUPLEX = 4;
        }

        public static class Status
        {
            public const int FEED_READY = 1;
        }

        public static class Formats
        {
            public const string BMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
        }

        #endregion WIA Constants

        #region Device/Item Management

        public static ScanDevice PromptForDevice()
        {
            var wiaCommonDialog = new CommonDialogClass();
            try
            {
                Device d = wiaCommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
                if (d == null)
                {
                    return null;
                }
                return new ScanDevice(d.DeviceID, GetDeviceName(d.DeviceID));
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == Errors.NO_DEVICE_FOUND)
                {
                    throw new NoDevicesFoundException();
                }
                if ((uint)e.ErrorCode == Errors.OFFLINE)
                {
                    throw new DeviceOfflineException();
                }
                throw new ScanDriverUnknownException(e);
            }
        }

        public static Device GetDevice(ScanDevice scanDevice)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == scanDevice.ID)
                {
                    try
                    {
                        return info.Connect();
                    }
                    catch (COMException e)
                    {
                        if ((uint)e.ErrorCode == Errors.OFFLINE)
                        {
                            throw new DeviceOfflineException();
                        }
                        throw new ScanDriverUnknownException(e);
                    }
                }
            }
            throw new DeviceNotFoundException();
        }

        public static string GetDeviceName(Device device)
        {
            return GetDeviceProperty(device, DeviceProperties.DEVICE_NAME);
        }

        public static string GetDeviceName(string deviceID)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == deviceID)
                {
                    Device device = info.Connect();
                    return GetDeviceName(device);
                }
            }
            throw new DeviceNotFoundException();
        }

        public static Item GetItem(Device device, ExtendedScanSettings settings)
        {
            Item item;
            if (settings.UseNativeUI)
            {
                try
                {
                    var items = new CommonDialogClass().ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent,
                        WiaImageBias.MaximizeQuality, true, true, true);
                    item = items[1];
                }
                catch (COMException e)
                {
                    if ((uint)e.ErrorCode == Errors.UI_CANCELED)
                        return null;
                    throw;
                }
            }
            else
            {
                item = device.Items[1];
            }
            return item;
        }

        #endregion Device/Item Management

        #region Device/Item Configuration

        public static void Configure(Device device, Item item, ExtendedScanSettings settings)
        {
            ConfigureDeviceProperties(device, settings);
            ConfigureItemProperties(device, item, settings);
        }

        private static void ConfigureItemProperties(Device device, Item item, ExtendedScanSettings settings)
        {
            switch (settings.BitDepth)
            {
                case ScanBitDepth.Grayscale:
                    SetItemIntProperty(item, 2, ItemProperties.DATA_TYPE);
                    break;

                case ScanBitDepth.C24Bit:
                    SetItemIntProperty(item, 3, ItemProperties.DATA_TYPE);
                    break;

                case ScanBitDepth.BlackWhite:
                    SetItemIntProperty(item, 0, ItemProperties.DATA_TYPE);
                    break;
            }

            int resolution = settings.Resolution.ToIntDpi();
            SetItemIntProperty(item, resolution, ItemProperties.VERTICAL_RESOLUTION);
            SetItemIntProperty(item, resolution, ItemProperties.HORIZONTAL_RESOLUTION);

            PageDimensions pageDimensions = settings.PageSize.PageDimensions() ?? settings.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            int pageWidth = pageDimensions.WidthInThousandthsOfAnInch() * resolution / 1000;
            int pageHeight = pageDimensions.HeightInThousandthsOfAnInch() * resolution / 1000;

            int horizontalSize =
                GetDeviceIntProperty(device, settings.PaperSource == ScanSource.Glass
                    ? DeviceProperties.HORIZONTAL_BED_SIZE
                    : DeviceProperties.HORIZONTAL_FEED_SIZE);
            int verticalSize =
                GetDeviceIntProperty(device, settings.PaperSource == ScanSource.Glass
                    ? DeviceProperties.VERTICAL_BED_SIZE
                    : DeviceProperties.VERTICAL_FEED_SIZE);

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;

            int horizontalPos = 0;
            if (settings.PageAlign == ScanHorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (settings.PageAlign == ScanHorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            SetItemIntProperty(item, pageWidth, ItemProperties.HORIZONTAL_EXTENT);
            SetItemIntProperty(item, pageHeight, ItemProperties.VERTICAL_EXTENT);
            SetItemIntProperty(item, horizontalPos, ItemProperties.HORIZONTAL_START);
            SetItemIntProperty(item, settings.Contrast, -1000, 1000, ItemProperties.CONTRAST);
            SetItemIntProperty(item, settings.Brightness, -1000, 1000, ItemProperties.BRIGHTNESS);
        }

        private static void ConfigureDeviceProperties(Device device, ExtendedScanSettings settings)
        {
            if (settings.PaperSource != ScanSource.Glass && DeviceSupportsFeeder(device))
            {
                SetDeviceIntProperty(device, 1, DeviceProperties.PAGES);
            }

            switch (settings.PaperSource)
            {
                case ScanSource.Glass:
                    SetDeviceIntProperty(device, Source.FLATBED, DeviceProperties.PAPER_SOURCE);
                    break;

                case ScanSource.Feeder:
                    SetDeviceIntProperty(device, Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;

                case ScanSource.Duplex:
                    SetDeviceIntProperty(device, Source.DUPLEX | Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;
            }
        }

        #endregion Device/Item Configuration

        #region Derived Properties

        public static bool DeviceSupportsFeeder(Device device)
        {
            int capabilities = GetDeviceIntProperty(device, DeviceProperties.DOCUMENT_HANDLING_CAPABILITIES);
            return (capabilities & Source.FEEDER) != 0;
        }

        public static bool DeviceFeederReady(Device device)
        {
            int status = GetDeviceIntProperty(device, DeviceProperties.DOCUMENT_HANDLING_STATUS);
            return (status & Status.FEED_READY) != 0;
        }

        #endregion Derived Properties

        #region WIA Property Getters and Setters

        private static string GetDeviceProperty(Device device, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return property.get_Value().ToString();
                }
            }
            return "";
        }

        private static int GetDeviceIntProperty(Device device, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return (int)property.get_Value();
                }
            }
            return 0;
        }

        private static void SetDeviceIntProperty(Device device, int value, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    object objprop = value;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        private static void SetItemIntProperty(Item item, int value, int propid)
        {
            foreach (Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    object objprop = value;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        private static void SetItemIntProperty(Item item, int value, int expectedMin, int expectedMax, int propid)
        {
            foreach (Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    int expectedAbs = value - expectedMin;
                    int expectedRange = expectedMax - expectedMin;
                    int actualRange = property.SubTypeMax - property.SubTypeMin;
                    int actualValue = expectedAbs * actualRange / expectedRange + property.SubTypeMin;
                    if (property.SubTypeStep != 0)
                    {
                        actualValue -= actualValue % property.SubTypeStep;
                    }
                    actualValue = Math.Min(actualValue, property.SubTypeMax);
                    actualValue = Math.Max(actualValue, property.SubTypeMin);
                    object objprop = actualValue;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        #endregion WIA Property Getters and Setters
    }
}