from BoardDisplay import BoardDisplay
from BoardState import BoardState
from Direction import Direction
import random
import numpy

choices = [Direction.Down, Direction.Up, Direction.Left, Direction.Right]

class AutoPlayMethods:
    @staticmethod
    def random_play():
        step_number = 0
        while not board_state.Lost:
            step_number += 1
            # print(step_number)
            # print(board_state.get_2d_state())
            board_state.move(random.choice(choices))
        return step_number # returns number of moves

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
number_of_plays = 20 # number of iterations to test autoplay method
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

# board = BoardDisplay(board_state) this displays the state of the board in the GUI
# print("Score: " + str(board_state.get_score()))
