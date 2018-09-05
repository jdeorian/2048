from autoplay.BoardReplay import BoardReplay
from game.BoardState import BoardState
from game.Move import Move
import numpy as np

file_to_import = 'logs/random_20180904_153451.982868.log'

log_file = open(file_to_import, "r")
moves = list(map(Move.from_log_entry, log_file.readlines()))
log_file.close()
board = BoardState()
board.import_from_log(moves)
replay = BoardReplay(board)