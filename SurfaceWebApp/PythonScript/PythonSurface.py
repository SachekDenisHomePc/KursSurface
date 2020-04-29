import pylab
from mpl_toolkits.mplot3d import Axes3D
import numpy
import math
import scipy
import scipy.integrate
import json
import sympy
import time
import win32pipe
import win32file
import pywintypes
from sympy.parsing.sympy_parser import parse_expr
import os

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

quit = False
resp = []
while not quit:
     try:
        handle = win32file.CreateFile(r'\\.\pipe\pythonPipe',
                        win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                        0,
                        None,
                        win32file.OPEN_EXISTING,
                        0,
                        None)
        win32pipe.SetNamedPipeHandleState(handle, win32pipe.PIPE_READMODE_MESSAGE, None, None)
        resp = win32file.ReadFile(handle, 64 * 1024)[1].decode('utf-16')
     except pywintypes.error as e:
         if e.args[0] == 2:
             time.sleep(1)
         if resp != []:
             quit = True

inputData = json.loads(resp)

xSymbol,ySymbol = sympy.symbols('x y')

expression = parse_expr(inputData["Expression"].replace("^","**"))

x, y, z = makeData(inputData["XStart"], inputData["XEnd"], inputData["YStart"], inputData["YEnd"], expression)

fig = pylab.figure()

axes = Axes3D(fig)

axes.xaxis.pane.fill = False
axes.yaxis.pane.fill = False
axes.zaxis.pane.fill = False
axes.w_xaxis.set_pane_color((1.0, 1.0, 1.0, 1.0))

# Now set color to white (or whatever is "invisible")
axes.xaxis.pane.set_edgecolor('w')
axes.yaxis.pane.set_edgecolor('w')
axes.zaxis.pane.set_edgecolor('w')

# Bonus: To get rid of the grid as well:
axes.grid(True)

axes.plot_surface(x, y, z, color="#ff8500")
axes.set_facecolor('#333333')
axes.tick_params(axis='x', colors='white')
axes.tick_params(axis='y', colors='white')
axes.tick_params(axis='z', colors='white')

os.remove("wwwroot\images\python.png")
pylab.savefig("wwwroot\images\python.png")

xDif =sympy.diff(expression,xSymbol);
yDif =sympy.diff(expression,ySymbol);
xExpr = sympy.lambdify([xSymbol,ySymbol],xDif,"numpy")
yExpr = sympy.lambdify([xSymbol,ySymbol],yDif,"numpy")

square = scipy.integrate.dblquad(Square,inputData["XStart"], inputData["XEnd"], inputData["YStart"], inputData["YEnd"],args=(xExpr,yExpr))

#pylab.show()

print(square[0])
