using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public  class Debug
{
    public static void Info(string str)
    {
        Console.WriteLine(string.Format("{0} [Info]-->{1}",System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),str));
    }
    public static void Error(string str)
    {
        Console.WriteLine(string.Format("{0} [Error]-->{1}", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), str));
    }
}

