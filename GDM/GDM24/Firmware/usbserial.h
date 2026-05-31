/* 
 * File:   usbserial.h
 * Author: mark
 *
 * Created on June 28, 2013, 10:05 AM
 */

#ifndef USBSERIAL_H
#define	USBSERIAL_H

#ifdef	__cplusplus
extern "C" {
#endif


#define SERTXBUFFSIZE 12
#define SERRXBUFFSIZE 10

extern char serTxBuff[SERTXBUFFSIZE];	// 7+7+2+1 + extra

extern char serRxBuff[SERRXBUFFSIZE];	// 7+7+2+1 + extra
extern char serRxIndex;

extern unsigned char serRXtimer;



int ParseUsb(char *s,int n);


// command format

// byte 0	- 0xff
// byte 1	- register
//			- top bit is 0 for read, 1 for write and read back
// byte 2	- byte 1 of data lsb
// byte 3	- byte 2 of data
// byte 4	- byte 3 of data
// byte 5	- byte 4 of data msb

// reply format

// byte 0	- 0xff
// byte 1	- register
//			- top bit is 0 for read, 1 for write and read back
//			- for reads the data is ignored
// byte 2	- byte 1 of data lsb
// byte 3	- byte 2 of data
// byte 4	- byte 3 of data
// byte 5	- byte 4 of data msb


// registers
//
// all registers are 32 bits
// could be long, bits, or float

// 0x00 status .. read only
// 0x01 config bits
// 0x02 ma offset
// 0x03 ma gain
// 0x04 pressure offset
// 0x05 pressture gain
// 0x06 press filter
// 0x07 temp filter
// 0x08 Gas Calibration Constant
// 0x09	DAC output value, writable for calibration, resumes after 10seconds

// read only registers
// 0x10 percent full reading
// 0x11 ma output
// 0x12 raw ADC reading
// 0x13 filtered ADC reading
// 0x14 raw temp reading
// 0x15 filtered temperuture
// 0x16 temp rise 1
// 0x17 temp rise 2
// 0x18 temp rise 3






#ifdef	__cplusplus
}
#endif

#endif	/* USBSERIAL_H */

