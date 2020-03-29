using System;
using System.Collections.Generic;
using System.Text;

namespace KursSurface
{
    class Surface
    {
        private const double A = -2;

        private const double B = -2;

        private const double C = -3;
        public double CalculateSourceFunction(double x, double y)
        {
            return A * x * x + B * y * y + C * Math.Sin(x) * Math.Cos(y);
        }

        public double CalculateXDerivative(double x, double y)
        {
            return 2 * A * x + C * Math.Cos(x) * Math.Cos(y);
        }

        public double CalculateYDerivative(double x, double y)
        {
            return 2 * B * y - C * Math.Sin(x) * Math.Sin(y);
        }

        public double CalculateSurfaceFunction(double x, double y)
        {
            var numericalDifferentiation = new NumericalDifferentiation();
            var firstx = CalculateXDerivative(x, y);
            var firsty = CalculateYDerivative(x, y);
            var secondx = numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.X);
            var secondy = numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.Y);
            return Math.Sqrt(1+Math.Pow(CalculateXDerivative(x,y),2)+Math.Pow(CalculateYDerivative(x,y),2));
        }
    }
}
