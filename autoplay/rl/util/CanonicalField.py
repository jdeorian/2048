from util.ArrayMagic import invert, transpose, flatten, flip

#returns both the fieldID as well as the canonical transformation of the field 
def get_canonical_fieldID(field):
    fields = { get_fieldID(f):f for f in [field,
                                          flip(invert(field)),
                                          transpose(invert(field)),
                                          invert(transpose(field))] }
    can_ID = min(fields.keys())
    return can_ID, fields[can_ID]
    
#TODO: find a faster way to do this
def to_hex_digit(d: int):
    return "%0.1X" % d

def get_fieldID(field):
    return sum([x*16**idx for idx, x in enumerate(flatten(field))])

def get_fieldID_Bitshift(field):
    fieldID = 0   
    for x in range(4):
        for y in range(4):
            fieldID <<= 4
            fieldID += field[x][y]
    return fieldID
