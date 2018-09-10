import tkinter as tk
from abc import abstractmethod
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

BTN_FONT = ("Helvetica", 24, "bold")
LBL_FONT = ("Helvetica", 16, "bold")
VAL_FONT = ("Helvetica", 16, "")

class DisplayBase(tk.Tk):
    def __init__(self, board_state: BoardState):
        self.board = board_state

        # initialize window and frame
        tk.Tk.__init__(self)
        self.title(TITLE)
        self.frame = tk.Frame(self)
        self.frame.pack(side=tk.TOP)

        self.key_binds()
        self.bind("x", self.quit_program) # press 'x' opr 'q' to quit
        self.bind("q", self.quit_program)
        self.bind("r", self.reset)
        self.custom_components()        
        
        # initialize grid
        self.grid_cells = []
        self.init_grid()
        self.update_grid()
        self.mainloop()
    
    @abstractmethod
    def key_binds(self):
        pass

    @abstractmethod
    def custom_components(self):
        pass
    
    @abstractmethod
    def update_board(self):
        pass

    @abstractmethod
    def reset(self):
        pass
    
    def init_grid(self):
        size = self.board.BOARD_SIZE
        background = tk.Frame(self.frame, bg=BACKGROUND_COLOR, \
                                            width=size,          \
                                            height=size)
        background.grid()
        for i in range(size):
            grid_row = []
            for j in range(size):
                cell = tk.Frame(background, width=SIZE_PX/size, \
                                            height=SIZE_PX/size)
                cell.grid(row=i,                    \
                            column=j,                 \
                            padx=GRID_SPACING,        \
                            pady=GRID_SPACING)
                cell_text = tk.Label(master=cell,      \
                                    justify=tk.CENTER,   \
                                    font=FONT,           \
                                    width=TXT_WIDTH,     \
                                    height=TXT_HEIGHT)
                cell_text.grid()
                grid_row.append(cell_text)
            self.grid_cells.append(grid_row)

    def update_grid(self):
        self.update_board()
        for i in range(self.board.BOARD_SIZE):
            for j in range(self.board.BOARD_SIZE):
                val = self.board.get_value(i,j)
                txt = self.board.get_display_value(i,j)
                self.grid_cells[i][j].configure(text=txt,                             \
                                                  bg=CELL_COLORS[((val+1) % 12) - 1], \
                                                  fg=TEXT_COLORS[((val+1) % 12) - 1])
        self.title("Score: " + str(self.board.get_score()))
        self.update_idletasks() # performs rendering tasks while avoiding race conditions due to callbacks
    
    def quit_program(self, e):
        quit()

    def reset_program(self, e):
        self.reset()
        self.update_grid()