
#include "version.h"
#include "globals.h"
#include "gaslaw.h"

#include "dac8311.h"

void DoGasLaw(void)
{
	float l_offset;
	float l_mult;
	float l_calc;
        float GasConstant;
        
// do some magic calculation
// PV = nRT
// n = moles
// P = pressure
// V = volumn
// R = 8.3145 ( universal gas constant )
// T = temperature

// n = PV/RT
// n = P/T * V/R
// V/R is a constant for any particular config
// call it C

// n = C * P/T

// we don't really care how many moles there are.
// so rather than n and C we pick f and G such that

// f = G * P/T

// G should be selected at calibration time such that
// f = 100 when nominaly full

// relay set points will be referenced to 100 %.
// the ma loops will be referneced to 100 % as well.

	tempK = tempC + 273.15;

        GasConstant = (273.0 + GasConstantTemp ) * 100.0 / gas_ma_max;

        if ( sim_timer == 0)
        {
            percentFull = GasConstant * pressure / (tempK);
            sim_pressure = pressure;
        }
        else
        {
            pressure = sim_pressure;
            percentFull = GasConstant * sim_pressure / (GasConstantTemp+273.0);
        }

	if ( percentFull > 100.0 )
		percentFull = 100.0;

	if ( percentFull < 0.0 )
		percentFull = 0.0;

	percentFull *= JoshsConstant;

	l_calc = percentFull - gas_ma_min*100.0/gas_ma_max ;
	if ( l_calc < 0.0 )
		l_calc = 0.0;
	
	l_calc = l_calc * ( 100.0 / ( 100.0 - gas_ma_min*100.0/gas_ma_max));


	l_offset = 4.0;
	l_mult = 0.16;
	
	ma_out = l_calc * l_mult + l_offset;

	if ( DACtimeout )		// ma out calibrate timeout
	{
		DACtimeout--;
	}
	else
	{
		DACout = ma_offset + (ma_out-l_offset) * ma_gain;
	}
	WriteDac(DACout);
}