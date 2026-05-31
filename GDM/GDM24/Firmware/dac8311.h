/* 
 * File:   dac8311.h
 * Author: mark
 *
 * Created on July 22, 2013, 2:55 PM
 */

#ifndef DAC8311_H
#define	DAC8311_H

#ifdef	__cplusplus
extern "C" {
#endif

void WriteDac(unsigned short value);
void  maCycle(void);

#ifdef	__cplusplus
}
#endif

#endif	/* DAC8311_H */

