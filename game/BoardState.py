from game.Direction import Direction
from math import floor
from game.Move import Move
from util.ArrayMagic import invert, transpose, array_2d_copy
import random
from game.Field import Field

class BoardState:

    def __init__(self):        
        self.BOARD_SIZE = 4
        self.FOUR_CHANCE = .11 # the likelihood that random new squares will be a 4 instead of a 2
        self.reset_board()

        # the board starts with two random tiles
        self.new_random_square()
        self.new_random_square()

    def get_value(self, x, y):
        return self.field[x][y]

    def get_display_value(self, x, y):
        return self.display_value(self.field[x][y])
    
    @staticmethod
    def display_value(square_value):
        return str(Field.score_value(square_value))

    # direction must be Direction object. Returns the move object.
    # :add_random_squares determines whether random squares will be added after the move
    # :apply_results determines whether the board state will be re-set to its original state
    #                after the calculation. This is useful for branch assessments.
    def move(self, direction: Direction, weights:dict = {}, add_random_squares=True, apply_results=True):            
		# shortcut for repeat moves
        last_move = None
        if len(self.move_history) > 0:
            last_move = self.move_history[len(self.move_history) - 1]

            #if we already tried this move and nothing happened, don't bother re-calculating
            if last_move.direction == direction and not last_move.trigger_new_block:
                return last_move
        
        move = Move(direction, last_move)
        move.apply(self)

        if move.trigger_new_block and add_random_squares:
            self.new_random_square() # only spawn new squares if something was moved or combined
            move.end_state = array_2d_copy(self.field)

        if apply_results:
           self.check_if_lost()
           if weights: # if the dictionary provided isn't empty
               move.weights = weights #assign it
           self.move_history.append(move)
        else: #undo the move
            self.field = Field(field=move.start_state)

        #either way, return the move data
        return move
    
    def new_random_square(self):
        new_val = 2 if random.random() <= self.FOUR_CHANCE else 1
        self.field.set_random_empty_square(new_val)
    
    def check_if_lost(self):
        no_empty_squares =  not any(self.field.get_empty_squares())
        self.Lost = no_empty_squares and not self.moves_on_board()
        
    # checks to see if there are adjacent numbers
    def moves_on_board(self):
        fld = self.field
        sz = fld.size
        return any([(x,y) for x in range(sz) \
                          for y in range(sz) \
                          if (x != (sz - 1) and fld[x][y] == fld[x + 1][y]) or \
                             (y != (sz - 1) and fld[x][y] == fld[x][y + 1])])

    def reset_board(self):
        self.field = Field(self.BOARD_SIZE)
        self.Lost = False
        self.move_history = []

    # the import history is a list of Move objects
    def import_from_log(self, import_history: list):
        #importing overwrites a board, so reset it
        self.reset_board()
        self.move_history = import_history

        #set the current state of the board to the last state
        last_move = len(import_history) - 1
        self.field = import_history[last_move].end_state