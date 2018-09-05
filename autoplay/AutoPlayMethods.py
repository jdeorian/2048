from game.BoardState import BoardState
from game.Direction import Direction
import random
from autoplay.pseudo_ML import Pseudo_ML

# All play methods must accept board_state (a BoardState object) as a parameter,
# and no other parameters.
class AutoPlayMethods:

    @staticmethod
    def random(board_state):
        while not board_state.Lost:
            board_state.move(random.choice([d for d in Direction]))

    directions = []

    @staticmethod
    def predetermined_random(board_state: BoardState):
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
    def stutter(board_state):
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
    def pseudo_ML(board_state):
        pml = Pseudo_ML()
        while not board_state.Lost:
            weights = pml.get_direction_weights(board_state)
            direction_rec = pml.get_direction_recommendation(weights)
            this_dir = direction_rec[0]

            # get options with weights equivalent to top scorer;
            # if there are more than one, pick a random one
            top_scorers = [key for key, value in weights.items() if value == direction_rec[1]]
            if (len(top_scorers) > 1):
                this_dir = random.choice(top_scorers)

            board_state.move(this_dir)