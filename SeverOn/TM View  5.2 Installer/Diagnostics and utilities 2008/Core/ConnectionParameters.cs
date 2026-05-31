/// Copyright (c) 2008 Serveron Corporation. All rights reserved.
using System;
using System.Collections.Generic;

namespace Serveron.Utility.Core
{
	public enum ConnectionInfoKeys : int
	{
		CompanyNameKey,
		CompanyIDKey,
		SiteNameKey,
		SiteIDKey,
		AssetNameKey,
		AssetIDKey,
		MonitorSerialKey,
		MonitorIDKey,
		MonitorTypeKey,
		FileNameKey,
		ConnectionTypeKey,
		IPAddressKey,
		TcpPortKey,
		COMPortKey,
		BaudrateKey,
		HandshakeKey,
		PhoneNumberKey,
		ModemInitKey,
		ScriptKey,
		RadioUnitNumberKey,
		RS485NodeKey,
		ModBusAddressKey,
		DnpEnableKey,

		TitleKey,		// for wizards and menus
		InitialFileBrowseDirectoryKey,	// for wizards and menus
		HelpMessageKey,	// for wizards and menus
		LocalFileNameKey,
		UserNameKey,
		PasswordKey,
		UseSpecialFlagKey,
	}

	/// <summary>
	/// Contains wizard state
	/// </summary>
	public class ConnectionParameters : Dictionary<ConnectionInfoKeys, string>
	{
		new public void Add(ConnectionInfoKeys key, string value)
		{
			if (ContainsKey(key))
			{
				base[key] = value;
			}
			else
			{
				base.Add(key, value);
			}
		}

		/// <summary>
		/// Gets or set a connection parameter indexed by the enum ConnectionInfoKeys
		/// </summary>
		/// <param name="key">A member of the ConnectionInfoKeys enum.</param>
		/// <returns></returns>
		new public string this[ConnectionInfoKeys key]
		{
			get
			{
				if (ContainsKey(key))
					return base[key];
				else
					return "";
			}
			set
			{
				if (ContainsKey(key))
					base[key] = value;
				else
					base.Add(key, value);
			}
		}

		/// <summary>
		/// Get a required connection parameter as a string.
		/// If the parameter is not found an ArgumentException is thrown.
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <param name="key"></param>
		/// <param name="msgBase"></param>
		/// <returns></returns>
		public string Required(ConnectionInfoKeys key, string msgBase)
		{
			if (!ContainsKey(key))
				throw new ArgumentException(string.Format(
					"{0}: argument {1} not provided",
					msgBase, ConnectionInfo.GetName(key)));
			else
				return this[key];
		}

		/// <summary>
		/// Get an integer parameter
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetInt32(ConnectionInfoKeys key, int defaultValue)
		{
			string s = this[key];
			if (string.IsNullOrEmpty(s))
				return defaultValue;
			else
			{
				int result;
				if (int.TryParse(s, out result))
					return result;
				else
					return defaultValue;
			}
		}

		/// <summary>
		/// Get a boolean parameter.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public bool GetBoolean(ConnectionInfoKeys key, bool defaultValue)
		{
			string s = this[key];
			if (string.IsNullOrEmpty(s))
				return defaultValue;
			else
			{
				bool result;
				if (bool.TryParse(s, out result))
					return result;
				else
					return defaultValue;
			}
		}

	}

	public static class ConnectionInfo
	{
		static List<string> KeyNames()
		{
			string[] keyNames = Enum.GetNames(typeof(ConnectionInfoKeys));
			List<string> names = new List<string>();
			foreach (string kn in keyNames)
			{
				if (kn.EndsWith("Key"))
					names.Add(kn.Replace("Key", ""));
			}
			return names;
		}

		static public string GetName(ConnectionInfoKeys key)
		{
			List<string> names = KeyNames();
			return names[(int)key];
		}

		static public ConnectionInfoKeys GetKey(string name)
		{
			List<string> keys = KeyNames();
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i] == name)
					return (ConnectionInfoKeys)i;
			}
            throw new Exception ("Invalid ConnectionInfoKey used" + name);
		}
	}
}