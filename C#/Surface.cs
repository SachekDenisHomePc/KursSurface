using System;
using System.Collections.Generic;
using System.Text;

namespace KursSurface
{
    class Surface
    {
        NumericalDifferentiation _numericalDifferentiation = new NumericalDifferentiation();

        private const double A = -2;

        private const double B = -2;

        private const double C = -3;
        private double CalculateSourceFunction(double x, double y)
        {
            return A * x * x + B * y * y + C * Math.Sin(x) * Math.Cos(y);
        }

        private double CalculateXDerivative(double x, double y)
        {
            return _numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.X);
        }

        private double CalculateYDerivative(double x, double y)
        {
            return _numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.Y);
        }

        public double CalculateSurfaceFunction(double x, double y)
        {
            return Math.Sqrt(1+Math.Pow(CalculateXDerivative(x,y),2)+Math.Pow(CalculateYDerivative(x,y),2));
        }
    }
}
