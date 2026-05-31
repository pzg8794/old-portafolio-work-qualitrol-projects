#include "version.h"

#include "string.h"
#include "stdlib.h"

#include "globals.h"
#include "relaystate.h"

#include "sercmd.h"
#include "adc.h"
#include "ee.h"
#include "eeprom.h"

#include "./USB/usb_function_cdc.h"

char serTxBuff[SERTXBUFFSIZE];	// 7+7+2+1 + extra

char serRxBuff[SERRXBUFFSIZE];	// 7+7+2+1 + extra
char serRxIndex = 0;

unsigned char serRXtimer = 8;

union
{
    unsigned long long  llval;
    unsigned long	lval[2];
    unsigned int	ival[4];
    unsigned char	bval[8];
    float		fval[2];
} uval;



void SerCmdProc(void)
{
    int passresult = 0;

	if (( serRxIndex >= 10 ) && (serRxBuff[0] == 0xff ))
	{
		char write = 0;

		if ( serRxBuff[1] & 0x80 )	// save the write flag
			write = 1;
		serRxBuff[1] &= 0x7f;		// clear the flag

		if ( write )
		{
                    uval.bval[0] = serRxBuff[2];
                    uval.bval[1] = serRxBuff[3];
                    uval.bval[2] = serRxBuff[4];
                    uval.bval[3] = serRxBuff[5];
                    uval.bval[4] = serRxBuff[6];
                    uval.bval[5] = serRxBuff[7];
                    uval.bval[6] = serRxBuff[8];
                    uval.bval[7] = serRxBuff[9];

                    switch ( serRxBuff[1] )
                    {
                    case REG_CONFIG:
                        sysConfig = uval.lval[0];
                        eepromWriteLong( EE_sysConfig,&sysConfig);
                        break;
                    case REG_MA_OFFSET:
                        ma_offset = uval.fval[0];
                        eepromWriteLong( EE_ma_offset,&ma_offset);
                        break;
                    case REG_MA_GAIN:
                        ma_gain = uval.fval[0];
                        eepromWriteLong(EE_ma_gain,&ma_gain);
                        break;
                    case REG_PRESSURE_OFFSET:
                        pressure_offset = uval.fval[0];
                        eepromWriteLong(EE_pressure_offset,&pressure_offset);
                        break;
                    case REG_PRESSURE_GAIN:
                        pressure_gain = uval.fval[0];
                        eepromWriteLong(EE_pressure_gain,&pressure_gain);
                        break;
                    case REG_PRESSURE_FILTER:
                        press_ir = uval.fval[0];
                        eepromWriteLong(EE_press_ir,&press_ir);
                        break;
                    case REG_TEMP_FILTER:
                        temp_ir = uval.fval[0];
                        eepromWriteLong(EE_temp_ir,&temp_ir);
                        break;

                    case REG_DAC_CALIBRATION:
                        DACout = uval.ival[0];
                        DACtimeout = 20;
                        break;
                    case REG_CAL_TEMP:
                        GasConstantTemp = uval.fval[0];
                        eepromWriteLong(EE_GasConstantTemp,&GasConstantTemp);
                        break;
                    case REG_GAS_MA_MIN:
                        gas_ma_min = uval.fval[0];
                        eepromWriteLong(EE_gas_ma_min,&gas_ma_min);
                        break;
                    case REG_GAS_MA_MAX:
                        gas_ma_max = uval.fval[0];
                        eepromWriteLong(EE_gas_ma_max,&gas_ma_max);
                        break;
                    case REG_JOSHS_CONSTANT:
                        JoshsConstant = uval.fval[0];
                        eepromWriteLong(EE_JoshsConstant,&JoshsConstant);
                        break;
                    case REG_GAUGE_OFFSET:
                         gauge_offset = uval.fval[0];
                         eepromWriteLong(EE_GAUGE_OFFSET,&gauge_offset);
                         break;
                    case REG_SENSOR_OFFSET:
                         sensor_offset = uval.fval[0];
                         eepromWriteLong(EE_SENSOR_OFFSET,&sensor_offset);
                         break;
                    case REG_RELAY_SETPOINT_1:
                        relay[0].trip = uval.fval[0];
                        eepromWriteLong(EE_SETPOINT_1,&relay[0].trip);
                        break;
                    case REG_RELAY_SETPOINT_2:
                        relay[1].trip = uval.fval[0];
                        eepromWriteLong(EE_SETPOINT_2,&relay[1].trip);
                        break;
                    case REG_RELAY_SETPOINT_3:
                        relay[2].trip = uval.fval[0];
                        eepromWriteLong(EE_SETPOINT_3,&relay[2].trip);
                        break;
                    case REG_RELAY_HYST_1:
                        relay[0].hyst = uval.fval[0];
                        eepromWriteLong(EE_HYST_1,&relay[0].hyst);
                        break;
                    case REG_RELAY_HYST_2:
                        relay[1].hyst = uval.fval[0];
                        eepromWriteLong(EE_HYST_2,&relay[1].hyst);
                        break;
                    case REG_RELAY_HYST_3:
                        relay[2].hyst = uval.fval[0];
                        eepromWriteLong(EE_HYST_3,&relay[2].hyst);
                        break;
                    case REG_RELAY_DELAY_1:
                        relay[0].delay = uval.lval[0];
                        eepromWriteLong(EE_DELAY_1,&relay[0].delay);
                        break;
                    case REG_RELAY_DELAY_2:
                        relay[1].delay = uval.lval[0];
                        eepromWriteLong(EE_DELAY_2,&relay[1].delay);
                        break;
                    case REG_RELAY_DELAY_3:
                        relay[2].delay = uval.lval[0];
                        eepromWriteLong(EE_DELAY_3,&relay[2].delay);
                        break;
                    case REG_PASSWORD_SIM:
                        strncpy((char *)&pass_simulate,(char*)&uval.bval[0],8);
                        eepromWriteLong(EE_PASS_SIM_1,&pass_simulate.l[0]);
                        eepromWriteLong(EE_PASS_SIM_2,&pass_simulate.l[1]);
                        break;
                    case REG_PASSWORD_CAL:
                        strncpy((char *)&pass_calibrate,(char *)uval.bval,8);
                        eepromWriteLong(EE_PASS_SIM_1,&pass_calibrate.l[0]);
                        eepromWriteLong(EE_PASS_SIM_2,&pass_calibrate.l[1]);
                        break;
                    case REG_PASSWORD_CUS:
                        strncpy((char *)&pass_cust,(char *)uval.bval,8);
                        eepromWriteLong(EE_PASS_SIM_1,&pass_cust.l[0]);
                        eepromWriteLong(EE_PASS_SIM_2,&pass_cust.l[1]);
                        break;
                    case REG_PASSWORD_FAC:
                        strncpy((char *)&pass_factory,(char *)uval.bval,8);
                        eepromWriteLong(EE_PASS_CALIBRATE_1,&pass_factory.l[0]);
                        eepromWriteLong(EE_PASS_CALIBRATE_2,&pass_factory.l[1]);
                        break;
                    case REG_SERIAL_NUMBER:
                        strncpy((char *)&serial_number,(char *)uval.bval,8);
                        eepromWriteLong(EE_SERIAL_NUMBER_1,&serial_number.l[0]);
                        eepromWriteLong(EE_SERIAL_NUMBER_2,&serial_number.l[1]);
                        break;
                    case REG_MANF_DATE:
                        strncpy((char *)&manuf_date,(char *)uval.bval,8);
                        eepromWriteLong(EE_MANUF_DATE_1,&manuf_date.l[0]);
                        eepromWriteLong(EE_MANUF_DATE_2,&manuf_date.l[1]);
                        break;
                    case REG_PASSWORD_FIND:
                        if ( 0 == strncmp((char*)uval.bval,(char*)&pass_simulate,8))
                            passresult = 2;
                        if ( 0 == strncmp((char*)uval.bval,(char*)&pass_calibrate,8))
                            passresult = 3;
                        if ( 0 == strncmp((char*)uval.bval,(char*)&pass_cust,8))
                            passresult = 4;
                        if ( 0 == strncmp((char*)uval.bval,(char*)&pass_factory,8))
                            passresult = 5;
                        break;

                    case REG_IO_SIM_TIMER:
                        sim_timer = uval.ival[0];
                        break;
                    case REG_IO_SIM_PRESSURE:
                        sim_pressure = uval.fval[0];
                        break;
                    case REG_TEMP_GAIN_LOW:
                        tempGainLow = uval.fval[0];
                        eepromWriteLong(EE_TEMP_GAIN_LOW,&tempGainLow);
                        break;
                    case REG_TEMP_GAIN_HIGH:
                        tempGainHigh = uval.fval[0];
                        eepromWriteLong(EE_TEMP_GAIN_HIGH,&tempGainHigh);
                        break;
                    case REG_TEMP_SLOPE_LOW:
                        tempOffsetLow = uval.fval[0];
                        eepromWriteLong(EE_TEMP_SLOPE_LOW,&tempOffsetLow);
                        break;
                    case REG_TEMP_SLOPE_HIGH:
                        tempOffsetHigh = uval.fval[0];
                        eepromWriteLong(EE_TEMP_SLOPE_HIGH,&tempOffsetHigh);
                        break;
                    }
		}

		uval.llval = 0;	// clear it
		switch ( serRxBuff[1] )
		{
                    case REG_CONFIG:
                        uval.lval[0] = sysConfig;
                        break;
                    case REG_MA_OFFSET:
                        uval.fval[0] = ma_offset;
                        break;
                    case REG_MA_GAIN:
                        uval.fval[0] = ma_gain;
                        break;
                    case REG_PRESSURE_OFFSET:
                        uval.fval[0] = pressure_offset;
                        break;
                    case REG_PRESSURE_GAIN:
                        uval.fval[0] = pressure_gain;
                        break;
                    case REG_PRESSURE_FILTER:
                        uval.fval[0] = press_ir;
                        break;
                    case REG_TEMP_FILTER:
                        uval.fval[0] = temp_ir;
                        break;
                    case REG_GAS_MA_MAX:
                        uval.fval[0] = gas_ma_max;
                        break;
                    case REG_DAC_CALIBRATION:
                        uval.ival[0] = DACout;
                        break;
                    case REG_CAL_TEMP:
                        uval.fval[0] = GasConstantTemp;
                        break;
                    case REG_GAS_MA_MIN:
                        uval.fval[0] = gas_ma_min;
                        break;
                    case REG_JOSHS_CONSTANT:
                        uval.fval[0] = JoshsConstant;
                        break;
                    case REG_GAUGE_OFFSET:
                        uval.fval[0] = gauge_offset;
                        break;
                    case REG_SENSOR_OFFSET:
                        uval.fval[0] = sensor_offset;
                        break;
                    case REG_PERCENT_FULL:
                        uval.fval[0] = percentFull;
                        break;
                    case REG_MA_OUTPUT:
                        uval.fval[0] = ma_out;
                        break;
                    case REG_RAW_ADC:
                        uval.ival[0] = pressS1;
                        break;
                    case REG_PRESSURE_FILTERED:
                        uval.fval[0] = pressure;
                        break;
                    case REG_RAW_TEMP:
                        uval.ival[0] = tempS1;
                        break;
                    case REG_TEMPERATURE_FILTERED:
                        uval.fval[0] = tempC;
                        break;

                    case REG_RELAY_SETPOINT_1:
                        uval.fval[0] = relay[0].trip;
                        break;
                    case REG_RELAY_SETPOINT_2:
                        uval.fval[0] = relay[1].trip;
                        break;
                    case REG_RELAY_SETPOINT_3:
                        uval.fval[0] = relay[2].trip;
                        break;

                    case REG_RELAY_HYST_1:
                        uval.fval[0] = relay[0].hyst;
                        break;
                    case REG_RELAY_HYST_2:
                        uval.fval[0] = relay[1].hyst;
                        break;
                    case REG_RELAY_HYST_3:
                        uval.fval[0] = relay[2].hyst;
                        break;

                    case REG_RELAY_DELAY_1:
                        uval.lval[0] = relay[0].delay;
                        break;
                    case REG_RELAY_DELAY_2:
                        uval.lval[0] = relay[1].delay;
                        break;
                    case REG_RELAY_DELAY_3:
                        uval.lval[0] = relay[2].delay;
                        break;

                    case REG_SERIAL_NUMBER:
                        strncpy((char *)uval.bval,(char *)&serial_number,8);
                        break;
                    case REG_MANF_DATE:
                        strncpy((char *)uval.bval,(char *)&manuf_date,8);
                        break;
                    case REG_REV_LEVEL:
                        strcpypgm2ram((char*)uval.bval,(const rom far char *)REV_LEVEL);
                        break;

                    case REG_PASSWORD_FIND:
                        uval.llval = passresult;
                        break;
                    case REG_IO_SIM_PRESSURE:
                        uval.fval[0] = sim_pressure;
                        break;
                    case REG_TEMP_GAIN_LOW:
                        uval.fval[0] = tempGainLow;
                        break;
                    case REG_TEMP_GAIN_HIGH:
                        uval.fval[0] = tempGainHigh;
                        break;
                    case REG_TEMP_SLOPE_LOW:
                        uval.fval[0] = tempOffsetLow;
                        break;
                    case REG_TEMP_SLOPE_HIGH:
                        uval.fval[0] = tempOffsetHigh;
                        break;
                }
		serTxBuff[0] = serRxBuff[0];
		serTxBuff[1] = serRxBuff[1];
		serTxBuff[2] = uval.bval[0];
		serTxBuff[3] = uval.bval[1];
		serTxBuff[4] = uval.bval[2];
		serTxBuff[5] = uval.bval[3];
		serTxBuff[6] = uval.bval[4];
		serTxBuff[7] = uval.bval[5];
		serTxBuff[8] = uval.bval[6];
		serTxBuff[9] = uval.bval[7];
                serTxBuff[10] = 0;			// extra to make sure all is flushed out
		
                putUSBUSART(serTxBuff,11);
		
		serRxBuff[0] = 0;
		serRxIndex = 0;
	}
}


