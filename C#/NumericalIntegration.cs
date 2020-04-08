﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KursSurface
{
    class NumericalIntegration
    {
        public double CalculateBySimpsonMethod(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction)
        {
            int n = 500;
            double resultWithoutThreads = 0;
            double resultWithThreads = 0;

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            resultWithoutThreads = CalculateDoubleSimpsonWithoutThreads(integrationInfo, surfaceFunction, n);
            stopwatch.Stop();

            var timeWithoutThreads = stopwatch.Elapsed;

            stopwatch.Reset();

            Console.WriteLine($"Время без потоков: {timeWithoutThreads}");

            for (int i = 2; i < 8; i++)
            {
                stopwatch.Start();
                resultWithThreads = CalculateDoubleSimpsonWithThreads(integrationInfo, surfaceFunction, n, i);
                stopwatch.Stop();
                Console.WriteLine($"Время с {i} потоками: {stopwatch.Elapsed}");
                stopwatch.Reset();
            }

            var timeWithThreads = stopwatch.Elapsed;

            Console.WriteLine($"Площадь : {resultWithoutThreads}");

            return resultWithThreads;
        }

        private double GetStep(int n, double start, double end)
        {
            return (end - start) / (2 * n);
        }

        private double GetByOffset(int offset, double start, double step)
        {
            return start + offset * step;
        }

        private double CalculateDoubleSimpsonWithThreads(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction, int n, int numberOfThreads)
        {
            double stepX = GetStep(n, integrationInfo.XStart, integrationInfo.XEnd);
            double stepY = GetStep(n, integrationInfo.YStart, integrationInfo.YEnd);
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            ConcurrentBag<double> sum = new ConcurrentBag<double>();
            ConcurrentBag<int> threadTest = new ConcurrentBag<int>();

            Parallel.For(0, n, options, i =>
            {
                threadTest.Add(Thread.CurrentThread.ManagedThreadId);
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
                };
            });

            var test = threadTest.Distinct().ToList();
            return stepX * stepY / 9 * sum.Sum();
        }

        private double CalculateDoubleSimpsonWithoutThreads(DoubleIntegrationInfo integrationInfo, Func<double, double, double> surfaceFunction, int n)
        {
            double stepX = GetStep(n, integrationInfo.XStart, integrationInfo.XEnd);
            double stepY = GetStep(n, integrationInfo.YStart, integrationInfo.YEnd);
            double sum = 0;

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    sum +=
                        surfaceFunction(
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
                            GetByOffset(2 * j + 1, integrationInfo.YStart, stepY));
                }


            return stepX * stepY / 9 * sum;
        }

    }
}
