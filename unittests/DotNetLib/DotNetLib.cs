using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetLib
{
    public class DotNetLib
    {
        public int TellProcessId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
    }
}
