#include "version.h"

// EE stuff is stored in order
// this way we can block read at power up.
// read 36 bytes. check the init flags.
// if not set write a full block.

long	eeConfigFlags       = 0x00000000;
long	sysConfig           = 0x00000010;       // C, PSI, absolute
float	ma_offset           = 2986.0;
float	ma_gain             = 747.2;
float	gas_ma_min          = 14.7;
float	pressure_offset     = -289.46;
float	pressure_gain       = 74.8667;
float   temperature_offset  = 0.0;
float   temperature_gain    = 1.0;
float	press_ir            = 5.0;
float	temp_ir             = 5.0;
float	GasConstantTemp     = 20.0;
float	gas_ma_max          = 74.7;
float	JoshsConstant       = 1.0;
float   gauge_offset        = 14.7;
float   sensor_offset       = 14.7;

// end of ee vars...

signed int	pressS1;
signed int	pressS2;

float		percentFull = 100.0;
float		pressure = 150.0;
float           newPressure = 150.0;

float density_gl;
float density_lbft3;

signed int tempS1 = 800;	// temperature sensor reading 0.03125 d C per count
signed int tempS2 = 800;	// temperature sensor reading 0.03125 d C per count

float		tempC = 25.0;
float		tempK = 298.15;



float tempNC;           


unsigned int	DACout = 0x1234;
char            DACtimeout = 0; 	// time dac spend holding a calibration constant

float		ma_out = 20.0;

union
{
    unsigned long l[2];
    unsigned char b[8];
} pass_cust = {0x3333};

union
{
    unsigned long l[2];
    unsigned char b[8];
} pass_factory = {0x3434};

union
{
    unsigned long l[2];
    unsigned char b[8];
} pass_calibrate = {0x3232};

union
{
    unsigned long l[2];
    unsigned char b[8];
} pass_simulate = {0x3131};

union
{
    unsigned long l[2];
    unsigned char b[8];
} serial_number = {0x31};

union
{
    unsigned long l[2];
    unsigned char b[8];
} manuf_date = {0};

int sim_timer = 0;

float sim_pressure = 150.0;

float tempGainLow   = TGAINL;
float tempGainHigh  = TGAINH;

float tempOffsetLow  = TSLOPL;
float tempOffsetHigh = TSLOPH;

int loopType = 0;

