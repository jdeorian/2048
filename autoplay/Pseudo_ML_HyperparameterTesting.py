from game.BoardDisplay import BoardDisplay
from game.BoardState import BoardState
import numpy as np
import webbrowser
import time
from datetime import datetime
from autoplay.AutoPlayMethods import AutoPlayMethods
import os

import itertools

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

# name: min, max, step
PARAM_DICT = {
	'POINTS_PER_FREE_SQUARE': (0,100,20),
	'HIGHEST_TILE_MULTIPLER': (0,100,20),
	'ORDER_MULT': (0,100,20),
	'DISORDER_MULT': (0,100,20),
	'BASE_SCORE_MULTIPLIER ': (0,100,20),
	'ADJACENT_TILES_MULT': (0,100,20),
	'TILE_BETWEEN_HIGH_AND_CORNER_MULT': (0,100,20)
}

########################################################################

def run_method(x, params: dict):
    new_board = BoardState()
    start_time = time.time()
    AutoPlayMethods.pseudo_ML(new_board, params)
    end_time = time.time()
    print("End: " + str(x))

    # save the detailed log, if appropriate
    if save_detailed_logs:
        #make sure the path exists and get the filename
        detailed_filename = log_directory + autoplay_method + datetime.now().strftime('_%Y%m%d_%H%M%S.%f.log')

        #create a log file
        log_file = open(detailed_filename, 'w')
        log_file.writelines([entry.as_log_entry() + '\n' for entry in new_board.move_history])
        log_file.close()        
    
    # return the results
    return [x + 1, len(new_board.move_history), new_board.get_score(), end_time - start_time, *params.values()]

if __name__ == '__main__':
    #make a list of all the possibilities to try
    iteration_sets = [[(key, x) for x in range(*val)] for key, val in PARAM_DICT.items()]
    iterations = [dict(p) for p in itertools.product(*iteration_sets)]

    # prepare for eventual writing of the text file
    results = []
    headers = "Iteration\tSteps\tScore\tTime(s)\t" + "\t".join(PARAM_DICT.keys())
    format = ['%i', '%i', '%i', '%f', *(['%i'] * len(PARAM_DICT.items()))]

    it_cnt = str(len(iterations) * number_of_plays)
    print("Expected iterations: " + it_cnt)
    p_start = time.time()
    
    #make sure the path exists and get the filename
    if save_detailed_logs:
        os.makedirs(log_directory, exist_ok=True)

    # perform test iterations
    p = mp.Pool()
    i = 0
    for i_params in iterations:
        for x in range(number_of_plays):
            i += 1
            print("Start:" + str(i) + " of " + it_cnt)
            if multiprocessing_enabled:
                p.apply_async(run_method, (i, i_params), callback=results.append)                       
            else:
                results.append(run_method(i, i_params))
    p.close() # doesn't do any harm if we're not multi-processing
    p.join()

    # save results to text file
    if save_to_file:
        if len(results) > 0:
            np.savetxt(output_filename, results, format, '\t', header=headers)
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
