/// Serveron.Utility.Core.Serial C# code
/// Copyright (c) 2005 Serveron Corporation. All rights reserved.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Win32;	/// for registry probe
using Microsoft.Win32.SafeHandles;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// represents a serial port resource
	/// </summary>
	[Serializable]
	public class Serial : IDeserializationCallback, IDisposable
	{
		#region Private constants and state

		/// <summary>
		/// Log4net support
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Serial));

		//win32 API constants
		internal const int GENERIC_READ = unchecked((int)0x80000000);
		internal const int GENERIC_WRITE = 0x40000000;
		internal const int OPEN_EXISTING = 3;
		internal const int FILE_FLAG_OVERLAPPED  = 0x40000000;
		internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
		private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		internal const int ERROR_IO_PENDING = 997;

		// called whenever any async i/o operation completes.
		private unsafe static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(AsyncFSCallback);

		private SerialStream internalSerialStream = null;

		/// <summary>
		/// Win32 handle for open comm port.
		/// Initialized to INVALID_HANDLE_VALUE by constructor,
		/// set to actual handle at Open(), and set back to 
		/// Initialized state at Close().
		/// </summary>
		[NonSerialized] internal IntPtr handle;
        [NonSerialized] internal
        SafeFileHandle _handle;

		///  For a good background, see:  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnfiles/html/msdn_serial.asp
		///
		///		' From the Win32 Documentation
		///		
		///		// DTR Control Flow Values.
		///
		///		#define DTR_CONTROL_DISABLE    0x00
		///		#define DTR_CONTROL_ENABLE     0x01
		///		#define DTR_CONTROL_HANDSHAKE  0x02
		///
		///
		///		// RhwSchedule Control Flow Values
		///
		///		#define RhwSchedule_CONTROL_DISABLE    0x00
		///		#define RhwSchedule_CONTROL_ENABLE     0x01
		///		#define RhwSchedule_CONTROL_HANDSHAKE  0x02
		///		#define RhwSchedule_CONTROL_TOGGLE     0x03
		///
		///		typedef struct _DCB 
		///				{
		///					DWORD DCBlength;			/* sizeof(DCB)			*/
		///					DWORD BaudRate; 			/* Baud rate at which running 	*/
		///					DWORD fBinary: 1;			/* Binary Mode (skip EOF check)	*/
		///					DWORD fParity: 1;			/* Enable parity checking		*/
		///					DWORD fOutxCtsFlow:1;		/* ChwSchedule handshaking on output	*/
		///					DWORD fOutxDsrFlow:1;		/* DSR handshaking on output	*/
		///					DWORD fDtrControl:2;		/* DTR Flow control			*/
		///					DWORD fDsrSensitivity:1;	/* DSR Sensitivity			*/
		///					DWORD fTXContinueOnXoff: 1;	/* Continue TX on Xoff sent	*/
		///					DWORD fOutX: 1;				/* Enable output X-ON/X-OFF	*/
		///					DWORD fInX: 1;				/* Enable input X-ON/X-OFF		*/
		///					DWORD fErrorChar: 1;		/* Enable Err Replacement		*/
		///					DWORD fNull: 1;				/* Enable Null stripping		*/
		///					DWORD fRtsControl:2;		/* Rts Flow control			*/
		///					DWORD fAbortOnError:1;		/* Abort reads & writes on Error	*/
		///					DWORD fDummy2:17;			/* Reserved				*/
		///					WORD wReserved;				/* Not currently used		*/
		///					WORD XonLim;				/* Transmit X-ON threshold		*/
		///					WORD XoffLim;				/* Transmit X-OFF threshold	*/
		///					BYTE ByteSize;				/* Number of bits/byte, 4-8	*/
		///					BYTE Parity;				/* 0-4=None,Odd,Even,Mark,Space	*/
		///					BYTE StopBits;				/* 0,1,2 = 1, 1.5, 2		*/
		///					char XonChar;				/* Tx and Rx X-ON character	*/
		///					char XoffChar;				/* Tx and Rx X-OFF character	*/
		///					char ErrorChar;				/* Error replacement char		*/
		///					char EofChar;				/* End of Input character		*/
		///					char EvtChar;				/* Received Event character	*/
		///					WORD wReserved1;			/* Fill for now.			*/
		///				} DCB, *LPDCB;

		[StructLayout(LayoutKind.Explicit)]
		private struct DCB 
		{
			[FieldOffset(0)]  public int DCBlength;		// sizeof(DCB), must be 28 base 10 
			[FieldOffset(4)]  public int BaudRate;      // current baud rate
			[FieldOffset(8)]  public uint Flags;		// bit fields (see below)
			[FieldOffset(12)] public ushort wReserved;  // not currently used 
			[FieldOffset(14)] public ushort XonLim;     // transmit XON threshold 
			[FieldOffset(16)] public ushort XoffLim;    // transmit XOFF threshold 
			[FieldOffset(18)] public byte ByteSize;     // number of bits/byte, 4-8 
			[FieldOffset(19)] public byte Parity;       // 0-4=no,odd,even,mark,space 
			[FieldOffset(20)] public byte StopBits;     // 0,1,2 = 1, 1.5, 2 
			[FieldOffset(21)] public char XonChar;      // Tx and Rx XON character 
			[FieldOffset(22)] public char XoffChar;     // Tx and Rx XOFF character 
			[FieldOffset(23)] public char ErrorChar;    // error replacement character 
			[FieldOffset(24)] public char EofChar;      // end of input character 
			[FieldOffset(25)] public char EvtChar;      // received event character 
			[FieldOffset(26)] public ushort wReserved1; // reserved; do not use
		}

		// Values of Flags bitfield in DCB
		private const uint BINARY_MODE =			0x0001;
		private const uint ENABLE_PARITY =			0x0002;
		private const uint ChwSchedule_CONTROL_ENABLE =		0x0004;
		private const uint DSR_CONTROL_ENABLE =		0x0008;
		private const uint DTR_CONTROL_DISABLE =    0x0000;
		private const uint DTR_CONTROL_ENABLE =     0x0010;
		private const uint DTR_CONTROL_HANDSHAKE =  0x0020;
		private const uint DSR_SENSITIVITY =		0x0040;
		private const uint XOFF_CONTINUES_TX =		0x0080;
		private const uint XOFF_ON_OUTPUT_ENABLE =  0x0100;
		private const uint XOFF_ON_INPUT_ENABLE =   0x0200;
		private const uint ERR_REPLACEMENT_ENABLE = 0x0400;
		private const uint NULL_STRIPPING_ENABLE =  0x0800;
		private const uint RhwSchedule_CONTROL_DISABLE =    0x0000;
		private const uint RhwSchedule_CONTROL_ENABLE =		0x1000;
		private const uint RhwSchedule_CONTROL_HANDSHAKE =  0x2000;
		private const uint RhwSchedule_CONTROL_TOGGLE =		0x3000;
		private const uint ABORT_ON_ERROR =			0x4000;
		private const uint DCB_FLAGS_SET   = ( BINARY_MODE | ChwSchedule_CONTROL_ENABLE | DSR_CONTROL_ENABLE | DTR_CONTROL_ENABLE | RhwSchedule_CONTROL_ENABLE );        
        private const uint DCB_FLAGS_CLEAR = ( ENABLE_PARITY | DTR_CONTROL_HANDSHAKE | DSR_SENSITIVITY | XOFF_ON_OUTPUT_ENABLE | XOFF_ON_INPUT_ENABLE | ERR_REPLACEMENT_ENABLE | NULL_STRIPPING_ENABLE | RhwSchedule_CONTROL_HANDSHAKE | ABORT_ON_ERROR );
        private const uint DCB_FLAGS_NOFLOWCONTROL_CLEAR = ( ChwSchedule_CONTROL_ENABLE | DSR_CONTROL_ENABLE | DTR_CONTROL_ENABLE | DTR_CONTROL_HANDSHAKE  | RhwSchedule_CONTROL_ENABLE	| RhwSchedule_CONTROL_HANDSHAKE );

		[StructLayout(LayoutKind.Explicit)]
		private struct COMMTIMEOUhwSchedule 
		{
			[FieldOffset(0)]  public int ReadIntervalTimeout;
			[FieldOffset(4)]  public int ReadTotalTimeoutMultiplier;
			[FieldOffset(8)]  public int ReadTotalTimeoutConstant;
			[FieldOffset(12)] public int WriteTotalTimeoutMultiplier;
			[FieldOffset(16)] public int WriteTotalTimeoutConstant;
		}
		//	ReadIntervalTimeout 
		//	  Maximum time allowed to elapse between the arrival of two characters on the communications line, in milliseconds.
		//    During a ReadFile operation, the time period begins when the first character is received. If the interval between
		//    the arrival of any two characters exceeds this amount, the ReadFile operation is completed and any buffered data is returned.
		//    A value of zero indicates that interval time-outs are not used. 
		//    A value of MAXDWORD, combined with zero values for both the ReadTotalTimeoutConstant and ReadTotalTimeoutMultiplier members,
		//    specifies that the read operation is to return immediately with the characters that have already been received,
		//    even if no characters have been received.
		//																																																																			 
		//  ReadTotalTimeoutMultiplier 																																																																			 Multiplier used to calculate the total time-out period for read operations, in milliseconds. For each read operation, this value is multiplied by the requested number of bytes to be read. 
		//  ReadTotalTimeoutConstant 
		//	  Constant used to calculate the total time-out period for read operations, in milliseconds.
		//    For each read operation, this value is added to the product of the ReadTotalTimeoutMultiplier member and the requested number of bytes. 
		//    A value of zero for both the ReadTotalTimeoutMultiplier and ReadTotalTimeoutConstant members indicates that total time-outs are not used
		//    for read operations.
		//
		//  WriteTotalTimeoutMultiplier 
		//	  Multiplier used to calculate the total time-out period for write operations, in milliseconds.
		//    For each write operation, this value is multiplied by the number of bytes to be written. 
		//
		//	WriteTotalTimeoutConstant 
		//    Constant used to calculate the total time-out period for write operations, in milliseconds.
		//    For each write operation, this value is added to the product of the WriteTotalTimeoutMultiplier member and the number of bytes to be written. 
		//    A value of zero for both the WriteTotalTimeoutMultiplier and WriteTotalTimeoutConstant members indicates
		//    that total time-outs are not used for write operations.
		//  Remarks
		//    If an application sets ReadIntervalTimeout and ReadTotalTimeoutMultiplier to MAXDWORD and
		//    sets ReadTotalTimeoutConstant to a value greater than zero and less than MAXDWORD,
		//    one of the following occurs when the ReadFile function is called:
		//
		//		If there are any characters in the input buffer, ReadFile returns immediately with the characters in the buffer. 
		//		If there are no characters in the input buffer, ReadFile waits until a character arrives and then returns immediately. 
		//		If no character arrives within the time specified by ReadTotalTimeoutConstant, ReadFile times out. 

		#endregion

		#region Private methods

		private string DCBFlagDecode( uint f )
		{
			StringBuilder s = new StringBuilder(256);
			string sep = "";
			if ((f & BINARY_MODE) != 0)				{ s.Append(sep).Append("BINARY_MODE"); sep = "|"; }
			if ((f & ENABLE_PARITY) != 0)			{ s.Append(sep).Append("ENABLE_PARITY"); sep = "|"; }
			if ((f & ChwSchedule_CONTROL_ENABLE) != 0)		{ s.Append(sep).Append("ChwSchedule_CONTROL_ENABLE"); sep = "|"; }
			if ((f & DSR_CONTROL_ENABLE) != 0)		{ s.Append(sep).Append("DSR_CONTROL_ENABLE"); sep = "|"; }
			if ((f & DTR_CONTROL_ENABLE) != 0)		{ s.Append(sep).Append("DTR_CONTROL_ENABLE"); sep = "|"; }
			if ((f & DSR_SENSITIVITY) != 0)			{ s.Append(sep).Append("DSR_SENSITIVITY"); sep = "|"; }
			if ((f & XOFF_CONTINUES_TX) != 0)		{ s.Append(sep).Append("XOFF_CONTINUES_TX"); sep = "|"; }
			if ((f & XOFF_ON_OUTPUT_ENABLE) != 0)	{ s.Append(sep).Append("XOFF_ON_OUTPUT_ENABLE"); sep = "|"; }
			if ((f & XOFF_ON_INPUT_ENABLE) != 0)	{ s.Append(sep).Append("XOFF_ON_INPUT_ENABLE"); sep = "|"; }
			if ((f & ERR_REPLACEMENT_ENABLE) != 0)	{ s.Append(sep).Append("ERR_REPLACEMENT_ENABLE"); sep = "|"; }
			if ((f & NULL_STRIPPING_ENABLE) != 0)	{ s.Append(sep).Append("NULL_STRIPPING_ENABLE"); sep = "|"; }
			if ((f & RhwSchedule_CONTROL_ENABLE) != 0)		{ s.Append(sep).Append("RhwSchedule_CONTROL_ENABLE"); sep = "|"; }
			if ((f & RhwSchedule_CONTROL_HANDSHAKE) != 0)	{ s.Append(sep).Append("RhwSchedule_CONTROL_HANDSHAKE"); sep = "|"; }
			if ((f & ABORT_ON_ERROR) != 0)			{ s.Append(sep).Append("ABORT_ON_ERROR"); sep = "|"; }
			return s.ToString();
		}

		private string PrintDCB( DCB d )
		{
			return String.Format("----------------------------------\n") +
				   String.Format("	DCBlength: {0}\n", d.DCBlength) +
				   String.Format("	BaudRate: {0}\n", d.BaudRate) +
				   String.Format("	flags: ({0})\n", DCBFlagDecode(d.Flags)) +
				   String.Format("	XonLim: {0}\n", d.XonLim) +
				   String.Format("	XoffLim: {0}\n", d.XoffLim) +
				   String.Format("	ByteSize: {0}\n", d.ByteSize) +
				   String.Format("	Parity: {0}\n", d.Parity) +
				   String.Format("	StopBits: {0}\n", d.StopBits) +
				   String.Format("	XonChar: {0:X}\n", d.XonChar) +
				   String.Format("	XoffChar: {0:X}\n", d.XoffChar) +
				   String.Format("	ErrorChar: {0:X}\n", d.ErrorChar) +
				   String.Format("	EofChar: {0:X}\n", d.EofChar) +
				   String.Format("	EvtChar: {0:X}\n", d.EvtChar) +
				   String.Format("----------------------------------\n");
		}
		#endregion

		#region serial ports probing

		private static string[] _portNames = null;

		/// <summary>
		/// find available local machine serial ports using data in the registry
		/// </summary>
		/// <remarks>this finds hard and virtual serial ports
		/// on Windows 2000, probably also works for Windows XP</remarks>
		private static string[] ProbeForPortNames( )
		{
			System.Collections.ArrayList names = new System.Collections.ArrayList();

			try
			{
				// the values under this registry key:
				//   HKEY_LOCAL_MACHINE/HARDWARE/DEVICEMAP/SERIALCOMM
				// enumerate the available serial communications ports
				RegistryKey serialCommRegKey = Registry.LocalMachine
					.CreateSubKey( "HARDWARE" )
					.CreateSubKey( "DEVICEMAP" )
					.CreateSubKey( "SERIALCOMM" );
				string[] valueNames = serialCommRegKey.GetValueNames();
				foreach( string s in valueNames )
				{
					names.Add( serialCommRegKey.GetValue( s ) );
				}
			}
			catch ( System.UnauthorizedAccessException ex )
			{
				// user does not have permission to look at the local machine registry
				log.Warn( "primary COM port probe failed: " + ex.Message );
				return null;
			}

			names.Sort( new PortNameComparer() );
			string[] portNames = new string[ names.Count ];
			names.CopyTo( portNames );
			return portNames;
		}

		/// <summary>
		/// a secondary method for finding the available local machine serial
		/// ports (not using the registry)
		/// </summary>
		/// <remarks>this may find an incomplete and/or inaccurate list</remarks>
		private static string[] SecondaryProbeForPortNames( )
		{
			System.Collections.ArrayList names = new System.Collections.ArrayList();
			for ( int port = 1; port < 10; ++port )
			{
				string portName = ToPortName( port );
				IntPtr handle = CreateFile( "\\\\.\\" + portName,
					GENERIC_READ,
					0,
					IntPtr.Zero,
					OPEN_EXISTING,
					FILE_FLAG_OVERLAPPED | FILE_ATTRIBUTE_NORMAL,
					IntPtr.Zero );
				if ( handle != INVALID_HANDLE_VALUE )
				{
					names.Add( portName );
					try
					{
						if (CloseHandle(handle) == false)
						{
							int winrc = Marshal.GetLastWin32Error();
							throw new ApplicationException(
								String.Format( "{0} close failed with Win32 error code: {1}",
									portName, winrc) );
						}
						log.Debug( String.Format( "probe found {0}, handle {1} opened/closed",
							portName, handle ) );
					}
					finally
					{
						handle = INVALID_HANDLE_VALUE;
					}
				}
			}

			if ( names.Count == 0 )
			{	// this secondary method of finding ports found nothing;
				// fall back to a list consisting of the default commport.
				log.Warn( "secondary COM port probe failed" );
                names.Add( ToPortName (ServeronConfiguration.GetAppSettingValue<int>("DefaultCommPort")));
			}

			names.Sort( new PortNameComparer() );
			string[] portNames = new string[ names.Count ];
			names.CopyTo( portNames );
			return portNames;
		}

		/// <summary>
		/// class used to sort serial port name strings
		/// in numerical port order
		/// </summary>
		private class PortNameComparer : System.Collections.IComparer
		{
			#region IComparer Members

			/// <summary>
			/// compare two communication port name strings of
			/// the form 'COMd' where d is a number from 1 to 255
			/// </summary>
			/// <param name="x">first communication port name</param>
			/// <param name="y">second communication port name</param>
			/// <returns>-1 if x comes before y, +1 if x comes after
			/// y and 0 if x and y are the same</returns>
			public int Compare( object x, object y )
			{
				int xPortNum = ToPortNumber( x as string );
				int yPortNum = ToPortNumber( y as string );
				if ( ( xPortNum == InvalidPortNumber )
					|| ( yPortNum == InvalidPortNumber ) )
				{
					throw new System.InvalidOperationException( "serial port names invalid for Compare" );
				}

				if ( xPortNum < yPortNum )
				{
					return -1;
				}
				else if ( xPortNum > yPortNum )
				{
					return 1;
				}
				return 0;
			}

			#endregion
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Numeric value of comm port (1, 2, 3, ...) we are associated with.
		/// </summary>
		public int port;

		/// <summary>
		/// A readonly bool that indicates whether
		/// we have executed an Open successfully.
		/// </summary>
		public bool AlreadyOpen
		{
			get
			{
                return !_handle.IsInvalid;// (INVALID_HANDLE_VALUE != handle);
			}
		}

		#endregion

		#region DllImports

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/createfile.asp
		[DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern IntPtr CreateFile( string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile );
		
		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/closehandle.asp
		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle( IntPtr hCommDev );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/setcommstate.asp
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern int SetCommState( IntPtr hCommDev, ref DCB lpDCB );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/getcommstate.asp
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern int GetCommState( IntPtr hCommDev, ref DCB lpDCB );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/setcommtimeouts.asp
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern int SetCommTimeouts( IntPtr hCommDev, ref COMMTIMEOUhwSchedule lpCommTimeouts );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/writefile.asp
		[DllImport("kernel32.dll", SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int WriteFile( IntPtr hCommDev, byte *lpBuffer, int NumberOfBytesToWrite, ref int NumberOfBytesWritten, NativeOverlapped* overlapped );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/writefile.asp
		[DllImport("kernel32.dll", SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int WriteFile( IntPtr hCommDev, byte *lpBuffer, int NumberOfBytesToWrite, IntPtr NumberOfBytesWritten, NativeOverlapped* overlapped );

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/readfile.asp
		[DllImport("kernel32.dll", SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int ReadFile( IntPtr hCommDev, byte* bytes, int NumberOfBytesToRead, ref int NumberOfBytesRead, NativeOverlapped* overlapped ); 

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/readfile.asp
		[DllImport("kernel32.dll", SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int ReadFile( IntPtr hCommDev, byte* bytes, int NumberOfBytesToRead, IntPtr NumberOfBytesRead, NativeOverlapped* overlapped ); 

		[DllImport("kernel32.dll", SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		internal static extern bool SetEvent(IntPtr eventHandle);

		#endregion

		#region Public methods

		/// <summary>
		/// Return the stream interface to serial class.
		/// </summary>
		/// <returns></returns>
		public Stream GetStream()
		{
			return internalSerialStream;
		}

		/// <summary>
		/// Reads from a serial port.  Calls the Win32 ReadFile() API.
		/// Returns when the number of bytes requested are satisfied or
		/// when a timeout condition is met. 
		/// </summary>
		/// <param name="count">Max number of bytes to read</param>
		/// <returns>Byte array containing actual bytes read</returns>
		public byte[] Read(int count)
		{
			if (_handle.IsInvalid)//(INVALID_HANDLE_VALUE == handle)
				throw new ApplicationException("Comm Port Not Open");
			return ReadCore(count);
		}

		unsafe private byte[] ReadCore(int Count)
		{
			if (Count <= 0)
				return new byte[0];

			AsyncResult asyncResult = new AsyncResult();
			ManualResetEvent waitHandle = new ManualResetEvent(false);
			asyncResult._manualEvent = waitHandle;
			Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

			// Pack the Overlapped class, and store it in the async result
			NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback, null);
			byte[] buf = new byte[Count];

			fixed (byte *p = buf)
			{
				int r = ReadFile(handle, p, Count, IntPtr.Zero, intOverlapped);
				if (r == 0)
				{
					int hr = Marshal.GetLastWin32Error();
					if (hr != ERROR_IO_PENDING)
					{
						log.Debug("ReadFile failed: hr == " + hr);
						return new byte[0];
					}
					//log.Debug("ReadFile, Async IO pending");
					do
					{
						asyncResult._manualEvent.WaitOne();
						//log.Debug("Woke up from WaitOne");
					} while (asyncResult.IsCompleted == false);
					//log.Debug("ReadFile completed async (numBytes=" + asyncResult._numBytes + ")");
					byte [] ret = new byte[asyncResult._numBytes];
					Array.Copy(buf, ret, asyncResult._numBytes);
					return ret;
				}
				else	// read completed immediately
				{
					//log.Debug("ReadFile completed immediately (count=" + Count + ")");
					byte [] ret = new byte[Count];
					Array.Copy(buf, ret, Count);
					return ret;
				}
			}
		}

		/// <summary>
		/// Writes to a serial port.  Calls the Win32 WriteFile API.
		/// Returns the number of bytes written.  This can call can
		/// return early if a write timeout occurs.
		/// </summary>
		/// <param name="buf">Bytes to be written</param>
		/// <returns>Number of bytes written</returns>
		public int Write(byte [] buf)
		{
			if (_handle.IsInvalid)//(INVALID_HANDLE_VALUE == handle)
				throw new ApplicationException("Comm Port Not Open");
			return WriteCore(buf);
		}

		unsafe private int WriteCore(byte[] buf)
		{
			AsyncResult asyncResult = new AsyncResult();
			ManualResetEvent waitHandle = new ManualResetEvent(false);
			asyncResult._manualEvent = waitHandle;
			Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

			// Pack the Overlapped class, and store it in the async result
			NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback, null);
			int Count = buf.Length;

			fixed (byte *p = buf)
			{
				int r = WriteFile(handle, p, Count, IntPtr.Zero, intOverlapped);
				if (r == 0)
				{
					int hr = Marshal.GetLastWin32Error();
					if (hr != ERROR_IO_PENDING)
					{
						log.Debug("WriteFile failed: hr == " + hr);
						return 0;
					}
					//log.Debug("WriteFile, Async IO pending");
					do
					{
						asyncResult._manualEvent.WaitOne();
						//log.Debug("Woke up from WaitOne");
					} while (asyncResult.IsCompleted == false);
					Count = asyncResult._numBytes;
					//log.Debug("WriteFile completed async (numBytes=" + asyncResult._numBytes + ")");
					return Count;
				}
				else	// write completed immediately
				{
					log.Debug("WriteFile completed immediately (count=" + Count + ")");
					return Count;
				}
			}
		}

		// This is a the callback prompted when a thread completes any async I/O operation.  
		unsafe private static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
		{
			//log.Debug(String.Format("AsyncFSCallback(errorCode={0}, numBytes={1}, pOverlapped)", errorCode, numBytes));
			// Unpack overlapped
			Overlapped overlapped = Overlapped.Unpack(pOverlapped);

			// Extract async the result from overlapped structure
			AsyncResult asyncResult = (AsyncResult)overlapped.AsyncResult;
			asyncResult._numBytes = (int)numBytes;
			asyncResult._errorCode = (int)errorCode;
			asyncResult._isComplete = true;
			asyncResult._manualEvent.Set();
			Overlapped.Free(pOverlapped);
		}

		/// <summary>
		/// Write to a serial port (string version).
		/// </summary>
		/// <param name="strA">String to be written</param>
		/// <returns>Number of bytes written</returns>
		public int Write(string strA) 
		{
			ASCIIEncoding e = new ASCIIEncoding();
			byte[] buf = e.GetBytes(strA);
			return Write(buf);
		}

		/// <summary>
		/// Open a Serial line (and allocate an OS handle)
		/// </summary>
		public void Open(int delaySeconds)
		{
			log.Debug("Open Enter.");

			if (-1 == port)
			{
				throw new ApplicationException("Comm Port not set before use");
			}
			handle = CreateFile(
				"\\\\.\\COM" + port,
				GENERIC_READ | GENERIC_WRITE,
				0,
				IntPtr.Zero,
				OPEN_EXISTING,
				FILE_FLAG_OVERLAPPED | FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero);
            _handle = new SafeFileHandle(handle, true);
			if (_handle.IsInvalid) // (INVALID_HANDLE_VALUE == handle)
			{
				int ret = Marshal.GetLastWin32Error();
				throw new ApplicationException(String.Format("Comm Port #{0} Can Not Be Opened. Win32 Error: {1}", port, ret));
			}

			if (!ThreadPool.BindHandle(_handle))
			{
				int ret = Marshal.GetLastWin32Error();
                _handle.Close();
//				CloseHandle(handle);
				throw new ApplicationException(String.Format("Comm Port #{0} Can Not Be Opened (handle bind failed). Win32 Error: {1}", port, ret));
			}

			internalSerialStream = new SerialStream();
			internalSerialStream.Serial = this;

			if (0 != delaySeconds)
			{
				log.Debug("Sleeping " + delaySeconds + " seconds.");
				Thread.Sleep(delaySeconds * 1000);
			}

			log.Debug("Open Exit.");
		}

		/// <summary>
		/// Close a serial line (and release OS handle).
		/// </summary>
		public void Close()
		{
			if (this.internalSerialStream != null)
			{
				try
				{
					this.internalSerialStream.Close();
				}
				finally
				{
					this.internalSerialStream = null;
				}
			}
			if (!_handle.IsInvalid)//(INVALID_HANDLE_VALUE != handle)
			{
				try
				{
					if (CloseHandle(handle) == false)
					{
						int ret = Marshal.GetLastWin32Error();
						throw new ApplicationException(String.Format("Comm Port #{0} Can Not Be Closed. Win32 Error: {1}", port, ret));
					}
					log.Debug("OS Handle " + handle + " closed succefully.");
				}
				finally
				{
					handle = INVALID_HANDLE_VALUE;
                    _handle.SetHandleAsInvalid();
				}
			}
		}

		/// <summary>
		/// Set the baud rate and other state flags of the previously opened COM Port.
		/// </summary>
		/// <param name="rate">Desired baud (must be supported by hardware)</param>
		public void Baud(int rate)
		{
			log.Debug("Baud (" + rate + ")");

			if (INVALID_HANDLE_VALUE != handle)
			{
				DCB dcb = new DCB();

				if (0 == GetCommState(handle, ref dcb))
				{
					int ret = Marshal.GetLastWin32Error();
					throw new ApplicationException("GetCommState failed. Win32 Error: " + ret);
				}

				log.Debug("Before DCB: \n" + PrintDCB(dcb));
				dcb.BaudRate = rate;
				dcb.Flags &= ~(DCB_FLAGS_CLEAR);
				dcb.Flags |= (DCB_FLAGS_SET);
				dcb.Parity = 0;
				dcb.StopBits = 0;
				dcb.ByteSize = 8;
				log.Debug("After DCB: \n" + PrintDCB(dcb));

				if (0 == SetCommState(handle, ref dcb))
				{
					int ret = Marshal.GetLastWin32Error();
					throw new ApplicationException("SetCommState failed. Win32 Error: " + ret);
				}
			}
		}

		/// <summary>
		/// set serial flow control mode to "none"
		/// </summary>
		/// <remarks>call this after <see>Baud</see> because that
		/// method forces its own control flow setting (which
		/// may be different from "none".)</remarks>
        public void SetNoFlowControl(uint SpecialFlag)
		{
			log.Debug("SetNoFlowControl ()");

			if (INVALID_HANDLE_VALUE != handle)
			{
				DCB dcb = new DCB();

				if (0 == GetCommState(handle, ref dcb))
				{
					int ret = Marshal.GetLastWin32Error();
					throw new ApplicationException("GetCommState failed. Win32 Error: " + ret);
				}

				log.Debug("Before DCB: \n" + PrintDCB(dcb));
                if (SpecialFlag != 0)
                {
                    dcb.Flags &= ~(SpecialFlag);                  
                }
                else
                {
                    dcb.Flags &= ~(DCB_FLAGS_NOFLOWCONTROL_CLEAR);                      
                }				
				log.Debug("After DCB: \n" + PrintDCB(dcb));

				if (0 == SetCommState(handle, ref dcb))
				{
					int ret = Marshal.GetLastWin32Error();
					throw new ApplicationException("SetCommState failed. Win32 Error: " + ret);
				}
			}
		}

		/// <summary>
		/// Set device timeouts (using COMMTIMEOUhwSchedule structure)
		///   Read Interval Timeout:
		/// Maximum time allowed to elapse between the arrival of two characters on the communications line, in milliseconds.
		/// During a ReadFile operation, the time period begins when the first character is received. If the interval between
		/// the arrival of any two characters exceeds this amount, the ReadFile operation is completed and any buffered data
		/// is returned. A value of zero indicates that interval time-outs are not used.
		/// A value of MAXDWORD, combined with zero values for both the ReadTotalTimeoutConstant and ReadTotalTimeoutMultiplier
		/// members, specifies that the read operation is to return immediately with the characters that have already been received,
		/// even if no characters have been received.
		///   Read Total Timeout Multiplier:
		/// Multiplier used to calculate the total time-out period for read operations, in milliseconds. For each read operation,
		/// this value is multiplied by the requested number of bytes to be read.
		///   Read Total Timeout Constant:
		/// Constant used to calculate the total time-out period for read operations, in milliseconds. For each read operation,
		/// this value is added to the product of the ReadTotalTimeoutMultiplier member and the requested number of bytes.
		/// A value of zero for both the ReadTotalTimeoutMultiplier and ReadTotalTimeoutConstant members indicates that total
		/// time-outs are not used for read operations.
		///   Write Total Timeout Multiplier:
		/// Multiplier used to calculate the total time-out period for write operations, in milliseconds. For each write operation,
		/// this value is multiplied by the number of bytes to be written.
		///   Write Total Timeout Constant:
		/// Constant used to calculate the total time-out period for write operations, in milliseconds. For each write operation,
		/// this value is added to the product of the WriteTotalTimeoutMultiplier member and the number of bytes to be written.
		/// A value of zero for both the WriteTotalTimeoutMultiplier and WriteTotalTimeoutConstant members indicates that total
		/// time-outs are not used for write operations.
		/// 
		/// Reference: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/commtimeouts_str.asp
		/// </summary>
		/// <param name="rit">Read Interval Timeout</param>
		/// <param name="rttm">Read Total Timeout Multiplier</param>
		/// <param name="rttc">Read Total Timeout Constant</param>
		/// <param name="wttm">Write Total Timeout Multiplier</param>
		/// <param name="wttc">Write Total Timeout Constant</param>
		public void SetTimeouts(int rit, int rttm, int rttc, int wttm, int wttc)
		{
			log.Debug(String.Format("SetTimeouts({0},{1},{2},{3},{4})", rit, rttm, rttc, wttm, wttc));

			if (INVALID_HANDLE_VALUE != handle)
			{
				COMMTIMEOUhwSchedule commtimeouts = new COMMTIMEOUhwSchedule();

				commtimeouts.ReadIntervalTimeout = rit;
				commtimeouts.ReadTotalTimeoutMultiplier = rttm;
				commtimeouts.ReadTotalTimeoutConstant = rttc;
				commtimeouts.WriteTotalTimeoutMultiplier = wttm;
				commtimeouts.WriteTotalTimeoutConstant = wttc;

				if (0 == SetCommTimeouts(handle, ref commtimeouts))
				{
					throw new ApplicationException("SetCommTimeouts failed. Win32 Error: " + Marshal.GetLastWin32Error());
				}
			}
		}

		/// <summary>
		/// gets an array of the local machine's serial ports' names
		/// </summary>
		/// <returns>array of the local machine's serial ports' names</returns>
		/// <remarks>the intent is to match the behavior of the .net 2.x
		/// method System.IO.Ports.SerialPort.GetPortNames(). There is an
		/// assumption that the serial port names are in the form 'COMd' where
		/// d is a decimal number from 1 to 255. This is consistent with the
		/// documentation of the SerialPort() class constructor with a
		/// port name string argument.
		/// (I couldn't verify this directly since the .net 2.x framework
		/// was not available to me at the time this was written.)</remarks>
        /// 
        /// depreciated use: System.IO.Ports.SerialPort.GetPortNames();
		public static string[] GetPortNames()
		{
			if ( _portNames == null )
			{
				_portNames = ProbeForPortNames( );
				if ( _portNames == null )
				{
					_portNames = SecondaryProbeForPortNames( );
				}
			}
			return _portNames;
		}

		#endregion

		#region port number/port name conversion

		public const int InvalidPortNumber = -1;

		/// <summary>
		/// get port number from port name string
		/// </summary>
		/// <param name="name">serial port name, expected to start with
		/// 'COM' (\\.\ prefix is not accepted)</param>
		/// <returns>InvalidPortNumber if port name is not valid, otherwise
		/// the port number</returns>
		public static int ToPortNumber( string name )
		{
			int portNumber = InvalidPortNumber;
			Regex portNumRegEx =
				new Regex( "^COM(?'n'[0-9]{1,3})$", RegexOptions.IgnoreCase );
			Match numParse = portNumRegEx.Match( name );
			if ( numParse.Success )
			{
				portNumber = System.Convert.ToInt32( numParse.Groups["n"].Value );
				if ( ( portNumber < 1 ) || ( portNumber > 255 ) )
				{
					portNumber = InvalidPortNumber;
				}
			}
			return portNumber;
		}

		/// <summary>
		/// get port name from a port number
		/// </summary>
		/// <param name="number">number from 1 to 255</param>
		/// <returns>null if port number is invalid, otherwise returns
		/// port name</returns>
		public static string ToPortName( int number )
		{
			if ( ( number < 1 ) || ( number > 255 ) )
			{
				return null;
			}
			string portName = String.Format( "COM{0:D}", number );
			return portName;
		}

		#endregion

		#region Construction

		/// <summary>
		/// After deserialization, set any needed state.
		/// 
		/// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconcustomserialization.asp
		/// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemRuntimeSerializationIDeserializationCallbackClassTopic.asp
		/// 
		/// Objects are reconstructed from the inside out; and calling methods during deserialization can have undesirable side effects,
		/// because the methods called might refer to object references that have not been deserialized by the time the call is made.
		/// If the class being deserialized implements the IDeserilizationCallback, the OnDeserialization method is automatically called
		/// when the entire object graph has been deserialized. At this point, all the child objects referenced have been fully restored.
		/// A hash table is a typical example of a class that is difficult to deserialize without using the event listener described above.
		/// It is easy to retrieve the key/value pairs during deserialization, but adding these objects back to the hash table can cause
		/// problems, because there is no guarantee that classes that derived from the hash table have been deserialized. Calling methods
		/// on a hash table at this stage is therefore not advisable.
		/// </summary>
		/// <param name="sender">Sender</param>
		void IDeserializationCallback.OnDeserialization(Object sender) 
		{
			// After being deserialized, initialize our handle to invalid.
			if (handle != INVALID_HANDLE_VALUE)
				log.Debug("BUG AVOIDED:  setting handle to invalid");
			handle = INVALID_HANDLE_VALUE;
		}

		public Serial(int portNum)
		{
			handle = INVALID_HANDLE_VALUE;
			port = portNum;
		}

		/// <summary>
		/// Construct a serial object.  
		/// </summary>
		public Serial()
		{
			handle = INVALID_HANDLE_VALUE;
			port = -1;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Free other state (managed objects).
				this.internalSerialStream.Close();
				this.internalSerialStream = null;
			}
			// Free your own state (unmanaged objects).
			// Set large fields to null.
			if (handle != INVALID_HANDLE_VALUE)
				CloseHandle(handle);
		}

		// Use C# destructor syntax for finalization code.
		~Serial()
		{
			// Simply call Dispose(false).
			Dispose (false);
		}


		#endregion
	}
	#region AsyncResult Class

	unsafe internal class AsyncResult : IAsyncResult
	{
		internal Object _userStateObject = null;
		internal bool _completedSynchronously = false;
		internal WaitHandle _waitHandle = null;
		internal bool _isComplete = false;

		internal ManualResetEvent _manualEvent;
		internal int _numBytes;
		internal int _errorCode;

		public AsyncResult()
		{
		}
		#region IAsyncResult Members

		public object AsyncState
		{
			get { return _userStateObject; }
		}

		public bool CompletedSynchronously
		{
			get { return _completedSynchronously; }
		}

		public WaitHandle AsyncWaitHandle
		{
			get { return _waitHandle; }
		}

		public bool IsCompleted
		{
			get { return _isComplete; }
			set { _isComplete = value; }
		}

		#endregion
	}

	#endregion

	#region SerialStream Class

	public class SerialStream : Stream
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SerialStream));
		private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		private COMSTAT comstat = new COMSTAT();

		// Ref: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/clearcommerror.asp
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern int ClearCommError( IntPtr hCommDev, out UInt32 lpErrors, out COMSTAT cs );
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern int ClearCommError( IntPtr hCommDev, out UInt32 lpErrors, IntPtr lpStat );

		//Constants for lpErrors:
		internal const UInt32 CE_RXOVER = 0x0001;
		internal const UInt32 CE_OVERRUN = 0x0002;
		internal const UInt32 CE_RXPARITY = 0x0004;
		internal const UInt32 CE_FRAME = 0x0008;
		internal const UInt32 CE_BREAK = 0x0010;
		internal const UInt32 CE_TXFULL = 0x0100;
		internal const UInt32 CE_PTO = 0x0200;
		internal const UInt32 CE_IOE = 0x0400;
		internal const UInt32 CE_DNS = 0x0800;
		internal const UInt32 CE_OOP = 0x1000;
		internal const UInt32 CE_MODE = 0x8000; 

		[StructLayout(LayoutKind.Explicit)]
			internal struct COMSTAT
		{
			internal const uint fCtsHold = 0x1;
			internal const uint fDsrHold = 0x2;
			internal const uint fRlsdHold = 0x4;
			internal const uint fXoffHold = 0x8;
			internal const uint fXoffSent = 0x10;
			internal const uint fEof = 0x20;
			internal const uint fTxim = 0x40;
			[FieldOffset(0)]	internal UInt32 Flags;
			[FieldOffset(4)]	internal UInt32 cbInQue;
			[FieldOffset(8)]	internal UInt32 cbOutQue;
		}

		private Serial _serial;

		// a hack for now to get the Serial object
		public Serial Serial
		{
			set { _serial = value; }
		}

		public bool DataAvailable
		{
			get 
			{
				if (_serial == null || _serial._handle.IsInvalid )
					throw new InvalidOperationException("DataAvailable - port not open");

				comstat.Flags = 0;
				comstat.cbInQue = 0;
				comstat.cbOutQue = 0;
				UInt32 errorStatus = 0;

				if (0 == ClearCommError(_serial.handle, out errorStatus, out comstat))
				{
					throw new InvalidOperationException("ClearCommError failed. Win32 Error: " + Marshal.GetLastWin32Error());
				}

				//if (comstat.cbInQue != 0)
				//	log.Debug(String.Format("ClearCommError: flags 0x{0:X}, cbInQueue {1}", errorStatus, comstat.cbInQue));

				return (comstat.cbInQue != 0);
			}
		}

		// These six properties are required for SerialStream to inherit from abstract Stream Class
		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }	
		}

		// These nn methods are required for SerialStream to inherit from abstract Stream Class
		public override void Flush()
		{
			//log.Debug("Flush() called (ignored)");
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			byte [] result = _serial.Read(count);
			if (result.Length == 0)
				return 0;
			result.CopyTo(buffer, offset);
			return result.Length;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte [] writeBuffer = new byte[count];
			Array.Copy(buffer, offset, writeBuffer, 0, count);
			_serial.Write(buffer);
		}
	}

	#endregion
}