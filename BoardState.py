from Direction import Direction
import random
from math import floor

class BoardState:
    def __init__(self):        
        self.BOARD_SIZE = 4        
        self.FOUR_CHANCE = .5 # the likelihood that random new squares will be a 4 instead of a 2
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

    # square values are stored as powers of 2. So:
    # 1 = 2, 2 = 4, 3 = 8, and so on. That way when
    # square combine, they can increment without
    # needing to be aware of their actual values.
    @staticmethod
    def display_value(square):
        return str(2 << square)

    def get_display_value(self, x, y):
        return self.display_value(self.get_square(x, y))

    # direction must be Direction object
    def move(self, direction):
        while (self.slide_squares(direction)):
            pass # slide until the squares won't slide or combine anymore
        
        self.new_random_square()

    #slide all the squares to one direction on the board
    def slide_squares(self, direction):
        moved_a_square = True
        while moved_a_square:
            moved_a_square = False # resets for every loop
            squares_with_values = [idx for idx, val in enumerate(self.Squares) if val != 0] # TODO: limit based on direction if it actually improves perormance
            for index in squares_with_values:
                if self.slide_square(index, direction):
                    moved_a_square = True

    def get_x(self, index):
        return int(index - floor(index/self.BOARD_SIZE) * self.BOARD_SIZE + 1)
    
    def get_y(self, index):
        return int((index - index % self.BOARD_SIZE) / self.BOARD_SIZE + 1)

    # slides a single square to one direction; returns true if successful
    def slide_square(self, square_index, direction):
        new_y = self.get_y(square_index) + direction.value[1]
        new_x = self.get_x(square_index) + direction.value[0]
        if not 1 <= new_y <= self.BOARD_SIZE or not 1 <= new_x <= self.BOARD_SIZE:
            return False

        # get the destination square and deal with it if it's not empty
        new_index = self.get_index(new_x, new_y)
        if (self.Squares[new_index] != 0): # if the destination isn't empty
            if (self.Squares[new_index] == self.Squares[square_index]): # if they match
                # combine them
                self.Squares[new_index] += self.Squares[square_index]
                self.Squares[square_index] = 0
                return True
            else:
                return False # don't change anything

        # but if the destination is empty, move the square
        self.Squares[new_index] = self.Squares[square_index] # set the new square value
        self.Squares[square_index] = 0 #and empty the old square
        return True
    
    # incidentally, this is also where it is possible to lose
    def new_random_square(self):
        # check if we lost
        empty_squares = [idx for idx, val in enumerate(self.Squares) if val == 0]
        if len(empty_squares) == 0: # if there are no more empty squares
            self.Lost = True
            return
        
        # get a random index
        random_square = random.choice(empty_squares)

        # increment the random square (twice if necessary)
        self.Squares[random_square] += 1
        if (random.random() >= self.FOUR_CHANCE):
            self.Squares[random_square] += 1

    def reset_board(self):
        self.Squares = [0] * self.BOARD_SIZE**2
        self.Lost = False