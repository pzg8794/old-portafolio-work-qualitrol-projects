
#include <compiler.h>
#include <math.h>
#include "version.h"
#include "globals.h"
#include "ads1118.h"
#include "iirfilter.h"

void IIRfilter(float *value,float newvalue,float timeconst)
{

	// fix up any overflow that may have somehow happened.
	// most likely during a faulty calibration.
        // from a web search on "microchip c18 nan", pos or neg infinity should trigger this.
//	if (( (*value).i[0] & 0x7f80 ) == 0x7f80 )
//		(*value).f = 0.0;

	*value = ((*value)*timeconst + newvalue)/(timeconst+1.0);
}

void FilterTemp(float IR)
{
    tempS2 = tempS1;
    tempS1 = ReadTemperature();

    if ( fabs(tempS1 - tempS2 ) > 10.0 )
    {
        return;
    }

    tempNC = ((float) tempS1)*0.03125; // convert to degrees C (or K)
    IIRfilter(& tempC, tempNC, IR);
}

void FilterPressure(float IR)
{
    float pressTmpO;
    float pressTmpS;

    pressS2 = pressS1;
    pressS1 = ReadPressure();

    if ( fabs(pressS1 - pressS2 ) > 10.0 )
    {
        return;
    }

//    tempC = -30.0;    // for test only

    // first, the sensor temperature offset.
    if ( tempC < 20.0)
    {
        pressTmpO = (20.0-tempC)*tempOffsetLow;
    }
    else
    {
        pressTmpO = (20.0-tempC)*tempOffsetHigh;
    }
    // second, the raw offset.
    pressTmpO += pressure_offset;

    // third, the sensor value.
    pressTmpO += (float) pressS1;

    // forth, the sensor temperature gain.
    if ( tempC < 20.0 )
    {
        pressTmpS = pressTmpO*(1.0+(20.0-tempC)*tempGainLow);
    }
    else
    {
        pressTmpS = pressTmpO*(1.0+(20.0-tempC)*tempGainHigh);
    }

    newPressure = pressTmpS / pressure_gain;

    if ( sysConfig & GAUGE_MASK)
    {
        newPressure = newPressure + sensor_offset;
    }
    // filter reading using a IIR type filter
    IIRfilter(& pressure, newPressure, IR);
}

