from enum import Enum

# direction of board movement
class Direction(Enum):
    Up    = [ 0,-1]  # 0 horizontal, -1 vertical
    Down  = [ 0, 1]
    Left  = [-1, 0] 
    Right = [ 1, 0]

#        mve cmb
# left   asc dsc
# right  dsc asc
# down   dsc asc
# up     asc dsc
