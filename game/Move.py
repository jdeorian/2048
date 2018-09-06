from game.Direction import Direction

class Move:
    def __init__(self, direction: Direction):
        self.direction = direction
    
    def apply(self, board_state):
        self.start_state = board_state.Squares[:]
        self.board = board_state
        self.slide_squares()
        self.end_state = board_state.Squares[:]
        self.trigger_new_block = self.start_state != self.end_state

    def changed_board(self):
        return self.start_state != self.end_state

    # cs start_state|direction|cs end_state
    def as_log_entry(self):
        comma = ","
        pipe = "|"
        return pipe.join(map(str,[comma.join(map(str,self.start_state)), \
                                  str(self.direction).split('.')[1],     \
                                  comma.join(map(str,self.end_state))]))

    @staticmethod
    def from_log_entry(text: str):        
        items = text.split('|')
        new_move = Move(Direction[items[1]])
        new_move.start_state = list(map(int, items[0].split(',')))
        new_move.end_state = list(map(int, items[2].split(',')))
        return new_move

    
    # slide all the squares to one direction on the board
    # returns whether at least one was moved or combined
    def slide_squares(self):
        # slide all the squares over
        indexes = self.get_index_iteration_order()
        for index in indexes:
            self.slide_square(index)

        # combine squares if necessary
        self.combine_squares(indexes)

        # once they are combined, move one last time
        for index in indexes:
            self.slide_square(index)

    # slides a single square to one direction
    def slide_square(self, square_index):
        # make sure this square can be moved at all
        index = square_index
        if self.board.Squares[index] == 0:
            return
        new_index = self.get_moved_index(index)
        if new_index == -1 or self.board.Squares[new_index] != 0:
            return
        
        #if it's eligible move it all the way in that direction
        while (self.board.Squares[new_index] == 0):
            self.board.Squares[new_index] = self.board.Squares[index] # set the new square value
            self.board.Squares[index] = 0 # and empty the old square
            index = new_index
            new_index = self.get_moved_index(index)

    def combine_squares(self, indexes):
        sq = self.board.Squares
        for idx in indexes:
            idx_new = self.board.combined_index_lookup[(self.direction, idx)]
            if idx_new == -1 or sq[idx] == 0:
                continue
            if sq[idx] == sq[idx_new]:
                sq[idx] += 1
                sq[idx_new] = 0


    def get_moved_index(self, index):
        return -1 if index == -1 else self.board.moved_index_lookup[(self.direction, index)]

    # to support executing all moves in one iteration, squares must be
    # evaluated in the correct order based on the direction
    def get_index_iteration_order(self):
        indexes = list(range(self.board.BOARD_SIZE**2))
        if self.direction.value[0] > 0 or self.direction.value[1] > 0:
            indexes.reverse() # in place reverse
        return indexes