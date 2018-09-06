from game.Direction import Direction
from math import floor
from game.Move import Move
import random

class BoardState:
    moved_index_lookup = {}

    def __init__(self):        
        self.BOARD_SIZE = 4
        self.FOUR_CHANCE = .15 # the likelihood that random new squares will be a 4 instead of a 2
        self.Squares = [0] * self.BOARD_SIZE**2
        self.Lost = False
        self.move_history = []

        #initialize moved_index_lookup if necessary
        if len(BoardState.moved_index_lookup.keys()) == 0:
            for d in Direction:
                for idx in range(self.BOARD_SIZE**2):
                    old_x = self.get_x(idx)
                    old_y = self.get_y(idx)
                    new_x = old_x + d.value[0]
                    new_y = old_y + d.value[1]                    
                    invalid = self.invalid_coordinates(new_x, new_y)
                    new_index = -1 if invalid else self.get_index(new_x, new_y)
                    BoardState.moved_index_lookup[(d, idx)] = new_index

        # the board starts with two random tiles
        self.new_random_square()
        self.new_random_square()

    def get_moved_index_dict(self):
        return BoardState.moved_index_lookup

    # square positions are as follows
    #    x: 1  2  3  4
    # y:    -  -  -  -
    # 1:    0  1  2  3
    # 2:    4  5  6  7
    # 3:    8  9 10 11
    # 4:   12 13 14 15 
    def get_value(self, x, y):
        return self.Squares[self.get_index(x, y)]

    def get_score(self):
        score = 0
        for val in self.Squares:
            if val > 0:
                score += self.score_value(val)
        return score

    def get_index(self, x, y):
        return (x - 1) + self.BOARD_SIZE * (y - 1)

    def get_display_value(self, x, y):
        return self.display_value(self.get_value(x, y))

    def get_y(self, index):
        return int(index/self.BOARD_SIZE + 1)
    
    def get_x(self, index):
        return int(index % self.BOARD_SIZE + 1)

    def get_2d_state(self):
        return self.state_to_2d_state(self.Squares, self.BOARD_SIZE)

    @staticmethod
    def state_to_2d_state(state, size): # state is an array of values, size is the board size
        return [state[x:x+size] for x in range(0, size**2, size)]

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
            move.end_state = self.Squares[:]

        if apply_results:
           self.check_if_lost()
           self.move_history.append(move)
        else: #undo the move
            self.Squares = move.start_state[:]

        #either way, return the move data
        return move

        # print("After:")
        # print(self.get_2d_state())
        # print("Score: " + str(self.get_score()))
    
    def invalid_coordinates(self, x, y):
        return not 1 <= x <= self.BOARD_SIZE or \
               not 1 <= y <= self.BOARD_SIZE

    def get_empty_squares(self):
        return [idx for idx, val in enumerate(self.Squares) \
                              if val == 0]

    def new_random_square(self):
        # if we need an empty square but can't place one, we lost
        empty_squares = self.get_empty_squares()
        if len(empty_squares) == 0: # if there are no more empty squares
            self.Lost = True
            return
        
        # get a random index
        random_square = random.choice(empty_squares)

        # increment the random square (twice if necessary)
        self.Squares[random_square] += 1
        if (random.random() >= (1 - self.FOUR_CHANCE)):
            self.Squares[random_square] += 1
    
    def check_if_lost(self):
        no_empty_squares = len(self.get_empty_squares()) == 0
        moves = self.moves_on_board()
        self.Lost = no_empty_squares and not moves
        
    # checks to see if there are adjacent numbers
    def moves_on_board(self):
        board_matrix = self.get_2d_state()
        for x in range(self.BOARD_SIZE - 1):
            for y in range(self.BOARD_SIZE - 1):
                if board_matrix[x][y] == board_matrix[x + 1][y] or \
                   board_matrix[x][y] == board_matrix[x][y + 1]:
                    return True
        return False # if we weren't able to find any adjacent moves

    def reset_board(self):
        self.Squares = [0] * self.BOARD_SIZE**2
        self.Lost = False

    # the import history is a list of Move objects
    def import_from_log(self, import_history: list):
        #importing overwrites a board, so reset it
        self.reset_board()
        self.move_history = import_history

        #set the current state of the board to the last state
        last_move = len(import_history) - 1
        self.Squares = import_history[last_move].end_state