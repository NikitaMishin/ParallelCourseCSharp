using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GetHashParallel
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch clock = Stopwatch.StartNew();
            GetStringHash("/home/nikita/RiderProjects");
            clock.Stop();
            Console.WriteLine("Time Elapsed: {0}", clock.Elapsed);
        }

        private static string GetStringHash(string path)
        {
            return GetHash(path);
        }

        private static string GetHashFromFile(string path)
        {
            StringBuilder result = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
                foreach (byte t in byteHash)
                {
                    result.Append(t.ToString("X2"));
                }
                using (var stream = new BufferedStream(File.OpenRead(path), 1200000))
                {
                    byte[] data = md5.ComputeHash(stream);
                    foreach (byte t in data)
                    {
                        result.Append(t.ToString("X2"));
                    }
                }
            }
            return result.ToString();
            
        }

        private static string GetHash(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] subDirs = Directory.GetDirectories(path);
            int amountFiles = files.Length;
            int recursiveCalls = subDirs.Length;
            StringBuilder result = new StringBuilder();
            Task<String>[] fileTasks = new Task<string>[amountFiles];
            Task<String>[] dirTasks = new Task<string>[recursiveCalls];
            for (int i = 0; i < amountFiles; i++)
            {
                var i1 = i;
                fileTasks[i] = Task.Run((() => GetHashFromFile(files[i1]))); //files
            }
            for (int i = 0; i < recursiveCalls; i++)
            {
                var i1 = i;
                dirTasks[i] = Task.Run((() => GetHash(subDirs[i1]))); // dirs
            }

            using (MD5 md5 = MD5.Create())
            {
                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
                StringBuilder pathBuilder = new StringBuilder();
                foreach (byte t in byteHash)
                {
                    pathBuilder.Append(t.ToString("X2"));
                }
                result.Append(pathBuilder.ToString());
            }
            Task.WaitAll(fileTasks);

            for (int i = 0; i < amountFiles; i++)
            {
                result.Append(fileTasks[i].Result);
            }
            Task.WaitAll(dirTasks);

            for (int i = 0; i < recursiveCalls; i++)
            {
                result.Append(dirTasks[i].Result);
            }
            return result.ToString();
        }
    }
}
