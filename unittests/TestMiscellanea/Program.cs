using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.Management;

namespace TestMiscellanea
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.UserName);
            Console.WriteLine(Environment.UserDomainName);

            Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("Processor Count: " + Environment.ProcessorCount.ToString());
            Console.WriteLine("Machine Name: " + Environment.MachineName);
            
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            Console.WriteLine("Available RAM: " + ramCounter.NextValue() + " (M)");

            Console.WriteLine("Getting disk space on drive C:");
            try
            {
                string deviceID = "c:";
                ManagementObject disk = new
                        ManagementObject("win32_logicaldisk.deviceid=\'" + deviceID + "'", null);
                disk.Get();
                float diskSize = (ulong)disk["Size"] / 1024.0f / 1024.0f / 1024.0f;
                float freeSpace = (ulong)disk["FreeSpace"] / 1024.0f / 1024.0f / 1024.0f;
                Console.WriteLine(" Drive=" + (string)disk["DeviceID"] +
                        " Size=" + diskSize + " Free=" + freeSpace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Win32_PerfFormattedData_PerfOS_Processor instance");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("PercentProcessorTime: {0}", queryObj["PercentProcessorTime"]);
                    Console.WriteLine("ProcessorName: " + queryObj["Name"]);
                }
            }
            catch (ManagementException e)
            {
               
            }

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PerfFormattedData_PerfOS_Memory");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Win32_PerfFormattedData_PerfOS_Memory instance");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("AvailableMBytes: {0}", queryObj["AvailableMBytes"]);
                    Console.WriteLine("CommitLimit: {0}", double.Parse(queryObj["CommitLimit"].ToString()) / 1024.0 / 1024.0);
                    Console.WriteLine("CommittedBytes: {0}", double.Parse(queryObj["CommittedBytes"].ToString()) / 1024.0 / 1024.0);
                }
            }
            catch (ManagementException e)
            {
                
            }

            /*
            byte[] buf;
            BinaryReader dataFileReader = new BinaryReader(File.Open(@"D:\Users\guomxin\Codes\sd\SDB.exe", FileMode.Open, FileAccess.Read));
            
            bool fReadFinish = false;
            bool fFirst = true;
            while (!fReadFinish)
            {
                BinaryWriter dataFileWriter;
                if (fFirst)
                {
                    dataFileWriter = new BinaryWriter(File.Open(@"D:\test.exe", FileMode.Create, FileAccess.Write));
                    fFirst = false;
                }
                else
                {
                    dataFileWriter = new BinaryWriter(File.Open(@"D:\test.exe", FileMode.Append, FileAccess.Write));
                }
                buf = dataFileReader.ReadBytes(1024);
                dataFileWriter.Write(buf);
                if (buf.Length < 1024)
                {
                    fReadFinish = true;
                }
                dataFileWriter.Close();
            }
            dataFileReader.Close();
            */
        }
    }
}
