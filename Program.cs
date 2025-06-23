using System.Diagnostics;

namespace InterprocessCommunication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Информация о системе:
ОС: Microsoft Windows NT 10.0.26100.0
Версия .NET: 8.0.13
Количество ядер CPU: 12
64-битная система
CPU: 13th Gen Intel(R) Core(TM) i5-1335U
Оперативная память: 8гб
Видеокарты: Intel(R) Iris(R) Xe Graphics, NVIDIA GeForce RTX 2050
");
            int[] hundredThousandInts = GenerateArray(100000);
            int[] millionInts = GenerateArray(1000000);
            int[] tenMillionsInts = GenerateArray(10000000);

            Console.WriteLine("| {0,-10} |", "Обычное");
            Console.WriteLine(new string('-', 18));
            Console.WriteLine("|100000  | {0} ms |", CommonSum(hundredThousandInts));
            Console.WriteLine("|1000000 | {0} ms |", CommonSum(millionInts));
            Console.WriteLine("|10000000| {0} ms |", CommonSum(tenMillionsInts));
            Console.WriteLine(new string('-', 18));
            Console.WriteLine("| {0,-10} |", "Параллельное");
            Console.WriteLine(new string('-', 18));
            Console.WriteLine("|100000  | {0} ms |", ParallelSum(hundredThousandInts));
            Console.WriteLine("|1000000 | {0} ms |", ParallelSum(millionInts));
            Console.WriteLine("|10000000| {0} ms |", ParallelSum(tenMillionsInts));
            Console.WriteLine(new string('-', 18));
            Console.WriteLine("| {0,-10} |", "PLINQ");
            Console.WriteLine(new string('-', 18));
            Console.WriteLine("|100000  | {0} ms |", PLINQSum(hundredThousandInts));
            Console.WriteLine("|1000000 | {0} ms |", PLINQSum(millionInts));
            Console.WriteLine("|10000000| {0} ms |", PLINQSum(tenMillionsInts));

        }

        static int[] GenerateArray(int count)
        {
            return Enumerable.Range(1, count).ToArray();
        }

        static long CommonSum(int[] ints)
        {
            long sum = 0;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var item in ints)
            {
                sum += item;
            }

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }

        static long PLINQSum(int[] arr)
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                long sum = arr.AsParallel().Sum(item => (long)item);

                stopWatch.Stop();

                return stopWatch.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        static long ParallelSum(int[] arr)
        {
            int threadCount = Environment.ProcessorCount;
            long totalSum = 0;
            int chunkSize = arr.Length / threadCount;
            List<Thread> threads = new List<Thread>();
            List<long> partialSums = new List<long>(new long[threadCount]);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < threadCount; i++)
            {
                int threadIdx = i;
                int start = threadIdx * chunkSize;
                int end = (threadIdx == threadCount - 1) ? arr.Length : (threadIdx + 1) * chunkSize;

                Thread thread = new Thread(() =>
                {
                    long localSum = 0;

                    for (int j = start; j < end; j++)
                    {
                        localSum += arr[j];
                    }

                    partialSums[threadIdx] = localSum;
                });

                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            totalSum = partialSums.Sum();
            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }
    }
}
