from game.BoardDisplay import BoardDisplay
from game.BoardState import BoardState
import numpy as np
import webbrowser
import time
from datetime import datetime
from autoplay.AutoPlayMethods import AutoPlayMethods
import os

import autoplay.pseudo_ML as pml

import multiprocessing as mp

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
open_on_finish = True
multiprocessing_enabled = True

save_detailed_logs = False  #this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs/"

number_of_plays = 5 # number of iterations to test autoplay method
autoplay_method = "pseudo_ML" # pick the method to run here

# min, max, step
POINTS_PER_FREE_SQUARE_rng = (50,100,5)
HIGHEST_TILE_MULTIPLER_rng = (30,100,5)
ORDER_SCORE_rng = (10,11,1)


########################################################################

methods = AutoPlayMethods()
method_to_call = getattr(methods, autoplay_method)
results = []

def run_method(x, ppfs, htm, os):
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
    return [x + 1, len(new_board.move_history), new_board.get_score(), end_time - start_time, ppfs, htm, os]

if __name__ == '__main__':    
    p_start = time.time()
    
    #make sure the path exists and get the filename
    if save_detailed_logs:
        os.makedirs(log_directory, exist_ok=True)

    # perform test iterations
    if multiprocessing_enabled:
        p = mp.Pool()
        i = 0
        for ppfs in range(POINTS_PER_FREE_SQUARE_rng[0], POINTS_PER_FREE_SQUARE_rng[1], POINTS_PER_FREE_SQUARE_rng[2]):
            pml.POINTS_PER_FREE_SQUARE = ppfs
            for htm in range(HIGHEST_TILE_MULTIPLER_rng[0], HIGHEST_TILE_MULTIPLER_rng[1], HIGHEST_TILE_MULTIPLER_rng[2]):
                pml.HIGHEST_TILE_MULTIPLER = htm
                for os in range(ORDER_SCORE_rng[0], ORDER_SCORE_rng[1], ORDER_SCORE_rng[2]):
                    pml.ORDER_SCORE = os
                    for x in range(number_of_plays):                            
                        i += 1
                        p.apply_async(run_method, (i,ppfs, htm, os), callback=results.append)
        p.close()
        p.join()
    else:
        i = 0
        for ppfs in range(POINTS_PER_FREE_SQUARE_rng):
            pml.POINTS_PER_FREE_SQUARE = ppfs
            for htm in range(HIGHEST_TILE_MULTIPLER_rng):
                pml.HIGHEST_TILE_MULTIPLER = htm
                for os in range(ORDER_SCORE_rng):
                    pml.ORDER_SCORE = os
                    for x in range(number_of_plays):                            
                        i += 1
                        results.append(run_method(i,ppfs, htm, os))

    # save results to text file
    if save_to_file:
        if len(results) > 0:
            np.savetxt(output_filename, results, ['%i', '%i', '%i', '%f', '%i', '%i', '%i'], '\t', header="Iteration\tSteps\tScore\tTime(s)\tPPFS\tHTM\tOS")
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
