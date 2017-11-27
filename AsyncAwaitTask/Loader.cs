using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

namespace AsyncAwaitTask
{
    public class Loader
    {
        private readonly int _depth;

        public Loader(Url url, int depth = 1)
        {
            _depth = depth;
        }

        private async Task<bool> RecurcivePrintAsync(Url url, int currentDepth)
        {
            var str = await GetStringAndLengthASync(url.Value);
            Console.WriteLine(url.Value + ":" + str.Item2);

            if (currentDepth == 0) return true;
            var links = GetUrls(str.Item1);

            var tasks = new List<Task>();
            foreach (var link in links)
            {
                tasks.Add(RecurcivePrintAsync(new Url(link), currentDepth - 1));
            }

            //Task.WaitAll(tasks.ToArray());
            await Task.WhenAll(tasks.ToArray());
            return true;
        }

        //Return string and length of byte[]
        private async Task<Tuple<string, int>> GetStringAndLengthASync(string page)
        {
            string str;
            int contentLength;
            using (var client = new WebClient())
            {
                var bytes = await client.DownloadDataTaskAsync(page);
                str = client.Encoding.GetString(bytes);
                contentLength = bytes.Length;
            }
            return new Tuple<string, int>(str, contentLength);
        }

        public void PrintResult(Url url)
        {
            var task = RecurcivePrintAsync(url, _depth);
            task.Wait();
        }

        private List<string> GetUrls(string data)
        {
            var urlRegex = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            var result = new HashSet<string>();
            foreach (Match match in urlRegex.Matches(data))
            {
                result.Add(match.Value);
            }
            return result.ToList();
        }
    }
}