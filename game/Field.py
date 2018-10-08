from util.ArrayMagic import flatten, unflatten, array_2d_copy, invert, transpose
from autoplay.rl.util.CanonicalField import get_canonical_fieldID
from random import choice
from game.Direction import Direction

# (do, undo)
transform_dict = { 
    Direction.Left:  (lambda field: field,\
                        lambda field: field),  
    Direction.Right: (lambda field: invert(field),\
                        lambda field: invert(field)),                       
    Direction.Up:    (lambda field: transpose(invert(field)),\
                        lambda field: invert(transpose(field))),
    Direction.Down:  (lambda field: invert(transpose(field)),\
                        lambda field: transpose(invert(field)))
}

class Field(list):

    def as_flat(self):            
        return flatten(self)

    # accepts just a size (defaults values to 0), a flat array,
    # or a field/[][] object. No_copy applies only to field or
    # [][] objects, and if set to true skips the array copy that
    # would normally take place.
    def __init__(self, size: int = 4, field: list = [], no_copy:bool = False):
        self.size = size        
        if len(field) == 0:
            list.__init__(self, [[0] * size for _ in range(size)])
            return

        # flat array
        if len(field[0]) == 1:
            list.__init__(self, unflatten(field, size))
        else: # 2d array
            init_fld = field if no_copy else array_2d_copy(field)
            list.__init__(self, init_fld)
    
    # returns both the canonical version of the field as well as the field ID (ID, field)
    def as_canonical(self):
        return get_canonical_fieldID(self)

    def get_empty_squares(self):
        return [(i,j) for i in range(self.size) \
                      for j in range(self.size) \
                      if self[i][j] == 0]
    
    def get_random_empty_square(self):
        empty = self.get_empty_squares()
        if len(empty) == 0: return None
        
        return choice(self.get_empty_squares())

    def set_random_empty_square(self, val: int):
        rnd = self.get_random_empty_square()
        if rnd == None:
            return
        else:
            i,j = rnd
        self[i][j] = val

    def invalid_coordinates(self, x, y):
        return not 1 <= x <= self.size or \
               not 1 <= y <= self.size

    def slide(self, direction: Direction):
        transformed = self.transform(direction) # makes a copy of the field that is transformed
        transformed.slide_rows_left()
        return transformed.un_transform(direction)
    
    # type 0 transforms, type 1 un-transforms, but there is also an un_transform method to use.
    def transform(self, direction: Direction, type:int = 0):
        return Field(field=transform_dict[direction][type](self))
    
    def un_transform(self, direction: Direction):
        return self.transform(direction, 1)

    def slide_rows_left(self):
        for x in range(self.size):
            self[x] = self.slide_row_left(self[x])

    def slide_row_left(self, row: list):
        slide = [num for num in row if num]
        pairs = []
        for idx, num in enumerate(slide):
            if idx == len(slide)-1:
                pairs.append(num)
                break
            elif num == slide[idx+1]:
                pairs.append(num+1)
                slide[idx+1] = None
            else:
                pairs.append(num)  # Even if not pair you must append
        slide = [pair for pair in pairs if pair] 
        slide.extend([0] * (len(row) - len(slide)))
        return slide

    # updates the field at a coordinate with a value, as a copy by default
    def update_at_coord(self, coord, val: int, as_copy:bool = True):
        fld = Field(field=self) if as_copy else self
        x,y = coord
        fld[x][y] = val
        return fld

    #TODO: this enumerate method doesn't really belong here; we need to find a better place for it

    # Saves to possible_outcomes an array of possible outcomes with their associated likelihood
    #               (field: Field, chance: Float)
    # 
    # Returns the direction that will produce the highest expected score.
    def enumerate_possible_outcomes(self, d: Direction, FOUR_CHANCE: float):
        outcomes = []
        # skip any outcomes that don't change anything
        fld = Field(field=self).slide(d) # fld is the base field that represents a direction action,
        if fld == self:                  # but will branch into the different possible outcomes
            return outcomes

        # get all added square chances
        chances = [(2, FOUR_CHANCE), (1, 1 - FOUR_CHANCE)]
        return [(fld.update_at_coord(coord, val), chance) \
                 for coord in fld.get_empty_squares()     \
                 for val, chance in chances]

    # square values are stored as powers of 2. So:
    # 1 = 2, 2 = 4, 3 = 8, and so on. That way when
    # square combine, they can increment without
    # needing to be aware of their actual values.
    @staticmethod
    def score_value(square_value):
        return 1 << square_value

    def get_score(self):
        return sum([self.score_value(item) for row in self for item in row])

    # an even more naive score calculation that performs extremely well
    def get_sum(self):
        return sum(map(sum, self))