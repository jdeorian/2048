from game.Direction import Direction
from util.ArrayMagic import flatten, unflatten, array_2d_copy

comma = ","
pipe = "|"
colon = ":"

LOG_FIELD_CNT = 4
BOARD_SIZE = 4 # TODO: eliminate dependence on this

class Move:
    def __init__(self, direction: Direction):
        self.direction = direction
        self.weights = { d:0 for d in Direction } # store the direction weights
    
    def apply(self, board_state):
        self.start_state = array_2d_copy(board_state.field)
        self.board = board_state
        # old_score = self.board.get_score()
        self.slide()
        self.end_state = array_2d_copy(board_state.field)
        # self.reward = self.board.get_score() - old_score # naive reward calculation
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

    def is_valid(self):
        return hasattr(self, 'start_state') and hasattr(self, 'end_state')
    
    @staticmethod
    def from_log_entry(text: str):       
        items = [str.strip(item) for item in text.split(pipe)]
        if len(items) != LOG_FIELD_CNT:
            return Move(Direction.Up)
        new_move = Move(Direction[items[1]])

        # (try to) parse start and end state
        try:
            psi = [int(s) for s in items[0].split(comma)]
            pei = [int(s) for s in items[2].split(comma)]
        except:
            return Move(Direction.Up) # this means there was a parsing error

        new_move.start_state = unflatten(psi, BOARD_SIZE) # TODO: make this so it accepts an arbitary size
        new_move.end_state = unflatten(pei, BOARD_SIZE)
        
        # TODO: Do this with list comprehension
        for w in items[3].split(comma):
            weight = w.rstrip().split(colon)
            w_d = Direction[weight[0]]
            new_move.weights[w_d] = float(weight[1])

        return new_move
