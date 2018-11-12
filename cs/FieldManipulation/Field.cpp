#include "Field.h"

#define BITS_PER_SQ 4
#define BITS_PER_DIM 16
#define FIELD_DIM 4
#define FILLED_SQ 0xFF
#define REVERSE_OFFSET 64 - BITS_PER_SQ
#define FIELD_LEN sizeof(unsigned long)
#define offset(row, col) (row * FIELD_DIM + col) * BITS_PER_SQ

unsigned long data = 0;

Field::Field() { }
Field::~Field()
{
	
}

//array must have length of 16
unsigned long Field::GetFieldID(unsigned char *dataArray)
{
	unsigned long fieldID;
	for (int i = 0; i < 4; i++)
		for (int j = 0; j < 4; j++)
		{
			fieldID <<= 4; //bitshift left 4 bits
			fieldID |= dataArray[i*FIELD_DIM+j];
		}
	return fieldID;
}

//sets a given row/col
//note: only the last 4 bits of val will be used
void Field::Set(int row, int col, char val)
{	
	int offset = offset(row, col);
	unsigned long setter = val << offset;
	data &= ~(FILLED_SQ << offset); //clear bits
	data |= setter;
}

char Field::Get(int row, int col)
{	
	char retVal = *(char*)(&retVal + offset(row, col) - BITS_PER_SQ) & 0x0F; //get the value by memory position and clear extra bits
	return retVal;	
}

void Field::slideRowLeft(int row) //oops this is exactly just getting the rows with 0s
{
	char write_pos = 0;
	for (char x = 0; x < FIELD_DIM; x++)
	{
		char sq = Get(row, x);
		if (sq != 0) {
			if (write_pos < x) //slide the value over, setting the current square to 0
				Set(row, write_pos, sq);
			write_pos++;
		}
	}

	//see if remaining squares need to be set to 0
	char sq_zero_count = FIELD_DIM - write_pos - 1;
	if (sq_zero_count == 0) return;

	//get a bit mask for things to set to 0  //TODO: perf test on this vs just looping through the last couple squares
	unsigned long mask = (1 << (BITS_PER_SQ * sq_zero_count)) - 1; //this is the 2s complement
	mask <<= BITS_PER_DIM * (FIELD_DIM - row); //offset to the correct position

	//set the remaining squares to 0
	data ^= mask;
}

void Field::slideLeft()
{
	for (int x = 0; x < FIELD_DIM; x++)
		slideRowLeft(x);
}
