import random
class block:
    def __init__(self):
        self.value = 2

class board:
    import random
    def __init__(self): # initialize a new board
        self.game_status = "Playing"
        self.positions = [0,0,0,0]
        self.add_block()
        self.add_block()
        print(self.positions)
        self.user_action = "n/a"
        while self.user_action != "quit" and self.game_status != "Game Over":
            print("Type 'a' to swipe left. Type 'quit' to quit.")
            self.user_action = input()
            if self.user_action == "a":
                self.swiped_left()
            elif self.user_action == "quit":
                continue
            else: 
                print("Not a valid entry.")
    
    def swiped_left(self): # activate cascade of methods constituting a swipe to the left
        self.move_left()
        self.combine_left()
        self.move_left()
        self.add_block()
        if self.game_status != "Game Over":
            print(self.positions)

    def combine_left(self): # combine like blocks, starting from the left
        for idx, i in enumerate(self.positions): 
            if idx != len(self.positions)-1:
                if self.positions[idx] == self.positions[idx+1]:
                    self.positions[idx] = i*2
                    self.positions[idx+1] = 0
    
    def move_left(self): # move blocks left
        self.target_idx = -1
        for idx,i in enumerate(self.positions): 
            if i == 0:
                continue
            else:
                for jdx,j in enumerate(self.positions[0:idx]):
                    if j == 0:
                        self.target_idx = jdx
                        break
                    else: self.target_idx = idx
                
                if self.target_idx != idx and self.target_idx != -1:
                    self.positions[self.target_idx] = self.positions[idx]
                    self.positions[idx] = 0
            self.target_idx = -1
    
    def add_block(self): # add a new, random block to the board
        empty_idx = []
        for idx,i in enumerate(self.positions):
            if i==0:
                empty_idx.append(idx)
        if len(empty_idx) > 0:
               self.positions[int(random.sample(empty_idx,1)[0])] = 1
        else: 
            print("Game Over")
            self.game_status = "Game Over"

x = board()
