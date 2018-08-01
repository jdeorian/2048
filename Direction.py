from enum import Enum

# direction of board movement
class Direction(Enum):
    Left  = [ 0,-1] # -1 horizontal, 0 vertical
    Right = [ 0, 1]
    Up    = [-1, 0] 
    Down  = [ 1, 0]

KEY_DIRECTION_DICT = { 
    "w": Direction.Up,
    "s": Direction.Down,
    "a": Direction.Left,
    "d": Direction.Right,
    "<Up>": Direction.Up, #arrow keys
    "<Down>": Direction.Down,
    "<Left>": Direction.Left,
    "<Right>": Direction.Right
}
