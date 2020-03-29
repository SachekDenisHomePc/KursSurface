using System;

namespace KursSurface
{
    public class NumericalDifferentiation
    {
        const double Epsilon = 1e-10;
        public double CalculateDerivative(Func<double, double, double> function, double x, double y, DerivativeType type)
        {
            var xStep = 0.0;
            var yStep = 0.0;

            if (type == DerivativeType.X)
                xStep = Epsilon;
            else
                yStep = Epsilon;

            var derivative = (function(x + xStep, y + yStep) - function(x - xStep, y - yStep)) / (2 * (xStep + yStep));
            return derivative;
        }

    }
}