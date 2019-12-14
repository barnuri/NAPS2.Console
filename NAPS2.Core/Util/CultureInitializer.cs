using NAPS2.Config;
using System;
using System.Globalization;
using System.Threading;

namespace NAPS2.Util
{
    public class CultureInitializer
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;

        public CultureInitializer(IUserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
        }

        public void InitCulture()
        {
            var cultureId = userConfigManager.Config.Culture ?? appConfigManager.Config.DefaultCulture;
            if (!String.IsNullOrWhiteSpace(cultureId))
            {
                try
                {
                    var culture = new CultureInfo(cultureId);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                catch (CultureNotFoundException e)
                {
                    Log.ErrorException("Invalid culture.", e);
                }
            }
        }
    }
}