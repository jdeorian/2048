import tkinter as tk

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

class BoardReplay(tk.Tk): # inherit from the tkinter frame object
    def __init__(self, board_state: BoardState):
        # create board state
        self.board = board_state
        self.current_move = 0

        # initialize window and frame
        tk.Tk.__init__(self)
        self.title(TITLE)
        self.frame = tk.Frame(self)
        self.frame.pack(side=tk.TOP)

        # bind all keys
        self.bind("<Left>", self.go_back)
        self.bind("<Right>", self.go_forward)
        self.bind("x", self.quit_program) # press 'x' to quit
        self.bind("q", self.quit_program) # press 'x' to quit   

        # initialize control buttons
        self.btn_to_beginning = tk.Button(self, text="<<", font=BTN_FONT, command=self.go_beginning)
        self.btn_to_beginning.pack(side=tk.LEFT)
        self.btn_back = tk.Button(self, text="<", font=BTN_FONT, command=self.go_back)
        self.btn_back.pack(side=tk.LEFT)
        self.btn_forward = tk.Button(self, text=">", font=BTN_FONT, command=self.go_forward)
        self.btn_forward.pack(side=tk.LEFT)
        self.btn_to_end = tk.Button(self, text=">>", font=BTN_FONT, command=self.go_end)
        self.btn_to_end.pack(side=tk.LEFT)
        self.lbl_move_num_label = tk.Label(self, text=" Move:", font=LBL_FONT)
        self.lbl_move_num_label.pack(side=tk.LEFT)
        self.lbl_move_num = tk.Label(self, text="", font=VAL_FONT)
        self.lbl_move_num.pack(side=tk.LEFT)
        self.lbl_next_move_label = tk.Label(self, text=" Next: ", font=LBL_FONT)
        self.lbl_next_move_label.pack(side=tk.LEFT)
        self.lbl_next_move = tk.Label(self, text="", font=VAL_FONT)
        self.lbl_next_move.pack(side=tk.LEFT)

        # initialize grid
        self.grid_cells = []
        self.init_grid()
        self.update_board()
        self.update_grid()
        self.mainloop()

    def go_forward(self):
        if self.current_move < len(self.board.move_history):
            self.current_move += 1
            self.update_board()
            self.update_grid()

    def go_back(self):
        if self.current_move > 0:
            self.current_move -= 1
            self.update_board()
            self.update_grid()

    def go_beginning(self):
        if self.current_move > 0:
            self.current_move = 0
            self.update_board()
            self.update_grid()

    def go_end(self):
        last_move_index = len(self.board.move_history) - 1
        already_at_end = self.current_move >= last_move_index

        if already_at_end:
            self.update_board()
            self.update_grid()
        else:
            self.current_move = last_move_index
            self.update_board()
            self.update_board() #this happens twice to trigger the final state
            self.update_grid()

    def update_board(self):
        mv = self.board.move_history[self.current_move]
        self.lbl_move_num["text"] = str(self.current_move + 1)

        # handle end case
        if self.current_move == (len(self.board.move_history) - 1) and self.lbl_next_move.cget("text") != "(end)":
            self.board.Squares = mv.end_state
            self.lbl_next_move.config(text="(end)")
            self.btn_to_end.config(state=tk.DISABLED)
            self.btn_forward.config(state=tk.DISABLED)
            return
        
        # all other cases
        self.board.Squares = mv.start_state
        self.btn_to_end.config(state=tk.NORMAL)
        self.btn_forward.config(state=tk.NORMAL)
        self.lbl_next_move.config(text=str(mv.direction))        

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
        for i in range(self.board.BOARD_SIZE):
            for j in range(self.board.BOARD_SIZE):
                val = self.board.get_value(j + 1, i + 1)
                txt = self.board.get_display_value(j + 1, i + 1)
                self.grid_cells[i][j].configure(text=txt,                          \
                                                  bg=CELL_COLORS[val],             \
                                                  fg=TEXT_COLORS[val])
     
        self.update_idletasks() # performs rendering tasks while avoiding race conditions due to callbacks

    def quit_program(self, e):
        quit()