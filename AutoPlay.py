from game.BoardDisplay import BoardDisplay
from game.BoardState import BoardState
import numpy as np
import webbrowser
import time
from datetime import datetime
from autoplay.AutoPlayMethods import AutoPlayMethods
import os

import multiprocessing as mp

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
open_on_finish = True
multiprocessing_enabled = True

save_detailed_logs = False  #this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs/"

number_of_plays = 100 # number of iterations to test autoplay method
autoplay_method = "pseudo_ML" # pick the method to run here
########################################################################

methods = AutoPlayMethods()
method_to_call = getattr(methods, autoplay_method)
results = []

def run_method(x):
    new_board = BoardState()
    start_time = time.time()
    method_to_call(new_board)
    end_time = time.time()

    # save the detailed log, if appropriate
    if save_detailed_logs:
        #make sure the path exists and get the filename
        detailed_filename = log_directory + autoplay_method + datetime.now().strftime('_%Y%m%d_%H%M%S.%f.log')

        #create a log file
        log_file = open(detailed_filename, 'w')
        log_file.writelines([entry.as_log_entry() + '\n' for entry in new_board.move_history])
        log_file.close()        
    
    # return the results
    return [x + 1, len(new_board.move_history), new_board.get_score(), end_time - start_time]

if __name__ == '__main__':    
    p_start = time.time()
    
    #make sure the path exists and get the filename
    if save_detailed_logs:
        os.makedirs(log_directory, exist_ok=True)

    # perform test iterations
    if multiprocessing_enabled:
        p = mp.Pool()
        for x in range(number_of_plays):
            p.apply_async(run_method, (x,), callback=results.append)
        p.close()
        p.join()
    else:
        for x in range(number_of_plays):
            results.append(run_method(x))

    # save results to text file
    if save_to_file:
        if len(results) > 0:
            np.savetxt(output_filename, results, ['%i', '%i', '%i', '%f'], '\t', header="Iteration\tSteps\tScore\tTime(s)")
        else:
            new_file = open(output_filename, 'w')
            new_file.write("(no results)")
            new_file.close()
        if open_on_finish:
            webbrowser.open(output_filename)

    print("start:")
    print(p_start)
    print("end:")
    p_end = time.time()
    print(p_end)
    print("duration:")
    print(p_end - p_start)
