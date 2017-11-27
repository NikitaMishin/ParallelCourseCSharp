using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace AsyncAwaitTask
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var res = new Loader(new Url("https://habrahabr.ru/"), 1);
            res.PrintResult(new Url("https://habrahabr.ru/"));
        }
    }
}