using System;
using System.Collections.Generic;
using System.Linq;

using HidSharp;

namespace winhiddump
{
    class Program
    {
        static string GetArgument(IEnumerable<string> args, string option) => args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();
        static bool IsArgument(IEnumerable<string> args, string option) => args.Any(i => i == option);

        static void Main(string[] args)
        {
            var help = IsArgument(args, "--help");
            if (help)
            {
                Console.WriteLine("Usage: winhiddump.exe [--help] [--vid hex] [--pid hex]");
                Environment.Exit(0);
            }

            int vid = 0;
            int pid = 0;

            int.TryParse(GetArgument(args, "--vid"), System.Globalization.NumberStyles.HexNumber, null, out vid);
            int.TryParse(GetArgument(args, "--pid"), System.Globalization.NumberStyles.HexNumber, null, out pid);

            var list = DeviceList.Local;
            foreach (var dev in list.GetHidDevices())
            {
                if ((vid != 0 && vid != dev.VendorID) ||
                    (pid != 0 && pid != dev.ProductID))
                {
                    continue;
                }

                Console.Write(string.Format("{0:X4}:{1:X4}: {2} - {3}\nPATH:{4}\n", 
                    dev.VendorID, dev.ProductID, dev.GetManufacturer(), dev.GetProductName(), dev.DevicePath));

                string manu = "";
                string prod = "";

                try
                {
                    manu = dev.GetManufacturer();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Failed to get manufacturer");
                }

                try
                {
                    prod = dev.GetProductName();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Failed to get product name");
                }

                Console.Write(string.Format("{0:X4}:{1:X4}: {2} {3}\nPATH:{4}\n",
                    dev.VendorID, dev.ProductID, manu, prod, dev.DevicePath));

                byte[] rawReportDescriptor = dev.GetRawReportDescriptor();
                Console.Write("DESCRIPTOR: ({0} bytes)\n  ", rawReportDescriptor.Length);
                for (int i = 0; i < rawReportDescriptor.Length; i++)
                {
                    Console.Write(rawReportDescriptor[i].ToString("X2") + " ");
                    Console.Write((i % 16 == 15) ? "\n  " : " ");
                }
                Console.WriteLine("\n");
            }
        }
    }
}
