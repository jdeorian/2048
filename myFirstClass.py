import random
import numpy as np

class board1:
    
    def __init__(self): # initialize a new board
        import random
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

class board:
    def __init__(self,size):
        self.positions = np.zeros((size,size))
        self.add_block()
        self.add_block()
        print(self.positions)
        self.get_input()
    
    def add_block(self):
        empty_idx = np.where(self.positions == 0)
        if len(empty_idx[0]) > 0:
            idx = random.randint(0,len(empty_idx[0])-1)
            self.positions[empty_idx[0][idx],empty_idx[1][idx]] = np.random.choice(a=[2,4],p=[0.85,0.15])
    
    def get_input(self):
        self.user_input = ""
        while self.user_input != "quit":
            print("Enter one of 'w','a','s','d' to swipe up, left, down, right, respectively.")
            print("Enter 'quit' to quit.")
            self.user_input = input()

            if self.user_input == "a":
                self.tmp_positions = self.positions
            elif self.user_input == "w":
                self.tmp_positions = self.positions.T
            elif self.user_input == "d":
                self.tmp_positions = np.flip(self.positions,1)
            elif self.user_input == "s":
                self.tmp_positions = np.flip(self.positions.T,1)
            elif self.user_input == "quit":
                continue
            else:
                print("Invalid input.")
                self.get_input()

            self.swiped()
        
    def swiped(self): # activate cascade of methods constituting a swipe
        self.move()
        #self.combine()
        #self.move()
        #self.add_block()
        print(self.positions)

    def move(self): # move blocks
        self.target_idx = [-1,-1]
        for rdx,r in enumerate(self.tmp_positions):         # For a given row...
            #print(rdx,r)
            for cdx, c in enumerate(r):                     # and a given value in that row...
                #print(rdx,cdx,int(c))                   
                if c == 0:                                  # if that value equals 0, do nothing and look at the next                                                  value.
                    continue
                else:                                       # If that value didn't equal 0...
                    for jdx,j in enumerate(r[0:cdx]):       # look along the row again...
                        if j == 0:                          # and if a value is equal to zero...
                            self.target_idx = [rdx,jdx]     # remember the index of that value...
                            break                           # then get out of this loop.
                        else: self.target_idx = [rdx,cdx]   # If we don't find any 0, it means the value in question is already butted up against another value, so remember its current location.
                    
                if self.target_idx != [rdx,cdx] and self.target_idx != [-1,-1]: # If the value in question needs to move, and if our target index value is valid...
                    print(self.tmp_positions[self.target_idx])
                    print(self.tmp_positions[rdx,cdx])
                    print(self.target_idx,[rdx,cdx],r)
                    self.tmp_positions[self.target_idx] = self.tmp_positions[rdx,cdx]
                    print(self.tmp_positions[self.target_idx])
                    self.tmp_positions[rdx,cdx] = 0
                    print(self.target_idx,[rdx,cdx],r)
                self.target_idx = [-1,-1]

    def combine(self): # combine like blocks, starting from the left
        for rdx,r in enumerate(self.tmp_positions):
            for cdx,c in enumerate(r):
                if cdx != len(r)-1:
                    if self.tmp_positions[rdx,cdx] == self.tmp_positions[rdx,cdx+1]:
                        self.tmp_positions[rdx,cdx] = c*2
                        self.tmp_positions[rdx,cdx+1] = 0

x = board(4)