using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNetCom
{
    [ComVisible(true)]
    [Guid("33DBE922-5817-4a20-AE6E-4F334F3C1405")]
    public interface IDotNetInterface
    {
        int TellProcessId();
    }
    
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("8EA21375-2F9D-454a-873A-6B4F565E87CC")]
    public class MyDotNetClass : IDotNetInterface
    {
        public MyDotNetClass()
        {
        }

        public int TellProcessId()
        {
            DotNetLib.DotNetLib dotNetLib = new DotNetLib.DotNetLib();
            return dotNetLib.TellProcessId();
        }
    }
}
