using System;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// AlarmStatus names the well-known states (unknown, nominal, caution, alarm).
	/// This is a very general idea that spans different instrument types, so it has
	/// been placed in Core for general sharing.
	/// </summary>
	/// <remarks>
	/// Note that these value (0, 1, 2, 3) are widely used as raw integers in the
	/// GridHawk, SMS Client and Burlington (SA View 2.0+) source bases.  It would
	/// be more or less impossible to ever find all the places that depend on these
	/// values being 0, 1, 2 and 3, so changing their representation is not practical.
	/// This class merely provides a way to make the code a little more readable.
	/// </remarks>
	public class AlarmStatus
	{
		private AlarmStatus()
		{
		}

		/// <summary>
		/// Status unknown, e.g. no report from instrument,
		/// monitoring administratively disabled, etc.
		/// Often represented as gray in user interfaces.
		/// </summary>
		public const int Unknown = 0;

		/// <summary>
		/// Status nominal.  Often represented as green
		/// in user interfaces.
		/// </summary>
		public const int Nominal = 1;

		/// <summary>
		/// Status caution.  Often represented as yellow
		/// or amber in user interfaces.
		/// </summary>
		public const int Caution = 2;

		/// <summary>
		/// Status alarm.  Often represented as red in
		/// user interfaces; may be represented as blue
		/// when applied to monitor ("Service Required").
		/// </summary>
		public const int Alarm = 3;

		/// <summary>
		/// The unit requires service
		/// </summary>
		public const int ServiceRequired = 4;

		/// <summary>
		/// Data is older than 8 hours
		/// </summary>
		public const int DataIsOld = 5;

		/// <summary>
		/// Data is older than 24 hours
		/// </summary>
		public const int DataIsVeryOld = 6;
	}
}
