def array_2d_copy(field):
    return [row[:] for row in field]

def unflatten(state, size: int): # state is an array of values, size is square size
    return [state[x:x+size] for x in range(0, len(state), size)]

#flatten 2d array to 1d array
def flatten(state):
    return [item for row in state for item in row]

# transpose a 2d array
def transpose(field):
    return [list(row) for row in zip(*field)]

# invert a 2d array
def invert(field):
    return [row[::-1] for row in field]

def flip(field):
    return [field[len(field) - x - 1] for x in range(len(field))]