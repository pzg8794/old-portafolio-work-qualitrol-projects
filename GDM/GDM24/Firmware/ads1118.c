
#include <compiler.h>

#include "version.h"
#include "globals.h"
#include "sercmd.h"
#include "hwdef.h"


#include "ads1118.h"

static unsigned int timeout;
static unsigned int delay;

#define CS_DELAY 0x10
#define SEN_TIMEOUT 0x3000


static     unsigned int cmd;
short ReadPressure(void)
{
    unsigned short value;
    unsigned char result[4];
    unsigned char dummy;
    unsigned short reply;

    SSP1CON1 = 0 ; NOP;
    SSP1CON1bits.SSPM1 = 1; NOP;
    SSP1CON1bits.CKP = 0;
    SSP1STAT = 0;NOP;
    SSP1STATbits.CKE = 0; NOP;
    SSP1ADD  = 5;NOP;
    SSP1CON1bits.SSPEN = 1;NOP;

    SEN_CS_BAR  = 0;

    dummy = SSP1BUF;  // clear out the spi receiver

    // set for pressure, convert later.
    cmd = 0x8B8B;

    SSP1BUF = 0xff & (cmd >> 8 );   // high byte
    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[0] = SSP1BUF;

    SSP1BUF = 0xff & cmd;           // low byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[1] = SSP1BUF;
    SEN_CS_BAR = 1;

    delay = CS_DELAY;
        while (--delay);
// now read

    SEN_CS_BAR  = 0;

    cmd = 0x8000;

    timeout = 0;
    while (( SEN_DATA_READY ) && (timeout <  SEN_TIMEOUT))     // wait for new conversion
        timeout++ ;

    SSP1BUF = 0xff & (cmd >> 8 );   // high byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[2] = SSP1BUF;

    SSP1BUF = 0xff & cmd;           // low byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[3] = SSP1BUF;
    SEN_CS_BAR = 1;


    reply = result[2];
    reply <<= 8;
    reply |= result[3];

    return reply;

}

short ReadTemperature(void )
{
//    unsigned int cmd;
    unsigned short value;
    unsigned char result[4];
    unsigned char dummy;
    short reply;

    SSP1CON1 = 0 ; NOP;
    SSP1CON1bits.SSPM1 = 1; NOP;
    SSP1CON1bits.CKP = 0;
    SSP1STAT = 0;NOP;
    SSP1STATbits.SMP = 0;
    SSP1STATbits.CKE = 0; NOP;
    SSP1ADD  = 5;NOP;
    SSP1CON1bits.SSPEN = 1;NOP;

    SEN_CS_BAR  = 0;

    dummy = SSP1BUF;  // clear out the spi receiver

    // set for temperature, convert later.
    cmd = 0x8B9B;

    SSP1BUF = 0xff & (cmd >> 8 );   // high byte
    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[0] = SSP1BUF;

    SSP1BUF = 0xff & cmd;           // low byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[1] = SSP1BUF;
    SEN_CS_BAR = 1;

    delay = CS_DELAY;
        while (--delay);


// now read

    SEN_CS_BAR  = 0;

    cmd = 0x8000;

    timeout = 0;
    while (( SEN_DATA_READY ) && (timeout <  SEN_TIMEOUT))     // wait for new conversion
        timeout++ ;

    SSP1BUF = 0xff & (cmd >> 8 );   // high byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[2] = SSP1BUF;

    SSP1BUF = 0xff & cmd;           // low byte

    while ( !SSP1STATbits.BF )      // wait for send
       ;

    result[3] = SSP1BUF;
    SEN_CS_BAR = 1;


    reply = result[2];
    reply <<= 8;
    reply |= result[3];

    // answer is left justified. bottom 2 bits don't count.
    reply /= 4;

    return reply;
}


