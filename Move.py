from Direction import Direction

class Move:
    def __init__(self, direction):
        self.direction = direction        
    
    def apply(self, board_state):
        self.trigger_new_block = False
        self.board = board_state
        self.slide_squares()
    
    # slide all the squares to one direction on the board
    # returns whether at least one was moved or combined
    def slide_squares(self):
        print(self.board.Squares)

        # slide all the squares over
        indexes = self.get_index_iteration_order()
        for index in indexes:
            if self.slide_square(index):
                self.trigger_new_block = True

        # combine squares if necessary
        combine_indexes = indexes.copy()
        while len(combine_indexes) > 0:
            index = combine_indexes.pop()
            if (self.combine_square(combine_indexes, index)): # if the square was combined
                dest_index = self.get_moved_index(index)
                combine_indexes.remove(dest_index) # destination square cannot be combined anymore either
                self.trigger_new_block = True

        # once they are combined, move one last time
        for index in indexes:
            if self.slide_square(index):
                self.trigger_new_block = True

        # slides a single square to one direction; returns true if any moving or combining was done
    def slide_square(self, square_index):
        # make sure this square can be moved at all
        index = square_index
        if self.board.Squares[index] == 0:
            return False
        new_index = self.get_moved_index(index)
        if new_index == -1:
            return False
        
        # make sure it is eligible for moving
        if self.board.Squares[new_index] != 0:
            return False
        
        #if it's eligible move it all the way in that direction
        while (self.board.Squares[new_index] == 0):
            self.board.Squares[new_index] = self.board.Squares[index] # set the new square value
            self.board.Squares[index] = 0 # and empty the old square
            index = new_index
            new_index = self.get_moved_index(index)
            if new_index == -1:
                return True # a square was moved, but no combining can be done
        
        # return whether a move has occurred
        return index == square_index # if the square has moved, these indexes will be different
    
    def combine_square(self, indexes, square_index):
        # make sure this square can be moved at all
        index = square_index
        if self.board.Squares[index] == 0:
            return False
        new_index = self.get_moved_index(index)
        if new_index == -1:
            return False

        # make sure it is eligible for combining
        if (self.board.Squares[new_index] != self.board.Squares[index]):
            return False
        if not new_index in indexes: #make sure the destination index is allowed to be combined
            return False
        
        # handle combining
        self.board.Squares[new_index] += 1
        self.board.Squares[index] = 0
        return True

    def get_moved_index(self, index):
        new_y = self.board.get_y(index) + self.direction.value[1]
        new_x = self.board.get_x(index) + self.direction.value[0]
        return -1 if self.board.invalid_coordinates(new_x, new_y) \
          else self.board.get_index(new_x, new_y)

    # to support executing all moves in one iteration, squares must be
    # evaluated in the correct order based on the direction
    def get_index_iteration_order(self):
        indexes = list(range(self.board.BOARD_SIZE**2))
        if self.direction.value[0] > 0 or self.direction.value[1] > 0:
            indexes.reverse() # in place reverse
        return indexes