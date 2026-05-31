
#include <compiler.h>

#include "version.h"
#include "globals.h"
#include "iirfilter.h"

#include "hwdef.h"
#include "dac8311.h"
#include "relaystate.h"


struct RelayType relay[3];

void RelayInit(void)
{
    int i;

    for ( i = 0 ; i < 3 ; i++ )
    {
        relay[i].trip = 50.0;
        relay[i].hyst = 5.0;
        relay[i].delay = 1;
        relay[i].state = RS_IDLE;
        relay[i].timer = 0;
        relay[i].failsafe = R_NORMAL;
    }

    relay[0].trip = 22.5;
    relay[1].trip = 60.0;
    relay[2].trip = 120.0;

    relay[0].delay = 0;
    relay[1].delay = 0;
    relay[2].delay = 0;
}

void RelayInit2(void)
{
    relay[0].failsafe = sysConfig & FAILSAFE_MASK_1;
    relay[1].failsafe = sysConfig & FAILSAFE_MASK_2;
    relay[2].failsafe = sysConfig & FAILSAFE_MASK_3;

    K1_DRV = relay[0].failsafe ? RELAY_ON : RELAY_OFF;
    K2_DRV = relay[1].failsafe ? RELAY_ON : RELAY_OFF;
    K3_DRV = relay[2].failsafe ? RELAY_ON : RELAY_OFF;
}

void DoRelayStates(void)
{
    int i;

    for ( i = 0 ; i < 3 ; i++ )
    {
        // check the delay timers.
        if ( relay[i].timer )
            relay[i].timer--;

        switch (relay[i].state)
        {
            case RS_IDLE:
                if ( percentFull < (relay[i].trip*100.0/gas_ma_max) )
                {
                    relay[i].state = RS_DELAY;
                    relay[i].timer = relay[i].delay;
                }
                break;
            case RS_DELAY:
                if ( percentFull > (relay[i].trip*100.0/gas_ma_max) )
                {
                    relay[i].state = RS_IDLE;
                    relay[i].timer = 0;;
                }
                if ( relay[i].timer == 0 )
                {
                    relay[i].state = RS_TRIPPED;
                }
                break;
            case RS_TRIPPED:
                if ( percentFull > (( relay[i].trip + relay[i].hyst)*100.0/gas_ma_max) )
                {
                    relay[i].state = RS_IDLE;
                    relay[i].timer = 0;;
                }
                break;
        }
    }

    switch ( relay[0].state )
    {
        case RS_IDLE:
        case RS_DELAY:
            K1_DRV = relay[0].failsafe ? RELAY_ON : RELAY_OFF;
            break;
        case RS_TRIPPED:
            K1_DRV = relay[0].failsafe ? RELAY_OFF : RELAY_ON;
            break;
    }

    switch ( relay[1].state )
    {
        case RS_IDLE:
        case RS_DELAY:
            K2_DRV = relay[1].failsafe ? RELAY_ON : RELAY_OFF;
            break;
        case RS_TRIPPED:
            K2_DRV = relay[1].failsafe ? RELAY_OFF : RELAY_ON;
            break;
    }

    switch ( relay[2].state )
    {
        case RS_IDLE:
        case RS_DELAY:
            K3_DRV = relay[2].failsafe ? RELAY_ON : RELAY_OFF;
            break;
        case RS_TRIPPED:
            K3_DRV = relay[2].failsafe ? RELAY_OFF : RELAY_ON;
            break;
    }

}

int relayCycleState = 7;

void relayCycle(void)
{
    relayCycleState = 0x07 & ( relayCycleState + 1);

    switch (relayCycleState )
    {
        case 0:
            K1_DRV = 0;NOP;
            K2_DRV = 0;NOP;
            K3_DRV = 0;NOP;
            break;
        case 1:
            K1_DRV = 1;NOP;
            K2_DRV = 0;NOP;
            K3_DRV = 0;NOP;
            break;
        case 2:
            K1_DRV = 0;NOP;
            K2_DRV = 1;NOP;
            K3_DRV = 0;NOP;
            break;
        case 3:
            K1_DRV = 1;NOP;
            K2_DRV = 1;NOP;
            K3_DRV = 0;NOP;
            break;
        case 4:
            K1_DRV = 0;NOP;
            K2_DRV = 0;NOP;
            K3_DRV = 1;NOP;
            break;
        case 5:
            K1_DRV = 1;NOP;
            K2_DRV = 0;NOP;
            K3_DRV = 1;NOP;
            break;
        case 6:
            K1_DRV = 0;NOP;
            K2_DRV = 1;NOP;
            K3_DRV = 1;NOP;
            break;
        case 7:
            K1_DRV = 1;NOP;
            K2_DRV = 1;NOP;
            K3_DRV = 1;NOP;
            break;
        default:
            relayCycleState = 0;
            break;

    }
}
