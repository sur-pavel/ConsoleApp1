using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Reflection;
namespace ConsoleApp1
{
    public class Log
    {
        private static object sync = new object();
        public static void WriteLine(string log)
        {
            try
            {
                string path = @"d:\TestDir\";
                lock (sync)
                {
                    File.AppendAllText(path + "search.log", log + "\n", Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);// Перехватываем все и ничего не делаем
                Console.ReadKey();
            }
        }
    }
}
