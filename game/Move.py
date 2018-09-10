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
        transformed = self.board.transform_dict[self.direction][0](self.start_state)
        moved = [self.slide_row_left(row) for row in transformed]
        self.board.field = self.board.transform_dict[self.direction][1](moved)

    # the board will be transformed to suit this
    def slide_row_left(self, row):
        slide = [num for num in row if num]
        pairs = []
        for idx, num in enumerate(slide):
            if idx == len(slide)-1:
                pairs.append(num)
                break
            elif num == slide[idx+1]:
                pairs.append(num+1)
                slide[idx+1] = None
            else:
                pairs.append(num)  # Even if not pair you must append
        slide = [pair for pair in pairs if pair] 
        slide.extend([0] * (len(row) - len(slide)))
        return slide

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
