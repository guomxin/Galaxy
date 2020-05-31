using System;
using System.Collections.Generic;
using System.Text;

namespace DoSomething
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            for (int j = 0; j < 100000; j++)
            {
                for (int k = 0; k < 200000; k++)
                {
                    i += j;
                    i -= j;
                }
            }

            Console.WriteLine(DateTime.Now.ToLongTimeString());
        }
    }
}
