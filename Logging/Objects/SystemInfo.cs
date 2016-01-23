/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;               
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Text;
using System.Windows.Forms;                     // for IP address info

namespace Logging
{
    public class SystemInfo
    {
        #region Fields

            private string _CreatedDate;
            private string _MachineName;
            private string _MachineDomain;
            private string _UserName;
            private string _OperatingSystem;
            private string _DotNetVersion;
            private string _ProcessorCount;
            private string _ProcessorSpeed;
            private ulong _TotalRam;
            private List<string> _IpAddresses;

        #endregion

        #region Public Methods

            public SystemInfo()
            {
                _CreatedDate        = DateTime.UtcNow.ToString();
                _MachineName        = Environment.MachineName;

                _MachineDomain      = Environment.UserDomainName;
                _UserName           = Environment.UserName;
                _OperatingSystem    = Environment.OSVersion.ToString();
                _DotNetVersion      = Environment.Version.ToString();
                _ProcessorCount     = Environment.ProcessorCount.ToString();
                _ProcessorSpeed     = GetPrimaryCpuSpeed().ToString();
                _TotalRam           = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;    

                _IpAddresses        = new List<string>();

                foreach (IPAddress ip in Dns.GetHostEntry(SystemInformation.ComputerName).AddressList)
                    _IpAddresses.Add(ip.ToString());
            }
             
            public string SerializeData()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Date Time: " + _CreatedDate);

                sb.AppendLine();

                sb.AppendLine("Machine Domain: " + _MachineDomain);
                sb.AppendLine("Machine Name: " + _MachineName);
                sb.AppendLine("User Name: " + _UserName);
                sb.AppendLine("Operating System: " + _OperatingSystem);
                sb.AppendLine("Dot NET Version: " + _DotNetVersion);
                sb.AppendLine("Processor Count: " + _ProcessorCount);
                sb.AppendLine("Processor Speed: " + _ProcessorSpeed);
                sb.AppendLine("Total Ram: " + _TotalRam);                

                foreach (string s in _IpAddresses)
                    sb.AppendLine("IPAddress: " + s);

                return sb.ToString();
            }

            protected uint GetPrimaryCpuSpeed()
            {
                using (var management_object = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
                {
                    uint speed = (uint)(management_object["CurrentClockSpeed"]);
                    return speed;
                }
            }

        #endregion
    }
}