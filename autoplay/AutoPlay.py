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
multiprocessing_enabled = False
file_delim = '\t' #tab-delimited file output

save_detailed_logs = True  # this outputs a detailed move-by-move log of the game which can also be used for "playback"
log_directory = "logs"

number_of_plays =  1# number of iterations to test autoplay method
autoplay_method = "branch" # pick the method to run here
########################################################################

# Blank log_path of no log desired
def run_method(x, method_to_call, log_path:str = '', params: dict = {}):
    new_board = BoardState()
    start_time = time.time()
    method_to_call(new_board, params)
    end_time = time.time()

    print(f'End: {x}')

    # save the detailed log, if appropriate
    if log_path:
        #create a log file
        log_file = open(log_path, 'w')
        log_file.writelines([entry.as_log_entry() + '\n' for entry in new_board.move_history])
        log_file.close()        
    
    # return the results
    return [x, len(new_board.move_history), new_board.field.get_score(), end_time - start_time, *params.values()]

def get_param_permutations(params: dict):
    iteration_sets = [[(key, x) for x in range(*val)] for key, val in params.items()]
    return [dict(p) for p in itertools.product(*iteration_sets)]

# Info includes (in this order)
# filename: str
# results: list
# format: list
# delim: str
# headers: str
def save_summary(it_results: list, params:dict = {}):
    # save results to text file
    if save_to_file:
        if len(it_results) > 0:
            np.savetxt(output_filename, it_results, get_formats(params), file_delim, header=get_headers(params))
        else:
            new_file = open(output_filename, 'w')
            new_file.write("(no results)")
            new_file.close()
        if open_on_finish:
            webbrowser.open(output_filename)

def get_headers(params: dict = {}, delim: str = '\t'):
    headers = ["Iter", "Steps", "Score", "Time(s)"] + list(params.keys())
    return delim.join(headers)

def get_formats(params: dict = {}):
    return ['%i', '%i', '%i', '%f', *(['%i'] * len(params.items()))]

def get_log_path(method: str, datefmt: str = '%Y%m%d_%H%M%S.%f', ext: str = "log"):
    return f'{log_directory}/{method}_{datetime.now().strftime(datefmt)}.{ext}'

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
def run_iterations(it_results: list, param_dict: dict = {}):
    it_params = get_param_permutations(param_dict) if param_dict else []
    methods = AutoPlayMethods()
    method_to_call = getattr(methods, autoplay_method)

    #make sure the path exists and get the filename
    if log_directory:
        os.makedirs(log_directory, exist_ok=True)

    it_cnt = max(1,len(it_params)) * number_of_plays
    print(f"Expected iterations: {it_cnt}")

    #if there's no params list, add an empty dictionary so the loop will run once
    if not it_params:
        it_params.append({})

    # perform test iterations
    p = mp.Pool() if multiprocessing_enabled else None
    i = 0
    p_start = time.time()
    for i_params in it_params:
        for _ in range(number_of_plays):
            i += 1
            print(f"Start: {i} of {it_cnt}")
            method_params = (i, method_to_call, get_log_path(autoplay_method), i_params)
            if multiprocessing_enabled:
                p.apply_async(run_method, method_params, callback=it_results.append)                       
            else:
                it_results.append(run_method(*(method_params)))
    if multiprocessing_enabled:
        p.close()
        p.join()
    save_summary(it_results, param_dict)  
    print_process_summary(p_start)

if __name__ == '__main__':
    results = []
    run_iterations(results)
