import random
import numpy as np

class board:
    def __init__(self,size):
        self.positions = np.zeros((size,size))
        self.add_block()
        self.add_block()
        print("Enter one of 'w','a','s','d' to swipe up, left, down, right, respectively.")
        print("Enter 'quit' to quit.")
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
            print(self.positions)
        
    def swiped(self): # activate cascade of methods constituting a swipe
        self.any_movement = False
        self.move()
        self.combine()
        self.move()
        if self.any_movement == True:
            self.add_block()

    def move(self): # move blocks
        for rdx,r in enumerate(self.tmp_positions):        # For a given row...
            for cdx, c in enumerate(r):                    # and a given value in that row...                  
                if c != 0:                                 # If that value doesn't equal 0...
                    for jdx,j in enumerate(r[:cdx]):       # look along the row again...
                        if j == 0:                         # and if a value is equal to zero...
                            self.tmp_positions[rdx,jdx] = self.tmp_positions[rdx,cdx] # move the original value there...
                            self.tmp_positions[rdx,cdx] = 0
                            self.any_movement = True                           # and overwrite its original place with 0       
                            break                           # then get out of this loop.

    def combine(self): # combine like blocks, starting from the left
        for rdx,r in enumerate(self.tmp_positions):
            for cdx,c in enumerate(r):
                if c != 0:
                    if cdx != len(r)-1:
                        if self.tmp_positions[rdx,cdx] == self.tmp_positions[rdx,cdx+1]:
                            self.tmp_positions[rdx,cdx] = c*2
                            self.tmp_positions[rdx,cdx+1] = 0
                            self.any_movement = True

x = board(4)