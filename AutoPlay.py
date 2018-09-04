from game.BoardDisplay import BoardDisplay
from game.BoardState import BoardState
import numpy
import webbrowser
import time
from datetime import datetime
from autoplay.AutoPlayMethods import AutoPlayMethods
import os

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename

save_detailed_legs = True  #this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs/"

number_of_plays = 5 # number of iterations to test autoplay method
autoplay_method = "random" # pick the method to run here
########################################################################

# prepare for run
methods = AutoPlayMethods()
results = []
method_to_call = getattr(methods, autoplay_method)

# perform test iterations
for x in range(number_of_plays):
    board_state = BoardState() #initialize new board
    start_time = time.time()
    method_to_call(board_state)
    end_time = time.time()
    result = [x + 1, len(board_state.move_history), board_state.get_score(), end_time - start_time]
    results.append(result)
    print(result) # prints results

    #save the detailed log, if appropriate
    if save_detailed_legs:
        #make sure the path exists and get the filename
        os.makedirs(log_directory, exist_ok=True)
        detailed_filename = log_directory + autoplay_method + datetime.now().strftime('_%Y%m%d_%H%M%S.%f.log')

        #create a log file
        log_text = []
        for entry in board_state.move_history:
            log_text.append(entry.as_log_entry())
        numpy.savetxt(detailed_filename, log_text, '%s')        

# save results to text file
if save_to_file:
    numpy.savetxt(output_filename, results, ['%i', '%i', '%i', '%f'], '\t', header="Iteration\tSteps\tScore\tTime(s)")
    webbrowser.open(output_filename)

# board = BoardDisplay(board_state) this displays the state of the board in the GUI
# print("Score: " + str(board_state.get_score()))
