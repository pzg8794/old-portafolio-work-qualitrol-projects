#include "version.h"
#include "globals.h"
#include "sercmd.h"


int ParseUsb(char *s,int n)
{
    int i;
    
    if ( n )
    {
        for(i=0;i<n;i++)
        {
            if ( serRxIndex >= SERRXBUFFSIZE )
                serRxIndex = SERRXBUFFSIZE-1;

            serRxBuff[serRxIndex] = s[i];

            if ((serRxIndex == 0 ) && (serRxBuff[0] != 0xff ))
            {
                    ; // probably in a break;
            }
            else
            {
                serRxIndex++;
                serRXtimer = 0x04;

                SerCmdProc();
            }
        }
    }
    return 0;
}