import tkinter as tk
from game.DisplayBase import DisplayBase, BTN_FONT, LBL_FONT, VAL_FONT

from game.Direction import Direction
from game.BoardState import BoardState

class BoardReplay(DisplayBase): # inherit from the tkinter frame object
    def __init__(self, board_state: BoardState):
        self.current_move = 0
        self.show_end = False # determines whether the end state of a move should be displayed instead of the start state
        DisplayBase.__init__(self, board_state)        

    def key_binds(self):
        # bind all keys
        self.bind("<Left>", self.key_down)
        self.bind("<Right>", self.key_down)

    def custom_components(self):
        # initialize control buttons
        self.lbl_weights = tk.Label(self, text="", font=VAL_FONT)
        self.lbl_weights.pack(side=tk.BOTTOM)
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

    def go_forward(self):
        last_move_index = len(self.board.move_history) - 1
        already_at_end = self.current_move >= last_move_index
        if not already_at_end:
            self.current_move += 1
        else:
            self.set_end()
        self.update_grid()

    def go_back(self):
        self.btn_to_end.config(state=tk.NORMAL)
        self.btn_forward.config(state=tk.NORMAL)
        if self.current_move > 0:
            self.current_move -= 1
        self.update_grid()

    def go_beginning(self):
        self.current_move = 0
        self.go_back()

    def go_end(self):
        last_move_index = len(self.board.move_history) - 1
        self.current_move = last_move_index
        self.set_end()
        self.update_grid()

    def update_board(self):
        mv = self.board.move_history[self.current_move]
        self.lbl_move_num["text"] = str(self.current_move + 1)
        self.board.field = mv.end_state if self.show_end else mv.start_state
        self.lbl_next_move.config(text=str(mv.direction))
        self.lbl_weights.config(text=mv.get_weight_str())

    def set_end(self):
        self.show_end = True
        self.lbl_next_move.config(text="(end)")
        self.btn_to_end.config(state=tk.DISABLED)
        self.btn_forward.config(state=tk.DISABLED)

    def key_down(self, e):
        if e.keysym == "Left":
            self.go_back()
        if e.keysym == "Right":
            if self.current_move < (len(self.board.move_history) - 1):
                self.go_forward()