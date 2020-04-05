using System;
using System.Collections.Generic;
using MathNet.Symbolics;

namespace KursSurface
{
    class Program
    {
        static void Main(string[] args)
        {
            string expression = "-2*x^2+(-2*y^2)+(-3*sin(x)*cos(y))";

            Surface _surface = new Surface(expression);

            var integrationInfo = new DoubleIntegrationInfo
            {
                XStart = -100,
                XEnd = 100,
                YStart = -20,
                YEnd = 20
            };

            var numericalIntegration = new NumericalIntegration();

            numericalIntegration.CalculateBySimpsonMethod(integrationInfo, _surface.CalculateSurfaceFunction);
        }
    }
}
