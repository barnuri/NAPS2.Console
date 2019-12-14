﻿using System;

namespace NAPS2.Util
{
    public class Log
    {
        private static ILogger _logger = new NullLogger();

        public static ILogger Logger
        {
            get { return _logger; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _logger = value;
            }
        }

        public static void Error(string message, params object[] args)
        {
            _logger.Error(string.Format(message, args));
        }

        public static void ErrorException(string message, Exception exception)
        {
            _logger.ErrorException(message, exception);
        }

        public static void FatalException(string message, Exception exception)
        {
            _logger.FatalException(message, exception);
        }
    }
}