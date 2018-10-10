# easy setup for testing the performance of two methods
import time
from game.Field import Field

def method1():
    fld = Field(4)
    fld.get_score()

def method2():
    s = sum(map(sum, Field(4)))
    
###################
iterations = 100000
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
