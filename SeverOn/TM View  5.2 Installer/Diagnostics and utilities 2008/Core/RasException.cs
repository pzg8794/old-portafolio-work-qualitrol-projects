// TMx CLI adapter C# code
// Copyright 2005 Serveron Corporation. All rights reserved.

using System;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// An exception from the RAS layer, typically during connect.
	/// </summary>
	public class RasException: System.Net.Sockets.SocketException
	{
		private int _rasErrno = 0;
 
		/// <summary>
		/// Create a RasException as a kind of SocketException having
		/// a second integer property which is the RAS error code.
		/// </summary>
		/// <param name="socketErrno"></param>
		/// <param name="rasErrno"></param>
		public RasException(int socketErrno, int rasErrno): base(socketErrno)
		{
			_rasErrno = rasErrno;
		}

		/// <summary>
		/// Return the RAS error code as an integer.
		/// </summary>
		public int RasErrno
		{
			get
			{
				return _rasErrno;
			}
		}

		/// <summary>
		/// Get the RAS error description.
		/// </summary>
		public string RasErrorDescription
		{
			get
			{
				string message = new string('\u0000', 1024);
				RasApi.RasGetErrorString((uint)_rasErrno, message, 1024);
				return message;
			}
		}
	}

	public class FatalRasException : ApplicationException
	{
		public FatalRasException(string msg): base(msg)
		{
		}
	}
}
