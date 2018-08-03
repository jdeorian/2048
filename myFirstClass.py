import random
import numpy as np

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
        print(self.tmp_positions)
        for rdx,r in enumerate(self.tmp_positions):         # For a given row...
            for cdx, c in enumerate(r):      
                print("[",rdx,",",cdx,"]",c)               # and a given value in that row...                  
                if c == 0:                                  # if that value equals 0, do nothing and look at the next                                                  value.
                    continue
                else:                                       # If that value didn't equal 0...
                    for jdx,j in enumerate(r[:cdx]):       # look along the row again...
                        if j == 0:                          # and if a value is equal to zero...
                            self.target_idx = [rdx,jdx]     # remember the index of that value...
                            break                           # then get out of this loop.
                        else:
                            self.target_idx = [rdx,cdx]   # If we don't find any 0, it means the value in question is already butted up against another value, so remember its current location.
                    if self.target_idx != [rdx,cdx] and self.target_idx != [-1,-1]: # If the value in question needs to move, and if our target index value is valid...
                        print(self.target_idx)
                        print(self.tmp_positions[self.target_idx])
                        self.tmp_positions[self.target_idx] = self.tmp_positions[rdx,cdx]
                        self.tmp_positions[rdx,cdx] = 0
                    self.target_idx = [-1,-1]

    def combine(self): # combine like blocks, starting from the left
        for rdx,r in enumerate(self.tmp_positions):
            for cdx,c in enumerate(r):
                if cdx != len(r)-1:
                    if self.tmp_positions[rdx,cdx] == self.tmp_positions[rdx,cdx+1]:
                        self.tmp_positions[rdx,cdx] = c*2
                        self.tmp_positions[rdx,cdx+1] = 0

x = board(4)