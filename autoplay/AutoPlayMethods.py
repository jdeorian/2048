from game.BoardState import BoardState
from game.Direction import Direction
import random
from autoplay.pml.pseudo_ML import Pseudo_ML
from game.Move import Move
from statistics import mean

MAX_MOVES = 5000 # after this point we're probably going in a circle for some reason

# All play methods must accept board_state (a BoardState object) as a parameter,
# a parameter dictionary (optional), and no other parameters.
class AutoPlayMethods:

    @staticmethod
    def random(board_state, params: dict = None):
        while not board_state.Lost:
            board_state.move(random.choice([d for d in Direction]))

    directions = []

    @staticmethod
    def predetermined_random(board_state: BoardState, params: dict = None):
        # get the pre-determined set of random moves if it hasn't already been loaded
        if len(AutoPlayMethods.directions) == 0:
            rnd_filename = "random.dat"
            rnd_file = open(rnd_filename, "r")
            AutoPlayMethods.directions = [Direction[str.rstrip(line)] for idx, line in enumerate(rnd_file.readlines())]

        #run the random simulation
        i = 0
        while not board_state.Lost:            
            board_state.move(AutoPlayMethods.directions[i])
            i += 1

    
    # a simple improvement on random play. Alternate corner directions for a
    # while, then occasionally throw in another direction so it doesn't stall out
    @staticmethod
    def stutter(board_state, params: dict = None):
        stutters = 5 # number of repetitions of corner keys before random direction
        while not board_state.Lost:
            start_score = board_state.get_score()
            for _ in range(stutters):              
                board_state.move(Direction.Down)
                board_state.move(Direction.Left)
            if start_score == board_state.get_score(): # if the score hasn't changed after the stutters
                random_move = random.choice([Direction.Up, Direction.Right]) #then let's try a random direction
                if (random_move == Direction.Up):
                    board_state.move(Direction.Up)
                    board_state.move(Direction.Down)
                else:
                    board_state.move(Direction.Right)
                    board_state.move(Direction.Left)

    #an attempt to do some state analysis
    @staticmethod
    def pseudo_ML(board_state: BoardState, params: dict = None):
        pml = Pseudo_ML() if params == None else Pseudo_ML(params)
        while not board_state.Lost:
            weights = pml.get_direction_weights(board_state)
            direction_rec = pml.get_direction_recommendation(weights)
            this_dir = direction_rec[0]

            # get options with weights equivalent to top scorer;
            # if there are more than one, pick a random one
            top_scorers = [key for key, value in weights.items() if value == direction_rec[1]]
            if (len(top_scorers) > 1):
                this_dir = random.choice(top_scorers)

            board_state.move(this_dir, weights.copy())

    @staticmethod
    def branch(board_state: BoardState, params: dict = None):
        layers: int = params.get("LAYERS")
        if not layers:
            layers = 2
        if layers < 1: return

        move_num = 1
        root: Move = Move.as_root_node(board_state)
        while not board_state.Lost:
            print(f"Move: {move_num}")
            print(str(board_state.field))
            move_num += 1            
            current_layer = []
            next_layer = [root]
            for _ in range(layers):
                current_layer = next_layer
                next_layer = []
                for move in current_layer:
                    next_layer.extend(move.generate_children())
                if not any(next_layer):
                    next_layer = current_layer
                    break
            
            # when we're done, the last layer is the final set of children
            weights = { d:0 for d in Direction }
            for d in Direction:
                scores = [move.get_reward() for move in filter(lambda mv: mv.reward_direction() == d, next_layer)]
                if len(scores) > 0:
                    weights[d] = mean(scores)
            
            top_score = max(weights.values())
            # get options with weights equivalent to top scorer;
            # if there are more than one, pick a random one
            top_scorers = [key for key, value in weights.items() if value == top_score]
            rec_dir = top_scorers[0] if len(top_scorers) == 1 else random.choice(top_scorers)

            board_state.move(rec_dir, weights)

            # find the move that represents what actually happened and make it the new root
            root = next(move for move in root.children if move.end_state == board_state.field)
            root.parent = None
