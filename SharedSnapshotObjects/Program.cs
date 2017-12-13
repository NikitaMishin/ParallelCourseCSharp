using System;
using System.Threading.Tasks;

namespace AtomicSnapshot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var boundedSw = new SingleWrMultRdr(readers: 2);
            var randomNumber = new Random();
            var tasks = new Task[2];
            for (int i = 0; i < 17; i++)
            {
                var id = i % 2;
                var value = randomNumber.Next(100);
                tasks[id] = Task.Run(() =>
                    {
                        boundedSw.UpdateI(id, value);
                        Console.WriteLine("write {0} in {1} register", value, id);
                    }
                );

                if (i % 3 == 0)
                {
                    var count = i;
                    Task.Run(() =>
                    {
                        var shot = boundedSw.ScanI(id);
                        Console.WriteLine("read from {0} register on {1} iteration: [{2} ,{3}]", id, count, shot[0],
                            shot[1]);
                    });
                }

                if (i % 2 == 1)
                {
                    Task.WaitAll(tasks);
                }
            }
        }
    }
}