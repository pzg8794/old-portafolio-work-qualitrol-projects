#include "version.h"
#include "globals.h"

#include "tmp123.h"


// sensor is on port 0
#define TMP123_CS		0x40
#define TMP123_SCLK		0x20
#define TMP123_SDOUT	0x10


void TMP123_start(void)
{
	PRT0DR |=  TMP123_CS;	// clock high
												
// wait for conversion 320ms in the main loop
// it is expected that this wait will overlap with the humidity wakeup.
}

unsigned int TMP123_fetch(void)
{
	signed int result;
	int i;

	PRT0DR &= ~TMP123_CS;	// clock low

// ready for conversion read
	result = 0;
  
	for( i = 0; i < 16; i++ )
	{
		result <<= 1;

 		PRT0DR &= ~TMP123_SCLK;		// clock low

		if( PRT0DR & TMP123_SDOUT )
		{
			result |= 1;
		}

		PRT0DR |= TMP123_SCLK;		// clock high
	}

// lower three bits don't count
	result /=8 ;

	return result;			
}



