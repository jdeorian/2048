from autoplay.BoardReplay import BoardReplay
from game.BoardState import BoardState
from game.Move import Move
import numpy as np

# this file is here just for testing, but usually you'd put in a filename from the log folder
file_to_import = 'logs/pseudo_ML_20180907_102703.999084.log' # 'logs/random_20180906_095102.239753.log'

log_file = open(file_to_import, "r")
moves = list(map(Move.from_log_entry, log_file.readlines()))
log_file.close()
board = BoardState()
board.import_from_log(moves)
replay = BoardReplay(board)