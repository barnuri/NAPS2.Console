using NAPS2.Console.DI;
using NAPS2.Console.Lang.Resources;
using NAPS2.Util;
using Ninject;
using Ninject.Parameters;
using System;
using System.IO;
using System.Reflection;

namespace NAPS2.Console
{
    public static class Program
    {
        public static void Main()
        {
            var file = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(file);
            var output = folder + "\\test.png";
            System.Console.WriteLine("output " + output);
            Scan(new AutomatedScanningOptions { OutputPath = output }, true);
            System.Console.ReadLine();
        }
        public static void Scan(string outputPath, bool dontThrow = false)
        {
            Scan(new AutomatedScanningOptions { OutputPath = outputPath }, dontThrow);
        }

        public static void Scan(AutomatedScanningOptions options = null, bool dontThrow = false)
        {
            options = options ?? new AutomatedScanningOptions();
            try
            {
                var scanning = KernelManager.Kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
                scanning.Execute();
            }
            catch (Exception ex)
            {
                Log.FatalException("An error occurred that caused the console application to close.", ex);
                System.Console.WriteLine(ConsoleResources.UnexpectedError);
                throw;
            }
            finally
            {
                if (options.WaitForEnter)
                {
                    System.Console.ReadLine();
                }
            }
        }
    }
}