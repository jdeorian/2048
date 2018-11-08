#include "Field.h"

#define BITS_PER_SQ 4
#define FIELD_DIM 4
#define FILLED_SQ 0xFF
#define REVERSE_OFFSET 64 - BITS_PER_SQ
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

void slideLeft(int row)
{
	int read_pos = FIELD_DIM * BITS_PER_SQ* row;
	int write_pos= read_pos;
	for (int x = 0; x < FIELD_DIM; x++)
	{
		
	}
}