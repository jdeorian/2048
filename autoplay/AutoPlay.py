from game.BoardState import BoardState
import numpy as np
import webbrowser
import time
from datetime import datetime
from autoplay.AutoPlayMethods import AutoPlayMethods
import os

import itertools
import multiprocessing as mp

####################### SET TEST PARAMETERS HERE #######################
save_to_file = True
output_filename = "output.txt" # set output filename
open_on_finish = True
multiprocessing_enabled = True
file_delim = '\t' #tab-delimited file output

save_detailed_logs = True  #this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs"

number_of_plays = 10 # number of iterations to test autoplay method
autoplay_method = "pseudo_ML" # pick the method to run here
########################################################################

# Blank log_path of no log desired
def run_method(x, method_to_call, log_path:str = '', params: dict = {}):
    new_board = BoardState()
    start_time = time.time()
    method_to_call(new_board, params)
    end_time = time.time()

    # save the detailed log, if appropriate
    if log_path:
        #create a log file
        log_file = open(log_path, 'w')
        log_file.writelines([entry.as_log_entry() + '\n' for entry in new_board.move_history])
        log_file.close()        
    
    # return the results
    return [x, len(new_board.move_history), new_board.get_score(), end_time - start_time, *params.values()]

def get_param_permutations(params: dict):
    iteration_sets = [[(key, x) for x in range(*val)] for key, val in params.items()]
    return [dict(p) for p in itertools.product(*iteration_sets)]

# Info includes (in this order)
# filename: str
# results: list
# format: list
# delim: str
# headers: str
def save_summary(info: list, open_on_finish: bool = True):
    # save results to text file
    if save_to_file:
        if len(info[1]) > 0:
            np.savetxt(*info)
        else:
            new_file = open(info[0], 'w')
            new_file.write("(no results)")
            new_file.close()
        if open_on_finish:
            webbrowser.open(output_filename)

def get_headers(params: dict = {}, delim: str = '\t'):
    headers = ["Iteration", "Steps", "Score", "Time(s)"] + list(params.keys())
    return delim.join(headers)

def get_formats(params: dict = {}):
    return ['%i', '%i', '%i', '%f', *(['%i'] * len(params.items()))]

def get_log_path(directory: str, method: str, datefmt: str = '%Y%m%d_%H%M%S.%f', ext: str = "log"):
    return f'{directory}/{method}_{datetime.now().strftime(datefmt)}.{ext}'

def print_process_summary(p_start: float):
    print("start:")
    print(p_start)
    print("end:")
    p_end = time.time()
    print(p_end)
    print("duration:")
    print(p_end - p_start)

#its: number of iterations
#method: the method to run for the iteration
#it_results: list that will contain the results of the iterations
#it_params: list of dictionary objects that contain parameters; if
#   empty, only one set it run with no params passed
def run_iterations(method:str, its: int, it_results: list, multiprocessing: bool = True, log_dir: str = "logs", it_params: list = []):
    methods = AutoPlayMethods()
    method_to_call = getattr(methods, method)

    #make sure the path exists and get the filename
    if log_dir:
        os.makedirs(log_dir, exist_ok=True)

    it_cnt = max(1,len(it_params)) * its
    print(f"Expected iterations: {it_cnt}")

    #if there's no params list, add an empty dictionary so the loop will run once
    if not it_params:
        it_params.append({})

    # perform test iterations
    p = mp.Pool()
    i = 0
    for i_params in it_params:
        for _ in range(its):
            i += 1
            print(f"Start: {i} of {it_cnt}")
            method_params = (i, method_to_call, get_log_path(log_dir, method), i_params)
            if multiprocessing:
                p.apply_async(run_method, method_params, callback=it_results.append)                       
            else:
                it_results.append(run_method(*(method_params)))
    p.close() # doesn't do any harm if we're not multi-processing
    p.join()

if __name__ == '__main__':
    results = []
    p_start = time.time()
    run_iterations(autoplay_method, number_of_plays, results, multiprocessing_enabled, log_directory)
    
    if save_to_file:
        save_summary([output_filename, results, get_formats(), file_delim, '\n', get_headers()], open_on_finish)
    
    print_process_summary(p_start)
