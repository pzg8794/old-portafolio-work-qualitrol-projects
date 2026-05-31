#include <compiler.h>
#include "version.h"
#include "ee.h"
#include "eeprom.h"
#include "globals.h"
#include "relaystate.h"

// eeram is initalized to 0x1234
long EE_init_value = {0x1234};

// all none volital stuff is read and written at once
// this save both ram and rom


void ee_readAll(void)
{
    eeConfigFlags= eepromReadLong( EE_eeConfigFlags);

    if ( eeConfigFlags != EE_init_value )
    {
        ee_writeAll();
    }

    sysConfig     = eepromReadLong( EE_sysConfig);
    ma_offset     = eepromReadFloat( EE_ma_offset);
    ma_gain = eepromReadFloat(EE_ma_gain);
    gas_ma_min = eepromReadFloat(EE_gas_ma_min);
    pressure_offset = eepromReadFloat(EE_pressure_offset);
    pressure_gain = eepromReadFloat(EE_pressure_gain);
    temperature_offset = eepromReadFloat(EE_temperature_offset);
    temperature_gain = eepromReadFloat(EE_temperature_gain);
    press_ir = eepromReadFloat(EE_press_ir);
    temp_ir = eepromReadFloat(EE_temp_ir);
    GasConstantTemp = eepromReadFloat(EE_GasConstantTemp);
    gas_ma_max   = eepromReadFloat(EE_gas_ma_max);
    JoshsConstant = eepromReadFloat(EE_JoshsConstant);
    gauge_offset = eepromReadFloat(EE_GAUGE_OFFSET);
    sensor_offset = eepromReadFloat(EE_SENSOR_OFFSET);

    relay[0].trip = eepromReadFloat(EE_SETPOINT_1);
    relay[1].trip = eepromReadFloat(EE_SETPOINT_2);
    relay[2].trip = eepromReadFloat(EE_SETPOINT_3);

    relay[0].hyst = eepromReadFloat(EE_HYST_1);
    relay[1].hyst = eepromReadFloat(EE_HYST_2);
    relay[2].hyst = eepromReadFloat(EE_HYST_3);

    relay[0].delay = eepromReadLong(EE_DELAY_1);
    relay[1].delay = eepromReadLong(EE_DELAY_2);
    relay[2].delay = eepromReadLong(EE_DELAY_3);

    pass_simulate.l[0] = eepromReadLong(EE_PASS_SIM_1);
    pass_simulate.l[1] = eepromReadLong(EE_PASS_SIM_2);

    pass_calibrate.l[0] = eepromReadLong(EE_PASS_CALIBRATE_1);
    pass_calibrate.l[1] = eepromReadLong(EE_PASS_CALIBRATE_2);

    pass_cust.l[0] = eepromReadLong(EE_PASS_CUST_1);
    pass_cust.l[1] = eepromReadLong(EE_PASS_CUST_2);

    pass_factory.l[0] = eepromReadLong(EE_PASS_FACTORY_1);
    pass_factory.l[1] = eepromReadLong(EE_PASS_FACTORY_2);

    serial_number.l[0] = eepromReadLong(EE_SERIAL_NUMBER_1);
    serial_number.l[1] = eepromReadLong(EE_SERIAL_NUMBER_2);

    manuf_date.l[0] = eepromReadLong(EE_MANUF_DATE_1);
    manuf_date.l[1] = eepromReadLong(EE_MANUF_DATE_2);

    tempGainLow = eepromReadFloat(EE_TEMP_GAIN_LOW);
    tempGainHigh = eepromReadFloat(EE_TEMP_GAIN_HIGH);

    tempOffsetLow = eepromReadFloat(EE_TEMP_SLOPE_LOW);
    tempOffsetHigh = eepromReadFloat(EE_TEMP_SLOPE_HIGH);

}

void ee_writeAll(void)
{
    eepromWriteLong(EE_eeConfigFlags,&EE_init_value);
    eepromWriteLong(EE_sysConfig,&sysConfig);
    eepromWriteLong(EE_ma_offset,&ma_offset);
    eepromWriteLong(EE_ma_gain,&ma_gain);
    eepromWriteLong(EE_gas_ma_min,&gas_ma_min);
    eepromWriteLong(EE_pressure_offset,&pressure_offset);
    eepromWriteLong(EE_pressure_gain,&pressure_gain);
    eepromWriteLong(EE_temperature_offset,&temperature_offset);
    eepromWriteLong(EE_temperature_gain,&temperature_gain);
    eepromWriteLong(EE_press_ir,&press_ir);
    eepromWriteLong(EE_temp_ir,&temp_ir);
    eepromWriteLong(EE_GasConstantTemp,&GasConstantTemp);
    eepromWriteLong(EE_gas_ma_max,&gas_ma_max);
    eepromWriteLong(EE_JoshsConstant,&JoshsConstant);
    eepromWriteLong(EE_GAUGE_OFFSET,&gauge_offset);
    eepromWriteLong(EE_SENSOR_OFFSET,&sensor_offset);

    eepromWriteLong(EE_SETPOINT_1,&relay[0].trip);
    eepromWriteLong(EE_SETPOINT_2,&relay[1].trip);
    eepromWriteLong(EE_SETPOINT_3,&relay[2].trip);

    eepromWriteLong(EE_HYST_1,&relay[0].hyst);
    eepromWriteLong(EE_HYST_2,&relay[1].hyst);
    eepromWriteLong(EE_HYST_3,&relay[2].hyst);

    eepromWriteLong(EE_DELAY_1,&relay[0].delay);
    eepromWriteLong(EE_DELAY_2,&relay[1].delay);
    eepromWriteLong(EE_DELAY_3,&relay[2].delay);


    eepromWriteLong(EE_PASS_SIM_1,&pass_simulate.l[0]);
    eepromWriteLong(EE_PASS_SIM_2,&pass_simulate.l[1]);

    eepromWriteLong(EE_PASS_CALIBRATE_1,&pass_calibrate.l[0]);
    eepromWriteLong(EE_PASS_CALIBRATE_2,&pass_calibrate.l[1]);

    eepromWriteLong(EE_PASS_CUST_1,&pass_cust.l[0]);
    eepromWriteLong(EE_PASS_CUST_2,&pass_cust.l[1]);

    eepromWriteLong(EE_PASS_FACTORY_1,&pass_factory.l[0]);
    eepromWriteLong(EE_PASS_FACTORY_2,&pass_factory.l[1]);

    eepromWriteLong(EE_SERIAL_NUMBER_1,&serial_number.l[0]);
    eepromWriteLong(EE_SERIAL_NUMBER_2,&serial_number.l[1]);

    eepromWriteLong(EE_MANUF_DATE_1,&manuf_date.l[0]);
    eepromWriteLong(EE_MANUF_DATE_2,&manuf_date.l[1]);

    eepromWriteLong(EE_TEMP_GAIN_LOW, &tempGainLow);
    eepromWriteLong(EE_TEMP_GAIN_HIGH,&tempGainHigh);

    eepromWriteLong(EE_TEMP_SLOPE_LOW, &tempOffsetLow);
    eepromWriteLong(EE_TEMP_SLOPE_HIGH,&tempOffsetHigh);

    
}

