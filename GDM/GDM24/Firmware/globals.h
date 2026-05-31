
void ParseSysConfig(void);


// the order of the next 12 varibles is important.
// it determines the block read/write that takes place to/from the eerom

extern long	eeConfigFlags;
extern long	sysConfig;
// bit 0 0=C,1=F
// bits 21  00 = psi, 01 = bar, 10=kPa, 11=mPa
// bit 3 0 = absolute 1 = gauge
#define GAUGE_MASK      0b00001000

#define FAILSAFE_MASK_1 0b00100000
#define FAILSAFE_MASK_2 0b01000000
#define FAILSAFE_MASK_3 0b10000000

#define LOOP_RANGE_MASK 0b0000011100000000
#define LOOP_RANGE_SHIFT 8

#define PSI2KPA 6.8947573
#define R_KPA_L 8.3144621
#define R_KPA_FT3 0293622458
#define MM_SF6_G  146.0559    // grams/mol
#define MM_SF6_LBS  0.321998141    // lbs/mol

extern float	ma_offset;
extern float	ma_gain;
extern float	gas_ma_min;
extern float	pressure_offset;
extern float	pressure_gain;
extern float    temperature_offset;
extern float    temperature_gain;
extern float	press_ir;
extern float	temp_ir;
extern float	GasConstantTemp;
extern float	gas_ma_max;
extern float	JoshsConstant;
extern float    gauge_offset;
extern float    sensor_offset;


// sysconfig def
		// bit 0	unused
		// bit 1	ads1100 adresss bit 0
		// bit 2	ads1100 adresss bit 1
		// bit 3	ads1100 adresss bit 2
		// bit 4	tbd
		// bit 5	tbd
		// bit 6	tbd
		// bit 7	tbd
		// bit 8	matype 0
		// bit 9	matype 1
		// bit 10	matype 2
		// bit 11	gauge type 
		// bit 12	units 0
		// bit 13	units 1
		// bit 14	units temperature
		// bit 15	language
		// bit 16



extern unsigned int	DACout;
extern char		DACtimeout; 	// time dac spend holding a calibration constant

extern float		ma_out;

extern signed int	pressS1;
extern signed int	pressS2;

extern float		percentFull;
extern float		pressure;
extern float            newPressure;

extern float            density_gl;
extern float            density_lbft3;

extern signed int	tempS1;	// temperature sensor reading 0.03125 d C per count
extern signed int	tempS2;

extern float		tempK;
extern float		tempC;

extern float tempNC;

extern union 
{
    unsigned long l[2];
    unsigned char b[8];
} pass_cust;

extern union 
{
    unsigned long l[2];
    unsigned char b[8];
} pass_factory;
extern union
{
    unsigned long l[2];
    unsigned char b[8];
} pass_calibrate;

extern union 
{
    unsigned long l[2];
    unsigned char b[8];
} pass_simulate;

extern union
{
    unsigned long l[2];
    unsigned char b[8];
} serial_number;

extern union
{
    unsigned long l[2];
    unsigned char b[8];
} manuf_date;

extern int sim_timer;
extern float sim_pressure;

extern float tempGainLow;
extern float tempGainHigh;

extern float tempOffsetLow;
extern float tempOffsetHigh;

extern int loopType;
