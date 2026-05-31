// TMx CLI adapter C# code
// Copyright 2005 Serveron Corporation. All rights reserved.

//*********************************************************************
//* RasAPI.cs                                                         *
//*                                                                   *
//* Copyright 2003 ComponentScience Corporation.                      *
//* All Rights Reserved.                                              *
//*********************************************************************

#region ComponentScience License Block
//******* BEGIN LICENSE BLOCK *****************************************
//*                                                                   *
//* This file is part of the "Elements(Ex)" component Library.        *
//*                                                                   *
//* The contents of this file are subject to the ComponentScience     *
//* Corporation Elements(Ex) License (the "License"), and may not be  *
//* used except in compliance with the License. A copy of the License *
//* should have been installed in the library's root installation     *
//* directory.  You may also obtain a copy of the License at...       *
//*                                                                   *
//* http://www.componentscience.net/ElementsEx/license.pdf            *
//*                                                                   *
//* Software distributed under this License is distributed on an      *
//* "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or    *
//* implied. See the License for the specific language governing      *
//* rights and limitations under the License.                         *
//*                                                                   *
//*     ##   ##                                                       *
//*    #    #  #                                                      *
//*   #    #    #     The Elements(Ex) component library is ©2003     *
//*  #    #      #    ComponentScience Incorporated.                  *
//*  #      #    #    All Rights Reserved.                            *
//*   #    #    #                                                     *
//*    #  #    #                                                      *
//*     ##   ##                                                       *
//*                                                                   *
//******* END LICENSE BLOCK *******************************************
#endregion //ComponentScience License Block

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;

namespace Serveron.Utility.Core
{
	#region Design description
	/*
		The RAS header contains several #if defines for different Windows versions.
		This is how they defined it:
		  WINVER values in this file:
			WINVER < 0x400 = Windows NT 3.5, Windows NT 3.51
			WINVER = 0x400 = Windows 95, Windows98, Windows NT4 (default)
			WINVER > 0x400 = Windows NT4 enhancements
			WINVER = 0x500 = Windows 2000
		We're really not concerned with < 0x400 since it doesn't support .NET
		The noticable differences are primarily in the structs, which are passed
		to and fro with the exported methods. The RAS version is determined based on
		the size of those structs, and they are not necessarily forwards compatible. 
		Since the size of the struct needs to be known at compile time for the MarshalAs 
		attribute, we could either use the 0x400 version or create multiple versions of 
		all of the structs and methods that use them. We chose to implement the 0x400 version.       
      
	  Not implementing the RASSUBENTRY, or RASGETCREDENTIALS, nor the AutoDial DLL, nor the Projection stuff
    
	Several DLLImports are defined here but not used.
	  */
	#endregion

	#region RasApi
	/// <summary>
	/// Imported methods from RASAPI32.DLL
	/// </summary>
	/// <remarks>The DLL imports are used by the <see cref="RasClient"/> component to access
	/// the RAS API.</remarks>
	public sealed class RasApi
	{
		#region Front matter
		private RasApi()
		{
			// empty constructor to make FxCop happy
		}
		internal const string RasDll = "rasapi32.dll";
		internal const string RasDlgDll = "rasdlg.dll";
		#endregion

		#region RasDial
		/// <summary>
		/// The RasDial function establishes a RAS connection between a RAS client and a RAS server.
		/// </summary>
		/// <param name="lpRasDialExtensions">dial extensions</param>
		/// <param name="lpszPhonebook">Path\name of phonebook file</param>
		/// <param name="lpRasDialParams">Dialing parameters</param>
		/// <param name="dwNotifierType">Type of RasDial callback,0=RasDialFunc, 1=RasDialFunc1, 2=RasDialFunc2</param>
		/// <param name="lpvNotifier">Callback function pointer</param>
		/// <param name="lphRasConn">Connection handle</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasDial(
			[In]RASDIALEXTENSIONS lpRasDialExtensions,
			[In]string lpszPhonebook,
			[In]RASDIALPARAMS lpRasDialParams,
			uint dwNotifierType,
			Delegate lpvNotifier,
			ref IntPtr lphRasConn
			);
		#endregion

		#region RasDialDlg
		/// <summary>
		/// Attempts to establish a RAS connection using a specified phone-book entry and the credentials of the logged-on user. The function displays a stream of dialog boxes that indicate the state of the connection operation. 
		/// </summary>
		/// <param name="lpszPhonebook">full path and filename of the phone-book file</param>
		/// <param name="lpszEntry">name of the phone-book entry to dial</param>
		/// <param name="lpszPhoneNumber">replacement phone number to dial</param>
		/// <param name="lpInfo">structure that contains additional parameters</param>
		[DllImport(RasDlgDll)]    
		internal extern static bool RasDialDlg(
			[In]string lpszPhonebook,
			[In]string lpszEntry,
			[In]string lpszPhoneNumber,
			[Out]out RASDIALDLG lpInfo
			);
		#endregion

		#region RasEnumConnection
		/// <summary>
		/// The RasEnumConnections function lists all active RAS connections.
		/// </summary>
		/// <param name="lprasconn">Connection data</param>
		/// <param name="lpcb">size of the connection data buffer</param>
		/// <param name="lpcConnections">Number of connections in the buffer</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasEnumConnections(
			[In,Out]RasConnection[] lprasconn,
			ref int lpcb,
			out int lpcConnections
			);
		#endregion

		#region RasEnumEntries
		/// <summary>
		/// lists all entry names in a remote access phone book. 
		/// </summary>
		/// <param name="reserved">Reserved, must be null</param>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lprasentryname">Array of phonebook entries</param>
		/// <param name="lpcb">Size of buffer</param>
		/// <param name="lpcEntries">Number of entries</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasEnumEntries (
			string reserved,
			string lpszPhonebook,
			[In,Out]RASENTRYNAME[] lprasentryname,
			ref int lpcb,
			out int lpcEntries
			);
		#endregion

		#region RasGetConnectStatus
		/// <summary>
		/// Retrieves information on the current status of the specified remote access connection.
		/// </summary>
		/// <param name="hrasconn">Handle to the RAS connection</param>
		/// <param name="lpRasConnectionState">Returned status</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasGetConnectStatus(
			IntPtr hrasconn,
			[In,Out]RasConnectState lpRasConnectionState
			);
		#endregion

		#region RasGetProjectionInfo
		[DllImport(RasDll)]
		internal extern static uint RasGetProjectionInfo(
			IntPtr hrasconn,
			RasProjection rasprojection,
			[In,Out]RASPPPIP lpprojection,
			ref int lpcb
			);
		#endregion

		#region RasGetErrorString
		/// <summary>
		/// Obtains an error message string for a specified RAS error value. 
		/// </summary>
		/// <param name="uErrorValue">Error to get string for</param>
		/// <param name="lpszErrorString">The error string</param>
		/// <param name="cBufSize">Size, in chars, of the string</param>
		/// <returns></returns>
		// NOTE: FxCop says this isn't called..it isn't, but will be once
		//       we get around to changing RasError so it will return a descriptive string
		[DllImport(RasDll)]
		internal extern static uint RasGetErrorString(
			uint uErrorValue,        // error to get string for
			string lpszErrorString,  // buffer to hold error string
			[In]int cBufSize           // size, in characters, of buffer
			);
		#endregion
    
		#region RasHangup
		/// <summary>
		/// Terminates the connection
		/// </summary>
		/// <param name="hrasconn">Handle of the connection to terminate</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasHangUp(
			IntPtr hrasconn
			);
		#endregion

		#region RasCreatePhoneboookEntry
		/// <summary>
		/// Creates a new phone-book entry.
		/// </summary>
		/// <remarks>Displays a dialog box in which the user types information about the phone-book entry.</remarks>
		/// <param name="hwnd">Handle of parent window</param>
		/// <param name="pszPhonebook">Full path\name of phonebook file</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasCreatePhonebookEntry( 
			IntPtr hwnd, 
			string pszPhonebook 
			);      
		#endregion

		#region RasEditPhoneBookEntry
		/// <summary>
		/// edits an existing phone-book entry.
		/// </summary>
		/// <remarks>Displays a dialog box in which the user can modify the existing information.</remarks>
		/// <param name="hwnd">Handle of parent window</param>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lpszEntryName">The name of the entry to edit</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasEditPhonebookEntry(
			IntPtr hwnd,
			string lpszPhonebook,
			string lpszEntryName
			);
		#endregion
    
		#region RasSetEntryDialParams
		/* not called, here for future expansion
			/// <summary>
			/// Changes the connection information saved by the last successful call to the RasDial or RasSetEntryDialParams function for a specified phonebook entry.
			/// </summary>
			/// <param name="lpszPhonebook">Full path\name of the phonebook file</param>
			/// <param name="lprasdialparams">New connection parameters</param>
			/// <param name="fRemovePassword">Indicates whether to remove the password or not</param>
			/// <returns>0 if successful</returns>
			[DllImport(RasDll)]
			internal extern static uint RasSetEntryDialParams(
			  string lpszPhonebook,
			  RasStructs.RASDIALPARAMS lprasdialparams,
			  out bool fRemovePassword
			  );
		*/
		#endregion

		#region RasGetEntryDialParams
		/* not called, here for future expansion
			/// <summary>
			/// Retrieves the connection information saved by the last successful call to the RasDial or RasSetEntryDialParams function for a specified phone-book entry.
			/// </summary>
			/// <param name="lpszPhonebook">Full path\name of the phonebook file</param>
			/// <param name="lprasdialparams">Connection parameters</param>
			/// <param name="lpfPassword">Indicates whether the password was retrieved</param>
			/// <returns></returns>
			[DllImport(RasDll)]
			internal extern static uint RasGetEntryDialParams(
			  string lpszPhonebook,
			  [In,Out]RasStructs.RASDIALPARAMS lprasdialparams,
			  out bool lpfPassword
			  );
		*/      
		#endregion

		#region RasEnumDevices
		/// <summary>
		/// Returns the name and type of all available RAS-capable devices.
		/// </summary>
		/// <param name="lpRasDevInfo">Receives information about devices</param>
		/// <param name="lpcb">Size of the buffer</param>
		/// <param name="lpcDevices">Number of devices</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasEnumDevices(
			[In,Out]RasDeviceInfo[] lpRasDevInfo,
			ref int lpcb,
			out int lpcDevices
			);
		#endregion

		#region RasGetCountryInfo
		/* not called, here for future expansion
			/// <summary>
			/// Retrieves country-specific dialing information from the Windows Telephony list of countries.
			/// </summary>
			/// <param name="lpRasCtryInfo">Retrieved country info</param>
			/// <param name="lpdwSize">Size of the structure</param>
			/// <returns></returns>
			[DllImport(RasDll)]
			internal extern static uint RasGetCountryInfo(
			  [In,Out]RasStructs.RASCTRYINFO lpRasCtryInfo, // buffer that receives country information
			  out int lpdwSize  // size, in bytes, of the buffer 
			  );
		*/      
		#endregion

		#region RasGetEntryProperties
		/// <summary>
		/// Retrieves the properties of a phone-book entry.
		/// </summary>
		/// <param name="lpszPhonebook">Full path\name of the phonebook file</param>
		/// <param name="lpszEntry">The entry name to get properties for</param>
		/// <param name="lpRasEntry">Entry information</param>
		/// <param name="lpdwEntryInfoSize">Size of the entry information</param>
		/// <param name="lpbDeviceInfo">Device-specific information</param>
		/// <param name="lpdwDeviceInfoSize">Size of the device-specific information</param>
		/// <returns></returns>
		[DllImport(RasDll)]
		internal extern static uint RasGetEntryProperties(
			string lpszPhonebook,
			string lpszEntry,
			[In,Out]ref RasEntry lpRasEntry,
			[In,Out]ref int lpdwEntryInfoSize,
			int lpbDeviceInfo,
			int lpdwDeviceInfoSize
			);
		#endregion

		#region RasSetEntryProperties
		/// <summary>
		/// Changes the connection information for an entry in the phone book or creates a new phone-book entry.
		/// </summary>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lpszEntry">Entry name to change/add</param>
		/// <param name="lpRasEntry">Entry information</param>
		/// <param name="dwEntryInfoSize">Size of lpRasEntry</param>
		/// <param name="lpbDeviceInfo">Device-specific information</param>
		/// <param name="dwDeviceInfoSize">Size of the device information</param>
		/// <returns></returns>
		[DllImport(RasDll)]
		internal extern static uint RasSetEntryProperties(
			string lpszPhonebook,
			string lpszEntry,
			ref RasEntry lpRasEntry,
			int dwEntryInfoSize,
			int lpbDeviceInfo,
			int dwDeviceInfoSize
			);
		#endregion

		#region RasRenameEntry
		/// <summary>
		/// Changes the name of an entry in a phone book.
		/// </summary>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lpszOldEntry">Old entry name</param>
		/// <param name="lpszNewEntry">New entry name</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasRenameEntry(
			string lpszPhonebook,
			string lpszOldEntry,
			string lpszNewEntry
			);
		#endregion

		#region RasDeleteEntry
		/// <summary>
		/// Deletes an entry from a phone book
		/// </summary>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lpszEntry">Entry name to delete</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasDeleteEntry(
			string lpszPhonebook,
			string lpszEntry
			);
		#endregion

		#region RasValidateEntryName
		/// <summary>
		/// Validate the format of a connection entry name
		/// </summary>
		/// <param name="lpszPhonebook">Full path\name of phonebook file</param>
		/// <param name="lpszEntry">Entry name to validate</param>
		/// <returns>0 if successful</returns>
		[DllImport(RasDll)]
		internal extern static uint RasValidateEntryName(
			string lpszPhonebook,
			string lpszEntry
			);
		#endregion
    
		#region RasGetConnectionStatistics
		/// <summary>
		/// Returns statistic information for the connection
		/// </summary>
		[DllImport(RasDll, CharSet=CharSet.Auto, SetLastError=true)]
		internal extern static uint RasGetConnectionStatistics(
			IntPtr hrasconn,
			ref RasStatistics lpRasStatistics
			);
		#endregion

		#region RasClearConnectionStatistics
		/// <summary>
		/// Clears the connection statistics
		/// </summary>
		[DllImport(RasDll)]
		internal extern static uint RasClearConnectionStatistics(
			IntPtr hrasconn
			);
		#endregion

	}
	#endregion

	#region Ras Structs
	#region RasConnection
	/// <summary>
	/// Identifies an active RAS connection.
	/// </summary>
	/// <remarks></remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct RasConnection 
	{       
		/// <summary>
		/// The size of the struct, used to indicate the version
		/// </summary>
		/// <remarks></remarks>
		internal int Size; 
		// FxCop will tag this as needing IDisposable, but this isn't allocating anything
		/// <summary>
		/// The handle of the RAS connection
		/// </summary>
		/// <remarks>Accessed via <see cref="ConnectionHandle"/></remarks>
		internal IntPtr connectionHandle;
		/// <summary>
		/// The handle of the connection
		/// </summary>
		/// <value>ConnectionHandle should be considered a readonly property, however
		/// it needs a set method for the DLL Import.</value>
		/// <remarks>RAS manages connections with Window Handles, ConnectionHandle is
		/// provided as an int for ease of use.</remarks>
		public int ConnectionHandle
		{
			get { return connectionHandle.ToInt32(); }
			set { connectionHandle = new IntPtr(value); }
		}

		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxEntryName+1)]
		private string entryName;
		/// <summary>
		/// The name of the phonebook entry
		/// </summary>
		/// <value>A string determining the name of the associated phone book entry.</value>
		/// <remarks></remarks>
		public string EntryName
		{
			get { return entryName; }
			set { entryName = value; }
		}

		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceType+1)]
		private string deviceType;
		/// <summary>
		/// The type of device used for this connection
		/// </summary>
		/// <value>A string indicating the type of device</value>
		/// <remarks></remarks>
		public string DeviceType
		{
			get { return deviceType; }
			set { deviceType = value; }
		}

		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceName+1)]
		private string deviceName;
		/// <summary>
		/// The name of the device being used for this connection
		/// </summary>
		/// <value>A string indicating the name of the device used to establish the connection</value>
		/// <remarks></remarks>
		public string DeviceName
		{
			get { return deviceName; }
			set { deviceName = value; }
		}
	}
	#endregion

	#region RasConnectState
	/// <summary>
	/// Describes the status of a RAS connection
	/// </summary>
	/// <remarks></remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal class RasConnectState 
	{ 
		internal readonly int dwSize = Marshal.SizeOf(typeof(RasConnectState)); 
		public RasConnectionState rasconnstate = RasConnectionState.OpenPort; 
		public int dwError = 0; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceType+1)]
		public string szDeviceType = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceName+1)]
		public string szDeviceName = null; 
	}
	#endregion

	#region RASDIALDLG
	[StructLayout(LayoutKind.Sequential)]
	internal struct RASDIALDLG
	{
		internal int dwSize;
		// FxCop will tag this as needing IDisposable, but this isn't allocating anything
		public IntPtr hwndOwner;
		public int dwFlags;
		public int xDlg;
		public int yDlg;
		public int dwSubEntry;
		public int dwError;
		public int reserved;
		public int reserved2;
	}
	#endregion

	#region RASDIALPARAMS
	/// <summary>
	/// Describes connection establishment parameters.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class RASDIALPARAMS 
	{ 
		public int dwSize = Marshal.SizeOf(typeof(RASDIALPARAMS)); 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxEntryName+1)]
		public string szEntryName = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxPhoneNumber+1)]
		public string szPhoneNumber = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxCallbackNumber+1)]
		public string szCallbackNumber = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.UNLen+1)]
		public string szUserName = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.PWLen+1)]
		public string szPassword = null; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.DNLen+1)]
		public string szDomain = null; 
		public int dwSubEntry = 0;
		public int dwCallbackId = 0;
	}
	#endregion

	#region RASDIALEXTENSIONS
	/// <summary>
	/// Describes extended connection establishment options.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class RASDIALEXTENSIONS 
	{ 
		public readonly int dwSize = Marshal.SizeOf(typeof(RASDIALEXTENSIONS));
		public uint dwfOptions = 0;
		public int hwndParent = 0;
		public int reserved = 0;
	}
	#endregion

	#region RASENTRYNAME
	/// <summary>
	/// Describes an enumerated RAS phone book entry name.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RASENTRYNAME 
	{ 
		/// <summary>
		/// The size of the structure
		/// </summary>
		public int dwSize;
		/// <summary>
		/// The string defining the entry name
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxEntryName + 1)]
		public string szEntryName; 
	}
	#endregion

	#region RasDeviceInfo
	/// <summary>
	/// Information describing a RAS-capable device.
	/// </summary>
	/// <remarks>Used by the <see cref="RasClient.ListDevices"/> method</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct RasDeviceInfo
	{
		/// <summary>
		/// The size of the structure
		/// </summary>
		/// <remarks>Determines the version of the structure</remarks>
		internal int Size;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceType+1)]
		private string deviceType;  
		/// <summary>
		/// The device type
		/// </summary>
		/// <remarks></remarks>
		public string DeviceType
		{
			get { return deviceType; }
			set { deviceType = value; }
		}

		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxDeviceName+1)]
		private string deviceName;
		/// <summary>
		/// The name of the device
		/// </summary>
		/// <remarks></remarks>
		public string DeviceName
		{
			get { return deviceName; }
			set { deviceName = value; }
		}
	}
	#endregion

	#region RASCTRYINFO
	/// <summary>
	/// RAS country information (currently retrieved from TAPI).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RASCTRYINFO 
	{
		public UInt32 dwSize;
		public UInt32 dwCountryID;
		public UInt32 dwNextCountryID;
		public UInt32 dwCountryCode;
		public UInt32 dwCountryNameOffset;
	} 
	#endregion

	#region RASIPADDR
	/// <summary>
	/// The RASIPADDR structure contains an IP address.
	/// </summary>
	/// <remarks>
	/// The RASENTRY structure uses this structure to specify the IP addresses 
	/// of various servers associated with an entry in a RAS phone book.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct RasIPAddress 
	{
		/// <summary>
		/// Specifies the value of the first of four positions in the IP address. 
		/// </summary>
		byte    a;
		/// <summary>
		/// Specifies the value of the second of four positions in the IP address. 
		/// </summary>
		byte    b;
		/// <summary>
		/// Specifies the value of the third of four positions in the IP address. 
		/// </summary>
		byte    c;
		/// <summary>
		/// Specifies the value of the fourth of four positions in the IP address. 
		/// </summary>
		byte    d;
		/// <summary>
		/// Returns an IPAddress for the address byte array 
		/// </summary>
		/// <returns>An IPAddress indicating the address</returns>
		public IPAddress ToIPAddress()
		{
			return new IPAddress(new byte[4] {a,b,c,d});
		}
	}
	#endregion

	#region RASPPPIP
	/// <summary>
	/// Contains the result of a PPP IP projection operation.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class RASPPPIP	// WINVER = 0x500 version
	{
		public int dwSize = Marshal.SizeOf(typeof(RASPPPIP));
		public int dwError = 0;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxIpAddress+1)]
		public string szIpAddress = null;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxIpAddress+1)]
		public string szServerIpAddress = null;
		//public int dwOptions = 0;
		//public int dwServerOptions = 0;
	}
	#endregion

	#region RasEntry
	/// <summary>
	/// Defines a RAS phonebook entry (connectoid)
	/// </summary>
	/// <remarks>RasEntry defines a RAS phonebook entry</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct RasEntry
	{
		/// <summary>
		/// The size, in bytes, of the RASENTRY structure, identifies the version of the structure
		/// </summary>
		/// <remarks>Determines the struct versions</remarks>
		internal int Size;
    
		//private int optionsFlags;
		/// <summary>
		/// A set of bit flags that specify connection options, see RasEntryOptions enum
		/// </summary>
		/// <remarks></remarks>
		public int OptionsFlags;

		/// <summary>
		/// Specifies the TAPI country identifier.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int CountryId;

		/// <summary>
		/// Specifies the country code portion of the phone number.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int CountryCode;

		/// <summary>
		/// Specifies the area code
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxAreaCode+1)]
		public string AreaCode;
    
		/// <summary>
		/// Specifies the telephone number of the RAS server
		/// </summary>
		/// <remarks>Accessed via <see cref="RasClient.PhoneNumber"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxPhoneNumber + 1)]
		public string LocalPhoneNumber;

		/// <summary>
		/// Specifies the offset, in bytes, of the beginning of the struct to a list of 
		/// consecutive strings providing alternate phone numbers.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int AlternateOffset;

		// PPP/Ip
		/// <summary>
		/// Specifies the IP address to be used while this connection is active.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public RasIPAddress IPAddress;

		/// <summary>
		/// Specifies the IP address of the DNS server to be used for this connection.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public RasIPAddress IPAddressDns;

		/// <summary>
		/// Specifies the IP address of a secondary DNS server.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public RasIPAddress IPAddressDnsAlt;

		/// <summary>
		/// Specifies the IP address of the WINS server.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public RasIPAddress IPAddressWins;

		/// <summary>
		/// Specifies the IP address of a secondary WINS server.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public RasIPAddress IPAddressWinsAlt;

		// Framing
		/// <summary>
		/// Specifies the network protocol frame size.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int FrameSize;

		/// <summary>
		/// Specifies the network protocols to negotiate. See RasEntryNetProtocols.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int NetProtocolsFlags;

		/// <summary>
		/// Specifies the framing protocols used by the server. See RasEntryFramingProtocol
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int FramingProtocol;

		// Scripting
		/// <summary>
		/// Specifies a string containing the name of a script file.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxPath)]
		public string Script;

		// AutoDial
		/// <summary>
		/// Specifies the path of a customized AutoDial handler DLL.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxPath)]
		public string AutodialDll;

		/// <summary>
		/// Specifies the name of the RASADFunc function from the AutoDial handler DLL.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasConsts.MaxPath)]
		public string AutodialFunction;

		/// <summary>
		/// A string indicating the RAS device type
		/// </summary>
		/// <remarks></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxDeviceType + 1)]
		public string DeviceType;
		/// <summary>
		/// String indicating the TAPI Device to use with this entry.
		/// </summary>
		/// <remarks></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxDeviceName + 1)]
		public string DeviceName;

		// X.25
		/// <summary>
		/// String that identifies the X.25 PAD type.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxPadType + 1)]
		public string X25PadType;

		/// <summary>
		/// String that identifies the X.25 address to connect to.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxX25Address + 1)]
		public string X25Address;
    
		/// <summary>
		/// String that specifies the facilities to request from the host at connection.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxFacilities + 1)]
		public string X25Facilities;

		/// <summary>
		/// String that specifies additional connection information.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasConsts.MaxUserData + 1)]
		public string X25UserData;

		/// <summary>
		/// dwChannels
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int Channels;

		// Reserved   
		/// <summary>
		/// Reserved, must be zero.
		/// </summary>
		internal int Reserved1;
    
		/// <summary>
		/// Reserved, must be zero.
		/// </summary>
		internal int Reserved2;
    
		// Multilink and BAP
		/// <summary>
		/// Specifies the number of multilink subentries associated with this entry.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int SubEntries;

		/// <summary>
		/// Indicates whether RAS should dial all of the multilink subentries.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int DialMode;

		/// <summary>
		/// Specifies a percent of the total bandwidth available for the currently connected subentries.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int DialExtraPercent;

		/// <summary>
		/// Specifies the number of seconds that current bandwidth usage must exceed before RAS dials an additional subentry.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int DialExtraSampleSeconds;

		/// <summary>
		/// Specifies a percent of the total bandwidth available from the currently connected subentries.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int HangUpExtraPercent;

		/// <summary>
		/// Specifies the number of seconds that current bandwidth usage must be less than the 
		/// threshold specified by dwHangUpExtraPercent before RAS terminates an existing subentry connection.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int HangUpExtraSampleSeconds;

		// Idle time out
		/// <summary>
		/// Specifies the number of seconds after which the connection is terminated due to inactivity.
		/// </summary>
		/// <remarks>Not directly supported in <see cref="RasClient"/></remarks>
		public int IdleDisconnectSeconds;
	}

	#endregion
  
	#region RasStatistics
	/// <summary>
	/// Defines connection statistics
	/// </summary>
	/// <remarks>Used by the ConnectionStatistics method, cleared by the ClearConnectionStatistics method</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct RasStatistics
	{
		internal uint Size;

		private int bytesTransmitted;
		/// <summary>
		/// Number of bytes transmitted
		/// </summary>
		public int BytesTransmitted
		{
			get { return bytesTransmitted; }
			set { bytesTransmitted = value; }
		}

		private int bytesReceived;
		/// <summary>
		/// Number of bytes received
		/// </summary>
		public int BytesReceived
		{
			get { return bytesReceived; }
			set { bytesReceived = value; }
		}

		private int framesTransmitted;
		/// <summary>
		/// Number of frames transmitted
		/// </summary>
		public int FramesTransmitted
		{
			get { return framesTransmitted; }
			set { framesTransmitted = value; }
		}

		private int framesReceived;
		/// <summary>
		/// Number of frames received
		/// </summary>
		public int FramesReceived
		{
			get { return framesReceived; }
			set { framesReceived = value; }
		}

		private int crcErrors;
		/// <summary>
		/// Number of CRC errors
		/// </summary>
		public int CrcErrors
		{
			get { return crcErrors; }
			set { crcErrors = value; }
		}

		private int timeoutErrors;
		/// <summary>
		/// Number of timeout errors
		/// </summary>
		public int TimeoutErrors
		{
			get { return timeoutErrors; }
			set { timeoutErrors = value; }
		}

		private int alignmentErrors;
		/// <summary>
		/// Number of alignment errors
		/// </summary>
		public int AlignmentErrors
		{
			get { return alignmentErrors; }
			set { alignmentErrors = value; }
		}

		private int hardwareOverrunErrors;
		/// <summary>
		/// Number of hardware overrun errors
		/// </summary>
		public int HardwareOverrunErrors
		{
			get { return hardwareOverrunErrors; }
			set { hardwareOverrunErrors = value; }
		}

		private int framingErrors;
		/// <summary>
		/// Number of framing errors
		/// </summary>
		public int FramingErrors
		{
			get { return framingErrors; }
			set { framingErrors = value; }
		}

		private int bufferOverrunErrors;
		/// <summary>
		/// Number of buffer overrun errors
		/// </summary>
		public int BufferOverrunErrors
		{
			get { return bufferOverrunErrors; }
			set { bufferOverrunErrors = value; }
		}

		private int compressionRatioIn;
		/// <summary>
		/// The connection's input compression ratio
		/// </summary>
		public int CompressionRatioIn
		{
			get { return compressionRatioIn; }
			set { compressionRatioIn = value; }
		}

		private int compressionRatioOut;
		/// <summary>
		/// The connection's output compression ratio
		/// </summary>
		public int CompressionRatioOut
		{
			get { return compressionRatioOut; }
			set { compressionRatioOut = value; }
		}

		private int bps;
		/// <summary>
		/// The connection rate in bits per seconds
		/// </summary>
		public int Bps
		{
			get { return bps; }
			set { bps = value; }
		}

		private int connectionDuration;
		/// <summary>
		/// The duration of the connection
		/// </summary>
		public int ConnectionDuration
		{
			get { return connectionDuration; }
			set { connectionDuration = value; }
		}
	}
	#endregion
 
	/// <summary>
	/// A struct containing commonly used information about a phonebook entry
	/// </summary>
	/// <remarks>This struct is tailored to provide the same connectoid information available from
	/// the Windows Explorer Network Connections view, as well as the connection state</remarks>
	public struct EntryDetails
	{
		/// <summary>
		/// An int describing the country code for this connectoid
		/// </summary>
		/// <value>An int describing the country code for this connectoid.</value>
		/// <remarks></remarks>
		public int CountryCode { get; private set; }

		/// <summary>
		/// The area code for this connectoid
		/// </summary>
		/// <value>The area code for this connectoid.</value>
		/// <remarks></remarks>
		public string AreaCode { get; private set; }

		/// <summary>
		/// The phone number for this connectoid
		/// </summary>
		/// <value>A string indicating the phone number dialed for this connection.</value>
		public string PhoneNumber { get; private set; }

		/// <summary>
		/// The type of device used for this connection
		/// </summary>
		/// <value>A string describing the type of device (modem, VPN, ISDN, etc) used to establish the connection.</value>
		/// <remarks></remarks>
		public string DeviceType { get; private set; }

		/// <summary>
		/// The name of the device used for this connection
		/// </summary>
		/// <value>A string containing the name of the device used to establish the connection.</value>
		/// <remarks></remarks>
		public string DeviceName { get; private set; }

		/// <summary>
		/// A RasConnectionState indicating the state of the connection
		/// </summary>
		/// <remarks>A <see cref="RasConnectionState"/> indicating the current state of the connection.</remarks>
		public RasConnectionState ConnectionState { get; private set; }

		/// <summary>
		/// An internal method to initialize the private fields
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="connstate"></param>
		internal void Init(RasEntry entry, RasConnectionState connstate)
		{       
			CountryCode = entry.CountryCode;
			AreaCode = entry.AreaCode;
			PhoneNumber = entry.LocalPhoneNumber;     
			DeviceType = entry.DeviceType;
			DeviceName = entry.DeviceName;  
			ConnectionState = connstate;
		}
	}
	#endregion

	#region Ras Callback Delegates
	// Delegates for internal use - RAS Callbacks
	// Prototypes for caller's RasDial callback handler.  Arguments are the
	// message ID (currently always WM_RASDIALEVENT), the current RASCONNSTATE and
	// the error that has occurred (or 0 if none).  Extended arguments are the
	// handle of the RAS connection and an extended error code.
	#region RasDialFunc
	/// <summary>
	/// RasDialFunc callback delegate
	/// </summary>
	/// <param name="Msg"/>The type of event (WM_RASDIALEVENT)
	/// <param name="rcs"/>The connection state about to be entered
	/// <param name="Error"/>An error that may have occurred
	internal delegate void RasDialFunc(
	uint Msg,
	RasConnectionState rcs,
	int Error
	);
	#endregion

	#region RasDialFunc1
	/// <summary>
	/// RasDialFunc1 enhanced callback delegate
	/// </summary>
	/// <param name="hrasconn">Handle to the RAS connection</param>
	/// <param name="Msg">The type of event (WM_RASDIALEVENT)</param>
	/// <param name="rcs">The connection state about to be entered</param>
	/// <param name="Error">An error that may have occurred</param>
	/// <param name="ErrorEx">Extended error information</param>
	/// <remarks>The RAS callback</remarks>
	internal delegate void RasDialFunc1(
	// FxCop will tag this as needing IDisposable, but this isn't allocating anything
	IntPtr hrasconn,
	uint Msg,
	RasConnectionState rcs,
	uint Error,
	uint ErrorEx
	);
	#endregion

	#region RasDialFunc2
	/// <summary>
	/// RasDialFunc2 enhanced callback delegate
	/// </summary>
	/// <param name="CallbackID"/>User-defined value from RasDial
	/// <param name="SubEntry"/>Subentry name in multilink connection
	/// <param name="hrasconn"/>Handle to the RAS connection
	/// <param name="Msg"/>The type of event (WM_RASDIALEVENT)
	/// <param name="rcs"/>The connection state about to be entered
	/// <param name="Error"/>An error that may have occurred
	/// <param name="ErrorEx"/>Extended error information
	/// <remarks>A RAS callback, not used by the RasClient</remarks>
	internal delegate void RasDialFunc2(
	int CallbackID,
	int SubEntry,
	// FxCop will tag this as needing IDisposable, but this isn't allocating anything
	IntPtr hrasconn,
	uint Msg,
	RasConnectionState rcs,
	uint Error,
	uint ErrorEx
	);
	#endregion
	#endregion

	#region RasClient enums

	#region RasConnectionState
	/// <summary>
	/// Enumerates intermediate states to a connection.
	/// </summary>
	/// <remarks>RasConnectionState indicates the state of the RAS connection.</remarks>
	public enum RasConnectionState
	{ 
		/// <summary>
		/// Connection state could not be determined
		/// </summary>
		Unknown = -2,
		/// <summary>
		/// No connection
		/// </summary>
		Idle = -1,
		/// <summary>
		/// The port is opening
		/// </summary>
		OpenPort = 0, 
		/// <summary>
		/// The port has opened
		/// </summary>
		PortOpened, 
		/// <summary>
		/// Connecting to device
		/// </summary>
		ConnectDevice, 
		/// <summary>
		/// Connected to the device
		/// </summary>
		DeviceConnected, 
		/// <summary>
		/// All devices are connected
		/// </summary>
		AllDevicesConnected, 
		/// <summary>
		/// Authenticating
		/// </summary>
		Authenticate, 
		/// <summary>
		/// Authenticating
		/// </summary>
		AuthNotify, 
		/// <summary>
		/// Retrying authentication
		/// </summary>
		AuthRetry, 
		/// <summary>
		/// Authenticating callback
		/// </summary>
		AuthCallback, 
		/// <summary>
		/// Changing password
		/// </summary>
		AuthChangePassword, 
		/// <summary>
		/// Registering with server
		/// </summary>
		AuthProject, 
		/// <summary>
		/// Authenticating connection speed
		/// </summary>
		AuthLinkSpeed, 
		/// <summary>
		/// Authentication acknowledgement
		/// </summary>
		AuthAcknowledgement, 
		/// <summary>
		/// Retrying authentication
		/// </summary>
		Reauthenticate, 
		/// <summary>
		/// The user is authenticated
		/// </summary>
		Authenticated, 
		/// <summary>
		/// Preparing for callback
		/// </summary>
		PrepareForCallback, 
		/// <summary>
		/// Waiting for the modem to reset
		/// </summary>
		WaitForModemReset, 
		/// <summary>
		/// Waiting for callback
		/// </summary>
		WaitForCallback,
		/// <summary>
		/// Registered on server network
		/// </summary>
		Projected, 
		/// <summary>
		/// Starting authentication sequence
		/// </summary>
		StartAuthentication,    
		/// <summary>
		/// Callback complete
		/// </summary>
		CallbackComplete,       
		/// <summary>
		/// Logging onto network
		/// </summary>
		LogonNetwork,     
		/// <summary>
		/// Subentry connected
		/// </summary>
		SubEntryConnected,
		/// <summary>
		/// Subentry disconnected
		/// </summary>
		SubEntryDisconnected,
		/// <summary>
		/// Waiting for interactive authentication
		/// </summary>
		Interactive = Paused, 
		/// <summary>
		/// Retrying authentication
		/// </summary>
		RetryAuthentication, 
		/// <summary>
		/// Callback set
		/// </summary>
		CallbackSetByCaller, 
		/// <summary>
		/// Authentication password expired
		/// </summary>
		PasswordExpired, 
		/// <summary>
		/// Connected
		/// </summary>
		Connected = Done, 
		/// <summary>
		/// DIsconnected
		/// </summary>
		Disconnected,
		/// <summary>
		/// Paused
		/// </summary>
		Paused = 0x1000,
		/// <summary>
		/// Done, complete, fini
		/// </summary>
		Done   = 0x2000
	}
	#endregion

	#region DialExtensionOptions
	/// <summary>
	/// Enum for the RASDIALEXTENSIONS.Options bit flags
	/// </summary>
	[Flags]
	internal enum DialExtensionOptions
	{     
		/// <summary>
		/// Determines whether dialing prefix/suffix are used
		/// </summary>
		UsePrefixSuffix           = 0x00000001,
		/// <summary>
		/// Indicates whether the connection sequence should allow paused states
		/// </summary>
		PausedStates              = 0x00000002,
		/// <summary>
		/// Indicates whether the device speaker settings should be ignored
		/// </summary>
		IgnoreModemSpeaker        = 0x00000004,
		/// <summary>
		/// Indicates whether the speaker setting is set
		/// </summary>
		SetModemSpeaker           = 0x00000008,
		/// <summary>
		/// Indicates whether the device software compression setting are ignored
		/// </summary>
		IgnoreSoftwareCompression = 0x00000010,
		/// <summary>
		/// Indicates whether the software compression settings are set
		/// </summary>
		SetSoftwareCompression    = 0x00000020,
		/// <summary>
		/// Indicates whether a UI will be presented during connection
		/// </summary>
		DisableConnectedUI        = 0x00000040,
		/// <summary>
		/// Indicates whether a UI is presented for reconnection
		/// </summary>
		DisableReconnectUI        = 0x00000080,
		/// <summary>
		/// Indicates whether reconnection is allowed
		/// </summary>
		DisableReconnect          = 0x00000100,
		/// <summary>
		/// Indicates that a user is not available
		/// </summary>
		NoUser                    = 0x00000200,
		/// <summary>
		/// Indicates whether the connection attempt should pause during the script
		/// </summary>
		PauseOnScript             = 0x00000400,
		/// <summary>
		/// Indicates that a router is specified
		/// </summary>
		Router                    = 0x00000800,
	}   
	#endregion

	#region EncryptionType
	/// <summary>
	/// Encryption type enumeration
	/// </summary>
	public enum EncryptionType
	{
		/// <summary>
		/// No encryption
		/// </summary>
		None      = 0,
		/// <summary>
		/// Require encryption
		/// </summary>
		Require   = 1,
		/// <summary>
		/// Require max encryption
		/// </summary>
		RequireMax  = 2,
		/// <summary>
		/// Do encryption if possible. None Ok.
		/// </summary>
		Optional    = 3
	}
	#endregion

	#region RasEntryOptions
	/// <summary>
	/// Used in RASENTRY.Options to specify options for this entry
	/// </summary>
	[Flags()]
	public enum RasEntryOptions : int
	{
		/// <summary>
		/// If this flag is set, the dwCountryID, dwCountryCode, and szAreaCode members are used to construct the phone number. If this flag is not set, these members are ignored.This flag corresponds to the Use Country and Area Codes check box in the Phone dialog box
		/// </summary>
		UseCountryAndAreaCodes    = 0x00000001,
		/// <summary>
		/// If this flag is set, RAS tries to use the IP address specified by ipaddr as the IP address for the dial-up connection. If this flag is not set, the value of the ipaddr member is ignored.Setting the RASEO_SpecificIpAddr flag corresponds to selecting the Specify an IP Address setting in the TCP/IP settings dialog box. Clearing the RASEO_SpecificIpAddr flag corresponds to selecting the Server Assigned IP Address setting in the TCP/IP settings dialog box.Currently, an IP address set in the phone-book entry properties or retrieved from a server overrides the IP address set in the network control panel.
		/// </summary>
		SpecificIPAddress         = 0x00000002,
		/// <summary>
		/// If this flag is set, RAS uses the ipaddrDns, ipaddrDnsAlt, ipaddrWins, and ipaddrWinsAlt members to specify the name server addresses for the dial-up connection. If this flag is not set, RAS ignores these members. Setting the RASEO_SpecificNameServers flag corresponds to selecting the Specify Name Server Addresses setting in the TCP/IP Settings dialog box. Clearing the RASEO_SpecificNameServers flag corresponds to selecting the Server Assigned Name Server Addresses setting in the TCP/IP Settings dialog box.
		/// </summary>
		SpecificNameServers       = 0x00000004,
		/// <summary>
		/// If this flag is set, RAS negotiates to use IP header compression on PPP connections. If this flag is not set, IP header compression is not negotiated.This flag corresponds to the Use IP Header Compression check box in the TCP/IP settings dialog box. It is generally advisable to set this flag because IP header compression significantly improves performance. The flag should be cleared only when connecting to a server that does not correctly negotiate IP header compression.
		/// </summary>
		IPHeaderCompression       = 0x00000008,
		/// <summary>
		/// If this flag is set, the default route for IP packets is through the dial-up adapter 
		/// when the connection is active.
		/// </summary>
		RemoteDefaultGateway      = 0x00000010,
		/// <summary>
		/// f this flag is set, the default route for IP packets is through the dial-up adapter when the connection is active. If this flag is clear, the default route is not modified. This flag corresponds to the Use Default Gateway on Remote Network check box in the TCP/IP settings dialog box.
		/// </summary>
		DisableLcpExtensions      = 0x00000020,
		/// <summary>
		/// If this flag is set, RAS disables the PPP LCP extensions defined in RFC 1570. This may be necessary to connect to certain older PPP implementations, but interferes with features such as server callback. Do not set this flag unless specifically required.
		/// </summary>
		TerminalBeforeDial        = 0x00000040,
		/// <summary>
		/// If this flag is set, RAS displays a terminal window for user input after dialing 
		/// the connection. 
		/// </summary>
		TerminalAfterDial         = 0x00000080,
		/// <summary>
		/// This flag is currently ignored. 
		/// </summary>
		ModemLights               = 0x00000100,
		/// <summary>
		/// If this flag is set, software compression is negotiated on the link. Setting this flag causes the PPP driver to attempt to negotiate CCP with the server. This flag should be set by default, but clearing it can reduce the negotiation period if the server does not support a compatible compression protocol.
		/// </summary>
		SoftwareCompression       = 0x00000200,
		/// <summary>
		/// If this flag is set, only secure password schemes can be used to authenticate the client 
		/// with the server.
		/// </summary>
		RequireEncryptedPassword  = 0x00000400,
		/// <summary>
		/// If this flag is set, only Microsoft's secure password schemes can be used to authenticate the client with the server. This prevents the PPP driver from using the PPP plain-text authentication protocol, MD5-CHAP, MS-CHAP, or SPAP. The flag should be cleared for maximum interoperability and should be set for maximum security. This flag takes precedence over RASEO_RequireEncryptedPw.This flag corresponds to the Require Microsoft Encrypted Password check box in the Security dialog box. See also RASEO_RequireDataEncryption.
		/// </summary>
		RequireMicrosoftEncryptedPassword      = 0x00000800,
		/// <summary>
		/// If this flag is set, data encryption must be negotiated successfully or the connection should be dropped. This flag is ignored unless RASEO_RequireMsEncryptedPw is also set.This flag corresponds to the Require Data Encryption check box in the Security dialog box.
		/// </summary>
		RequireDataEncryption     = 0x00001000,
		/// <summary>
		/// If this flag is set, RAS logs on to the network after the point-to-point connection is established.This flag currently has no effect under Windows NT.
		/// </summary>
		NetworkLogon              = 0x00002000,
		/// <summary>
		/// If this flag is set, RAS uses the user name, password, and domain of the currently logged-on user when dialing this entry. This flag is ignored unless RASEO_RequireMsEncryptedPw is also set. Note that this setting is ignored by the RasDial function, where specifying empty strings for the szUserName and szPassword members of the RASDIALPARAMS structure gives the same result.This flag corresponds to the Use Current Username and Password check box in the Security dialog box.
		/// </summary>
		UseLogonCredentials       = 0x00004000,
		/// <summary>
		/// This flag has an effect when alternate phone numbers are defined by the dwAlternateOffset member. If this flag is set, an alternate phone number that connects successfully becomes the primary phone number, and the current primary phone number is moved to the alternate list.
		/// This flag corresponds to the check box in the Alternate Numbers dialog box.
		/// </summary>
		PromoteAlternates         = 0x00008000,
		/// <summary>
		/// Windows NT only: If this flag is set, RAS checks for existing remote file system and remote printer bindings before making a connection with this entry. Typically, you set this flag on phone-book entries for public networks to remind users to break connections to their private network before connecting to a public network.
		/// </summary>
		SecureLocalFiles          = 0x00010000
	}
	#endregion

	#region RasEntryProtocols
	/// <summary>
	/// RASENTRY.Protocols bit flags.
	/// </summary>
	[Flags]
	public enum RasEntryProtocols
	{
		/// <summary>
		/// Negotiate the NetBEUI protocol.
		/// </summary>
		NetBeui = 1,
		/// <summary>
		/// Negotiate the IPX protocol.
		/// </summary>
		Ipx     = 2,
		/// <summary>
		/// Negotiate the TCP/IP protocol. 
		/// </summary>
		IP      = 4
	}
	#endregion

	#region RasEntryFramingProtocols
	/// <summary>
	/// RASENTRY 'FramingProtocols' bit flags.
	/// </summary>
	[Flags()]
	public enum RasEntryFramingProtocols : int
	{
		/// <summary>
		/// Point-to-Point Protocol (PPP)
		/// </summary>
		Ppp  = 1,
		/// <summary>
		/// Serial Line Internet Protocol (SLIP)
		/// </summary>
		Slip = 2,
		/// <summary>
		/// Microsoft proprietary protocol implemented in Windows NT 3.1 and Windows for Workgroups 3.11
		/// </summary>
		Ras  = 4
	}
	#endregion

	#region RasEntryType
	/// <summary>
	/// The entry type used to determine which UI properties
	/// are to be presented to user.  This generally corresponds
	/// to a Connections "add" wizard selection. 
	/// </summary>
	public enum RasEntryType
	{
		/// <summary>
		/// Phone lines: modem, ISDN, X.25, etc
		/// </summary>
		Phone     = 1,
		/// <summary>
		/// Virtual private network
		/// </summary>
		Vpn       = 2,
		/// <summary>
		/// Direct connect: serial, parallel
		/// </summary>
		Direct    = 3,
		/// <summary>
		/// BaseCamp internet
		/// </summary>
		Internet  = 4
	}
	#endregion

	#region RasConsts
	/// <summary>
	/// RAS constants 
	/// </summary>
	/// <remarks>Constants used by RAS to determine maximum string lengths</remarks>
	internal enum RasConsts : int
	{
		MaxDeviceType   = 16,
		MaxPhoneNumber    = 128,
		MaxIpAddress    = 15,
		MaxIpxAddress   = 21,
		MaxEntryName    = 256,
		MaxDeviceName   = 128,
		MaxCallbackNumber = 128,
		MaxAreaCode     = 10,
		MaxPadType      = 32,
		MaxX25Address   = 200,
		MaxFacilities   = 200,
		MaxUserData     = 200,
		MaxReplyMessage   = 1024,
		MaxDnsSuffix    = 256,
		UNLen       = 256,
		PWLen       = 256,
		DNLen       = 15,
		MaxPath       = 260
	} 
	#endregion

	#region RasError
	/// <summary>
	/// RAS errors
	/// </summary>
	public enum RasError
	{
		/// <summary>
		/// Successful
		/// </summary>
		Success = 0,

		/// <summary>
		/// Invalid handle
		/// </summary>
		InvalidHandle = 6,

		/// <summary>
		/// Base error code
		/// </summary>
		RasBase = 600,

		/// <summary>
		///  An operation is pending.
		/// </summary>
		Pending = RasBase + 0,

		/// <summary>
		///  An invalid port handle was detected.
		/// </summary>
		InvalidPortHandle = RasBase + 1,

		/// <summary>
		///  The specified port is already open.
		/// </summary>
		PortAlreadyOpen = RasBase + 2,

		/// <summary>
		///  The caller's buffer is too small.
		/// </summary>
		BufferTooSmall = RasBase + 3,

		/// <summary>
		///  Incorrect information was specified.
		/// </summary>
		WrongInfoSpecified = RasBase + 4,

		/// <summary>
		///  The port information cannot be set.
		/// </summary>
		CannotSetPortInfo = RasBase + 5,

		/// <summary>
		///  The specified port is not connected.
		/// </summary>
		PortNotConnected = RasBase + 6,

		/// <summary>
		///  An invalid event was detected.
		/// </summary>
		EventInvalid = RasBase + 7,

		/// <summary>
		///  A device was specified that does not exist.
		/// </summary>
		DeviceDoesNotExist = RasBase + 8,

		/// <summary>
		///  A device type was specified that does not exist.
		/// </summary>
		DeviceTypeDoesNotExist = RasBase + 9,

		/// <summary>
		///  An invalid buffer was specified.
		/// </summary>
		BufferInvalid = RasBase + 10,

		/// <summary>
		///  A route was specified that is not available.
		/// </summary>
		RouteNotAvailable = RasBase + 11,

		/// <summary>
		///  A route was specified that is not allocated.
		/// </summary>
		RouteNotAllocated = RasBase + 12,

		/// <summary>
		///  An invalid compression was specified.
		/// </summary>
		InvalidCompressionSpecified = RasBase + 13,

		/// <summary>
		///  There were insufficient buffers available.
		/// </summary>
		OutOfBuffers = RasBase + 14,

		/// <summary>
		///  The specified port was not found.
		/// </summary>
		PortNotFound = RasBase + 15,

		/// <summary>
		///  An asynchronous request is pending.
		/// </summary>
		AsyncRequestPending = RasBase + 16,

		/// <summary>
		///  The modem (or other connecting device) is already disconnecting.
		/// </summary>
		AlreadyDisconnecting = RasBase + 17,

		/// <summary>
		///  The specified port is not open.
		/// </summary>
		PortNotOpen = RasBase + 18,

		/// <summary>
		///  A connection to the remote computer could not be established, so the port used for this connection was closed. For further assistance, click More Info or search Help and Support Center for this error
		/// </summary>
		PortDisconnected = RasBase + 19,

		/// <summary>
		///  No endpoints could be determined.
		/// </summary>
		NoEndpoints = RasBase + 20,

		/// <summary>
		///  The system could not open the phone book file.
		/// </summary>
		CannotOpenPhonebook = RasBase + 21,

		/// <summary>
		///  The system could not load the phone book file.
		/// </summary>
		CannotLoadPhonebook = RasBase + 22,

		/// <summary>
		///  The system could not find the phone book entry for this connection.
		/// </summary>
		CannotFindPhonebookEntry = RasBase + 23,

		/// <summary>
		///  The system could not update the phone book file.
		/// </summary>
		CannotWritePhonebook = RasBase + 24,

		/// <summary>
		///  The system found invalid information in the phone book file.
		/// </summary>
		CorruptPhonebook = RasBase + 25,

		/// <summary>
		///  A string could not be loaded.
		/// </summary>
		CannotLoadString = RasBase + 26,

		/// <summary>
		///  A key could not be found.
		/// </summary>
		KeyNotFound = RasBase + 27,

		/// <summary>
		///  The connection was terminated by the remote computer before it could be completed. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		Disconnection = RasBase + 28,

		/// <summary>
		///  The connection was closed by the remote computer.
		/// </summary>
		RemoteDisconnection = RasBase + 29,

		/// <summary>
		///  The modem (or other connecting device) was disconnected due to hardware failure.
		/// </summary>
		HardwareFailure = RasBase + 30,

		/// <summary>
		///  The user disconnected the modem (or other connecting device).
		/// </summary>
		UserDisconnection = RasBase + 31,

		/// <summary>
		///  An incorrect structure size was detected.
		/// </summary>
		InvalidSize = RasBase + 32,

		/// <summary>
		///  The modem (or other connecting device) is already in use or is not configured properly.
		/// </summary>
		PortNotAvailable = RasBase + 33,

		/// <summary>
		///  Your computer could not be registered on the remote network.
		/// </summary>
		CannotProjectClient = RasBase + 34,

		/// <summary>
		///  There was an unknown error.
		/// </summary>
		Unknown = RasBase + 35,

		/// <summary>
		///  The device attached to the port is not the one expected.
		/// </summary>
		WrongDeviceAttached = RasBase + 36,

		/// <summary>
		///  A string was detected that could not be converted.
		/// </summary>
		BadString = RasBase + 37,

		/// <summary>
		///  The remote server is not responding in a timely fashion.
		/// </summary>
		RequestTimeout = RasBase + 38,

		/// <summary>
		///  No asynchronous net is available.
		/// </summary>
		CannotGetLana = RasBase + 39,

		/// <summary>
		///  An error has occurred involving NetBIOS.
		/// </summary>
		NetBiosError = RasBase + 40,

		/// <summary>
		///  The server cannot allocate NetBIOS resources needed to support the client.
		/// </summary>
		ServerOutOfResources = RasBase + 41,

		/// <summary>
		///  One of your computer's NetBIOS names is already registered on the remote network.
		/// </summary>
		NameExistsOnNet = RasBase + 42,

		/// <summary>
		///  A network adapter at the server failed.
		/// </summary>
		ServerGeneralNetFailure = RasBase + 43,

		/// <summary>
		///  You will not receive network message popups.
		/// </summary>
		MessageAliasNotAdded = RasBase + 44,

		/// <summary>
		///  There was an internal authentication error.
		/// </summary>
		InternalAuthenticationError = RasBase + 45,

		/// <summary>
		///  The account is not permitted to log on at this time of day.
		/// </summary>
		RestrictedLogonHours = RasBase + 46,

		/// <summary>
		///  The account is disabled.
		/// </summary>
		AccountDisabled = RasBase + 47,

		/// <summary>
		///  The password for this account has expired.
		/// </summary>
		PasswordExpired = RasBase + 48,

		/// <summary>
		///  The account does not have permission to dial in.
		/// </summary>
		NoDialInPermission = RasBase + 49,

		/// <summary>
		///  The remote access server is not responding.
		/// </summary>
		ServerNotResponding = RasBase + 50,

		/// <summary>
		///  The modem (or other connecting device) has reported an error.
		/// </summary>
		ErrorFromDevice = RasBase + 51,

		/// <summary>
		///  There was an unrecognized response from the modem (or other connecting device).
		/// </summary>
		UnrecognizedResponse = RasBase + 52,

		/// <summary>
		///  A macro required by the modem (or other connecting device) was not found in the device.INF file.
		/// </summary>
		MacroNotFound = RasBase + 53,

		/// <summary>
		///  A command or response in the device.INF file section refers to an undefined macro.
		/// </summary>
		MacroNotDefined = RasBase + 54,

		/// <summary>
		///  The message macro was not found in the device.INF file section.
		/// </summary>
		MessageMacroNotFound = RasBase + 55,

		/// <summary>
		///  The defaultoff macro in the device.INF file section contains an undefined macro.
		/// </summary>
		DefaultOffMacroNotFound = RasBase + 56,

		/// <summary>
		///  The device.INF file could not be opened.
		/// </summary>
		FileCouldNotBeOpened = RasBase + 57,

		/// <summary>
		///  The device name in the device.INF or media.INI file is too long.
		/// </summary>
		DeviceNameTooLong = RasBase + 58,

		/// <summary>
		///  The media.INI file refers to an unknown device name.
		/// </summary>
		DeviceNameNotFound = RasBase + 59,

		/// <summary>
		///  The device.INF file contains no responses for the command.
		/// </summary>
		NoResponses = RasBase + 60,

		/// <summary>
		///  The device.INF file is missing a command.
		/// </summary>
		NoCommandFound = RasBase + 61,

		/// <summary>
		///  There was an attempt to set a macro not listed in device.INF file section.
		/// </summary>
		WrongKeySpecified = RasBase + 62,

		/// <summary>
		///  The media.INI file refers to an unknown device type.
		/// </summary>
		UnknownDeviceType = RasBase + 63,

		/// <summary>
		///  The system has run out of memory.
		/// </summary>
		ErrorAllocatingMemory = RasBase + 64,

		/// <summary>
		///  The modem (or other connecting device) is not properly configured.
		/// </summary>
		PortNotConfigured = RasBase + 65,

		/// <summary>
		///  The modem (or other connecting device) is not functioning.
		/// </summary>
		DeviceNotReady = RasBase + 66,

		/// <summary>
		///  The system was unable to read the media.INI file.
		/// </summary>
		ErrorReadingIniFile = RasBase + 67,

		/// <summary>
		///  The connection was terminated.
		/// </summary>
		NoConnection = RasBase + 68,

		/// <summary>
		///  The usage parameter in the media.INI file is invalid.
		/// </summary>
		BadUsageInIniFile = RasBase + 69,

		/// <summary>
		///  The system was unable to read the section name from the media.INI file.
		/// </summary>
		ErrorReadingSectionName = RasBase + 70,

		/// <summary>
		///  The system was unable to read the device type from the media.INI file.
		/// </summary>
		ErrorReadingDeviceType = RasBase + 71,

		/// <summary>
		///  The system was unable to read the device name from the media.INI file.
		/// </summary>
		ErrorReadingDeviceName = RasBase + 72,

		/// <summary>
		///  The system was unable to read the usage from the media.INI file.
		/// </summary>
		ErrorReadingUsage = RasBase + 73,

		/// <summary>
		///  The system was unable to read the maximum connection BPS rate from the media.INI file.
		/// </summary>
		ErrorReadingMaxConnectBPS = RasBase + 74,

		/// <summary>
		///  The system was unable to read the maximum carrier connection speed from the media.INI file.
		/// </summary>
		ErrorReadingMaxCarrierBPS = RasBase + 75,

		/// <summary>
		///  The phone line is busy.
		/// </summary>
		LineBusy = RasBase + 76,

		/// <summary>
		///  A person answered instead of a modem (or other connecting device).
		/// </summary>
		VoiceAnswer = RasBase + 77,

		/// <summary>
		///  The remote computer did not respond. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		NoAnswer = RasBase + 78,

		/// <summary>
		///  The system could not detect the carrier.
		/// </summary>
		NoCarrier = RasBase + 79,

		/// <summary>
		///  There was no dial tone.
		/// </summary>
		NoDialtone = RasBase + 80,

		/// <summary>
		///  The modem (or other connecting device) reported a general error.
		/// </summary>
		ErrorInCommand = RasBase + 81,

		/// <summary>
		///  There was an error in writing the section name.
		/// </summary>
		ErrorWritingSectionName = RasBase + 82,

		/// <summary>
		///  There was an error in writing the device type.
		/// </summary>
		ErrorWritingDeviceType = RasBase + 83,

		/// <summary>
		///  There was an error in writing the device name.
		/// </summary>
		ErrorWritingDeviceName = RasBase + 84,

		/// <summary>
		///  There was an error in writing the maximum connection speed.
		/// </summary>
		ErrorWritingMaxConnectBPS = RasBase + 85,

		/// <summary>
		///  There was an error in writing the maximum carrier speed.
		/// </summary>
		ErrorWritingMaxCarrierBPS = RasBase + 86,

		/// <summary>
		///  There was an error in writing the usage.
		/// </summary>
		ErrorWritingUsage = RasBase + 87,

		/// <summary>
		///   There was an error in writing the default-off.
		/// </summary>
		ErrorWritingDefaultOff = RasBase + 88,

		/// <summary>
		///   There was an error in reading the default-off.
		/// </summary>
		ErrorReadingDefaultOff = RasBase + 89,

		/// <summary>
		///  ERROR_EMPTY_INI_FILE
		/// </summary>
		EmptyIniFile = RasBase + 90,

		/// <summary>
		///  Access was denied because the username and/or password was invalid on the domain.
		/// </summary>
		AuthenticationFailure = RasBase + 91,

		/// <summary>
		///  There was a hardware failure in the modem (or other connecting device).
		/// </summary>
		ErrorPortOrDevice = RasBase + 92,

		/// <summary>
		///  ERROR_NOT_BINARY_MACRO
		/// </summary>
		NotBinaryMacro = RasBase + 93,

		/// <summary>
		///  ERROR_DCB_NOT_FOUND
		/// </summary>
		DCBNotFound = RasBase + 94,

		/// <summary>
		///  The state machines are not started.
		/// </summary>
		StateMachinesNotStarted = RasBase + 95,

		/// <summary>
		///  The state machines are already started.
		/// </summary>
		StateMachinesAlreadyStarted = RasBase + 96,

		/// <summary>
		///  The response looping did not complete.
		/// </summary>
		PartialResponseLooping = RasBase + 97,

		/// <summary>
		///  A response keyname in the device.INF file is not in the expected format.
		/// </summary>
		UnknownResponseKey = RasBase + 98,

		/// <summary>
		///  The modem (or other connecting device) response caused a buffer overflow.
		/// </summary>
		ReceiveBufferFull = RasBase + 99,

		/// <summary>
		///  The expanded command in the device.INF file is too long.
		/// </summary>
		CommandTooLong = RasBase + 100,

		/// <summary>
		///  The modem moved to a connection speed not supported by the COM driver.
		/// </summary>
		UnsupportedBPS = RasBase + 101,

		/// <summary>
		///  Device response received when none expected.
		/// </summary>
		UnexpectedResponse = RasBase + 102,

		/// <summary>
		///  The connection needs information from you, but the application does not allow user interaction.
		/// </summary>
		InteractiveMode = RasBase + 103,

		/// <summary>
		///  The callback number is invalid.
		/// </summary>
		BadCallbackNumber = RasBase + 104,

		/// <summary>
		///  The authorization state is invalid.
		/// </summary>
		InvalidAuthorizationState = RasBase + 105,

		/// <summary>
		///  ERROR_WRITING_INITBPS
		/// </summary>
		ErrorWritingInitBPS = RasBase + 106,

		/// <summary>
		///  There was an error related to the X.25 protocol.
		/// </summary>
		ErrorX25Diagnostic = RasBase + 107,

		/// <summary>
		///  The account has expired.
		/// </summary>
		AccountExpired = RasBase + 108,

		/// <summary>
		///  There was an error changing the password on the domain.  The password might have been too short or might have matched a previously used password.
		/// </summary>
		ChangingPassword = RasBase + 109,

		/// <summary>
		///  Serial overrun errors were detected while communicating with the modem.
		/// </summary>
		Overrun = RasBase + 110,

		/// <summary>
		///  A configuration error on this computer is preventing this connection. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		RasManagerCannotInitialize   = RasBase + 111,

		/// <summary>
		///  The two-way port is initializing.  Wait a few seconds and redial.
		/// </summary>
		BiplexPortNotAvailable = RasBase + 112,

		/// <summary>
		///  No active ISDN lines are available.
		/// </summary>
		NoActiveISDNLines = RasBase + 113,

		/// <summary>
		///  No ISDN channels are available to make the call.
		/// </summary>
		NoISDNChannelsAvailable = RasBase + 114,

		/// <summary>
		///  Too many errors occurred because of poor phone line quality.
		/// </summary>
		TooManyLineErrors = RasBase + 115,

		/// <summary>
		///  The Remote Access Service IP configuration is unusable.
		/// </summary>
		IPConfiguration = RasBase + 116,

		/// <summary>
		///  No IP addresses are available in the static pool of Remote Access Service IP addresses.
		/// </summary>
		NoIPAddresses = RasBase + 117,

		/// <summary>
		///  The connection was terminated because the remote computer did not respond in a timely manner. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		PPPTimeout = RasBase + 118,

		/// <summary>
		///  The connection was terminated by the remote computer.
		/// </summary>
		PPPRemoteTerminated = RasBase + 119,

		/// <summary>
		///  A connection to the remote computer could not be established. You might need to change the network settings for this connection. For further assistance, click More Info or search Help and Support Cen
		/// </summary>
		PPPNoProtocolsConfigured = RasBase + 120,

		/// <summary>
		///  The remote computer did not respond. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		PPPNoResponse = RasBase + 121,

		/// <summary>
		///  Invalid data was received from the remote computer. This data was ignored.
		/// </summary>
		PPPInvalidPacket = RasBase + 122,

		/// <summary>
		///  The phone number, including prefix and suffix, is too long.
		/// </summary>
		PhoneNumberTooLong = RasBase + 123,

		/// <summary>
		///  The IPX protocol cannot dial out on the modem (or other connecting device) because this computer is not configured for dialing out (it is an IPX router).
		/// </summary>
		IPXCPNoDialoutConfigured = RasBase + 124,

		/// <summary>
		///  The IPX protocol cannot dial in on the modem (or other connecting device) because this computer is not configured for dialing in (the IPX router is not installed).
		/// </summary>
		IPXCPNoDialinConfigured = RasBase + 125,

		/// <summary>
		///  The IPX protocol cannot be used for dialing out on more than one modem (or other connecting device) at a time.
		/// </summary>
		IPXCPDialOutAlreadyActive = RasBase + 126,

		/// <summary>
		///  Cannot access TCPCFG.DLL.
		/// </summary>
		CannotAccessTcpCfgDll = RasBase + 127,

		/// <summary>
		///  The system cannot find an IP adapter.
		/// </summary>
		NoIPRasAdapter = RasBase + 128,

		/// <summary>
		///  SLIP cannot be used unless the IP protocol is installed.
		/// </summary>
		SLIPRequiresIP = RasBase + 129,

		/// <summary>
		///  Computer registration is not complete.
		/// </summary>
		ProjectionNotComplete = RasBase + 130,

		/// <summary>
		///  The protocol is not configured.
		/// </summary>
		ProtocolNotConfigured = RasBase + 131,

		/// <summary>
		/// Your computer and the remote computer could not agree on PPP control protocols.
		/// </summary>
		PPPNotConverging = RasBase + 132,

		/// <summary>
		///  A connection to the remote computer could not be completed. You might need to adjust the protocols on this computer. For further assistance, click More Info or search Help and Support Center for this
		/// </summary>
		PPPCPRejected = RasBase + 133,

		/// <summary>
		///  The PPP link control protocol was terminated.
		/// </summary>
		PPPLCPTerminated = RasBase + 134,

		/// <summary>
		///  The requested address was rejected by the server.
		/// </summary>
		PPPRequiredAddressRejected = RasBase + 135,

		/// <summary>
		///  The remote computer terminated the control protocol.
		/// </summary>
		PPPNCPTerminated = RasBase + 136,

		/// <summary>
		///  Loopback was detected.
		/// </summary>
		PPPLoopbackDetected = RasBase + 137,

		/// <summary>
		///  The server did not assign an address.
		/// </summary>
		PPPNoAddressAssigned = RasBase + 138,

		/// <summary>
		///  The authentication protocol required by the remote server cannot use the stored password.  Redial, entering the password explicitly.
		/// </summary>
		CannotUseLogonCredentials = RasBase + 139,

		/// <summary>
		///  An invalid dialing rule was detected.
		/// </summary>
		TAPIConfiguration = RasBase + 140,

		/// <summary>
		///  The local computer does not support the required data encryption type.
		/// </summary>
		NoLocalEncryption = RasBase + 141,

		/// <summary>
		///  The remote computer does not support the required data encryption type.
		/// </summary>
		NoRemoteEncryption = RasBase + 142,

		/// <summary>
		///  The remote computer requires data encryption.
		/// </summary>
		RemoteRequiresEncryption = RasBase + 143,

		/// <summary>
		///  The system cannot use the IPX network number assigned by the remote computer.  Additional information is provided in the event log.
		/// </summary>
		IPXCPNetNumberConflict = RasBase + 144,

		/// <summary>
		///  ERROR_INVALID_SMM
		/// </summary>
		InvalidSMM = RasBase + 145,

		/// <summary>
		///  ERROR_SMM_UNINITIALIZED
		/// </summary>
		SMMUninitialized = RasBase + 146,

		/// <summary>
		///  ERROR_NO_MAC_FOR_PORT
		/// </summary>
		NoMacForPort = RasBase + 147,

		/// <summary>
		///  ERROR_SMM_TIMEOUT
		/// </summary>
		SMMTimeout = RasBase + 148,

		/// <summary>
		///  ERROR_BAD_PHONE_NUMBER
		/// </summary>
		BadPhoneNumber = RasBase + 149,

		/// <summary>
		///  ERROR_WRONG_MODULE
		/// </summary>
		WrongModule = RasBase + 150,

		/// <summary>
		///  The callback number contains an invalid character.  Only the following 18 characters are allowed:  0 to 9, T, P, W, (, ), -, @, and space.
		/// </summary>
		InvalidCallbackNumber = RasBase + 151,

		/// <summary>
		///  A syntax error was encountered while processing a script.
		/// </summary>
		ScriptSyntax = RasBase + 152,

		/// <summary>
		///  The connection could not be disconnected because it was created by the multi-protocol router.
		/// </summary>
		HangupFailed = RasBase + 153,

		/// <summary>
		///  The system could not find the multi-link bundle.
		/// </summary>
		BundleNotFound = RasBase + 154,

		/// <summary>
		///  The system cannot perform automated dial because this connection  has a custom dialer specified.
		/// </summary>
		CannotDoCustomDial = RasBase + 155,

		/// <summary>
		///  This connection is already being dialed.
		/// </summary>
		DialAlreadyInProgress = RasBase + 156,

		/// <summary>
		///  Remote Access Services could not be started automatically. Additional information is provided in the event log.
		/// </summary>
		RasAutoCannotInitialize  = RasBase + 157,

		/// <summary>
		///  Internet Connection Sharing is already enabled on the connection.
		/// </summary>
		ConnectionAlreadyShared = RasBase + 158,

		/// <summary>
		///  An error occurred while the existing Internet Connection Sharing settings were being changed.
		/// </summary>
		SharingChangeFailed = RasBase + 159,

		/// <summary>
		///  An error occurred while routing capabilities were being enabled.
		/// </summary>
		ErrorSharingRouterInstall = RasBase + 160,

		/// <summary>
		///  An error occurred while Internet Connection Sharing was being enabled for the connection.
		/// </summary>
		ShareConnectionFailed = RasBase + 161,

		/// <summary>
		///  An error occurred while the local network was being configured for sharing.
		/// </summary>
		ErrorSharingPrivateInstall = RasBase + 162,

		/// <summary>
		///  Internet Connection Sharing cannot be enabled.  There is more than one LAN connection other than the connection to be shared.
		/// </summary>
		CannotShareConnection = RasBase + 163,

		/// <summary>
		///  No smart card reader is installed.
		/// </summary>
		NoSmartCardReader = RasBase + 164,

		/// <summary>
		///  Internet Connection Sharing cannot be enabled.  A LAN connection is already configured with the IP address that is required for automatic IP addressing.
		/// </summary>
		SharingAddressExists = RasBase + 165,

		/// <summary>
		///  A certificate could not be found. Connections that use the L2TP protocol over IPSec require the installation of a machine certificate, also known as a computer certificate.
		/// </summary>
		NoCertificate = RasBase + 166,

		/// <summary>
		///  Internet Connection Sharing cannot be enabled. The LAN connection selected as the private network has more than one IP address configured.  Please reconfigure the LAN connection with a single IP addr
		/// </summary>
		SharingMultipleAddresses = RasBase + 167,

		/// <summary>
		///  The connection attempt failed because of failure to encrypt data.
		/// </summary>
		FailedToEncrypt = RasBase + 168,

		/// <summary>
		///  The specified destination is not reachable.
		/// </summary>
		BadAddressSpecified = RasBase + 169,

		/// <summary>
		///  The remote computer rejected the connection attempt.
		/// </summary>
		ConnectionReject = RasBase + 170,

		/// <summary>
		///  The connection attempt failed because the network is busy.
		/// </summary>
		Congestion = RasBase + 171,

		/// <summary>
		///  The remote computer's network hardware is incompatible with the type of call requested.
		/// </summary>
		Incompatible = RasBase + 172,

		/// <summary>
		///  The connection attempt failed because the destination number has changed.
		/// </summary>
		NumberChanged = RasBase + 173,

		/// <summary>
		///  The connection attempt failed because of a temporary failure.  Try connecting again.
		/// </summary>
		TemporaryFailure = RasBase + 174,

		/// <summary>
		///  The call was blocked by the remote computer.
		/// </summary>
		Blocked = RasBase + 175,

		/// <summary>
		///  The call could not be connected because the remote computer has invoked the Do Not Disturb feature.
		/// </summary>
		DoNotDisturb = RasBase + 176,

		/// <summary>
		///  The connection attempt failed because the modem (or other connecting device) on the remote computer is out of order.
		/// </summary>
		OutOfOrder = RasBase + 177,

		/// <summary>
		///  It was not possible to verify the identity of the server.
		/// </summary>
		UnableToAuthenticateServer = RasBase + 178,

		/// <summary>
		///  To dial out using this connection you must use a smart card.
		/// </summary>
		SmartCardRequired = RasBase + 179,

		/// <summary>
		///  An attempted function is not valid for this connection.
		/// </summary>
		InvalidFunctionForEntry = RasBase + 180,

		/// <summary>
		///  The connection requires a certificate, and no valid certificate was found. For further assistance, click More Info or search Help and Support Center for this error number.
		/// </summary>
		CertificateForEncryptionNotFound = RasBase + 181,

		/// <summary>
		///  Internet Connection Sharing (ICS) and Internet Connection Firewall (ICF) cannot be enabled because Routing and Remote Access has been enabled on this computer. To enable ICS or ICF, first disable Rou
		/// </summary>
		SharingRRasConflict = RasBase + 182,

		/// <summary>
		///  Internet Connection Sharing cannot be enabled. The LAN connection selected as the private network is either not present, or is disconnected from the network. Please ensure that the LAN adapter is con
		/// </summary>
		SharingNoPrivateLAN = RasBase + 183,

		/// <summary>
		///  You cannot dial using this connection at logon time, because it is configured to use a user name different than the one on the smart card. If you want to use it at logon time, you must configure it t
		/// </summary>
		NoDifferentUserAtLogin = RasBase + 184,

		/// <summary>
		///  You cannot dial using this connection at logon time, because it is not configured to use a smart card. If you want to use it at logon time, you must edit the properties of this connection so that it
		/// </summary>
		NoRegistrationCertificateAtLogon = RasBase + 185,

		/// <summary>
		///  The L2TP connection attempt failed because there is no valid machine certificate on your computer for security authentication.
		/// </summary>
		OakleyNoCertificate = RasBase + 186,

		/// <summary>
		///  The L2TP connection attempt failed because the security layer could not authenticate the remote computer.
		/// </summary>
		OakleyAuthenticationFail = RasBase + 187,

		/// <summary>
		///  The L2TP connection attempt failed because the security layer could not negotiate compatible parameters with the remote computer.
		/// </summary>
		OakleyAttribFail = RasBase + 188,

		/// <summary>
		///  The L2TP connection attempt failed because the security layer encountered a processing error during initial negotiations with the remote computer.
		/// </summary>
		OakleyGeneralProcessing = RasBase + 189,

		/// <summary>
		///  The L2TP connection attempt failed because certificate validation on the remote computer failed.
		/// </summary>
		OakleyNoPeerCertificate = RasBase + 190,

		/// <summary>
		///  The L2TP connection attempt failed because security policy for the connection was not found.
		/// </summary>
		OakleyNoPolicy = RasBase + 191,

		/// <summary>
		///  The L2TP connection attempt failed because security negotiation timed out.
		/// </summary>
		OakleyTimedOut = RasBase + 192,

		/// <summary>
		///  The L2TP connection attempt failed because an error occurred while negotiating security.
		/// </summary>
		OakleyError = RasBase + 193,

		/// <summary>
		///  The Framed Protocol RADIUS attribute for this user is not PPP.
		/// </summary>
		UnknownFramedProtocol = RasBase + 194,

		/// <summary>
		///  The Tunnel Type RADIUS attribute for this user is not correct.
		/// </summary>
		WrongTunnelType = RasBase + 195,

		/// <summary>
		///  The Service Type RADIUS attribute for this user is neither Framed nor Callback Framed.
		/// </summary>
		UnknownServiceType = RasBase + 196,

		/// <summary>
		///  A connection to the remote computer could not be established because the modem was not found or was busy. For further assistance, click More Info or search Help and Support Center for this error numb
		/// </summary>
		ConnectingDeviceNotFound = RasBase + 197,

		/// <summary>
		///  A certificate could not be found that can be used with this Extensible Authentication Protocol.
		/// </summary>
		NoEAPTLSCertificate = RasBase + 198,

		/// <summary>
		///  Internet Connection Sharing (ICS) cannot be enabled due to an IP address conflict on the network. ICS requires the host be configured to use 192.168.0.1. Please ensure that no other client on the net
		/// </summary>
		SharingHostAddressConflict = RasBase + 199,

		/// <summary>
		///  Unable to establish the VPN connection.  The VPN server may be unreachable, or security parameters may not be configured properly for this connection.
		/// </summary>
		AutomaticVPNFailed = RasBase + 200,

		/// <summary>
		/// This connection is configured to validate the identity of the access server, but Windows cannot verify the digital certificate sent by the server.
		/// </summary>
		ErrorValidatingServerCertificate = RasBase + 201,

		/// <summary>
		/// The card supplied was not recognized. Please check that the card is inserted correctly, and fits tightly.
		/// </summary>
		ErrorReadingSmartCard = RasBase + 202,

		/// <summary>
		/// The PEAP configuration stored in the session cookie does not match the current session configuration.
		/// </summary>
		InvalidPEAPCookieConfig = RasBase + 203,

		/// <summary>
		/// The PEAP identity stored in the session cookie does not match the current identity.
		/// </summary>
		InvalidPEAPCookieUser = RasBase + 204,

		/// <summary>
		/// You cannot dial using this connection at logon time, because it is configured to use logged on user's credentials.
		/// </summary>
		InvalidMSCHAPV2Config = RasBase + 205,

		/// <summary>
		/// Operation cancelled
		/// </summary>
		Cancelled = RasBase + 300,
	}
	#endregion

	#region RasDialMode
	/// <summary>
	/// Determines how RAS will dial
	/// </summary>
	public enum RasDialMode 
	{
		/// <summary>
		/// Ras will dial synchronously (RasDial won't return until complete)
		/// </summary>
		Sync, 
		/// <summary>
		/// Ras will dial asynchronously (RasDial will return immediately)
		/// </summary>
		Async
	}
	#endregion

	#region RasCompressionMode
	/// <summary>
	/// Determines the compression enabled for the connection
	/// </summary>
	public enum RasCompressionMode
	{
		/// <summary>
		/// Compression determined by TAPI settings
		/// </summary>
		Default, 
		/// <summary>
		/// Compression is enabled
		/// </summary>
		CompressionOn, 
		/// <summary>
		/// Compression is disabled
		/// </summary>
		CompressionOff
	} 
	#endregion

	#region RasProjection
	/// <summary>
	/// Defines values that specify a particular authentication protocol
	/// or Point-to-Point Protocol (PPP) control protocol.  Pass RASP_PppIp
	/// to RasGetProjectionInfo() in order to get the server's IP address.
	/// </summary>
	public enum RasProjection
	{
		RASP_Amb = 0x10000, 
		RASP_PppNbf = 0x803F, 
		RASP_PppIpx = 0x802B, 
		RASP_PppIp = 0x8021, 
		RASP_PppCcp = 0x80FD, 
		RASP_PppLcp = 0xC021, 
		RASP_Slip = 0x20000
	}
	#endregion
	#endregion
}
