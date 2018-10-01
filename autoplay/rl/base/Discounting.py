from abc import abstractmethod, ABCMeta

# to use this inheritable base, declare a class that inherits from it,
# then implement the get_discounted_reward method

class RewardDiscountMethod:
    __metaclass__ = ABCMeta # prevents classes from inheriting unless the abstract methods have been overloaded

    def __init__(self, rate: float):
        self.discount_rate = rate
    
    # reward is the reward received for each move
    def get_discounted_reward_sum(self, rewards: list):
        rwd_count = len(rewards)
        if rwd_count == 0:
            return 0
        return sum([self.get_discounted_reward(rwd_count, idx, reward) for idx, reward in enumerate(rewards)])

    @abstractmethod
    def get_discounted_reward(self, move_count: int, move_idx: int, reward: float):
        pass