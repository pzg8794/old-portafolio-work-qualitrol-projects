
#include <compiler.h>


#include "version.h"
#include "globals.h"
#include "iirfilter.h"

#include "hwdef.h"
#include "dac8311.h"

static    unsigned int cmd;
void WriteDac(unsigned short value)
{
//    unsigned int cmd;
    unsigned char dummy;

    SSP1CON1 = 0 ; NOP;
    SSP1CON1bits.SSPM1 = 1; NOP;
    SSP1CON1bits.CKP = 1;
    SSP1STAT = 0;NOP;
    SSP1STATbits.CKE = 1; NOP;
    SSP1ADD  = 5;NOP;
    SSP1CON1bits.SSPEN = 1;NOP;

    DAC_CS_BAR  = 0; NOP;

    dummy = SSP1BUF;  // clear spi receive buffer

    cmd = 0x3fff & value;   // PD bits to zero

    SSP1BUF = 0xff & (cmd >> 8 );   // high byte

    while ( !SSP1STATbits.BF )
       ;

    dummy = SSP1BUF;  // clear spi receive buffer
    SSP1BUF = 0xff & cmd;           // low byte

    while ( !SSP1STATbits.BF )
       ;

    DAC_CS_BAR = 1; NOP;
    OUTPUT_DISABLE = 0; NOP;
}


int maCycleState = 0;
void maCycle(void)
{
    switch (maCycleState)
    {
        case 0:
            WriteDac(0x0BA0);
            maCycleState = 1;
            break;
        case 1:
            WriteDac(0x3A20);
            maCycleState= 0;
            break;
        default:
            maCycleState = 0;
    }

}