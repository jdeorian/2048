from game.BoardState import BoardState
from game.Direction import Direction
import random
import numpy as np
import webbrowser
from util.ArrayMagic import flatten, unflatten, invert, flip, array_2d_copy

class Pseudo_ML:
    # you need at least
    MIN_TILES_FOR_ENUM = 6
    # otherwise don't bother running every single scenario

    POINTS_PER_FREE_SQUARE = 10
    HIGHEST_TILE_MULTIPLER = 2
    ORDER_MULT = 1
    DISORDER_MULT = 1
    BASE_SCORE_MULTIPLIER = .1
    ADJACENT_TILES_MULT = 5
    TILE_BETWEEN_HIGH_AND_CORNER_MULT = 1

    def __init__(self, param_vals: dict = None):
        if param_vals != None:
            for key, val in param_vals.items():
                setattr(self, key, val)

    # given a set of weights, returns the top weight [0] and top score [1]
    # The "weights" arg is a dict of Direction:Value(Float)
    def get_direction_recommendation(self, weights: dict):
        high_val = max(weights.values())
        return [(key, val) for key, val in weights.items() if val == high_val][0]

    # given a board state, returns scores corresponding to each possible move
    def get_direction_weights(self, board_state: BoardState):
        statesAfterMove = []
        direction_scores = { d:0 for d in Direction }
        squares_array = flatten(board_state.field)
        # get a list of the board state after making all legal moves
        for direction in Direction:
            dir_move = board_state.move(direction, False, False)
            if dir_move.changed_board(): # ignore moves that do not change the board
                possible_states = self.enumerate_possible_outcomes(squares_array, board_state.FOUR_CHANCE, board_state.BOARD_SIZE)
                for state, prob in possible_states:
                    score = self.score_board_state2(board_state)
                    direction_scores[direction] += prob * score
                    statesAfterMove.append([direction, state, prob, score])
        
        return { key:val for key, val in direction_scores.items() if val != 0 }
    
    def enumerate_possible_outcomes(self, squares_array, four_chance, size):
        retVal = []        
        empty_indexes = [idx for idx, val in enumerate(squares_array) \
                              if val == 0]
        
        #don't run all the possibilities right at the beginning of the game
        if len(empty_indexes) < self.MIN_TILES_FOR_ENUM:
            retVal.append([unflatten(squares_array, size), 1])
        else:
            for idx in empty_indexes:
                new_outcome_1 =  squares_array[:]
                new_outcome_1[idx] = 1
                retVal.append([unflatten(new_outcome_1, size), 1 - four_chance])
                new_outcome_2 = squares_array[:]
                new_outcome_2[idx] = 2
                retVal.append([unflatten(new_outcome_2, size), four_chance])
        
        return retVal
    
    def score_board_state(self, board_2d_state):
        highest_score = 0
        board_size = len(board_2d_state[0])

        #add scores that don't change based on direction
        items = [item for row in board_2d_state for item in row]
        bonus = sum([item * self.POINTS_PER_FREE_SQUARE for item in items if item == 0])
        bonus += max(items) * self.HIGHEST_TILE_MULTIPLER

        for orientation in Direction:
            # tranform the array so the orientation is left
            working_board = BoardState.transform_dict[orientation][0](board_2d_state)

            #for every row, score value pairs by whether they are in ascending order
            score = bonus
            for x in range(board_size):
                #for every value pair
                values = [val for val in working_board[x] if val > 0]
                value_count = len(values)
                if value_count > 2: # there have to be at least 2 values to score the row
                    for idx in range(len(values) - 1): # for every value except the last
                        fst_val = values[idx]
                        sec_val = values[idx + 1]
                        if sec_val >= fst_val:
                            score += self.ORDER_MULT
                        else:
                            score -= self.ORDER_MULT
            if score > highest_score:
                highest_score = score

        return highest_score

    def score_board_state2(self, board: BoardState):
        scores = []
        base_score = board.get_score() * (self.BASE_SCORE_MULTIPLIER / 10)
        scores.append(base_score)

        #add scores that don't change based on direction
        items = flatten(board.field)
        bonus = sum([item * self.POINTS_PER_FREE_SQUARE for item in items if item == 0])
        bonus += max(items) * self.HIGHEST_TILE_MULTIPLER
        scores.append(bonus)

        # get the corner into the 0,0 spot
        corner = self.get_corner_by_tiles(board)
        working_board = array_2d_copy(board.field)
        if corner[0] > 0:
            working_board = invert(working_board)
        if corner[1] > 0:
            working_board = flip(working_board)

        # get the order score
        order_score = self.get_board_order_score(working_board)
        scores.append(order_score)

        # get the high value on the board (closest to the corner)
        (h_x, h_y, high_val) = self.get_high_val(working_board)
        high_score = high_val * self.HIGHEST_TILE_MULTIPLER
        scores.append(high_score)

        # get the count of tiles between the high_val and the corner
        bad_tile_count = len([(x,y) for x in range(h_x + 1) \
                                    for y in range(h_y + 1) \
                                    if (x > 0 or y > 0) and working_board[x][y] > 0])
        bad_tile_score = bad_tile_count * self.TILE_BETWEEN_HIGH_AND_CORNER_MULT * -1
        scores.append(bad_tile_score)

        return sum(scores)

    def get_corner_by_tiles(self, board: BoardState):
        items = flatten(board.field)
        c_y = 0 if sum(items[0:board.BOARD_SIZE * 2]) > sum(items[board.BOARD_SIZE+1:len(items)]) else board.BOARD_SIZE - 1
        left = flatten([row[0:int(board.BOARD_SIZE / 2)] for row in board.field])
        right = flatten([row[int((board.BOARD_SIZE / 2))+1:board.BOARD_SIZE] for row in board.field])
        c_x = 0 if sum(left) > sum(right) else board.BOARD_SIZE
        return c_x, c_y

    # scores the entire board based on whether it declines radiating from the corner or increases
    # assumes that the board has been transformed so that the corner is in the upper left
    def get_board_order_score(self, field: list):       
        size = len(field)
        return sum([self.get_order_score(field, (i,j)) for i in range(size) \
                                                       for j in range(size)])
    
    # look at the values below and to the right of a square, then score based on whether
    # it goes down (preferred) or up (not preferred). Scales based on how big the difference is.
    # For efficiency, it also adds the adjacent tile multiplier.
    def get_order_score(self, field, pair: tuple):
        size = len(field) - 1 # this is actually just the highest index, not the size
        (i,j) = pair
        val = field[i][j]
        score = 0
        if (i < size): # if we're not on the far right of the board
            dif = val - field[i+1][j]
            if dif == 0:
                score += self.ADJACENT_TILES_MULT
            mult = self.ORDER_MULT if dif >= 0 else self.DISORDER_MULT
            score += dif * mult
        if (j < size): # if we're not all the way at the bottom of the board
            dif = val - field[i][j+1]
            if dif == 0:
                score += self.ADJACENT_TILES_MULT
            mult = self.ORDER_MULT if dif >= 0 else self.DISORDER_MULT
            score += dif * mult
        return score        

    def get_high_val(self, field):
        size = len(field)
        high_val = max(flatten(field))
        high_vals = [(x,y,field[x][y]) for x in range(size) \
                                      for y in range(size) \
                                      if field[x][y] == high_val]
        if len(high_vals) == 1: # shortcut to avoid sorting computation on single-item arrays
            return high_vals[0]

        # sort the high values by their closeness to the corner and provide the corner-iest item
        def get_key(item):
            return item[0] + item[1] # x+y, a rough estimate for how far from the corner the item is
        high_vals.sort(key=get_key)
        return high_vals[0] # return the high value closest to the corner


# How I play:

# Stutter until first fail
# Identify corner anchor
# Avoid:
#    Random piece (or piece on board) preventing anchor tile from being (or returning to) corner
# Prefer:
#    Gradient moving outward from corner
#    Combining tiles, the higher the tile the more, bonus for corner
#    Moving to position with tiles that are adjacent (but less than combining),
#       the higher the tile the more, bonus for corner
