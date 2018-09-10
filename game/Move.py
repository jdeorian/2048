from game.Direction import Direction
from util.ArrayMagic import flatten, unflatten, array_2d_copy

class Move:
    def __init__(self, direction: Direction):
        self.direction = direction
    
    def apply(self, board_state):
        self.start_state = array_2d_copy(board_state.field)
        self.board = board_state
        self.slide()
        self.end_state = array_2d_copy(board_state.field)
        self.trigger_new_block = self.changed_board()

    def changed_board(self):
        return self.start_state != self.end_state

    def slide(self):
        transformed = self.board.transform_dict[self.direction][0](self.board, self.start_state)
        moved = [self.slide_row_left(row) for row in transformed]
        self.board.field = self.board.transform_dict[self.direction][1](self.board, moved)

    # build a self-sliding mechanism; the board will be transformed to suit this
    def slide_row_left(self, row):
        # remove 0's from 1d row
        def tighten_row(row):
            new_row = [i for i in row if i != 0]
            new_row += [0 for i in range(len(row) - len(new_row))]
            return new_row

        # merge items with the same value in a 1d row
        def merge_row(row):
            pair = False
            new_row = []
            for i in range(len(row)):
                if pair:
                    new_row.append(row[i]+1)
                    pair = False
                else:
                    if i + 1 < len(row) and row[i] == row[i + 1] and row[i] != 0:
                        pair = True
                        new_row.append(0)
                    else:
                        new_row.append(row[i])
            return new_row
        
        return tighten_row(merge_row(tighten_row(row)))

    # cs start_state|direction|cs end_state
    def as_log_entry(self):
        comma = ","
        pipe = "|"
        return pipe.join(map(str,[comma.join(map(str,flatten(self.start_state))), \
                                  str(self.direction).split('.')[1],     \
                                  comma.join(map(str,flatten(self.end_state)))]))
    
    @staticmethod
    def from_log_entry(text: str):        
        items = text.split('|')
        new_move = Move(Direction[items[1]])
        new_move.start_state = unflatten(list(map(int, items[0].split(','))), 4) # TODO: make this so it accepts an arbitary size
        new_move.end_state = unflatten(list(map(int, items[2].split(','))),4)
        return new_move
