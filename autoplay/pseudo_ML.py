from game.BoardState import BoardState
from game.Direction import Direction
import random
import numpy as np
import webbrowser
from util.ArrayMagic import flatten, unflatten

#score more free squares higher
#higher score for highest number on board

# you need at least
MIN_TILES_FOR_ENUM = 6
# otherwise don't bother running every single scenario

POINTS_PER_FREE_SQUARE = 50
HIGHEST_TILE_MULTIPLER = 2
ORDER_SCORE = 1

class Pseudo_ML:
    # given a set of weights, returns the top weight [0] and top score [1]
    def get_direction_recommendation(self, weights):
        #print scores and get max value
        retVal = Direction.Down
        top_score = -100
        for direction in Direction:
            this_score = weights[direction]
            # print(str(direction) + ": " + str(this_score))
            if this_score > top_score:
                top_score = this_score
                retVal = direction
        
        #return the top-scoring direction
        return [retVal, top_score]

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
                    score = self.score_board_state(board_state.field)
                    direction_scores[direction] += prob * score
                    statesAfterMove.append([direction, state, prob, score])
            else:
                direction_scores[direction] -= 1000 # never pick a direction that will have no impact
        
        return direction_scores
    
    def enumerate_possible_outcomes(self, squares_array, four_chance, size):
        retVal = []        
        empty_indexes = [idx for idx, val in enumerate(squares_array) \
                              if val == 0]
        
        #don't run all the possibilities right at the beginning of the game
        if len(empty_indexes) < MIN_TILES_FOR_ENUM:
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
        bonus = sum([item * POINTS_PER_FREE_SQUARE for item in items if item == 0])
        bonus += max(items) * HIGHEST_TILE_MULTIPLER

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
                            score += ORDER_SCORE
                        else:
                            score -= ORDER_SCORE
            if score > highest_score:
                highest_score = score

        return highest_score
                    