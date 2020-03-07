using System;

namespace KursSurface
{
    class Program
    {
        static void Main(string[] args)
        {
            Surface _surface = new Surface();

            var integrationInfo = new DoubleIntegrationInfo()
            {
                XStart = -100,
                XEnd = 100,
                YStart = -20,
                YEnd = 20
            };

            var numericalIntegration = new NumericalIntegration();

            numericalIntegration.CalculateBySimpsonMethod(integrationInfo,_surface.CalculateSurfaceFunction);

            Console.WriteLine("Hello World!");
        }
    }
}
