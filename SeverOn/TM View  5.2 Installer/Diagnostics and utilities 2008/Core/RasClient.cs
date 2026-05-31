// TMx CLI adapter C# code
// Copyright 2005 Serveron Corporation. All rights reserved.

//*********************************************************************
//* RasClient.cs                                                      *
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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;
using System.Management;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Provides access to the Win32 Remote Access Service API to manage network connectivity
	/// </summary>
	/// <remarks>
	/// The RasClient provides an easy way to establish, monitor, and terminate Microsoft
	/// Remote Access Services.  Remote Access Services (RAS) is also known as Dial Up Networking
	/// (DUN).  RAS is used to connect a computer to a network through phone lines, and to
	/// connect to a Virtual Private Network (VPN) through an existing network connection.
	/// In addition to establishing, monitoring and terminating RAS connections, the RasClient
	/// also provides methods to manipulate RAS Phonebook Entries (aka connectoids) which
	/// define the network connection settings.
	/// </remarks>
	public class RasClient : IDisposable
	{
		static ILog log = LogManager.GetLogger(typeof(RasClient));

		#region creators and destructors

		private TimeSpan DialTimeout = TimeSpan.FromSeconds(200);
		private TimeSpan HangUpTimeout = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Creates a new instance of the RasClient
		/// </summary>
		/// <remarks></remarks>
		public RasClient() 
		{
            log.Debug("Creating Instance of RasClient");
			// Add the RAS callback delegate which will receive the notifications from RAS
			RasDialFunc1Delegate = new RasDialFunc1(OnRasDialFunc1);
		}

		/// <summary>
		/// Destroys the RasClient
		/// </summary>
		~RasClient() 
		{
            log.Debug("Destorying instance of RasClient");
			if ((hangupOnDestroy)&&(connection != IntPtr.Zero))
				HangUp();   
		}

		#endregion

		#region Ras callback delegates and handlers 
		/// <summary>
		/// The RAS callback delegate
		/// </summary>
		private RasDialFunc1 RasDialFunc1Delegate;
		/// <summary>
		/// The RAS callback handler
		/// </summary>
		private void OnRasDialFunc1(IntPtr hrasconn, uint unMsg, RasConnectionState rascs,
			uint dwError, uint dwExtendedError) 
		{
			if (rascs==RasConnectionState.Connected)
				DoConnectionChanged(true);
			if (rascs==RasConnectionState.Disconnected)
				DoConnectionChanged(false);
			if (dwError==(uint)RasError.Success)
				DoDialStatus(rascs);
			else
				DoDialError((RasError)dwError);
		}     
		#endregion

		#region events and event generators
		#region ConnectionChanged event   
		/// <summary>
		/// Delegate for the ConnectionChanged event
		/// </summary>
		public delegate void ConnectionChangedEventHandler(object sender, ConnectionChangedEventArgs e);
		/// <summary>
		/// Event that is generated when a connection is established and terminated.
		/// </summary>
		/// <remarks>IsConnected is true when a connection is established, false when terminated</remarks>
		[Description("Event that is generated when a connection is established and terminated.")]
		public event ConnectionChangedEventHandler ConnectionChanged;
		/// <summary>
		/// Generates the ConnectionChanged event
		/// </summary>
		/// <param name="isConnected">Indicates whether a connection is established or not.</param>
		protected virtual void DoConnectionChanged(bool isConnected) 
		{
			if (ConnectionChanged != null)
				ConnectionChanged(this, new ConnectionChangedEventArgs(isConnected));
            if (!isConnected)
            {
                log.Debug("Setting connection to zero");
                connection = IntPtr.Zero;
            }
			if (dialMode == RasDialMode.Sync)
				if (isConnected)
					waitingForSync = false;
		}
		#endregion

		#region DialError event
		/// <summary>
		/// Delegate for the DialError event
		/// </summary>
		/// <param name="sender">The object that generated the event</param>
		/// <param name="e">A <see cref="DialErrorEventArgs"/> that indicates the error.</param>
		/// <remarks>The DialError event is generated when an error terminates a RAS
		/// connection attempt.</remarks>
		public delegate void DialErrorEventHandler(object sender, DialErrorEventArgs e);
		/// <summary>
		/// Event that is generated when an error occurs when establishing the connection
		/// </summary>
		[Description("Event that is generated when an error occurs when establishing the connection.")]
		public event DialErrorEventHandler DialError;
		/// <summary>
		/// Generates the DialError event
		/// </summary>
		/// <param name="rasError">A RasError that identifies the error</param>
		protected virtual void DoDialError(RasError rasError) 
		{
			if (DialError != null)
				DialError(this, new DialErrorEventArgs(rasError));

			if (dialMode == RasDialMode.Sync)
			{
				asynchronousError = rasError;
				waitingForSync = false;
			}
		}
		/// <summary>
		/// Generates the DialError event
		/// </summary>
		/// <param name="rasError">An int that identifies the error</param>
		protected virtual void DoDialError(int rasError) 
		{
			DoDialError((RasError)rasError);
		}
		#endregion

		#region DialStatus event
		/// <summary>
		/// Delegate for the DialStatus event
		/// </summary>
		/// <param name="sender">The object that generated the event.</param>
		/// <param name="e">A <see cref="DialStatusEventArgs"/> indicating the status of the connection attempt.</param>
		/// <remarks>While establishing a connection, the DialStatus event will be generated
		/// periodically to provide status information.</remarks>
		public delegate void DialStatusEventHandler(object sender, DialStatusEventArgs e);
		/// <summary>
		/// Event that is generated throughout the connection to indicate the connection's status
		/// </summary>
		/// <remarks></remarks>
		[Description("Event that is generated throughout the connection to indicate the connection's status.")]
		public event DialStatusEventHandler DialStatus;
		/// <summary>
		/// Generates the DialStatus event
		/// </summary>
		/// <param name="state">A <see cref="RasConnectionState"/> that defines the state of the connection</param>
		protected virtual void DoDialStatus(RasConnectionState state) 
		{
			if (DialStatus != null)
				DialStatus(this, new DialStatusEventArgs(state));
		}
		#endregion
		#endregion

		#region Private methods

		const uint ERROR_CANNOT_OPEN_PHONEBOOK = 621;

		private uint SetPhoneBookEntryBaudRate()
		{
			if (string.IsNullOrEmpty(BaudRate))
				BaudRate = "9600";

			string phoneBookPath = this.PhoneBook;
			if (string.IsNullOrEmpty(phoneBookPath))
			{
				phoneBookPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					@"Microsoft\Network\Connections\Pbk\Rasphone.pbk");
				if (!File.Exists(phoneBookPath))
				{
					phoneBookPath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						@"Microsoft\Network\Connections\Pbk\Rasphone.pbk");
				}
				if (!File.Exists(phoneBookPath))
				{
					return ERROR_CANNOT_OPEN_PHONEBOOK;
				}
				string newPhoneBook = FileUtility.LocateConfigFile("Rasphone.pbk");
				if (!File.Exists(newPhoneBook))
				{
					File.Copy(phoneBookPath, newPhoneBook, true);
				}
				phoneBookPath = newPhoneBook;
			}
			if (!File.Exists(phoneBookPath))
			{
				return ERROR_CANNOT_OPEN_PHONEBOOK;
			}
			RasEntry entry = new RasEntry();
			RasError err = GetPhonebookEntry("Serveron", ref entry);
			if (err != RasError.Success)
			{
				return (uint)err;
			}
			err = (RasError)RasApi.RasSetEntryProperties(this.InternalPB, EntryName, ref entry, entry.Size, 0, 0);
			try
			{
				bool changedBaud = false;
				using (MemoryStream ms = new MemoryStream())
				using (StreamWriter sw = new StreamWriter(ms))
				using (StreamReader sr = new StreamReader(phoneBookPath))
				{
					string line;
					bool isServeronSection = false;
					while ((line = sr.ReadLine()) != null)
					{
						if (line.StartsWith("[") && line.EndsWith("]"))
						{
							if (String.Compare(line, "[Serveron]", true) == 0)
								isServeronSection = true;
							else
								isServeronSection = false;
						}
						if (isServeronSection)
						{
							if (line.StartsWith("ConnectBPS="))
							{
								string oldBaudRate = line.Substring("ConnectBPS=".Length);
								if (String.Compare(oldBaudRate, BaudRate) != 0)
								{
									changedBaud = true;
									line = "ConnectBPS=" + BaudRate;
								}
							}
						}
						sw.WriteLine(line);
					}
					if (changedBaud)
					{
						sr.Close();
						sw.Flush();
						sw.Close();
						// write the changed file back
						byte [] changes = ms.ToArray();
						ms.Close();
						using (FileStream fs = new FileStream(phoneBookPath, FileMode.Open, FileAccess.Write))
						{
							fs.Write(changes, 0, changes.Length);
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Cannot open phone book", ex);
				return ERROR_CANNOT_OPEN_PHONEBOOK;
			}
			return (uint)RasError.Success;
		}

		#endregion

		#region SetPhonebookEntryDeviceName
		
		public RasError SetPhonebookEntryDeviceName(string EntryName, string DeviceName)
		{
			uint res = 0; // RasError.Success
			RasEntry rasEntry = new RasEntry();
			res = (uint)GetPhonebookEntry(EntryName, ref rasEntry);
			if (res != (uint)RasError.Success)
			{
				return (RasError)res;
			}			
			rasEntry.DeviceName = DeviceName;
			rasEntry.DeviceType = "modem";
			res = RasApi.RasSetEntryProperties(this.InternalPB, EntryName, ref rasEntry, rasEntry.Size, 0, 0);			
			return (RasError)res;
		}

		#endregion 

        # region ForceSettingBaudRate
        public RasError ForceBaudRate()
        {
            return (RasError)SetPhoneBookEntryBaudRate();
        }
        #endregion 

        #region properties

        public string DeviceName;
		public string DeviceType;
		public string BaudRate;

		private RASDIALPARAMS DialParams = new RASDIALPARAMS();

		#region Dial param properties

		/// <summary>
		/// Determines the RAS connectoid to be used for the connection
		/// </summary>
		/// <value>The name of the RAS phone book entry (connectoid) that defines the connection.</value>
		/// <remarks>
		/// Set this property to the name of the Phonebook Entry (connectoid) that defines
		/// the RAS connection you wish to use. Use the <see cref="ListEntries"/> method to retrieve a
		/// list of available connectoids.
		/// When EntryName is string.Empty, reading the <see cref="Connection"/> property will
		/// return the first RAS connection, which can then be used with several other RasClient
		/// methods to manage a connection that was not established by the RasClient.
		/// </remarks>
		[Category("Entry parameters")]
		[Description("Determines the RAS connectoid to be used for the connection")]
		public string EntryName 
		{
			get 
			{ 
				if (DialParams.szEntryName == null)
					return string.Empty;
				return DialParams.szEntryName;
			}        
			set { DialParams.szEntryName = value; }
		}

		/// <summary>
		/// Determines the phone number to dial to establish the connection
		/// </summary>
		/// <value>A string determining the phone number that the device will dial to establish the connection.</value>
		/// <remarks></remarks>
		[Category("Entry parameters")]
		[Description("Determines the phone number to dial to establish the connection")]
		public string PhoneNumber 
		{
			get 
			{
				if (DialParams.szPhoneNumber == null)
					return string.Empty;
				return DialParams.szPhoneNumber; 
			}
			set { DialParams.szPhoneNumber = value; }
		}
    
		/// <summary>
		/// String defining the number that the RAS Server will call back on
		/// </summary>
		/// <value>The phone number that a RAS Server will call back on.</value>
		/// <remarks>In some RAS configurations, a client will call the server, authenticate, then 
		/// the server will hangup and return the call. This property determines the phone number
		/// that the server will call back on.</remarks>
		[Category("Dial parameters")]
		[Description("String defining the number that the RAS Server will call back on.")]
		public string CallBackNumber 
		{
			get 
			{ 
				if (DialParams.szCallbackNumber == null)
					return string.Empty;
				return DialParams.szCallbackNumber; 
			}
			set { DialParams.szCallbackNumber = value; }
		}
    
		/// <summary>
		/// Determines the UserName used to authenticate the connection
		/// </summary>
		/// <value>A string determining the name of the user to authenticate.</value>
		/// <remarks>UserName is used with the <see cref="Password"/> property to authenticate
		/// the connection with the RAS server.  No encryption is used to stream this property from the resource file, 
		/// appropriate measures should be used to prevent the user name from being compromised.</remarks>
		[Category("Entry parameters")]
		[Description("Determines the UserName used to authenticate the connection.")]
		public string UserName 
		{
			get 
			{ 
				if (DialParams.szUserName == null)
					return string.Empty;
				return DialParams.szUserName; 
			}
			set { DialParams.szUserName = value; }
		}

		/// <summary>
		/// Determines the password used to authenticate the connection
		/// </summary>
		/// <value>A string containing the password used to authenticate the connection.</value>
		/// <remarks>The Password is used with the <see cref="UserName"/> property to authenticate
		/// the RAS client system on the network. No encryption is used to stream this property from the resource file, 
		/// appropriate measures should be used to prevent the password from being compromised.</remarks>
		[Category("Entry parameters")]
		[Description("Determines the password used to authenticate the connection.")]
		public string Password 
		{
			get 
			{ 
				if (DialParams.szPassword == null)
					return string.Empty;
				return DialParams.szPassword; 
			}
			set { DialParams.szPassword = value; }
		}

		/// <summary>
		/// Determines the domain that will authenticate the connection
		/// </summary>
		/// <value>A string containing the network domain that the RAS client should attempt to join.</value>
		/// <remarks></remarks>
		[Category("Entry parameters")]
		[Description("Determines the domain that will authenticate the connection.")]
		public string Domain 
		{
			get 
			{ 
				if (DialParams.szDomain == null)
					return string.Empty;
				return DialParams.szDomain; 
			}
			set { DialParams.szDomain = value; }
		}
		#endregion
    
		private BitArray DialExtOptions = new BitArray(7, false);

		#region Dial extension properties
		/// <summary>
		/// Determines whether the prefix and suffix from the phonebook is used
		/// </summary>
		/// <value>true to use the prefix and suffix defined by the phonebook entry,
		/// false to ignore them.</value>
		/// <remarks></remarks>
		[Category("Dial parameters")]
		[Description("Determines whether the prefix and suffix from the phonebook is used.")]
		public bool UsePrefixSuffix 
		{
			get { return DialExtOptions.Get(1); }
			set { DialExtOptions.Set(1, value); }
		}

		/// <summary>
		/// Determines whether paused states are accepted or not
		/// </summary>
		/// <value>true to accept paused states, false to not accept paused states.</value>
		/// <remarks>Paused states include terminal mode, retry logon, change password, etc.</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether paused states are accepted or not.")]
		public bool AcceptPausedStates 
		{
			get { return DialExtOptions.Get(2); }
			set { DialExtOptions.Set(2, value); }
		}

		/// <summary>
		/// Determines whether the modem speaker setting in the phonebook is used.
		/// </summary>
		/// <value>true to ignore the modem speaker setting defined in the phone book entry
		/// and use the value of <see cref="SetModemSpeaker"/>, false to honor that setting.</value>
		/// <remarks>If true, SetModemSpeaker determines the state of the modem speaker.</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether the modem speaker setting in the phonebook is used.")]
		public bool IgnoreModemSpeaker 
		{
			get { return DialExtOptions.Get(3); }
			set { DialExtOptions.Set(3, value); }
		}

		/// <summary>
		/// Determines whether the modem speaker is on or off when IgnoreModemSpeaker is true 
		/// </summary>
		/// <value>true to enable the modem speaker, false to disable it.</value>
		/// <remarks>This property is only relevant when the <see cref="IgnoreModemSpeaker"/> property
		/// is true.  When IgnoreModemSpeaker is true, this property determines whether the modem
		/// speaker is enabled or not when establishing the connection.</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether the modem speaker is on or off when IgnoreModemSpeaker is true.")]
		public bool SetModemSpeaker 
		{
			get { return DialExtOptions.Get(4); }
			set { DialExtOptions.Set(4, value); }
		}

		/// <summary>
		/// Determines whether software compression settings in the phonebook are used or not
		/// </summary>
		/// <value>true to ignore the software compression settings defined in the phone book entry,
		/// false to use those settings.</value>
		/// <remarks>If true, <see cref="SetSoftwareCompression"/> determines the software compression</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether software compression settings in the phonebook are used or not.")]
		public bool IgnoreSoftwareCompression 
		{
			get { return DialExtOptions.Get(5); }
			set { DialExtOptions.Set(5, value); }
		}

		/// <summary>
		/// Determines whether software compression is used if IgnoreSoftwareCompression is true.
		/// </summary>
		/// <value>true to enable software compression, false to disable it.</value>
		/// <remarks>This property is only relevant when the <see cref="IgnoreSoftwareCompression"/> property
		/// is true, otherwise the software compression settings in the entry's phone book are used.</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether software compression is used if IgnoreSoftwareCompression is true.")]
		public bool SetSoftwareCompression 
		{
			get { return DialExtOptions.Get(6); }
			set { DialExtOptions.Set(6, value); }
		}
		#endregion
    
		private RasCompressionMode compressionMode;
		/// <summary>
		/// Determines whether data compression is enabled or not
		/// </summary>
		/// <value>CompressionMode determines whether data compression is negotiated
		/// for the call, and the type of compression negotiated.</value>
		/// <remarks></remarks>
		[Category("Dial parameters")]
		[Description("Determines whether data compression is enabled or not.")]
		public RasCompressionMode CompressionMode 
		{
			get { return compressionMode; }
			set { compressionMode = value; }
		}
    
		// FxCop will tag this as needing IDisposable, but this isn't allocating anything
		private IntPtr connection = IntPtr.Zero;
		/// <summary>
		/// The handle of the RAS connection
		/// </summary>
		/// <value>RAS identifies connections through Window handles. For ease of use, this
		/// property provides the handle as an int instead of an IntPtr.</value>
		/// <remarks>
		/// If the EntryName property is empty, this will provide an int representing the
		/// first connection handle found, which can then be used with several RasClient 
		/// methods to control that connection. If EntryName is not empty, this will
		/// provide an int representing the handle of the connection associated with the
		/// EntryName. If no connections are present, Connection will be 0.
		/// </remarks>
		[Browsable(false)]
		public int Connection 
		{
			get 
			{
				if (connection != IntPtr.Zero)
					return connection.ToInt32(); 
				ArrayList list = new ArrayList();
				RasError res = ListConnections(list);
                if ((res != RasError.Success) || (list.Count == 0))
                {
                    log.Debug("Setting connection to zero");
                    connection = IntPtr.Zero;
                }
                else
                {
                    if (EntryName == string.Empty)
                        connection = (IntPtr)(((RasConnection)(list[0])).ConnectionHandle);
                    else
                        foreach (RasConnection conn in list)
                        {
                            if (conn.EntryName == EntryName)
                                connection = (IntPtr)(conn.ConnectionHandle);
                        }
                }
				return connection.ToInt32();        
			}
		}

		/// <summary>
		/// Indicates whether a connection is present or not
		/// </summary>
		/// <value>Indicates whether RAS is connected or not.</value>
		/// <remarks>After calling the Dial or DialDlg method, this property will indicate whether
		/// the connection has been established or not.</remarks>
		[Browsable(false)]
		public bool Connected 
		{
			get { return !(connection == IntPtr.Zero); }//connected; }
		}

		/// <summary>
		/// Get the IP address of the connected server.  The PPP support
		/// that underlies this property is not implemented by all servers.
		/// </summary>
		[Browsable(false)]
		public System.Net.IPAddress ServerIPAddress
		{
			get
			{
				RASPPPIP rasPppIP = new RASPPPIP();
				int size = Marshal.SizeOf(typeof(RASPPPIP));
				uint e = RasApi.RasGetProjectionInfo(connection, RasProjection.RASP_PppIp, rasPppIP, ref size);
				if (e == 0)
				{
					return System.Net.IPAddress.Parse(rasPppIP.szServerIpAddress);
				}
				else
				{
					throw new ApplicationException("RasGetProjectionInfo(): error " + e);
				}
			}
		}

		/// <summary>
		/// Indicates the state of the connection
		/// </summary>
		/// <returns>A <see cref="RasConnectionState"/> indicating the state of the connection</returns>
		/// <remarks>The RasConnectionState indicates the state of the connection.</remarks>
		public RasConnectionState ConnectState() 
		{
			if (!Connected)
				return RasConnectionState.Idle;
			return ConnectState(Connection);      
		}

		/// <summary>
		/// Indicates the state of a connection
		/// </summary>
		/// <param name="newConnection">The handle of the connection to get the state from</param>
		/// <returns>A <see cref="RasConnectionState"/> indicating the state of the connection</returns>
		/// <remarks>This overload can be used to obtain the state of a connection not managed by the RasClient.</remarks>
		public RasConnectionState ConnectState(int newConnection) 
		{
			RasConnectState rcs = new RasConnectState();
			if (RasApi.RasGetConnectStatus(new IntPtr(newConnection), rcs)==0)
				return rcs.rasconnstate;
			else 
				return RasConnectionState.Unknown;
		}   
  
		private RasDialMode dialMode = RasDialMode.Sync;
		private bool waitingForSync = false;
		private RasError asynchronousError = RasError.Success;

		/// <summary>
		/// Determines whether a Dial will occur synchronously or asynchronously
		/// </summary>
		/// <value>A <see cref="RasDialMode"/> determining whether calling <see cref="Dial"/> will return
		/// immediately (RasDialMode.Async) or will return once the connection is established 
		/// or fails (RasDialMode.Sync).</value>
		/// <remarks>Note that DialMode is ignored when the <see cref="DialDialog"/> method is used. DialDlg is
		/// always synchronous (does not return until the connection is established or fails).</remarks>
		[Category("Dial parameters")]
		[Description("Determines whether a Dial will return immediately (asynchronous) or will return after a connection attempt completes(synchronous).")] 
		public RasDialMode DialMode 
		{
			get { return dialMode; }
			set { dialMode = value; }
		}
    
		private bool hangupOnDestroy = true;
		/// <summary>
		/// Determines whether RAS will disconnect when this component is destroyed or not
		/// </summary>  
		/// <value>true to terminate the connection when the RasClient is destroyed, false
		/// to keep the connection alive.</value>  
		/// <remarks></remarks>
		[Category("Dial parameters")]
		[Description("Determines whether RAS will disconnect when this component is destroyed or not.")]
		public bool HangUpOnDestroy 
		{
			get { return hangupOnDestroy; }
			set { hangupOnDestroy = value; }
		}
    
		private string phoneBook = (Environment.OSVersion.Platform == PlatformID.Win32NT &&
			Environment.OSVersion.Version.Major >= 5 &&
			Environment.OSVersion.Version.Minor == 0) 
			? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			@"Microsoft\Network\Connections\Pbk\Rasphone.pbk")
			: string.Empty;
		/// <summary>
		/// Determines the RAS phone book that contains the connectoid
		/// </summary>
		/// <value>A string containing the full path and name of the phone book file.</value>
		/// <remarks>This property is ignored on Windows 9x/ME. For NT4 and above, this
		/// property can be string.Empty to indicate the default phonebook, or can be
		/// an explicit path\name of a secondary phone book.
		/// NOTE: for Windows 2000 and a service running as LocalSystem, using NULL will fail.
		/// See: http://support.microsoft.com/default.aspx?scid=kb%3Ben-us%3B884502 for details.
		/// As a workaround, we set it explicity for just Windows 2000</remarks>
		[Category("Entry parameters")]
		[Description("Determines the RAS phone book that contains the connectoid.")]
		public string PhoneBook 
		{
			get { return phoneBook; }
			set 
			{ 
				if (phoneBook != value)
					phoneBook = value; 
				if (phoneBook == null)
					phoneBook = string.Empty;
			}
		}

		private string script = string.Empty;
		/// <summary>
		/// Determines what script is run after the modem connects but before the PPP setup code is run
		/// </summary>
		/// <value>A string containing the full path of a script to be run.</value>
		/// <remarks>This property is String.Empty when no script is specified or the full path
		/// to a script file to run.  
		/// Details: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/rras/rras/rasentry_str.asp</remarks>
		[Category("Entry parameters")]
		[Description("Determines what script is run after the modem connects but before the PPP setup code is run")]
		public string Script
		{
			get { return script; }
			set
			{
				if (script != value)
					script = value;
				if (script == null)
					script = string.Empty;
			}
		}


		#endregion

		#region methods

		#region InternalPB - Internal phonebook translation
		/// <summary>
		/// Internal phonebook translation, NT/2K/XP require a real null if no phonebook
		/// is specified, 9x/ME require a real null all the time
		/// Vista also requires null
		/// </summary>
		private string InternalPB 
		{
			get 
			{   
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
					return null;
				if (phoneBook == string.Empty)
					return null;
				else
				{
					if (Environment.OSVersion.Version.Major >= 6)
						return null;
					return phoneBook;
				}
			}
		}
		#endregion

		#region Dial
		/// <summary>
		/// Establish the RAS connection
		/// </summary>
		/// <returns>A <see cref="RasError"/> indicating the result of the connection</returns>
		/// <example>The following example selects a RAS connectoid from a ListBox (previouly
		/// filled with ListEntries) and attempts to establish the connection.
		/// <code>
		/// ras.EntryName = listBox1.Text;
		/// ras.DialMode = RasDialMode.Sync;
		/// RasError res = ras.Dial();
		/// if (res==RasError.Success)
		///   // the connection was established
		/// </code>
		/// </example>
		/// 
		/// <remarks>When <see cref="RasDialMode"/> is RasDialMode.Sync, Dial will return RasError.Success
		/// if a connection was established, or another RasError indicating the failure. If RasDialMode is
		/// RasDialMode.Async, Dial will return RasError.Success immediately if the connection attempt could
		/// be started. The <see cref="DialStatus"/> event will be generated periodically when DialMode is
		/// RasDialMode.Async.
		/// Under some conditions, it may be desirable to specify the connection parameters without using
		/// a phone book. To implement this, at a minimum, the <see cref="PhoneNumber"/>, <see cref="UserName"/>,
		/// and <see cref="Password"/> must be set before calling Dial.</remarks>
		/// <seealso>DialDialog</seealso>
		public RasError Dial() 
		{
			if (connection != IntPtr.Zero) 
			{
				try { HangUp(); }
				catch (Exception) { /* ignore */ }
				return RasError.ShareConnectionFailed;
			}

			uint res = 0; // RasError.Success

			res = SetPhoneBookEntryBaudRate();
			if (res != (uint)RasError.Success)
			{
				try { HangUp(); }
				catch (Exception) { /* ignore */ }
				return (RasError)res;
			}

			// Handle setting the Script property into the Ras Entry
			RasEntry rasEntry = new RasEntry();
			int rasEntrySize = Marshal.SizeOf(typeof(RasEntry));
			rasEntry.Size = rasEntrySize;
			res = RasApi.RasGetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, ref rasEntrySize, 0, 0);
			if (res != (uint)RasError.Success)
			{
				try { HangUp(); }
				catch (Exception) { /* ignore */ }
				return (RasError)res;
			}
			if (this.Script != rasEntry.Script)
			{
				rasEntry.Script = this.Script;
				res = RasApi.RasSetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, rasEntrySize, 0, 0);
			}
			if (res != (uint)RasError.Success)
			{
				try { HangUp(); }
				catch (Exception) { /* ignore */ }
				return (RasError)res;
			}

			byte[] optarray = new byte[7];
			DialExtOptions.CopyTo(optarray, 0);
			uint opts = BitConverter.ToUInt32(optarray, 0);
			RASDIALEXTENSIONS DialExtensions;
			if (opts == 0)
				DialExtensions = null;
			else 
			{
				DialExtensions = new RASDIALEXTENSIONS();
				DialExtensions.dwfOptions = opts;
				DialExtensions.hwndParent = 0;
			}     
			waitingForSync = (dialMode == RasDialMode.Sync);

			res = RasApi.RasDial(DialExtensions, InternalPB, DialParams, 1, RasDialFunc1Delegate, ref connection);
			if (res != (uint)RasError.Success)
			{
                log.Debug("RAS Error Encountered. Error : " + res.ToString());               

				try { HangUp(); }
				catch (Exception) { /* ignore */ }
				return (RasError)res;
			}

			// if we're synchronous, spin here until we get a connection or timeout
			// we usually program the modems for 3 minutes so our timeout is a bit longer
			DateTime tStart = DateTime.UtcNow;
            if (dialMode == RasDialMode.Sync) 
			{
                log.Debug("Waiting for Sync.");
				while(waitingForSync && (DateTime.UtcNow < tStart + DialTimeout))
				{
					Thread.Sleep(20);
				}

				res = (uint)asynchronousError;
				if (res != (uint)RasError.Success)
				{
                    log.Debug("We got a connection handle but encountered an error.  Clean up.");
                    try { HangUp(); }
					catch (Exception) { /* ignore */ }
				}
			}
			
			return (RasError)res; 
		}
		#endregion

		#region DialDialog
		/// <summary>
		/// Displays the RAS Dial dialog for the current RAS connectoid.
		/// </summary>
		/// <returns>
		/// RasError.Success if successful, or a RasError if unsuccessful. Note
		/// that this is a synchronous method, control will not return until
		/// after the connection attempt is completed.
		/// </returns>
		/// <remarks>DialDialog is a synchronous method, control will not return until the connection
		/// attempt succeeds or fails.</remarks>
		/// <seealso>Dial</seealso>
		public RasError DialDialog() 
		{
			uint res = 0;
			if (connection == IntPtr.Zero) 
			{
				RASDIALDLG rdd = new RASDIALDLG();
				rdd.dwSize = Marshal.SizeOf(typeof(RASDIALDLG));
        
				bool result = RasApi.RasDialDlg(InternalPB, EntryName, PhoneNumber, out rdd);
				// a false result with dwError == 0 means the user cancelled the connection attempt
				if (!result) 
				{
					if (rdd.dwError == 0)
						res = (uint)RasError.Cancelled;
					else
						res = (uint)rdd.dwError;
				} 
				else 
				{
					// DialDlg doesn't give us the connection handle, so we'll
					// look it up
					ArrayList list = new ArrayList();
					res = (uint)ListConnections(list);
                    if (((RasError)res != RasError.Success) || (list.Count == 0))
                    {
                        log.Debug("Setting connection to zero");
                        connection = IntPtr.Zero;
                    }
                    else
                    {
                        foreach (RasConnection conn in list)
                        {
                            if (conn.EntryName == EntryName)
                                connection = (IntPtr)(conn.ConnectionHandle);
                        }
                    }
				}
			}
			return (RasError)res;
		}
		#endregion  

		#region Hangup
		/// <summary>
		/// Terminates the RAS connection
		/// </summary>
		/// <returns>RasError.Success if successful, or a RasError if unsuccessful. </returns>
		/// <remarks>Terminating a RAS connection can be immediate, or can take several seconds.
		/// Hangup returns after the connection has been terminated.</remarks>
		public RasError HangUp()
        {
            uint res = 0;
            log.Debug(string.Format("Connection Value = {0}", connection));
            if (connection == IntPtr.Zero)
            {
                ArrayList list = new ArrayList();
                res = (uint)ListConnections(list);
                if (((RasError)res != RasError.Success) || (list.Count == 0))
                {
                    log.Debug("Setting connection to zero");
                    connection = IntPtr.Zero;
                }
                else
                {
                    foreach (RasConnection conn in list)
                    {
                        if (conn.EntryName == EntryName)
                            connection = (IntPtr)(conn.ConnectionHandle);
                    }
                }
                if (connection == IntPtr.Zero)
                {
                    return RasError.NoConnection;
                }
            }

            res = RasApi.RasHangUp(connection);
            if (res != (uint)RasError.Success)
            {
                return (RasError)res;
            }

            // RasHangup returns immediately, but the hangup may actually occur later,
            // spin through a non-intrusive method until some kind of error comes back
            // or we hit a (long) timeout.
            RasConnectState rcs = new RasConnectState();
            DateTime tStart = DateTime.UtcNow;
            log.Debug("Waiting for actual hangup to happen.");
            log.Debug(string.Format("Abort Time = {0}", (tStart + HangUpTimeout).ToString()));
            while (RasApi.RasGetConnectStatus(connection, rcs) == (uint)RasError.Success &&
                (DateTime.UtcNow < tStart + HangUpTimeout))
            {
                Thread.Sleep(20);
            }
            log.Debug(string.Format("RasApi Status = {0}", RasApi.RasGetConnectStatus(connection, rcs).ToString()));
            {
                log.Debug("Setting connection to zero");
                connection = IntPtr.Zero;
            }
            return (RasError)res;
        }
		#endregion

		#region ListEntries
		/// <summary>
		/// Provides a list of installed RAS connectoids
		/// </summary>
		/// <param name="entryList">An ArrayList containing the names of the connectoids</param>
		/// <returns><see cref="RasError"/>.Success if successful, or a RasError if unsuccessful</returns>
		/// <remarks>This method is used to retrieve the names of available RAS phone book entries (connectoids).</remarks>
		/// <example>The following example shows how to list the installed RAS connectoids in a ListBox.
		/// <code>
		/// ArrayList list = new ArrayList();
		/// RasError res = ras.ListEntries(list);
		/// if (res == RasError.Success)      
		///   foreach(string name in list)
		///     listBox1.Items.Add(name);
		/// else
		///   listBox1.Items.Add("Couldn't list entries");</code>
		/// </example>
		public RasError ListEntries(ArrayList entryList) 
		{
			RASENTRYNAME[] entryname = new RASENTRYNAME[1];
			entryname[0].dwSize = Marshal.SizeOf(typeof(RASENTRYNAME));
			// read it once with a small buffer to receive the number of entries
			int numentries;       
			int buffsize = Marshal.SizeOf(typeof(RASENTRYNAME));
      
			uint res = RasApi.RasEnumEntries(null, InternalPB, entryname, ref buffsize, out numentries);
			if ((numentries > 0)&&((res == (uint)RasError.Success)||(res == (uint)RasError.BufferTooSmall))) 
			{
				// now that we have the number of entries, call it again
				RASENTRYNAME[] entrynames = new RASENTRYNAME[numentries];
				for(int i=0;i<entrynames.Length;i++)
					entrynames[i].dwSize = Marshal.SizeOf(typeof(RASENTRYNAME));
				buffsize = Marshal.SizeOf(typeof(RASENTRYNAME))* numentries;
				res = RasApi.RasEnumEntries(null, InternalPB, entrynames, ref buffsize, out numentries);
				entryList.Clear();

				foreach(RASENTRYNAME en in entrynames)
					entryList.Add(en.szEntryName);
			}
			return (RasError)res;
		}
		#endregion

		#region ListConnections
		/// <summary>
		/// Provides a list of active connections
		/// </summary>
		/// <param name="connectionList">An ArrayList which will contain a <see cref="RasConnection"/> structure for each active connection</param>
		/// <returns><see cref="RasError"/>.Success if successful, or another RasError if unsuccessful</returns>
		/// <remarks>
		/// If successful, the connectionList parameter contains an array of RasConnection structures. The RasConnection structure has several
		/// fields that can be used to identify the connection.
		/// </remarks>
		/// <example>The following example shows how to list all connections and display them in a ListBox.
		/// <code>
		///   ArrayList list = new ArrayList();
		///   RasError res = ras.ListConnections(list);
		///   if (res == RasError.Success)
		///     foreach(RasConnection conn in list)
		///     {         
		///       listBox1.Items.Add("Handle: " + conn.hrasconn.ToString());
		///       listBox1.Items.Add("Entry name: " + conn.EntryName);
		///       listBox1.Items.Add("Device type: " + conn.DeviceType);
		///       listBox1.Items.Add("Device name: " + conn.DeviceName);
		///       listBox1.Items.Add("");
		///     }
		///     else
		///       listBox1.Items.Add("Couldn't list connections (" + res.ToString() + ")");
		/// </code>
		/// </example>
		public RasError ListConnections(ArrayList connectionList) 
		{
			RasConnection[] conn = new RasConnection[1];
			conn[0].Size = Marshal.SizeOf(typeof(RasConnection));
			// read it once with a small buffer to receive the number of connections
			int numconnections;
			int buffsize = Marshal.SizeOf(typeof(RasConnection));

			uint res = RasApi.RasEnumConnections(conn, ref buffsize, out numconnections);
           


			if ((numconnections > 0)&&((res==(uint)RasError.Success)||(res==(uint)RasError.BufferTooSmall))) 
			{
				// now that we have the number of connections, call it again
				RasConnection[] conns = new RasConnection[numconnections];
				for(int i=0;i<conns.Length;i++)
					conns[i].Size = Marshal.SizeOf(typeof(RasConnection));
				buffsize = Marshal.SizeOf(typeof(RasConnection))*numconnections;
				res = RasApi.RasEnumConnections(conns, ref buffsize, out numconnections);
				connectionList.Clear();

				foreach(RasConnection con in conns)
					connectionList.Add(con);
			}
			return (RasError)res;	      
        }    
		#endregion

		#region ShowCreateEntryDialog
		/// <summary>
		/// Displays the Create New Phonebook Entry dialog
		/// </summary>
		/// <returns>A RasError indicating the result of the dialog</returns>
		/// <remarks>ShowCreateEntryDialog will display the same dialog that the Windows 2000 and XP
		/// New Connection Wizard displays.</remarks>
		public RasError ShowCreateEntryDialog() 
		{
			return (RasError)RasApi.RasCreatePhonebookEntry(IntPtr.Zero, InternalPB);
		}
		#endregion

		#region CreateEntry
		/// <summary>
		/// Creates New Phonebook Entry programmatically
		/// </summary>	
		/// <param name="entryName">The current name of the entry</param>		
		/// <remarks>CreateEntry will create a phonebook entry using the default entry and a basis.</remarks>
		public RasError CreateEntry() 
		{
			// Handle setting the new entry into the phone book
			uint res = 0; // RasError.Success
			RasEntry rasEntry = new RasEntry();
			int rasEntrySize = Marshal.SizeOf(typeof(RasEntry));
			rasEntry.Size = rasEntrySize;
			// On windows 2000, we must set the DeviceType and DeviceName fields to
			// the first modem we find (XP and later does this automatically).
			if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
				Environment.OSVersion.Version >= new Version("5.0"))
			{
				if (this.DeviceType == null && this.DeviceName == null)
				{
					bool found = false;
					ArrayList list = new ArrayList();
					if (this.ListDevices(list) != RasError.Success)
					{
						throw new FatalRasException("Could not list TAPI devices");
					}
					foreach (object o in list)
					{
						RasDeviceInfo r = (RasDeviceInfo)o;
						if (r.DeviceType == "modem")
						{
							this.DeviceType = r.DeviceType;
							this.DeviceName = r.DeviceName;
							found = true;
							break;
						}
					}
					if (found != true)
					{
						throw new FatalRasException("Could not find a TAPI modem");
					}
				}
				rasEntry.DeviceType = this.DeviceType;
				rasEntry.DeviceName = this.DeviceName;
			}
			res = RasApi.RasSetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, rasEntrySize, 0, 0);			
			return (RasError)res;
		}
		#endregion
		
		#region SetRemoteDefaultGateway
		/// <summary>
		/// Set/Unset the RASEO_RemoteDefaultGateway option
		/// </summary>
		/// <param name="featureEnabled">true if RemoteDefaultGateway is to be set</param>
		/// <returns>RasError indicating success or failure</returns>
		public RasError SetRemoteDefaultGateway(bool featureEnabled)
		{
			uint res = 0; // RasError.Success
			RasEntry rasEntry = new RasEntry();
			int rasEntrySize = Marshal.SizeOf(typeof(RasEntry));
			rasEntry.Size = rasEntrySize;
			res = RasApi.RasGetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, ref rasEntrySize, 0, 0);
			if (res != (uint)RasError.Success)
			{
				return (RasError)res;
			}
			if ((rasEntry.OptionsFlags & (int)RasEntryOptions.RemoteDefaultGateway) != 0)
			{
				rasEntry.OptionsFlags = (rasEntry.OptionsFlags & ~(int)RasEntryOptions.RemoteDefaultGateway);
				res = RasApi.RasSetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, rasEntrySize, 0, 0);
			}
			return (RasError)res;
		}

		#endregion

		#region EntryExists
		/// <summary>
		/// Attempts to load the entry to determine if it exsists.
		/// </summary>	
		/// <remarks></remarks>
		public RasError EntryExists() 
		{
			// Handle setting the new entry into the phone book
			uint res = 0; // RasError.Success
			RasEntry rasEntry = new RasEntry();
			int rasEntrySize = Marshal.SizeOf(typeof(RasEntry));
			rasEntry.Size = rasEntrySize;
			int devInfo = 0;
			int devInfoSize = 0;
			res = RasApi.RasGetEntryProperties(this.InternalPB, this.EntryName, ref rasEntry, ref rasEntrySize, devInfo, devInfoSize);			
			return (RasError)res;
		}
		#endregion

		#region DeleteEntry
		/// <summary>
		/// Deletes the specified entry from the phonebook
		/// </summary>
		/// <param name="entryName">The name of the entry to delete</param>
		/// <returns>A RasError indicating the result of the delete request</returns>
		/// <remarks>DeleteEntry permanently deletes the entry.</remarks>
		public RasError DeleteEntry(string entryName) 
		{
			return (RasError)RasApi.RasDeleteEntry(InternalPB, entryName);
		}

		/// <summary>
		/// Deletes the currently selected EntryName from the Phonebook
		/// </summary>
		/// <returns>A RasError indicating the result of the delete request</returns>
		/// <remarks>When deleting the entry defined by EntryName, further attempts to
		/// use the connectoid will result in RasError.CannotFindPhonebookEntry.</remarks>
		public RasError DeleteEntry() 
		{
			return DeleteEntry(DialParams.szEntryName);
		}
		#endregion

		#region RenameEntry
		/// <summary>
		/// Renames the specified entry in the phonebook
		/// </summary>
		/// <param name="oldName">The current name of the entry</param>
		/// <param name="newName">The new name of the entry</param>
		/// <returns>A RasError indicating the result of the rename request</returns>
		/// <remarks></remarks>
		public RasError RenameEntry(string oldName, string newName) 
		{
			return (RasError)RasApi.RasRenameEntry(InternalPB, oldName, newName);
		}

		/// <summary>
		/// Renames the currently selected EntryName in the phonebook
		/// </summary>
		/// <param name="newName">The new name of the entry</param>
		/// <returns>A RasError indicating the result of the rename request</returns>
		/// <remarks>RenameEntry does not change the <see cref="EntryName"/> property to reflect
		/// the new name of the entry.</remarks>
		public RasError RenameEntry(string newName) 
		{
			return (RasError)RasApi.RasRenameEntry(InternalPB, DialParams.szEntryName, newName);
		}
		#endregion

		#region EditEntry
		/// <summary>
		/// Displays the RAS edit dialog to edit the specified RAS phonebook entry
		/// </summary>
		/// <param name="entryName">The name of the entry to edit</param>
		/// <returns>A RasError indicating the result of the delete request</returns>
		/// <remarks></remarks>
		public RasError EditEntry(string entryName) 
		{
			return (RasError)RasApi.RasEditPhonebookEntry(IntPtr.Zero, InternalPB, entryName);
		}

		/// <summary>
		/// Displays the RAS edit dialog to edit the currently selected RAS phonebook entry
		/// </summary>
		/// <returns>A RasError indicating the result of the delete request</returns>
		/// <remarks></remarks>
		public RasError EditEntry() 
		{
			return (RasError)RasApi.RasEditPhonebookEntry(IntPtr.Zero, InternalPB, DialParams.szEntryName);
		}
		#endregion

		#region ValidateEntryName
		/// <summary>
		/// Validates the entry name
		/// </summary>
		/// <param name="entryName">The entry name to validate</param>
		/// <returns>A RasError indicating the result of the delete request</returns>
		/// <remarks>The entry name must contain at least one non-white-space character</remarks>
		public RasError ValidateEntryName(string entryName) 
		{
			return (RasError)RasApi.RasValidateEntryName(InternalPB, entryName);
		}
		#endregion

		#region GetPhonebookEntry
		/// <summary>
		/// Provides detailed information about the phonebook entry
		/// </summary>
		/// <param name="entryName">The name of the phonebook entry</param>
		/// <param name="rasEntry">A <see cref="RasEntry"/> which will contain the entry information.</param>
		/// <returns>A RasEntryResult containing the entry</returns>
		/// <remarks>
		/// <example>The following example shows how to retrieve a RasEntry containing the
		/// phone book information for a connectoid. listBox1 contains a list of available
		/// entries (filled with <see cref="ListEntries"/>). In this example, the name of the
		/// device used for this connection is displayed in the form's title bar.
		/// <code>
		/// RasEntry rasEntry = new RasEntry();
		/// RasError res = rasClient1.GetPhonebookEntry(listBox1.Text, ref rasEntry);
		/// Status("GetPhoneBookEntry : " + res.ToString());      
		/// Text = "Device name: " + rasEntry.DeviceName;
		/// </code>
		/// </example>
		///</remarks>
		public RasError GetPhonebookEntry(string entryName, ref RasEntry rasEntry)
		{
			if(entryName == string.Empty)
				return RasError.CannotFindPhonebookEntry;
      
			rasEntry.Size = Marshal.SizeOf(typeof(RasEntry));
			int entryInfoSize = rasEntry.Size;
			int devInfo = 0;
			int devInfoSize = 0;
    
			uint res = RasApi.RasGetEntryProperties(this.InternalPB, entryName, ref rasEntry, ref entryInfoSize, devInfo, devInfoSize);
			return (RasError)res;
		}
		#endregion

		#region GetEntryDetails
		/// <summary>
		/// Provides commonly-used information about a phonebook information
		/// </summary>
		/// <param name="entryName">The name of the phonebook entry</param>
		/// <param name="entryDetails">A <see cref="EntryDetails"/> that provides information about the entry.</param>
		/// <returns>A struct containing commonly used information about an entry and a RasError indicating the result</returns>
		/// <remarks>
		/// This method is tailored to provide the same connectoid information available from
		/// the Windows Explorer Network Connections view. This method also provides the 
		/// connection state.
		/// </remarks>
		public RasError GetEntrySummary(string entryName, ref EntryDetails entryDetails) 
		{   
			RasEntry rasEntry = new RasEntry();
			RasError res = GetPhonebookEntry(entryName, ref rasEntry);
			if (res == RasError.Success) 
			{
				RasConnectionState Connstate = RasConnectionState.Unknown;
				ArrayList list = new ArrayList();
				RasError error = ListConnections(list);
				if (error == RasError.Success) 
				{
					Connstate = RasConnectionState.Idle;
					foreach(RasConnection conn in list) 
					{     
						if (conn.EntryName == entryName) 
						{
							Connstate = ConnectState(conn.ConnectionHandle);
							break;
						}
					}
				}
				entryDetails.Init(rasEntry, Connstate);
			}
			return res;
		}
		#endregion

		#region ListDevices
		/// <summary>
		/// Lists the installed RAS-capable devices
		/// </summary>
		/// <param name="deviceList">An ArrayList which is filled with <see cref="RasDeviceInfo"/> structures defining the devices</param>
		/// <returns>A <see cref="RasError"/> indicating the result of the request</returns>
		/// <remarks>If successful, deviceList will contain a list of RasDeviceInfo structures
		/// defining the install devices capable of establishing a RAS connection.</remarks>
		/// <example>This example populates a ListBox with the name and type of installed devices
		/// <code>
		/// listBox1.Items.Clear();
		/// ArrayList list = new ArrayList();
		/// RasError res = ras.ListDevices(list);
		/// if (res == RasError.Success)      
		///   foreach(RasDeviceInfo name in list)
		///     listBox1.Items.Add(name.DeviceName + "(" + name.DeviceType + ")");
		/// else
		///   listBox1.Items.Add("Couldn't list entries");
		/// </code>
		/// </example>
		public RasError ListDevices(ArrayList deviceList) 
		{
			RasDeviceInfo[] devinfo = new RasDeviceInfo[1];
			devinfo[0].Size = Marshal.SizeOf(typeof(RasDeviceInfo));
			// read it once with a small buffer to receive the number of devices
			int numdevices;
			int buffsize = Marshal.SizeOf(typeof(RasDeviceInfo));

			uint res = RasApi.RasEnumDevices(devinfo, ref buffsize, out numdevices);
			if ((res == (uint)RasError.Success)||(res == (uint)RasError.BufferTooSmall)) 
			{
				// now that we have the number of entries, call it again
				RasDeviceInfo[] devices = new RasDeviceInfo[numdevices];
				for(int i=0;i<devices.Length;i++)
					devices[i].Size = Marshal.SizeOf(typeof(RasDeviceInfo));
				buffsize = Marshal.SizeOf(typeof(RasDeviceInfo))*numdevices;
				res = RasApi.RasEnumDevices(devices, ref buffsize, out numdevices);
				deviceList.Clear();

				foreach(RasDeviceInfo di in devices)
					deviceList.Add(di);
			}
			return (RasError)res;
		}
		#endregion

		#region RasErrorMsg
		/// <summary>
		/// Obtains an error message string for a specified RAS error value
		/// </summary>
		/// <param name="rasError">The RasError for which to obtain the string</param>
		/// <returns>A string describing the RasError error code, or string.Empty if a matching string could not be found</returns>
		/// <remarks></remarks>
		public string RasErrorMsg(RasError rasError)
		{
			string res = string.Empty;
			if (RasApi.RasGetErrorString((uint)rasError, res, 255) == (uint)(RasError.Success))
				return res;
			else
				return string.Empty;
		}
		#endregion

		#region GetStatistics

		/// <summary>
		/// Provides details statistics about the current connection.
		/// </summary>
		/// <param name="rasStatistics"></param>
		/// <returns></returns>
		public RasError GetStatistics(ref RasStatistics rasStatistics)
		{
			if (!Connected)
				return RasError.NoConnection;

			rasStatistics.Size = (uint)Marshal.SizeOf(typeof(RasStatistics));
			uint res = RasApi.RasGetConnectionStatistics(connection, ref rasStatistics);
			return (RasError)res;
		}

		#endregion

		#endregion

        public void Dispose()
        {
            log.Debug("Disposing instance of RasClient");
            log.Debug("Connection value : " + connection);
            if ((hangupOnDestroy) && (connection != IntPtr.Zero))
                HangUp();   
        }
    }
  
	#region RAS event args
	/// <summary>
	/// Event arguments for the ConnectionChanged event
	/// </summary>
	/// <remarks>
	/// ConnectionChangedEventArgs is provided in the ConnectionChanged event when the
	/// RasClient establishes and terminated a connection.</remarks>
	public class ConnectionChangedEventArgs : EventArgs
	{
		private bool connected;
		/// <summary>
		/// Indicates whether a connection is established or not
		/// </summary>
		/// <value>Connected indicates whether a connection is established or not.</value>
		/// <remarks>Connected will be true when a connection has been established and
		/// false when the connection has been terminated.</remarks>
		public bool Connected
		{
			get { return connected; }
		}
		/// <summary>
		/// Initializes a ConnectionChangedEventArgs
		/// </summary>
		/// <param name="isConnected">The state of the connection</param>
		public ConnectionChangedEventArgs(bool isConnected)
		{
			connected = isConnected;
		}
	}
	/// <summary>
	/// Event arguments for the DialError event
	/// </summary>
	/// <remarks>
	/// DialErrorEventArgs are provided in the DialError event when a connection attempt
	/// fails. The properties of this class provide the reason for the failure.
	/// </remarks>
	public class DialErrorEventArgs : EventArgs
	{
		private RasError raserror;
		/// <summary>
		/// A RasError indicating the error that caused the event
		/// </summary>
		/// <value>A <see cref="RasError"/> indicating the reason for the failure.</value>
		/// <remarks></remarks>
		public RasError RasError
		{
			get { return raserror; }
		}
		/// <summary>
		/// Initializes a DialErrorEventArgs
		/// </summary>
		/// <param name="rasError">The RasError describing the error</param>
		public DialErrorEventArgs(RasError rasError)
		{
			this.raserror = rasError;
		}
	}
	/// <summary>
	/// Event arguments for the DialStatus event
	/// </summary>
	public class DialStatusEventArgs : EventArgs
	{
		private RasConnectionState rcs;
		/// <summary>
		/// A RasConnectionState indicating the state of the connection
		/// </summary>
		/// <value>A <see cref="RasConnectionState"/> indicating the state of the connection.</value>
		/// <remarks>During a connection attempt, the <see cref="RasClient.DialStatus"/> event will be generated several
		/// times to provide an indication of the state of the connection attempt. The connection
		/// states will vary to some degree based on the operating system, connection type (dial-up,
		/// VPN, etc) and other factors.</remarks>
		public RasConnectionState ConnectionState
		{
			get { return rcs; }
		}
		/// <summary>
		/// Initializes a DialStatusEventArgs
		/// </summary>
		/// <param name="state">A RasConnectionState indicating the state of the connection</param>
		public DialStatusEventArgs(RasConnectionState state)
		{
			rcs = state;
		}
	}
	#endregion
}