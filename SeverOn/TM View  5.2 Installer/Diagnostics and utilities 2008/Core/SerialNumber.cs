using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Interpretation of serial numbers to determine product family.
	/// Supports: Truegas1, TMx, and BCM monitor serial numbers.
	/// </summary>
	/// <remarks>
	/// BCM support is weak (any serial number that doesn't appear to
	/// be a gas analyzer is presumed to be a BCM: there are no validity
	/// checks for BCM serial numbers).  Letters in serial numbers must
	/// be in upper case.
	/// </remarks>
	public class SerialNumber
	{
		/// <summary>
		/// Static class.
		/// </summary>
		private SerialNumber()
		{
		}

		/// <summary>
		/// Answer true if the string appears to be a TMx serial number.
		/// Null is not an acceptable argument, and an exception is thrown.
		/// </summary>
		/// <param name="serialnum">String to test.  May not be null.</param>
		/// <returns>true if the string appears to be TMx serial number</returns>
		public static bool IsTMx(string serialnum)
		{
			if (serialnum == null)
				throw new InvalidSerialNumberException("null serial number");
			return (serialnum.StartsWith("TM8")
				|| serialnum.StartsWith("TM3")
                || serialnum.StartsWith("TM1")
				|| serialnum.StartsWith("TM5")
				|| serialnum.StartsWith("TGA"));
		}

		/// <summary>
		/// compare two serial number strings, honor letter case
		/// </summary>
		/// <param name="a">serial number A</param>
		/// <param name="b">serial number B</param>
		/// <returns>less than 0: A is before B; 0: A equals B; greater than 0: A is after B</returns>
		public static int Compare(string a, string b)
		{
			return String.Compare(a, b, false, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// compare two serial number strings optionally ignore case
		/// </summary>
		/// <param name="a">serial number A</param>
		/// <param name="b">serial number B</param>
		/// <param name="ignoreCase">if true compare is case insensitive</param>
		/// <returns>less than 0: A is before B; 0: A equals B; greater than 0: A is after B</returns>
		public static int Compare(string a, string b, bool ignoreCase)
		{
			return String.Compare(a, b, ignoreCase, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// compare two serial number sub-strings, honor case
		/// </summary>
		/// <param name="a">serial number string A</param>
		/// <param name="indexA">start index of string A</param>
		/// <param name="b">serial number string B</param>
		/// <param name="indexB">start index of string B</param>
		/// <param name="length">length for compare</param>
		/// <returns>less than 0: A is before B; 0: A equals B; greater than 0: A is after B</returns>
		public static int Compare(string a, int indexA, string b, int indexB, int length)
		{
			return String.Compare(a, indexA, b, indexB, length, false, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// compare two serial number sub-strings, optionally ignore case
		/// </summary>
		/// <param name="a">serial number string A</param>
		/// <param name="indexA">start index of string A</param>
		/// <param name="b">serial number string B</param>
		/// <param name="indexB">start index of string B</param>
		/// <param name="length">length for compare</param>
		/// <param name="ignoreCase">if true compare is case insensitive</param>
		/// <returns>less than 0: A is before B; 0: A equals B; greater than 0: A is after B</returns>
		public static int Compare(string a, int indexA, string b, int indexB, int length, bool ignoreCase)
		{
			return String.Compare(a, indexA, b, indexB, length, ignoreCase, CultureInfo.InvariantCulture);
		}
	}


	/// <summary>
	/// Exception thrown when we successfully establish a session
	/// with the "wrong" analyzer (i.e. not the one we were told
	/// to expect).
	/// </summary>
	public class InvalidSerialNumberException : ApplicationException
	{
		public InvalidSerialNumberException(string msg)
			: base(msg)
		{
		}
	}
}
