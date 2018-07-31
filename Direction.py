from enum import Enum

# direction of board movement
class Direction(Enum):
    Left  = [-1, 0] # -1 horizontal, 0 vertical
    Right = [ 1, 0]
    Up    = [ 0,-1] 
    Down  = [ 0, 1]

KEY_DIRECTION_DICT = { 
    "w": Direction.Up,
    "s": Direction.Down,
    "a": Direction.Left,
    "d": Direction.Right,
    "\\uf700": Direction.Up, #arrow keys
    "\\uf701": Direction.Down,
    "\\uf702": Direction.Left,
    "\\uf703": Direction.Right
}
