import tkinter as tk
from game.Direction import Direction
from game.BoardState import BoardState
from game.DisplayBase import DisplayBase, TITLE

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

class BoardDisplay(DisplayBase): # inherit from the tkinter frame object
    def __init__(self, board_state):
        DisplayBase.__init__(self, board_state)
   
    def key_binds(self):
        # bind all keys
        for key in KEY_DIRECTION_DICT:
            self.master.bind(key, self.key_down)

    def key_down(self, e):
        direction = KEY_DIRECTION_DICT[e.char] if e.char != "" \
               else KEY_DIRECTION_DICT["<" + e.keysym + ">"]

        if (self.board.Lost):
            self.master.title("You lost! How embarassing!")
            return # don't process a keypress if it's resetting the board

        self.board.move(direction)
        self.update_grid()

    def custom_components(self):
        pass

    def update_board(self):
        pass

    def reset(self):
        self.board.reset_board()
        self.board = BoardState()
        self.title(TITLE)
    
    def quit_program(self, e):
        quit()
