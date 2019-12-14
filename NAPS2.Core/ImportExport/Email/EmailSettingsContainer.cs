﻿using NAPS2.Config;

namespace NAPS2.ImportExport.Email
{
    public class EmailSettingsContainer
    {
        private readonly UserConfigManager userConfigManager;

        private EmailSettings localEmailSettings;

        public EmailSettingsContainer(UserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public EmailSettings EmailSettings
        {
            get { return localEmailSettings ?? userConfigManager.Config.EmailSettings ?? new EmailSettings(); }
            set { localEmailSettings = value; }
        }
    }
}