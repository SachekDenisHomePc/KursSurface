using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KursSurface
{
    public class NumericalIntegration
    {
        public IntegrationExtendedInfo CalculateBySimpsonMethod(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction)
        {
            var n = (int)(((integrationInfo.XEnd - integrationInfo.XStart) + (integrationInfo.YEnd - integrationInfo.YStart)));
            //var n = 600;
            double result = 0;
            var threadsTime = new Dictionary<int, TimeSpan>();

            var stopwatch = new Stopwatch();

            for (var i = 1; i <= 15; i++)
            {
                stopwatch.Start();
                result = CalculateDoubleSimpsonWithThreads(integrationInfo, surfaceFunction, n, i);
                stopwatch.Stop();
                threadsTime.Add(i, stopwatch.Elapsed);
                stopwatch.Reset();
            }

            return new IntegrationExtendedInfo
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
            return (end - start) / n;
        }

        private double GetByOffset(int offset, double start, double step)
        {
            return start + offset * step;
        }

        private double CalculateDoubleSimpsonWithThreads(DoubleIntegrationInfo integrationInfo,
                                                         Func<double, double, double> surfaceFunction,
                                                         int n,
                                                         int numberOfThreads)
        {
            var lockObject = new object();
            var stepX = GetStep(n, integrationInfo.XStart, integrationInfo.XEnd);
            var stepY = GetStep(n, integrationInfo.YStart, integrationInfo.YEnd);

            var options = new ParallelOptions {MaxDegreeOfParallelism = numberOfThreads};

            var totalSum = 0.0;

            using (var writer = new StreamWriter("ThreadsInfo/" + numberOfThreads + ".txt"))
            {
                Parallel.For<double>(0, (long) n, options, () => 0, (i, state, subtotal) =>
                                                                    {
                                                                        for (var j = 0; j < n; j++)
                                                                        {
                                                                            subtotal +=
                                                                                surfaceFunction(GetByOffset(2 * (int) i, integrationInfo.XStart, stepX),
                                                                                                GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                                                                surfaceFunction(GetByOffset(2 * (int) i + 2, integrationInfo.XStart, stepX),
                                                                                                GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                                                                surfaceFunction(GetByOffset(2 * (int) i + 2, integrationInfo.XStart, stepX),
                                                                                                GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                                                                surfaceFunction(GetByOffset(2 * (int) i, integrationInfo.XStart, stepX),
                                                                                                GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                                                                4 *
                                                                                (surfaceFunction(GetByOffset(2 * (int) i + 1, integrationInfo.XStart, stepX),
                                                                                                 GetByOffset(2 * j, integrationInfo.YStart, stepY)) +
                                                                                 surfaceFunction(GetByOffset(2 * (int) i + 2, integrationInfo.XStart, stepX),
                                                                                                 GetByOffset(2 * j + 1, integrationInfo.YStart, stepY)) +
                                                                                 surfaceFunction(GetByOffset(2 * (int) i + 1, integrationInfo.XStart, stepX),
                                                                                                 GetByOffset(2 * j + 2, integrationInfo.YStart, stepY)) +
                                                                                 surfaceFunction(GetByOffset(2 * (int) i, integrationInfo.XStart, stepX),
                                                                                                 GetByOffset(2 * j + 1, integrationInfo.YStart, stepY))) +
                                                                                16 *
                                                                                surfaceFunction(GetByOffset(2 * (int) i + 1, integrationInfo.XStart, stepX),
                                                                                                GetByOffset(2 * j + 1, integrationInfo.YStart, stepY));
                                                                        }


                                                                        return subtotal;
                                                                    }, sum =>
                                                                       {
                                                                           lock (lockObject)
                                                                           {
                                                                               totalSum += sum;
                                                                               writer.WriteLine(Thread.CurrentThread.ManagedThreadId + " : " + totalSum);
                                                                           }
                                                                       });
            }

            return stepX * stepY / 9 * totalSum;
        }
    }
}