from abc import abstractstaticmethod, ABCMeta
from game.BoardState import BoardState
from autoplay.rl.base.Discounting import RewardDiscountMethod
from util.ArrayMagic import flatten

class RewardMethod:
    __metaclass__ = ABCMeta # prevents classes from inheriting unless the abstract methods have been overloaded

    # a reward system takes in a BoardState and Discount system and assigns a reward to each successive state change
    # It assigns a reward to each move, and a discounted reward to each move.
    @staticmethod
    def apply_rewards(board: BoardState, discount_method: RewardDiscountMethod):
        # apply the initial rewards
        last_score = RewardMethod.get_score(board.move_history[0].start_state)
        for move in board.move_history: 
            # rewards are the difference between the scores
            last_score = move.reward = RewardMethod.get_score(move.end_state) - last_score

        # apply discounted rewards
        discounted_rewards = discount_method.get_discounted_reward_sum([m.reward for m in board.move_history])
        for idx, dr in enumerate(discounted_rewards):
            board.move_history[idx].discounted_reward = dr
    
    @abstractstaticmethod
    def get_score(field: list): # scores a 2d list ([][])
        return sum(flatten(field)) # naive implementation
