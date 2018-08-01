from Direction import Direction
import random
from math import floor

class BoardState:
    def __init__(self):        
        self.BOARD_SIZE = 4        
        self.FOUR_CHANCE = .15 # the likelihood that random new squares will be a 4 instead of a 2
        self.Squares = [0] * self.BOARD_SIZE**2
        self.Lost = False

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
    def get_square(self, x, y):
        return self.Squares[self.get_index(x, y)]

    def get_index(self, x, y):
        return (x - 1) + self.BOARD_SIZE * (y - 1)

    # to support executing all moves in one iteration, squares must be
    # evaluated in the correct order based on the direction
    def get_index_iteration_order(self, direction):
        indexes = list(range(self.BOARD_SIZE**2))
        if direction.value[0] > 0 or direction.value[1] > 0:
            indexes.reverse() # in place reverse
        return indexes

    # square values are stored as powers of 2. So:
    # 1 = 2, 2 = 4, 3 = 8, and so on. That way when
    # square combine, they can increment without
    # needing to be aware of their actual values.
    @staticmethod
    def display_value(square):
        return str(1 << square)

    def get_display_value(self, x, y):
        return self.display_value(self.get_square(x, y))

    # direction must be Direction object
    def move(self, direction):
        if self.slide_squares(direction):
            pass #self.new_random_square() # only spawn new squares if something was moved or combined

    # slide all the squares to one direction on the board
    # returns whether at least one was moved or combined
    def slide_squares(self, direction):
        print(self.Squares)

        # slide all the squares over
        moved_a_square = False
        indexes = self.get_index_iteration_order(direction)
        for index in indexes:
            if self.slide_square(index, direction):
                moved_a_square = True

        # combine squares if necessary
        combine_indexes = indexes.copy()
        combine_indexes.reverse()
        while len(combine_indexes) > 0:
            index = combine_indexes.pop()
            if (self.combine_square(combine_indexes, index, direction)): # if the square was combined
                dest_index = self.get_moved_index(index, direction)
                combine_indexes.remove(dest_index) # destination square cannot be combined anymore either

        # once they are combined, move one last time
        for index in indexes:
            if self.slide_square(index, direction):
                moved_a_square = True

        return moved_a_square

    def get_y(self, index):
        return int(floor(index/self.BOARD_SIZE) + 1)
    
    def get_x(self, index):
        return int(index % self.BOARD_SIZE + 1)

    def get_indexes_with_values(self):
        return [idx for idx, val in enumerate(self.Squares) \
                     if val != 0] # TODO: limit based on direction if it actually improves perormance
    
    def get_moved_index(self, index, direction):
        new_y = self.get_y(index) + direction.value[1]
        new_x = self.get_x(index) + direction.value[0]
        return -1 if self.invalid_coordinates(new_x, new_y) \
          else self.get_index(new_x, new_y)

    def invalid_coordinates(self, x, y):
        return not 1 <= x <= self.BOARD_SIZE or \
               not 1 <= y <= self.BOARD_SIZE

    # slides a single square to one direction; returns true if any moving or combining was done
    def slide_square(self, square_index, direction):
        # make sure this square can be moved at all
        index = square_index
        if self.Squares[index] == 0:
            return False
        new_index = self.get_moved_index(index, direction)
        if new_index == -1:
            return False
        
        # make sure it is eligible for moving
        if self.Squares[new_index] != 0:
            return False
        
        #if it's eligible move it all the way in that direction
        while (self.Squares[new_index] == 0):
            self.Squares[new_index] = self.Squares[index] # set the new square value
            self.Squares[index] = 0 # and empty the old square
            index = new_index
            new_index = self.get_moved_index(index, direction)
            if new_index == -1:
                return True # a square was moved, but no combining can be done
        
        # return whether a move has occurred
        return index == square_index # if the square has moved, these indexes will be different
    
    def combine_square(self, indexes, square_index, direction):
        # make sure this square can be moved at all
        index = square_index
        if self.Squares[index] == 0:
            return False
        new_index = self.get_moved_index(index, direction)
        if new_index == -1:
            return False

        # make sure it is eligible for combining
        if (self.Squares[new_index] != self.Squares[index]):
            return False
        if not new_index in indexes: #make sure the destination index is allowed to be combined
            return False
        
        # handle combining
        self.Squares[new_index] += 1
        self.Squares[index] = 0
        return True

    # incidentally, this is also where it is possible to lose
    def new_random_square(self):
        # check if we lost
        empty_squares = [idx for idx, val in enumerate(self.Squares) \
                              if val == 0]
        if len(empty_squares) == 0: # if there are no more empty squares
            self.Lost = True
            return
        
        # get a random index
        random_square = random.choice(empty_squares)

        # increment the random square (twice if necessary)
        self.Squares[random_square] += 1
        if (random.random() >= (1 - self.FOUR_CHANCE)):
            self.Squares[random_square] += 1

    def reset_board(self):
        self.Squares = [0] * self.BOARD_SIZE**2
        self.Lost = False