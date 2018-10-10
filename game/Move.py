from game.Direction import Direction
from util.ArrayMagic import flatten, unflatten, array_2d_copy
from game.Field import Field

comma = ","
pipe = "|"
colon = ":"

LOG_FIELD_CNT = 4
BOARD_SIZE = 4 # TODO: eliminate dependence on this

class Move:
    children = []

    def __init__(self, direction: Direction, parent = None, board = None):
        self.parent = parent
        if parent:
            self.board = parent.board
        elif board:
            self.board = board

        self.chance = 1
        self.direction = direction

    # _weights property, only initialized if used
    _weights = {}

    def get_weights(self):
        if not self._weights:
            self._weights = { d:0 for d in Direction }
        return self._weights
    
    def set_weights(self, weights: dict):
        self._weights = weights

    weights = property(get_weights, set_weights)

    def get_start_state(self):
        return Field(BOARD_SIZE) if not self.parent else self.parent.end_state

    def set_start_state(self):
        raise Exception("You can't set this.")

    start_state = property(get_start_state, set_start_state)

    # return the reward decision, which is the ancestor direction nearest to the root that is not the root
    def reward_direction(self):
        if not self.parent:
            return None
        return self.get_root_eldest_child().direction

    def get_root(self):
        if not self.parent: # this is the root
            return self
        return self.get_root_eldest_child().parent

    def get_root_eldest_child(self):
        if not self.parent: # this is the root
            return None
        
        m = self
        while m.parent.parent:
            m = m.parent
        return m

   
    def apply(self, board_state):
        self.board = board_state
        self.board.field = self.end_state = board_state.field.slide(self.direction)
        self.trigger_new_block = self.changed_board()

    def changed_board(self):
        return self.start_state != self.end_state

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
    def from_log_entry(text: str, parent = None):       
        items = [str.strip(item) for item in text.split(pipe)]
        if len(items) != LOG_FIELD_CNT:
            return Move(Direction.Up)
        new_move = Move(Direction[items[1]], parent)

        # (try to) parse start and end state
        try:
            pei = [int(s) for s in items[2].split(comma)]
        except:
            return Move(Direction.Up) # this means there was a parsing error

        new_move.end_state = Field(field=unflatten(pei, BOARD_SIZE), no_copy=True) # TODO: make this so it accepts an arbitary size
        
        # TODO: Do this with list comprehension
        for w in items[3].split(comma):
            weight = w.rstrip().split(colon)
            w_d = Direction[weight[0]]
            new_move.weights[w_d] = float(weight[1])

        return new_move

    # creates a move object to represent a possible future state of a board
    @staticmethod
    def as_future_state(direction: Direction, parent, end_state:Field, chance: float):
        m = Move(direction, parent)
        m.end_state = end_state
        m.chance = chance * parent.chance
        return m

    @staticmethod
    def as_root_node(board):
        m = Move(Direction.Up)
        m.board = board
        m.end_state = board.field
        return m

    def generate_children(self, force_overwrite: bool = False):
        # if there are already children and we don't need to generate new ones,
        # return the ones we already have
        if not force_overwrite and self.children:
            return self.children

        # clear children
        self.children = []

        for d in Direction:
            if self.end_state != self.end_state.slide(d):
                outcomes = self.end_state.enumerate_possible_outcomes(d, self.board.FOUR_CHANCE)
                for o in outcomes:
                    self.children.append(Move.as_future_state(d, self, *o)) # add these to the tree
        
        return self.children
    
    # TODO: this needs to take in a reward calculation method
    def get_reward(self):
        return self.end_state.get_sum() - self.get_root().end_state.get_sum()