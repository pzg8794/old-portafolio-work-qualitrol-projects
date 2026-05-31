

void eepromWrite(unsigned char address,unsigned char data);
unsigned char eepromRead(unsigned char address);


union converter
{
    unsigned char bytes[4];
    unsigned long longs;
};

void eepromWriteLong(unsigned char address ,void * result);
unsigned long eepromReadLong(unsigned char address);
float eepromReadFloat(unsigned char address);



