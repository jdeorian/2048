from BoardDisplay import BoardDisplay
from BoardState import BoardState
from Direction import Direction
import random
import numpy
import webbrowser

class AutoPlayMethods:
    choices = [Direction.Down, Direction.Up, Direction.Left, Direction.Right]

    @staticmethod
    def random_play():
        step_number = 0
        while not board_state.Lost:
            step_number += 1
            # print(step_number)
            # print(board_state.get_2d_state())
            board_state.move(random.choice(AutoPlayMethods.choices))
        return step_number # returns number of moves
    
    # a simple improvement on random play. Alternate corner directions for a
    # while, then occasionally throw in another direction so it doesn't stall out
    @staticmethod
    def stutter_play():
        step_number = 0
        stutters = 5 # number of repetitions of corner keys before random direction
        while not board_state.Lost:
            step_number += 1
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
        return step_number

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
number_of_plays = 10 # number of iterations to test autoplay method
autoplay_method = "random_play" # pick the method to run here
########################################################################

# prepare for run
results = []
method_to_call = getattr(AutoPlayMethods, autoplay_method)

# perform test iterations
for x in range(number_of_plays):
    board_state = BoardState() #initialize new board
    steps = method_to_call()
    result = [x + 1, steps, board_state.get_score()]
    results.append(result)
    print(result) # prints results

# save results to text file
if save_to_file:
    numpy.savetxt(output_filename, results, '%i', '\t', header="Iteration\tSteps\tScore")
    webbrowser.open(output_filename)

# board = BoardDisplay(board_state) this displays the state of the board in the GUI
# print("Score: " + str(board_state.get_score()))
