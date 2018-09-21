import time
from autoplay.AutoPlay import run_iterations

####################### SET TEST PARAMETERS HERE #######################
#Set all other parameters in the autoplay module

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
    results = []
    run_iterations(results, PARAM_DICT)
