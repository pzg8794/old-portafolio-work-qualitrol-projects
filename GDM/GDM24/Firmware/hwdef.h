/* 
 * File:   hwdef.h
 * Author: mark
 *
 * Created on July 22, 2013, 3:30 PM
 */

#ifndef HWDEF_H
#define	HWDEF_H

#ifdef	__cplusplus
extern "C" {
#endif

#define RELAY_ON 1
#define RELAY_OFF 0

#define OUTPUT_DISABLE PORTBbits.RB2
#define LED_GREEN PORTDbits.RD0
#define K1_DRV PORTEbits.RE0
#define K2_DRV PORTEbits.RE1
#define K3_DRV PORTEbits.RE2
#define DAC_CS_BAR PORTBbits.RB4
#define SEN_CS_BAR PORTBbits.RB5
#define SEN_DATA_READY PORTBbits.RB0

#define NOP _asm nop _endasm;


#define LED_ON 0
#define LED_OFF 1

#ifdef	__cplusplus
}
#endif

#endif	/* HWDEF_H */

