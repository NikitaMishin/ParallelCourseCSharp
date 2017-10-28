using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace PrimeNumbesTask
{
    internal class Program
    {
        public class PrimeNumbers
        {
            private static int partitionDegree = 2;

            public static void Main(string[] args)
            {
                Console.Write("Write range\nLowerBound: ");
                int lowerBound = Convert.ToInt32(Console.ReadLine());
                Console.Write("UpperBound: ");
                int upperBound = Convert.ToInt32(Console.ReadLine()) + 1;
                
                Stopwatch timer = Stopwatch.StartNew();
                PrimesListByThreads(lowerBound, upperBound);
                timer.Stop();
                Console.WriteLine("PrimeListByThreads\nTime elapsed: {0}", timer.Elapsed);
                timer.Restart();
                PrimesListByThreadPool(lowerBound,upperBound);
                timer.Stop();
                Console.WriteLine("PrimeListByThreadPool\nTime elapsed: {0}",timer.Elapsed);
                timer.Restart();
                PrimesListByTasks(lowerBound,upperBound);
                timer.Stop();
                Console.WriteLine("PrimeListByTasks\nTime elapsed: {0}",timer.Elapsed);
            }

            public static bool IsPrime(int number)
            {
                for (int div = 2; div <= Math.Sqrt(number); div++)
                {
                    if (number % div == 0)
                    {
                        return false;
                    }
                }
                return true;
            }


            private static List<int> Partition(int range, int numThreads)
            {
                List<int> result = new List<int>();
                int residue = range;
                for (int i = 0; i < numThreads; i++)
                {
                    int interval = residue / partitionDegree;
                    residue -= interval;
                    result.Add(interval);
                }
                if (residue != 0) result[0] += residue;
                return result;
            }

            private static List<int> PrimesListByThreads(int lowerBound, int upperBound)
            {
                int numThreads = 10;
                int offset = lowerBound;
                bool[] primeNumbers = new bool[upperBound - lowerBound];
                var intervals = Partition(upperBound - lowerBound, numThreads);

                Thread[] threads = new Thread[numThreads];

                int startIndex = 0;
                int start = lowerBound;
                for (int i = 0; i < numThreads; i++)
                {
                    int start1 = start, index = startIndex,ind = i;
                    threads[i] = new Thread(() => CheckSubRange(start1, start1 + intervals[ind], primeNumbers, index));
                    threads[i].Start();
                    start += intervals[i];
                    startIndex += intervals[i];
                }

                for (int i = 0; i < numThreads; i++)
                {
                    threads[i].Join();
                }
                List<int> result = new List<int>();

                for (int i = 0; i < primeNumbers.Length; i++)
                {
                    if (primeNumbers[i]) result.Add(offset + i);
                }
                return result;
            }

            private static List<int> PrimesListByThreadPool(int lowerBound, int upperBound)
            {
                int offset = lowerBound;
                int amount = upperBound - lowerBound;
                int optimalThreads = Convert.ToInt32(Math.Log(amount, partitionDegree));
                bool[] primes = new bool[amount];
                List<int> interval = Partition(amount, optimalThreads);

                ManualResetEvent[] handles = new ManualResetEvent[optimalThreads];
                int start = lowerBound;
                int index = 0;
                for (int i = 0; i < optimalThreads; i++)
                {
                    handles[i] = new ManualResetEvent(false);
                    int start1 = start, index1 = index, i1 = i;
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        CheckSubRange(start1, start1 + interval[i1], primes, index1);
                        handles[i1].Set();
                    });
                    index += interval[i];
                    start += interval[i];
                }
                WaitHandle.WaitAll(handles);
                List<int> result = new List<int>();
                for (int i = 0; i < amount; i++)
                {
                    if (primes[i] == true) result.Add(i + offset);
                }
                return result;
            }


            private static List<int> PrimesListByTasks(int lowerBound, int upperBound)
            {
                int offset = lowerBound;
                int amount = upperBound - lowerBound;
                int optimalThreads = Convert.ToInt32(Math.Log(amount, partitionDegree));
                bool[] primes = new bool[amount];
                var interval = Partition(amount, optimalThreads);

                Task[] tasks = new Task[optimalThreads];
                for (int i = 0, start = lowerBound, index = 0; i < tasks.Length; i++)
                {
                    var start1 = start;
                    var i1 = i;
                    var index1 = index;
                    tasks[i] = Task.Run((() => CheckSubRange(start1, start1 + interval[i1], primes, index1)));
                    start += interval[i];
                    index += interval[i];
                }
                Task.WaitAll(tasks);

                var result = new List<int>();
                for (int i = 0; i < amount; i++)
                {
                    if (primes[i] == true) result.Add(offset + i);
                }
                return result;
            }

            public static void CheckSubRange(int lower, int upper, bool[] primes, int startIndex)
            {
                int currIndex = startIndex;
                for (int num = lower; num < upper; currIndex++, num++)
                {
                    if (IsPrime(num))
                    {
                        primes[currIndex] = true;
                    }
                }
            }
        }
    }
}