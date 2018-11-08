#pragma once
class Field
{
	unsigned long data;
public:
	Field();
	~Field();
	unsigned long GetFieldID(unsigned char *dataArray);
	unsigned long CanonicalFieldID();
	unsigned long Slide(unsigned char*);
	void Set(int, int, char);
	char Get(int row, int col);
};

