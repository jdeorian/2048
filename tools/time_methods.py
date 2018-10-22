# easy setup for testing the performance of two methods
import time
from game.Field import Field
from autoplay.rl.util.CanonicalField import get_fieldID_Bitshift

fld = [[9,0,0,0],[0,0,0,0],[0,6,0,0],[0,2,0,0]]

def method1():
    ID = get_fieldID_Bitshift(fld)
    ID += 1

def method2():
    pass #s = sum(map(sum, Field(4)))
    
###################
iterations = 10000000
###################

# don't touch anything below this

def run_method(method):
    start = time.time()
    for _ in range(iterations):
        method()
    end = time.time()
    duration = end - start
    print(f"{method.__name__} Time (s): {duration}")

run_method(method1)
run_method(method2)
print("Done!")
