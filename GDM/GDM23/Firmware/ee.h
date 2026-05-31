

// EE_REGS
// each reg is 4 bytes



void ee_readAll(void);
void ee_writeAll(void);


#define EE_eeConfigFlags 0
#define EE_sysConfig 4
#define EE_ma_offset 8
#define EE_ma_gain 12
#define EE_gas_ma_min 16
#define EE_pressure_offset 20
#define EE_pressure_gain 24
#define EE_temperature_offset 28
#define EE_temperature_gain 32
#define EE_press_ir 36
#define EE_temp_ir 40
#define EE_GasConstantTemp 44
#define EE_gas_ma_max 48
#define EE_JoshsConstant 52


#define EE_SETPOINT_1 56
#define EE_SETPOINT_2 60
#define EE_SETPOINT_3 64

#define EE_HYST_1 68
#define EE_HYST_2 72
#define EE_HYST_3 76

#define EE_DELAY_1 80
#define EE_DELAY_2 84
#define EE_DELAY_3 88

#define EE_PASS_SIM_1 92
#define EE_PASS_SIM_2 96

#define EE_PASS_CUST_1 100
#define EE_PASS_CUST_2 104

#define EE_PASS_FACTORY_1 108
#define EE_PASS_FACTORY_2 112

#define EE_PASS_CALIBRATE_1 116
#define EE_PASS_CALIBRATE_2 120

#define EE_SERIAL_NUMBER_1 124
#define EE_SERIAL_NUMBER_2 128

#define EE_MANUF_DATE_1 132
#define EE_MANUF_DATE_2 136

#define EE_GAUGE_OFFSET 140
#define EE_SENSOR_OFFSET 144

#define EE_TEMP_GAIN_LOW 148
#define EE_TEMP_GAIN_HIGH 152

#define EE_TEMP_SLOPE_LOW 156
#define EE_TEMP_SLOPE_HIGH 160

