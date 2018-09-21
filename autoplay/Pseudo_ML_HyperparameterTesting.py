import time
from autoplay.AutoPlay import run_method, get_headers, get_formats, run_iterations, save_summary, print_process_summary, get_param_permutations

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
open_on_finish = True
multiprocessing_enabled = True
file_delim = '\t' #tab-delimited file output

save_detailed_logs = True  #this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs"

number_of_plays = 2 # number of iterations to test autoplay method
autoplay_method = "pseudo_ML" # pick the method to run here

# name: min, max, step
PARAM_DICT = {
	'POINTS_PER_FREE_SQUARE': (0,100,50),
	'HIGHEST_TILE_MULTIPLER': (0,100,50),
	'ORDER_MULT': (0,100,50),
	'DISORDER_MULT': (0,100,50),
	'BASE_SCORE_MULTIPLIER ': (0,100,50),
	'ADJACENT_TILES_MULT': (0,100,50),
	'TILE_BETWEEN_HIGH_AND_CORNER_MULT': (0,100,50)
}

########################################################################

if __name__ == '__main__':
    #make a list of all the possibilities to try
    iterations = get_param_permutations(PARAM_DICT)

    results = []
    p_start = time.time()
    run_iterations(autoplay_method, number_of_plays, results, multiprocessing_enabled, log_directory, iterations)
    
    if save_to_file:
        save_summary([output_filename, results, get_formats(PARAM_DICT), file_delim, '\n', get_headers(PARAM_DICT)], open_on_finish)

    print_process_summary(p_start)
