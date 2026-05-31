
union v
{
	float f;
	unsigned short i[2];
};

void IIRfilter(float *value,float newvalue,float timeconst);
void FilterTemp(float IR);
void FilterPressure(float IR);

