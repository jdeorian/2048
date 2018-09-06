from tkinter import *
from game.Direction import Direction
from game.BoardState import BoardState

CELL_COLORS = ["#9e948a", "#eee4da", "#ede0c8", "#f2b179", "#f59563", "#f67c5f",\
               "#f65e3b", "#edcf72", "#edcc61", "#edc850", "#edc53f", "#edc22e"]
TEXT_COLORS = ["#9e948a", "#776e65", "#776e65", "#f9f6f2", "#f9f6f2", "#f9f6f2",\
               "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2"]
FONT = ("Verdana", 40, "bold")
BACKGROUND_COLOR = "#92877d"
TITLE = "DK Holdings Presents: 2048"
SIZE_PX = 500
GRID_SPACING = 10 # space between grid squares
TXT_WIDTH = 4
TXT_HEIGHT = 2

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


class BoardDisplay(Frame): # inherit from the tkinter frame object
    def __init__(self, board_state):
        Frame.__init__(self)
        self.master.title(TITLE)

        # bind all keys
        for key in KEY_DIRECTION_DICT:
            self.master.bind(key, self.key_down)
        self.master.bind("x", self.quit_program) # press 'x' to quit
        self.master.bind("r", self.reset)

        # create board state
        self.board = board_state

        # initialize grid
        self.grid()
        self.grid_cells = []
        self.init_grid()
        self.update_grid()
        self.mainloop()

    def init_grid(self):
        size = self.board.BOARD_SIZE
        background = Frame(self, bg=BACKGROUND_COLOR, \
                              width=size,             \
                             height=size)
        background.grid()
        for i in range(size):
            grid_row = []
            for j in range(size):
                cell = Frame(background, width=SIZE_PX/size, \
                                        height=SIZE_PX/size)
                cell.grid(row=i,                    \
                          column=j,                 \
                          padx=GRID_SPACING,        \
                          pady=GRID_SPACING)
                cell_text = Label(master=cell,      \
                                  justify=CENTER,   \
                                  font=FONT,        \
                                  width=TXT_WIDTH,  \
                                  height=TXT_HEIGHT)
                cell_text.grid()
                grid_row.append(cell_text)
            self.grid_cells.append(grid_row)
    
    def update_grid(self):
        for i in range(self.board.BOARD_SIZE):
            for j in range(self.board.BOARD_SIZE):
                val = self.board.get_value(j + 1, i + 1)
                txt = self.board.get_display_value(j + 1, i + 1)
                self.grid_cells[i][j].configure(text=txt,                             \
                                                  bg=CELL_COLORS[((val+1) % 12) - 1], \
                                                  fg=TEXT_COLORS[((val+1) % 12) - 1])
     
        self.update_idletasks() # performs rendering tasks while avoiding race conditions due to callbacks


    def key_down(self, e):
        direction = KEY_DIRECTION_DICT[e.char] if e.char != "" \
               else KEY_DIRECTION_DICT["<" + e.keysym + ">"]

        if (self.board.Lost):
            self.board.reset_board()
            return # don't process a keypress if it's resetting the board

        self.board.move(direction)
        self.update_grid()

    def reset(self, e):
        self.board.reset_board()
        self.board = BoardState()
        self.update_grid()
    
    def quit_program(self, e):
        quit()