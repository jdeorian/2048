# I didn't figure this out yet.

# What if you tried implementing the same display class I do? You just need to
# implement the methods that BoardDisplay current uses within your own board class,
# and prevent and take out (move into another class, remove, or make optional) the
# control logic/loop you have in your current game.

# As long as your methods/fields have the same names/signatures as mine,
# the display class will never know it's a different kind of board.

# The BoardDisplay.py class requires the following methods from a board class:
#  -self.BOARD_SIZE (4 is the only value I've used)
#
#  -self.get_value(x, y)     # get the value of a square at x, y; this needs
#                            # to be an exponent of 2 so the right color is
#                            # assigned. Luckily that just means you take the
#                            # actual value and take the square root.
#  -self.display_value(x, y) # gets the text that should be displayed
#                            # in the square for a given value
#
#  -self.move(direction)     # makes a move. Uses the Direction class to indicate
#                            # the move direction. You already have all this logic,
#                            # so you'd probably want to move your transform into
#                            # a method that accepts a Direction object, then change
#                            # your get_input method to call the new method after
#                            # it parses input into a direction. That way you can
#                            # use your game either as a console app OR as with a GUI
#
#  -self.Lost                # has the player lost?
#  -self.reset_board()       # resets the board to starting position

# When you're done, you'll be able to run the following:

from BoardDisplay import BoardDisplay
from jdGame import board

game = BoardDisplay(board(4))