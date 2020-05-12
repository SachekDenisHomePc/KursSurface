using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Symbolics;
using Microsoft.FSharp.Collections;

namespace KursSurface
{
    public class Surface
    {
        private readonly NumericalDifferentiation _numericalDifferentiation = new NumericalDifferentiation();

        private readonly Func<double,double,double> _expression;

        public Surface(string expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            _expression = Compile.compileExpression2(Infix.ParseOrThrow(expression), Symbol.NewSymbol("x"), Symbol.NewSymbol("y")).Value;
        }

        private const double A = -2;

        private const double B = -2;

        private const double C = -3;

        private double CalculateSourceFunction(double x, double y)
        {
            //var arguments = new Dictionary<string, FloatingPoint>() {{"x", x}, {"y", y}};
            //return Evaluate.Evaluate(arguments, _expression).RealValue;
            return _expression(x, y);

            //return A * x * x + B * y * y + C * Math.Sin(x) * Math.Cos(y);
        }

        private double CalculateXDerivative(double x, double y)
        {
            //return A*2*x+C*Math.Cos(x) * Math.Cos(y);
            return _numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.X);
        }

        private double CalculateYDerivative(double x, double y)
        {
            //return B*2*y-C*Math.Sin(x) * Math.Sin(y);
            return _numericalDifferentiation.CalculateDerivative(CalculateSourceFunction, x, y, DerivativeType.Y);
        }

        public double CalculateSurfaceFunction(double x, double y)
        {
            return Math.Sqrt(1 + Math.Pow(CalculateXDerivative(x, y), 2) + Math.Pow(CalculateYDerivative(x, y), 2));
        }
    }
}
