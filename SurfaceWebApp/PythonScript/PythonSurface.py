import pylab
from mpl_toolkits.mplot3d import Axes3D
import numpy
import math
import scipy
import scipy.integrate
import json
import sympy
from sympy.parsing.sympy_parser import parse_expr

def Square(x,y,xExpr, yExpr):
    xSymbol,ySymbol = sympy.symbols('x y')

    xDif = xExpr(x,y)
    yDif = yExpr(x,y)

    return math.sqrt(1 + xDif**2 + yDif**2)

def makeData(xStart,xEnd,yStart,yEnd,expression):
    xArray = numpy.arange(xStart, xEnd, 1)
    yArray = numpy.arange(yStart, yEnd, 1)
    xgrid, ygrid = numpy.meshgrid(xArray, yArray)

    x,y = sympy.symbols('x y')

    zgrid = numpy.array(numpy.arange(len(xgrid)*len(xgrid[0]),dtype=numpy.float))
    zgrid.shape = (len(xgrid),len(xgrid[0]))


    lExpr = sympy.lambdify([x,y],expression,"numpy")

    for i in range(0,len(xgrid)):
        for j in range(0,len(xgrid[0])):
            zgrid[i,j] = lExpr(xgrid[i,j],ygrid[i,j])

    #zgrid = -2 * xgrid ** 2 + -2 * ygrid ** 2 + -3 * numpy.sin(xgrid) * numpy.cos(ygrid)
    return xgrid, ygrid, zgrid

inputData = []

xSymbol,ySymbol = sympy.symbols('x y')

with open('PythonScript\pythonData.json') as file:
   inputData = json.load(file)

expression = parse_expr(inputData["Expression"].replace("^","**"))

x, y, z = makeData(inputData["XStart"], inputData["XEnd"], inputData["YStart"], inputData["YEnd"], expression)

fig = pylab.figure()
axes = Axes3D(fig)

axes.plot_surface(x, y, z)

pylab.savefig("wwwroot\images\python.png")

xDif =sympy.diff(expression,xSymbol);
yDif =sympy.diff(expression,ySymbol);
xExpr = sympy.lambdify([xSymbol,ySymbol],xDif,"numpy")
yExpr = sympy.lambdify([xSymbol,ySymbol],yDif,"numpy")

square = scipy.integrate.dblquad(Square,inputData["XStart"], inputData["XEnd"], inputData["YStart"], inputData["YEnd"],args=(xExpr,yExpr))

#pylab.show()

#print(square[0])
