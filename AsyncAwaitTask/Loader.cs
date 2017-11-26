using System;
using System.Security.Policy;

namespace AsyncAwaitTask
{
    public class Loader
    {
        private Url _url;
        private readonly int _depth;
        private Counter _counter;

        public Loader(Url url, int depth = 1)
        {
            _url = new Url(url.Value);
            _depth = 1;
        }

        private async void G(Url url, int currentDepth)
        {
            throw new NotImplementedException();
        }
    }
}