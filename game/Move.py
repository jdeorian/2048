from game.Direction import Direction
from util.ArrayMagic import flatten, unflatten, array_2d_copy

comma = ","
pipe = "|"
colon = ":"

class Move:
    def __init__(self, direction: Direction):
        self.direction = direction
        self.weights = { d:0 for d in Direction } # store the direction weights
    
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

    # cs start_state|direction|cs end_state|weights
    def as_log_entry(self):
        return pipe.join(map(str,[                          \
            comma.join(map(str,flatten(self.start_state))), \
            str(self.direction).split('.')[1],              \
            comma.join(map(str,flatten(self.end_state))),   \
            self.get_weight_str()                           \
        ]))
    
    def get_weight_str(self): # Up:0,Down:-11,Left:23,Right:4
        return comma.join([f"{str(d).split('.')[1]}:{'%.4f' % w}" for d, w in self.weights.items()])
    
    @staticmethod
    def from_log_entry(text: str):        
        items = text.split(pipe)
        new_move = Move(Direction[items[1]])
        new_move.start_state = unflatten(list(map(int, items[0].split(comma))), 4) # TODO: make this so it accepts an arbitary size
        new_move.end_state = unflatten(list(map(int, items[2].split(comma))),4)
        # new_move.weights = { Direction[weight[0]]:float(weight[1]) for w_str in items[3].split(comma) for weight in w_str.rstrip().split(colon) }

        for w in items[3].split(comma):
            weight = w.rstrip().split(colon)
            w_d = Direction[weight[0]]
            new_move.weights[w_d] = float(weight[1])

        return new_move
