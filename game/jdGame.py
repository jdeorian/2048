import random
import math
import numpy as np
from game.Direction import Direction

class jdBoard:
    def __init__(self,size):
        self.BOARD_SIZE = size
        self.Lost = False

        self.positions = np.zeros(shape = (self.BOARD_SIZE,self.BOARD_SIZE), dtype = int)
        self.add_block()
        self.add_block()

    def get_value(self,y,x):
        if self.positions[x-1,y-1] == 0:
            return 0
        else:
            return int(math.log(self.positions[x-1,y-1],2))
    
    def get_display_value(self,y,x):
        return self.positions[x-1,y-1]

    def add_block(self):
        empty_idx = np.where(self.positions == 0)
        if len(empty_idx[0]) > 0:
            idx = random.randint(0,len(empty_idx[0])-1)
            self.positions[empty_idx[0][idx],empty_idx[1][idx]] = np.random.choice(a=[2,4],p=[0.9,0.1])
        else:
            self.Lost = True
            return

    def reset_board(self):
        self.positions = np.zeros((self.BOARD_SIZE,self.BOARD_SIZE))
        self.Lost = False
    
    def move(self,direction):
            if direction == Direction.Left:
                self.tmp_positions = self.positions
            elif direction == Direction.Up:
                self.tmp_positions = self.positions.T
            elif direction == Direction.Right:
                self.tmp_positions = np.flip(self.positions,1)
            elif direction == Direction.Down:
                self.tmp_positions = np.flip(self.positions.T,1)

            self.swiped()
        
    def swiped(self): # activate cascade of methods constituting a swipe
        self.any_movement = False
        self.shift()
        self.combine()
        self.shift()
        if self.any_movement == True:
            self.add_block()

    def shift(self): # shift blocks
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