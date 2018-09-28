from abc import abstractmethod

# to use this inheritable base, declare a class that inherits from it,
# then 

# Randomness is a little more complex than discount. Randomness can be
# scaled both based on the current iteration AND on the current move.
# The randomness for a given iteration acts as the "base rate", meaning
# the rate applied to move 1. The randomness rate for moves is applied to
# successive moved to increase or decrease that move randomness. A move
# randomness rate of 0 means that the randomness rate will be unchanged
# from the base iteration randomness rate.
class RandomnessMethod:
    def __init__(self, iter_rate: float, move_rate):
        self.iteration_rate = iter_rate
        self.move_rate = move_rate

    @abstractmethod
    def get_randomness_rate(self, iter_count: int, current_iter: int, current_move: int):
        pass