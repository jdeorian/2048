from BoardDisplay import BoardDisplay
from BoardState import BoardState
from Direction import Direction
import random

class AutoPlayMethods:
    @staticmethod
    def random_play():
        step_number = 0
        while not board_state.Lost:
            step_number += 1
            print(step_number)
            print(board_state.get_2d_state())
            board_state.move(random.choice(choices))

choices = [Direction.Down, Direction.Up, Direction.Left, Direction.Right]
board_state = BoardState()

# pick the method to run here
AutoPlayMethods.random_play()

board = BoardDisplay(board_state)
print("Score: " + str(board_state.get_score()))