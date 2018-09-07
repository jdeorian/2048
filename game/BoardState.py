from game.Direction import Direction
from math import floor
from game.Move import Move
import random

class BoardState:
    moved_index_lookup = {}
    combined_index_lookup = {}
    # (do, undo)
    transform_dict = { 
        Direction.Left:  lambda board, field: board.field_copy(field),                         
        Direction.Right: lambda board, field: board.invert_field(field),                       
        Direction.Up:    lambda board, field: board.transpose_field(field),                    
        Direction.Down:  lambda board, field: board.invert_field(board.transpose_field(field))
    }

    def __init__(self):        
        self.BOARD_SIZE = 4
        self.FOUR_CHANCE = .11 # the likelihood that random new squares will be a 4 instead of a 2
        # self.Squares = [0] * self.BOARD_SIZE**2
        self.reset_board()

        # the board starts with two random tiles
        self.new_random_square()
        self.new_random_square()

    # square positions are as follows
    #    x: 1  2  3  4
    # y:    -  -  -  -
    # 1:    0  1  2  3
    # 2:    4  5  6  7
    # 3:    8  9 10 11
    # 4:   12 13 14 15 
    def get_value(self, x, y):
        return self.field[x][y]

    def get_score(self):
        return sum([self.score_value(item) for row in self.field for item in row])

    def get_index(self, x, y):
        return (x - 1) + self.BOARD_SIZE * (y - 1)

    def get_display_value(self, x, y):
        return self.display_value(self.field[x][y])

    def get_y(self, index):
        return int(index/self.BOARD_SIZE + 1)
    
    def get_x(self, index):
        return int(index % self.BOARD_SIZE + 1)

    def get_2d_state(self):
        return self.field

    @staticmethod
    def field_copy(field):
        return [row[:] for row in field]

    def get_field_copy(self):
        return BoardState.field_copy(self.field)

    @staticmethod
    def state_to_2d_state(state, size): # state is an array of values, size is the board size
        return [state[x:x+size] for x in range(0, size**2, size)]

    # transpose a 2d array
    @staticmethod
    def transpose_field(field):
        return [list(row) for row in zip(*field)]

    def transpose(self):
        return BoardState.transpose_field(self.field)
 
    # invert a 2d array
    @staticmethod
    def invert_field(field):
        return [row[::-1] for row in field]

    def invert(self):
        return BoardState.invert_field(self.field)

    # square values are stored as powers of 2. So:
    # 1 = 2, 2 = 4, 3 = 8, and so on. That way when
    # square combine, they can increment without
    # needing to be aware of their actual values.
    @staticmethod
    def score_value(square_value):
        return 1 << square_value
    
    @staticmethod
    def display_value(square_value):
        return str(BoardState.score_value(square_value))

    # direction must be Direction object. Returns the move object.
    # :add_random_squares determines whether random squares will be added after the move
    # :apply_results determines whether the board state will be re-set to its original state
    #                after the calculation. This is useful for branch assessments.
    def move(self, direction: Direction, add_random_squares=True, apply_results=True):
		# shortcut for repeat moves
        if len(self.move_history) > 0:
            last_move = self.move_history[len(self.move_history) - 1]

            #if we already tried this move and nothing happened, don't bother re-calculating
            if last_move.direction == direction and not last_move.trigger_new_block:
                return last_move

        # print("Before:")
        # print(self.get_2d_state())  
        # print("Score: " + str(self.get_score()))
        move = Move(direction)
        move.apply(self)

        if move.trigger_new_block and add_random_squares:
            self.new_random_square() # only spawn new squares if something was moved or combined
            move.end_state = self.get_field_copy()

        if apply_results:
           self.check_if_lost()
           self.move_history.append(move)
        else: #undo the move
            self.field = [row[:] for row in move.start_state]

        #either way, return the move data
        return move

        # print("After:")
        # print(self.get_2d_state())
        # print("Score: " + str(self.get_score()))
    
    def invalid_coordinates(self, x, y):
        return not 1 <= x <= self.BOARD_SIZE or \
               not 1 <= y <= self.BOARD_SIZE

    def get_empty_squares(self):
        return [(i,j) for i in range(self.BOARD_SIZE) \
                      for j in range(self.BOARD_SIZE) \
                      if self.field[i][j] == 0]

    def new_random_square(self):
        # get the value of the new square
        new_val = 2 if random.random() <= self.FOUR_CHANCE else 1

        # get coordinates of new square
        (i,j) = random.choice([(i,j) for i in range(self.BOARD_SIZE) \
                                     for j in range(self.BOARD_SIZE) \
                                     if self.field[i][j] == 0])

        # set the value
        self.field[i][j] = new_val
    
    def check_if_lost(self):
        no_empty_squares = not any([item == 0 for row in self.field for item in row])
        moves = self.moves_on_board()
        self.Lost = no_empty_squares and not moves
        
    # checks to see if there are adjacent numbers
    def moves_on_board(self):
        for x in range(self.BOARD_SIZE):
            for y in range(self.BOARD_SIZE):
                if (x != (self.BOARD_SIZE - 1) and self.field[x][y] == self.field[x + 1][y]) or \
                   (y != (self.BOARD_SIZE - 1) and self.field[x][y] == self.field[x][y + 1]):
                    return True
        return False # if we weren't able to find any adjacent moves

    def reset_board(self):
        self.field = [[0 for i in range(self.BOARD_SIZE)] for j in range(self.BOARD_SIZE)]
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