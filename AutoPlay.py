from game.BoardDisplay import BoardDisplay
from game.BoardState import BoardState
import numpy
import webbrowser
from AutoPlayMethods import AutoPlayMethods

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
number_of_plays = 10 # number of iterations to test autoplay method
autoplay_method = "random_play" # pick the method to run here
########################################################################

# prepare for run
methods = AutoPlayMethods()
results = []
method_to_call = getattr(methods, autoplay_method)

# perform test iterations
for x in range(number_of_plays):
    board_state = BoardState() #initialize new board
    steps = method_to_call(board_state)
    result = [x + 1, steps, board_state.get_score()]
    results.append(result)
    print(result) # prints results

# save results to text file
if save_to_file:
    numpy.savetxt(output_filename, results, '%i', '\t', header="Iteration\tSteps\tScore")
    webbrowser.open(output_filename)

# board = BoardDisplay(board_state) this displays the state of the board in the GUI
# print("Score: " + str(board_state.get_score()))
