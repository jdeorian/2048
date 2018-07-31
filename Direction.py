from enum import Enum

# direction of board movement
class Direction(Enum):
    Left  = [-1, 0] # -1 horizontal, 0 vertical
    Right = [ 1, 0]
    Up    = [ 0,-1] 
    Down  = [ 0, 1]
