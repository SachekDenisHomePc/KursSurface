using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KursSurface
{
    public class NumericalIntegration
    {
        public IntegrationExtendedInfo CalculateBySimpsonMethod(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction)
        {
            int n = 600;
            double result = 0;
            var threadsTime = new Dictionary<int, TimeSpan>();

            var stopwatch = new Stopwatch();

            for (int i = 1; i <= 15; i++)
            {
                stopwatch.Start();
                result = CalculateDoubleSimpsonWithThreads(integrationInfo, surfaceFunction, n, i);
                stopwatch.Stop();
                threadsTime.Add(i, stopwatch.Elapsed);
                stopwatch.Reset();
            }

            return new IntegrationExtendedInfo()
            {
                IntegrationResult = result,
                ThreadsTime = threadsTime
            };
        }

        private double GetStep(int n, double start, double end)
        {
            return (end - start) / (2 * n);
        }

        private double GetStepRect(int n, double start, double end)
        {
            return (end - start) / (n);
        }

        private double GetByOffset(int offset, double start, double step)
        {
            return start + offset * step;
        }

        private double CalculateDoubleSimpsonWithThreads(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction, int n, int numberOfThreads)
        {
            object lockObject = new object();
            double stepX = GetStep(n, integrationInfo.XStart, integrationInfo.XEnd);
            double stepY = GetStep(n, integrationInfo.YStart, integrationInfo.YEnd);
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            ConcurrentBag<double> sum = new ConcurrentBag<double>();
            ConcurrentBag<int> threadTest = new ConcurrentBag<int>();

            using (StreamWriter writer = new StreamWriter("ThreadsInfo/" + numberOfThreads + ".txt"))
            {
                Parallel.For(0, n, options, i =>
                {
                    for (int j = 0; j < n; j++)
                    {
                        sum.Add(surfaceFunction(
                                    GetByOffset(2 * i, integrationInfo.XStart, stepX),
                                    GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                surfaceFunction(
                                    GetByOffset(2 * i + 2, integrationInfo.XStart, stepX),
                                    GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                surfaceFunction(
                                    GetByOffset(2 * i + 2, integrationInfo.XStart, stepX),
                                    GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                surfaceFunction(
                                    GetByOffset(2 * i, integrationInfo.XStart, stepX),
                                    GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                4 * (surfaceFunction(
                                         GetByOffset(2 * i + 1, integrationInfo.XStart, stepX),
                                         GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                     surfaceFunction(
                                         GetByOffset(2 * i + 2, integrationInfo.XStart, stepX),
                                         GetByOffset(2 * j + 1, integrationInfo.YStart, stepY)) +
                                     surfaceFunction(
                                         GetByOffset(2 * i + 1, integrationInfo.XStart, stepX),
                                         GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                     surfaceFunction(
                                         GetByOffset(2 * i, integrationInfo.XStart, stepX),
                                         GetByOffset(2 * j + 1, integrationInfo.YStart, stepY))) +
                                16 * surfaceFunction(
                                    GetByOffset(2 * i + 1, integrationInfo.XStart, stepX),
                                    GetByOffset(2 * j + 1, integrationInfo.YStart, stepY)));
                    }

                    lock (lockObject)
                    {
                        writer.WriteLine(Thread.CurrentThread.ManagedThreadId + " : " + sum.Sum());
                    }
                });
            }

            var test = threadTest.Distinct().ToList();
            return stepX * stepY / 9 * sum.Sum();
        }
    }
}
