import pylab
from mpl_toolkits.mplot3d import Axes3D
import numpy
import math
import scipy
import scipy.integrate

def Square(x,y):
    return math.sqrt(1+math.pow(2 * -2 * x + -3 * math.cos(x) * math.cos(y),2)+math.pow(2 * -2 * y - -3 * math.sin(x) * math.sin(y),2));

def makeData():
    x = numpy.arange(-100, 100, 0.1)
    y = numpy.arange(-20, 20, 0.1)
    xgrid, ygrid = numpy.meshgrid(x, y)

    zgrid = -2 * xgrid ** 2 + -2*ygrid**2 + -3*numpy.sin(xgrid)*numpy.cos(ygrid)
    return xgrid, ygrid, zgrid

x, y, z = makeData()

fig = pylab.figure()
axes = Axes3D(fig)

axes.plot_surface(x, y, z)

square = scipy.integrate.dblquad(Square,-100,100,-20,20)

pylab.show()

print(square[0])
