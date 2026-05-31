using System;
using System.Collections;
using System.Text;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Helper class to assist with Modbus protocol.
	/// Static access only.
	/// </summary>
	public class Modbus
	{
		public static DateTime MODBUS_BASE_TIME = new DateTime(1970, 1, 1);

		/// <summary>
		/// Static class access only.
		/// </summary>
		private Modbus()
		{
		}

		/// <summary>
		/// Convert a float (4 bytes) to Modbus ascii format.
		/// NOTE: byte order is very important here.
		/// </summary>
		/// <param name="n">float to convert</param>
		/// <returns>string value of argument (8 chars in length)</returns>
		public static string CreateFloat(float n)
		{
			byte [] b = BitConverter.GetBytes(n);
			return String.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[3], b[2], b[1], b[0]);
		}

		/// <summary>
		/// Convert a long (4 bytes) to Modbus ascii format.
		/// </summary>
		/// <param name="n">int to convert</param>
		/// <returns>string value of argument (8 chars in length)</returns>
		public static string CreateLong(int n)
		{
			return n.ToString("X8");
		}

		/// <summary>
		/// Convert an int (16 bit) to Modbus ascii format.
		/// </summary>
		/// <param name="n">int to convert</param>
		/// <returns>string value of argument (4 chars in length)</returns>
		public static string CreateInt(ushort n)
		{
			return n.ToString("X4");
		}

		/// <summary>
		/// Convert an short (8 bit) to Modbus ascii format.
		/// </summary>
		/// <param name="n">short to convert</param>
		/// <returns>string value of argument (2 chars in length)</returns>
		public static string CreateShort(byte n)
		{
			return n.ToString("X2");
		}

		/// <summary>
		/// Create a checksum for a Modbus command.
		/// </summary>
		/// <remarks>
		/// Exception thrown if s.Length is less than 4.
		/// Modbus commands are a minimum of four digits.
		/// </remarks>
		/// <param name="s">ascii command string minus checksum</param>
		/// <returns>string with checksum added</returns>
		public static string CreateChecksum(string s)
		{
			// The first byte of the string is supposed
			// to be a colon ':' and is not considered
			// in the checksumming algorithm.

			int sum = 0;
			for (int i = 1; i < s.Length; i += 2)
				sum += MODBUS_HEX_DECODE(s[i], s[i+1]);
			return s + String.Format("{0:X2}", (-sum) & 0xff);
		}

		/// <summary>
		/// Decode two characters from ASCII to a byte.
		/// Bad input causes an Application Exception.
		/// </summary>
		/// <param name="hi">First ASCII character (hi nibble)</param>
		/// <param name="lo">Second ASCII character (lo nibble)</param>
		/// <returns>byte value of decoded value</returns>
		public static byte MODBUS_HEX_DECODE(char hi, char lo)
		{
			byte b = 0;

			if (hi >= '0' && hi <= '9') 
				b = (byte)((hi - '0') * 16);
			else if (hi >= 'A' && hi <= 'F')
				b = (byte)(((hi - 'A') + 10) * 16);
			else
				throw new InvalidOperationException("Bad hex input to MODBUS_HEX_DECODE(hi): " + String.Format("0x{0:X2}", hi));
			if (lo >= '0' && lo <= '9')
				b += (byte)(lo - '0');
			else if (lo >= 'A' && lo <= 'F')
				b += (byte)((lo - 'A') + 10);
			else
				throw new InvalidOperationException("Bad hex input to MODBUS_HEX_DECODE(lo): " + String.Format("0x{0:X2}", lo));
			return b;
		}

		/// <summary>
		/// Calculate the seconds since 1/1/1970.
		/// </summary>
		/// <param name="thisTime">a DateTime instance</param>
		/// <returns>seconds since 1970</returns>
		public static int SecondsSince1970( DateTime instance )
		{
			return Convert.ToInt32((instance - MODBUS_BASE_TIME).TotalSeconds);
		}

		/// <summary>
		/// Verify the checksum of a Modbus response
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool VerifyChecksum(string item)
		{
			// Short strings are errors.  We trap them
			// explicitly because CreateChecksum() throws
			// on very short strings.

			if (item.Length < 6) return false;

			string rawitem = item.Trim();
			rawitem = rawitem.Substring(0, rawitem.Length - 2);
			string newitem = CreateChecksum(rawitem);
			return newitem == item;
		}

		
		/// <summary>
		/// Decode a Modbus "SHORT" (2 ascii characters).
		/// </summary>
		/// <param name="s">string to decode</param>
		/// <param name="i">start decoding at this index</param>
		/// <returns>8-bit signed result</returns>
		public static byte SHORT(string s, int i)
		{
			return MODBUS_HEX_DECODE(s[i], s[i+1]);
		}

		/// <summary>
		/// Decode a Modbus "INT" (4 ascii characters)
		/// </summary>
		/// <param name="s">string to decode</param>
		/// <param name="i">start decoding at this index</param>
		/// <returns>16-bit signed result</returns>
		public static short INT(string s, int i)
		{
			return (short)((MODBUS_HEX_DECODE(s[i+0], s[i+1]) << 8) | MODBUS_HEX_DECODE(s[i+2], s[i+3]));
		}

		/// <summary>
		/// Decode a Modbus "LONG" (8 ascii characters)
		/// </summary>
		/// <param name="s">string to decode</param>
		/// <param name="i">start decoding at this index</param>
		/// <returns>32-bit signed result</returns>
		public static int LONG(string s, int i)
		{
			return (int)((MODBUS_HEX_DECODE(s[i+0], s[i+1]) << 24) |
						 (MODBUS_HEX_DECODE(s[i+2], s[i+3]) << 16) |
						 (MODBUS_HEX_DECODE(s[i+4], s[i+5]) <<  8) |
						 (MODBUS_HEX_DECODE(s[i+6], s[i+7])));
		}

		/// <summary>
		/// Decodes 8 ascii bytes into 4 binary bytes into a single precision
		/// IEEE floating point value.
		/// </summary>
		/// <param name="s">string to decode</param>
		/// <param name="i">start decoding at this index</param>
		/// <returns>a float representation of the value</returns>
		public static float FLOAT(string s, int i)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(LONG(s, i)), 0);
		}

		/// <summary>
		/// Decode a Modbus response that contains a string.
		/// </summary>
		/// <param name="s">string to decode</param>
		/// <param name="i">start decoding at this index</param>
		/// <returns>a string representation of the value</returns>
		public static string STRING(string s, int i)
		{
			// must ignore 2 char checksum at end
			string modstr = s.Substring(i, s.Length-(i+2));
			byte [] bytes = new byte[modstr.Length/2];
			for (int j = 0; j < modstr.Length; j += 2)
				bytes[j/2] = MODBUS_HEX_DECODE(modstr[j], modstr[j+1]);
			return Encoding.ASCII.GetString(bytes);
		}

		/// <summary>
		/// Decode a Modbus "Logged Data Record"
		/// Returns a hashtable with the appropriate names and values.
		/// If compat is true, convert everything to strings rather than
		/// their original types.
		/// </summary>
		/// <param name="item">string to decode</param>
		/// <param name="compat">true if old string values desired, false if binary values</param>
		/// <returns></returns>
		private static Hashtable DecodeDataRecordImpl(string item, bool compat)
		{
			Hashtable result = new Hashtable();

			if (compat == true)
			{
				result.Add("RecordNumber", INT(item, 5).ToString());
				result.Add("RecordStatus", SHORT(item, 9).ToString());
				result.Add("RecordType", INT(item, 11).ToString());
				result.Add("TimeStamp", MODBUS_BASE_TIME.AddSeconds((uint)Modbus.LONG(item, 15)).ToString("yyyy/MM/dd HH:mm"));
				result.Add("Status", LONG(item, 23).ToString("d"));
	
				switch ((string)result["RecordType"])
				{
					case "1":	// PPM
					case "2":	// Sensitivity
					case "3":	// Verification
					case "6":	// Retention
						result.Add("H2",   FLOAT(item, 31).ToString("F1"));
						result.Add("O2",   FLOAT(item, 39).ToString("F1"));
						result.Add("N2",   FLOAT(item, 47).ToString("F1"));
						result.Add("CH4",  FLOAT(item, 55).ToString("F1"));
						result.Add("CO",   FLOAT(item, 63).ToString("F1"));
						result.Add("CO2",  FLOAT(item, 71).ToString("F1"));
						result.Add("C2H6", FLOAT(item, 79).ToString("F1"));
						result.Add("C2H4", FLOAT(item, 87).ToString("F1"));
						result.Add("C2H2", FLOAT(item, 95).ToString("F1"));
						break;

					case "4":	// Power Up
					case "5":	// Sensor
						result.Add("LoadGuide",		FLOAT(item, 31).ToString("F1"));
						result.Add("OilTemp",		FLOAT(item, 39).ToString("F1"));
						result.Add("AmbientTemp",	FLOAT(item, 47).ToString("F1"));
						result.Add("TGATemp",		FLOAT(item, 55).ToString("F1"));
						result.Add("CalTankPres",	FLOAT(item, 63).ToString("F1"));
						result.Add("HeadSpacePres", FLOAT(item, 71).ToString("F1"));
						result.Add("TECHotTemp",    FLOAT(item, 79).ToString("F1"));
						result.Add("TECCoolTemp",   FLOAT(item, 87).ToString("F1"));
						result.Add("HeliumPres",	FLOAT(item, 95).ToString("F1"));
						result.Add("ColATemp",		FLOAT(item, 103).ToString("F1"));
						result.Add("TCDTemp",		FLOAT(item, 111).ToString("F1"));
						result.Add("SysPres",		FLOAT(item, 119).ToString("F1"));
						break;

					case "7":	// Extractor
						result.Add("OilTemp",			FLOAT(item, 31).ToString("F1"));
						result.Add("GasPres",			FLOAT(item, 39).ToString("F1"));
						result.Add("OilPres",			FLOAT(item, 47).ToString("F1"));
						result.Add("MaxOilPres",		FLOAT(item, 55).ToString("F1"));
						result.Add("EnclosureTemp",		FLOAT(item, 63).ToString("F1"));
						result.Add("PurgeCount",		LONG(item, 71).ToString("F1"));
						result.Add("MaxOilTemp",		FLOAT(item, 79).ToString("F1"));
						result.Add("MinOilTemp",		FLOAT(item, 87).ToString("F1"));
						result.Add("MaxEnclosureTemp",	FLOAT(item, 95).ToString("F1"));
						result.Add("MinEnclosureTemp",	FLOAT(item, 103).ToString("F1"));
						break;

					case "8":	// PeakTrack
						result.Add("H2PosOld", INT(item, 31).ToString());
						result.Add("H2PosNew", INT(item, 35).ToString());
						result.Add("O2PosOld", INT(item, 39).ToString());
						result.Add("O2PosNew", INT(item, 43).ToString());
						result.Add("N2PosOld", INT(item, 47).ToString());
						result.Add("N2PosNew", INT(item, 51).ToString());
						result.Add("CH4PosOld", INT(item, 55).ToString());
						result.Add("CH4PosNew", INT(item, 59).ToString());
						result.Add("COPosOld", INT(item, 63).ToString());
						result.Add("COPosNew", INT(item, 67).ToString());
						result.Add("CO2PosOld", INT(item, 71).ToString());
						result.Add("CO2PosNew", INT(item, 75).ToString());
						result.Add("C2H6PosOld", INT(item, 79).ToString());
						result.Add("C2H6PosNew", INT(item, 83).ToString());
						result.Add("C2H4PosOld", INT(item, 87).ToString());
						result.Add("C2H4PosNew", INT(item, 91).ToString());
						result.Add("C2H2PosOld", INT(item, 95).ToString());
						result.Add("C2H2PosNew", INT(item, 99).ToString());
						break;
				}
				return result;
			}
			else
			{
				result.Add("RecordNumber", INT(item, 5));
				result.Add("RecordStatus", SHORT(item, 9));
				result.Add("RecordType", INT(item, 11));
				result.Add("TimeStamp", MODBUS_BASE_TIME.AddSeconds((uint)Modbus.LONG(item, 15)));
				result.Add("Status", LONG(item, 23));
	
				switch (Convert.ToInt32((short)result["RecordType"]))
				{
					case 1:	// PPM
					case 2:	// Sensitivity
					case 3:	// Verification
					case 6:	// Retention
						result.Add("H2",   FLOAT(item, 31));
						result.Add("O2",   FLOAT(item, 39));
						result.Add("N2",   FLOAT(item, 47));
						result.Add("CH4",  FLOAT(item, 55));
						result.Add("CO",   FLOAT(item, 63));
						result.Add("CO2",  FLOAT(item, 71));
						result.Add("C2H6", FLOAT(item, 79));
						result.Add("C2H4", FLOAT(item, 87));
						result.Add("C2H2", FLOAT(item, 95));
						break;

					case 4:	// Power Up
					case 5:	// Sensor
						result.Add("LoadGuide",		FLOAT(item, 31));
						result.Add("OilTemp",		FLOAT(item, 39));
						result.Add("AmbientTemp",	FLOAT(item, 47));
						result.Add("TGATemp",		FLOAT(item, 55));
						result.Add("CalTankPres",	FLOAT(item, 63));
						result.Add("HeadSpacePres", FLOAT(item, 71));
						result.Add("TECHotTemp",    FLOAT(item, 79));
						result.Add("TECCoolTemp",   FLOAT(item, 87));
						result.Add("HeliumPres",	FLOAT(item, 95));
						result.Add("ColATemp",		FLOAT(item, 103));
						result.Add("TCDTemp",		FLOAT(item, 111));
						result.Add("SysPres",		FLOAT(item, 119));
						break;

					case 7:	// Extractor
						result.Add("OilTemp",			FLOAT(item, 31));
						result.Add("GasPres",			FLOAT(item, 39));
						result.Add("OilPres",			FLOAT(item, 47));
						result.Add("MaxOilPres",		FLOAT(item, 55));
						result.Add("EnclosureTemp",		FLOAT(item, 63));
						result.Add("PurgeCount",		LONG(item, 71));
						result.Add("MaxOilTemp",		FLOAT(item, 79));
						result.Add("MinOilTemp",		FLOAT(item, 87));
						result.Add("MaxEnclosureTemp",	FLOAT(item, 95));
						result.Add("MinEnclosureTemp",	FLOAT(item, 103));
						break;

					case 8:	// PeakTrack
						result.Add("H2PosOld", INT(item, 31));
						result.Add("H2PosNew", INT(item, 35));
						result.Add("O2PosOld", INT(item, 39));
						result.Add("O2PosNew", INT(item, 43));
						result.Add("N2PosOld", INT(item, 47));
						result.Add("N2PosNew", INT(item, 51));
						result.Add("CH4PosOld", INT(item, 55));
						result.Add("CH4PosNew", INT(item, 59));
						result.Add("COPosOld", INT(item, 63));
						result.Add("COPosNew", INT(item, 67));
						result.Add("CO2PosOld", INT(item, 71));
						result.Add("CO2PosNew", INT(item, 75));
						result.Add("C2H6PosOld", INT(item, 79));
						result.Add("C2H6PosNew", INT(item, 83));
						result.Add("C2H4PosOld", INT(item, 87));
						result.Add("C2H4PosNew", INT(item, 91));
						result.Add("C2H2PosOld", INT(item, 95));
						result.Add("C2H2PosNew", INT(item, 99));
						break;
					case 9:	// AutoCal
						//TODO add hashtable support for AutoCal
						break;
				}
				return result;
			}

		}

		/// <summary>
		/// Decode a Modbus "Logged Data Record"
		/// Returns a hashtable with the appropriate names and values.
		/// </summary>
		/// <param name="item">string to decode</param>
		/// <returns>hashtable of key/value pairs</returns>
		public static Hashtable DecodeDataRecord(string item)
		{
			return DecodeDataRecordImpl(item, true);
		}

		/// <summary>
		/// Decode a Modbus "Logged Data Record"
		/// Returns a hashtable with the appropriate names and values.
		/// </summary>
		/// <param name="item">string to decode</param>
		/// <param name="compat">true if string values desired (compatible with old code)</param>
		/// <returns></returns>
		public static Hashtable DecodeDataRecord(string item, bool compat)
		{
			return DecodeDataRecordImpl(item, compat);
		}
	}
}
