

#define SERTXBUFFSIZE 12
#define SERRXBUFFSIZE 10

extern char serTxBuff[SERTXBUFFSIZE];	// 7+7+2+1 + extra

extern char serRxBuff[SERRXBUFFSIZE];	// 7+7+2+1 + extra
extern char serRxIndex;

extern unsigned char serRXtimer;

void SerCmdProc(void);
void SerCmdReply(void);

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



#define REG_SERIAL_NUMBER	0x31	/* 1 reg */
#define REG_MANF_DATE		0x32	/* 1 reg */
#define REG_REV_LEVEL		0x33	/* 1 reg */




#define REG_PASSWORD_SIM	0x41	/* 1 reg */
#define REG_PASSWORD_CAL	0x42	/* 1 reg */
#define REG_PASSWORD_CUS	0x43	/* 1 reg */
#define REG_PASSWORD_FAC	0x44	/* 1 reg */

#define REG_PASSWORD_FIND	0x48	/* 1 reg */


// registers
//
// all registers are 32 bits
// could be long, bits, or float

//#define REG_STATUS 0x00 //status .. read only
#define REG_CONFIG          0x01 //config bits
#define REG_MA_OFFSET       0x02 //ma offset
#define REG_MA_GAIN         0x03 //ma gain
#define REG_PRESSURE_OFFSET 0x04 //pressure offset
#define REG_PRESSURE_GAIN   0x05 //pressture gain
#define REG_PRESSURE_FILTER 0x06 //press filter
#define REG_TEMP_FILTER     0x07 //temp filter
//#define REG_GAS_CALIBRATION_CONSTANT 0x08 //Gas Calibration Constant
#define REG_DAC_CALIBRATION 0x09	//DAC output value, writable for calibration, resumes after 10seconds
#define REG_CAL_TEMP        0x0a
#define REG_GAS_MA_MIN      0x0b
#define REG_JOSHS_CONSTANT  0x0c
#define REG_GAUGE_OFFSET    0x0d
#define REG_GAS_MA_MAX      0x0e
#define REG_SENSOR_OFFSET   0x0f

#define REG_IO_SIM_TIMER    0x50
#define REG_IO_SIM_PRESSURE 0x51


//read only registers
#define REG_PERCENT_FULL 0x10 //percent full reading
#define REG_MA_OUTPUT 0x11 //ma output
#define REG_RAW_ADC 0x12 //raw ADC reading
#define REG_PRESSURE_FILTERED 0x13 //filtered ADC reading
#define REG_RAW_TEMP 0x14 //raw temp reading
#define REG_TEMPERATURE_FILTERED 0x15 //filtered temperuture

#define REG_DENSITY_GL       0x16 // density
#define REG_DENSITY_LBFT3    0x17 // density


// relays
#define REG_RELAY_SETPOINT_1 0x20
#define REG_RELAY_SETPOINT_2 0x21
#define REG_RELAY_SETPOINT_3 0x22

#define REG_RELAY_HYST_1    0x23
#define REG_RELAY_HYST_2    0x24
#define REG_RELAY_HYST_3    0x25

#define REG_RELAY_DELAY_1   0x26
#define REG_RELAY_DELAY_2   0x27
#define REG_RELAY_DELAY_3   0x28

#define REG_TEMP_GAIN_LOW   0x29
#define REG_TEMP_GAIN_HIGH  0x2a

#define REG_TEMP_SLOPE_LOW   0x2b
#define REG_TEMP_SLOPE_HIGH  0x2c
