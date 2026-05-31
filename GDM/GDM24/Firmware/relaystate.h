/* 
 * File:   relaystate.h
 * Author: mark
 *
 * Created on July 23, 2013, 10:57 AM
 */

#ifndef RELAYSTATE_H
#define	RELAYSTATE_H

#ifdef	__cplusplus
extern "C" {
#endif

enum   RS_TYPE { RS_IDLE, RS_DELAY, RS_TRIPPED};
enum   R_TYPE  { R_NORMAL, R_FAILSAFE };

extern struct RelayType
{
//    float density;
    float trip;
    float hyst;
    int   delay;
    int   timer;
    enum  R_TYPE  failsafe;
    enum  RS_TYPE state;

} relay[3];



void RelayInit(void);
void RelayInit2(void);
void DoRelayStates(void);
void relayCycle(void);

#ifdef	__cplusplus
}
#endif

#endif	/* RELAYSTATE_H */

