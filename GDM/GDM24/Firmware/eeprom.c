
#include <compiler.h>
#include "version.h"
#include "globals.h"
#include "eeprom.h"

//
// This routine writes one byte to the internal pic eeprom memory
//
void eepromWrite(unsigned char address,unsigned char data)
{
	EEADR = address;				// Set address
	EEDATA = data;					// Set data
	EECON1bits.EEPGD = 0;			// Data memory
	EECON1bits.CFGS = 0;			// EEPROM
	EECON1bits.WREN = 1;			// Enable write
	INTCONbits.GIE = 0;				// Disable interrupts
	EECON2 = 0x55;					// Unlock
	EECON2 = 0xAA;
	EECON1bits.WR = 1;				// Begin write
	INTCONbits.GIE = 1;				// Enable Interrupts
	while(PIR2bits.EEIF != 1);		// Wait until write completed
	PIR2bits.EEIF = 0;				// Clear interrupt flag
	EECON1bits.WREN = 0;			// Disable write
}

//
// This routine reads one byte from the internal pic eeprom memory
//
unsigned char eepromRead(unsigned char address)
{
	EEADR = address;				// Set address
	EECON1bits.EEPGD = 0;			// Data memory
	EECON1bits.CFGS = 0;			// EEPROM
	EECON1bits.RD = 1;				// Enable read
	return EEDATA;					// Return data
}

union converter
{
    unsigned char bytes[4];
    unsigned long longs;
};

void eepromWriteLong(unsigned char address ,void * data)
{
    union converter result;


    eepromWrite(address  ,((union converter *)data)->bytes[0] & 0xff);
    eepromWrite(address+1,((union converter *)data)->bytes[1] & 0xff);
    eepromWrite(address+2,((union converter *)data)->bytes[2] & 0xff);
    eepromWrite(address+3,((union converter *)data)->bytes[3] & 0xff);
}

unsigned long eepromReadLong(unsigned char address)
{
    union
    {
        unsigned char bytes[4];
        unsigned long longs;
        float floats;
    }result;

    result.bytes[0] = eepromRead(address  );
    result.bytes[1] = eepromRead(address+1);
    result.bytes[2] = eepromRead(address+2);
    result.bytes[3] = eepromRead(address+3);

    return result.longs;
    
}

float eepromReadFloat(unsigned char address)
{
    union
    {
        unsigned char bytes[4];
        unsigned long longs;
        float floats;
    }result;

    result.bytes[0] = eepromRead(address  );
    result.bytes[1] = eepromRead(address+1);
    result.bytes[2] = eepromRead(address+2);
    result.bytes[3] = eepromRead(address+3);

    return result.floats;

}