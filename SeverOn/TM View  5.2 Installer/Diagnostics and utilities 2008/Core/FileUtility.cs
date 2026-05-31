using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Text;

/*
 * The classes ShareType, Share, and ShareCollection were obtained from
 * TheCodeProject.com.  TheCodeProject requires that authors who upload
 * code agree that the code is unencumbered.  No copyright or other notices
 * were attached to the code when it was downloaded.
 * 
 * The ShareCollection class makes use of P/Invoke and also supports Win9x.
 * Normally, we place P/Invoke stuff into Win32.cs and no .NET-based Serveron
 * software supports the Win9x platform.  But since the code was obtained
 * from the internet and seems to work well, I decided to leave it this way.
 * Various other changes were made to the code, however.
 * 
 * Jeff 12/2006, 3/2007.
 */
namespace Serveron.Utility.Core
{
    #region File Utility

    /// <summary>
	/// Static utilities for inquiring about file paths
	/// </summary>
	public static class FileUtility
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(FileUtility));

        public static string CleanName(string name)
        {
            string cleaned = name.Replace('-', '_');
            cleaned = cleaned.Replace(' ', '_');
            cleaned = cleaned.Replace('.', '_');
            return cleaned;
        }


		static FileUtility()
		{
			ApplicationSubPath = "TMView";
		}

        /// <summary>
        /// Checks the argument to see if it is
        /// synactically a UNC pathname.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true if the argument begins
        /// \\X\Y where each X and Y consist of one
        /// or more characters other than backslash
        /// and colon.</returns>
        public static bool IsUnc(ref string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) return false;

            // I don't think GetFullPath() ever computes
            // a UNC name for a non-UNC name.  But calling
            // GetFullPath() might clean up some garbage
            // names with too many slashes that could fool
            // the regular expression otherwise.
            filePath = Path.GetFullPath(filePath);
            Regex re = new Regex(@"\\\\[^\\:]+\\[^\\:]+");
            return re.IsMatch(filePath, 0);
        }

        /// <summary>
        /// Check the argument to see if it is an
        /// admin share ("X$").
        /// </summary>
        /// <param name="sharePath">path to check</param>
        /// <returns>true if it appears to be an
        /// admin share.</returns>
        public static bool IsAdminShareName(string sharePath)
        {
            if (String.IsNullOrEmpty(sharePath)) return false;
            sharePath = Path.GetFullPath(sharePath);
            Regex re = new Regex(@"\\[A-Za-z]\$");  // in "\\server\D$\...", the pattern matches "\D$"
            return re.IsMatch(sharePath, 0);
        }

        /// <summary>
        /// Checks the argument to see if it is
        /// syntactically a drive-lettered name.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true if the argument begins
        /// X:\ where the X is any letter.</returns>
        public static bool IsMappedOrLocal(ref string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) return false;
            filePath = Path.GetFullPath(filePath);
            Regex re = new Regex(@"[A-Za-z]:\\");
            return re.IsMatch(filePath, 0);
        }

        /// <summary>
        /// Attempt to create a UNC path corresponding to the argument.
        /// If the argument is a UNC name, it is returned.  If it is on
        /// a remote mapped share, its UNC name is returned.  This
        /// method does not check shares exposed by this host.  If no
        /// UNC name can be found, the argument is returned unchanged.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static String RemotePathToUnc(string filePath)
        {
            string result = PathToUnc(filePath, false, true);
            return result;
        }

        /// <summary>
        /// Attempt to create a UNC path corresponding to the argument.
        /// If the argument is a UNC name, it is returned.  If it is on
        /// an external mapped share, its UNC name is returned.  Finally
        /// if it is on a share we expose from this host, its UNC name
        /// is returned.  This method will detect and use admin shares.
        /// If no UNC name can be found, the argument is returned unchanged.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string PathToUnc(string filePath)
        {
            string result = PathToUnc(filePath, true, true);
            return result;
        }

        /// <summary>
        /// Attempt to create a UNC path corresponding to the argument.
        /// If the argument is a UNC name, it is returned.  If it is on
        /// an external mapped share, its UNC name is returned.  Finally
        /// if it is on a share we expose from this host, its UNC name
        /// is returned if includeSelfMaps is true.  This method will
        /// detect and use admin shares if includeAdminShares is true.
        /// If no UNC name can be found, the argument is returned unchanged.
        /// </summary>
        /// <param name="filePath">path to translate, if possible</param>
        /// <param name="includeSelfMaps">if true, check for shares
        /// we expose from this host, else do not.</param>
        /// <param name="includeAdminShares">if true, admin shares
        /// are returned if foudn, else they are not.</param>
        /// <returns>pathname</returns>
        public static string PathToUnc(string filePath,
            bool includeSelfMaps, bool includeAdminShares)
        {
            string result = ShareCollection.PathToUnc(filePath, includeSelfMaps, includeAdminShares);
			//log.Info(String.Format("PathToUnc({0}, {1}, {2}): {3}",
			//    filePath, includeSelfMaps, includeAdminShares, result));
            return result;
		}
		
		/// <summary>
		/// Path under the All Users\ApplicationData\Serveron folder for the current application
		/// </summary>
		static public string ApplicationSubPath { get; set; }

		static void CreateFolder(string folder)
		{
			if (!Directory.Exists(folder))
			{
				string parent = Path.GetDirectoryName(folder);
				CreateFolder(parent);
				Directory.CreateDirectory(folder);
			}
		}

		/// <summary>
		/// Return a data folder that all users will have Read/Write access to.
		/// </summary>
		static public string ApplicationDataFolder
		{
			get
			{
				string result = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Properties.Resources.CompanyFolder + "\\" + ApplicationSubPath);
				CreateFolder(result);
				return result;
			}
		}

		/// <summary>
		/// Return a data folder that all users will have Read/Write access to.
		/// </summary>
		static public string ApplicationDBFolder
		{
			get
			{
				string result = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					"Serveron\\Data");
				CreateFolder(result);
				return result;
			}
		}

		/// <summary>
		/// Return a data folder that all users will have Read/Write access to.
		/// </summary>
		static public string LocateUserConfigFile(string fileName)
		{
			return Path.Combine(
					Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						"Serveron\\" + ApplicationSubPath),
						fileName);
		}

		/// <summary>
		/// Return a data folder that all users will have Read/Write access to.
		/// </summary>
		static public string LocateConfigFile(string fileName)
		{
			return Path.Combine(
					ApplicationDataFolder,
					fileName);
		}

		/// <summary>
		/// Get the file name without extension for the process being executed.
		/// This is typically used to determine the default name of the log file
		/// and the default name of the configuration file.
		/// </summary>
		static public string ApplicationName
		{
			get
			{
				Assembly assy = Assembly.GetEntryAssembly();
				if (assy != null)
				{
					string programName = Assembly.GetEntryAssembly().CodeBase;
					return Path.GetFileNameWithoutExtension(programName);
				}
				else
				{
					return null;
				}
			}
		}

        static public bool LoadAssembly(string assyName)
        {
            try
            {
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                if (exeDir.StartsWith("file:\\"))
                {
                    int idx = exeDir.IndexOf('\\');
                    exeDir = exeDir.Substring(idx + 1);
                }
                Assembly opc = Assembly.LoadFile(Path.Combine(exeDir, assyName));
                if (opc != null)
                    return true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Can't Load Assembly {0}", assyName), ex);
            }
            return false;
        }

		#region XML Formatting

		static int _level = 0;

		static void Indent(StringBuilder sb)
		{
			for (int i = 0; i < _level; i++)
				sb.Append("\t");
		}

		static void DrillXml(StringBuilder sb, XmlNode parent)
		{
			_level++;
			foreach (XmlNode node in parent.ChildNodes)
			{
				if (node.Name.StartsWith("#"))
				{
					switch (node.Name)
					{
						case "#comment":
							Indent(sb);
							sb.AppendFormat("<!--{0}-->", node.Value);
							sb.AppendLine();
							break;
						case "#text":
							sb.Append(node.Value);
							break;
					}
					continue;
				}
				Indent(sb);
				sb.AppendFormat("<{0}>", node.Name);
				if (node.ChildNodes.Count > 0)
				{
					if (!node.ChildNodes[0].Name.StartsWith("#"))
						sb.AppendLine();
					DrillXml(sb, node);
					if (!node.ChildNodes[0].Name.StartsWith("#"))
						Indent(sb);
				}
				sb.AppendFormat("</{0}>", node.Name);
				sb.AppendLine();
			}
			_level--;
		}

		/// <summary>
		/// Formats the XML so it looks nice in a text editor window
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		static public string PrettyXml(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlElement root = doc.DocumentElement;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("<{0}>", root.Name);
			sb.AppendLine();
			DrillXml(sb, root);
			sb.AppendFormat("</{0}>", root.Name);
			sb.AppendLine();
			string pretty = sb.ToString();
			return pretty;
		}

		#endregion
    }

    #endregion

    #region Share Type

    /// <summary>
    /// Type of share
    /// </summary>
    [Flags]
    internal enum ShareType
    {
        /// <summary>Disk share</summary>
        Disk = 0,
        /// <summary>Printer share</summary>
        Printer = 1,
        /// <summary>Device share</summary>
        Device = 2,
        /// <summary>IPC share</summary>
        IPC = 3,
        /// <summary>Special share</summary>
        Special = -2147483648, // 0x80000000,
    }

    #endregion

    #region Share

    /// <summary>
    /// Information about a local share
    /// </summary>
    internal class Share
    {
        #region Private data

        private string _server;
        private string _netName;
        private string _path;
        private ShareType _shareType;
        private string _remark;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="shi"></param>
        public Share(string server, string netName, string path, ShareType shareType, string remark)
        {
            if (ShareType.Special == shareType && "IPC$" == netName)
            {
                shareType |= ShareType.IPC;
            }

            _server = server;
            _netName = netName;
            _path = path;
            _shareType = shareType;
            _remark = remark;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the computer that this share belongs to
        /// </summary>
        public string Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Share name
        /// </summary>
        public string NetName
        {
            get { return _netName; }
        }

        /// <summary>
        /// Local path
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Share type
        /// </summary>
        public ShareType ShareType
        {
            get { return _shareType; }
        }

        /// <summary>
        /// Comment
        /// </summary>
        public string Remark
        {
            get { return _remark; }
        }

        /// <summary>
        /// Returns true if this is a file system share
        /// </summary>
        public bool IsFileSystem
        {
            get
            {
                // Shared device
                if (0 != (_shareType & ShareType.Device)) return false;
                // IPC share
                if (0 != (_shareType & ShareType.IPC)) return false;
                // Shared printer
                if (0 != (_shareType & ShareType.Printer)) return false;

                // Standard disk share
                if (0 == (_shareType & ShareType.Special)) return true;

                // Special disk share (e.g. C$)
                if (ShareType.Special == _shareType && null != _netName && 0 != _netName.Length)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the root of a disk-based share
        /// </summary>
        public DirectoryInfo Root
        {
            get
            {
                if (IsFileSystem)
                {
                    if (null == _server || 0 == _server.Length)
                        if (null == _path || 0 == _path.Length)
                            return new DirectoryInfo(ToString());
                        else
                            return new DirectoryInfo(_path);
                    else
                        return new DirectoryInfo(ToString());
                }
                else
                    return null;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the path to this share
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (null == _server || 0 == _server.Length)
            {
                return string.Format(@"\\{0}\{1}", Environment.MachineName, _netName);
            }
            else
                return string.Format(@"\\{0}\{1}", _server, _netName);
        }

        /// <summary>
        /// Returns true if this share matches the local path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool MatchesPath(string path)
        {
            if (!IsFileSystem) return false;
            if (null == path || 0 == path.Length) return true;

            return path.ToLower().StartsWith(_path.ToLower());
        }

        #endregion
    }

    #endregion
	
    #region Share Collection

    /// <summary>
    /// A collection of shares
    /// </summary>
    internal class ShareCollection : ReadOnlyCollectionBase
    {
        #region Platform

        /// <summary>
        /// Is this an NT platform?
        /// </summary>
        protected static bool IsNT
        {
            get { return (PlatformID.Win32NT == Environment.OSVersion.Platform); }
        }

        /// <summary>
        /// Returns true if this is Windows 2000 or higher
        /// </summary>
        protected static bool IsW2KUp
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                if (PlatformID.Win32NT == os.Platform && os.Version.Major >= 5)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Interop

        #region Constants

        /// <summary>Maximum path length</summary>
        protected const int MAX_PATH = 260;
        /// <summary>No error</summary>
        protected const int NO_ERROR = 0;
        /// <summary>Access denied</summary>
        protected const int ERROR_ACCESS_DENIED = 5;
        /// <summary>Access denied</summary>
        protected const int ERROR_WRONG_LEVEL = 124;
        /// <summary>More data available</summary>
        protected const int ERROR_MORE_DATA = 234;
        /// <summary>Not connected</summary>
        protected const int ERROR_NOT_CONNECTED = 2250;
        /// <summary>Level 1</summary>
        protected const int UNIVERSAL_NAME_INFO_LEVEL = 1;
        /// <summary>Max extries (9x)</summary>
        protected const int MAX_SI50_ENTRIES = 20;

        #endregion

        #region Structures

        /// <summary>Unc name</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected struct UNIVERSAL_NAME_INFO
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpUniversalName;
        }

        /// <summary>Share information, NT, level 2</summary>
        /// <remarks>
        /// Requires admin rights to work. 
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        protected struct SHARE_INFO_2
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;
            public ShareType ShareType;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;
            public int Permissions;
            public int MaxUsers;
            public int CurrentUsers;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Password;
        }

        /// <summary>Share information, NT, level 1</summary>
        /// <remarks>
        /// Fallback when no admin rights.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        protected struct SHARE_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;
            public ShareType ShareType;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;
        }

        /// <summary>Share information, Win9x</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct SHARE_INFO_50
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string NetName;

            public byte bShareType;
            public ushort Flags;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string Remark;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Path;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
            public string PasswordRW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
            public string PasswordRO;

            public ShareType ShareType
            {
                get { return (ShareType)((int)bShareType & 0x7F); }
            }
        }

        /// <summary>Share information level 1, Win9x</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct SHARE_INFO_1_9x
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string NetName;
            public byte Padding;

            public ushort bShareType;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string Remark;

            public ShareType ShareType
            {
                get { return (ShareType)((int)bShareType & 0x7FFF); }
            }
        }

        #endregion

        #region Functions

        /// <summary>Get a UNC name</summary>
        [DllImport("mpr", CharSet = CharSet.Auto)]
        protected static extern int WNetGetUniversalName(string lpLocalPath,
            int dwInfoLevel, ref UNIVERSAL_NAME_INFO lpBuffer, ref int lpBufferSize);

        /// <summary>Get a UNC name</summary>
        [DllImport("mpr", CharSet = CharSet.Auto)]
        protected static extern int WNetGetUniversalName(string lpLocalPath,
            int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

        /// <summary>Enumerate shares (NT)</summary>
        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        protected static extern int NetShareEnum(string lpServerName, int dwLevel,
            out IntPtr lpBuffer, int dwPrefMaxLen, out int entriesRead,
            out int totalEntries, ref int hResume);

        /// <summary>Enumerate shares (9x)</summary>
        [DllImport("svrapi", CharSet = CharSet.Ansi)]
        protected static extern int NetShareEnum(
            [MarshalAs(UnmanagedType.LPTStr)] string lpServerName, int dwLevel,
            IntPtr lpBuffer, ushort cbBuffer, out ushort entriesRead,
            out ushort totalEntries);

        /// <summary>Free the buffer (NT)</summary>
        [DllImport("netapi32")]
        protected static extern int NetApiBufferFree(IntPtr lpBuffer);

        #endregion

        #region Enumerate shares

        /// <summary>
        /// Enumerates the shares on Windows NT
        /// </summary>
        /// <param name="server">The server name</param>
        /// <param name="shares">The ShareCollection</param>
        protected static void EnumerateSharesNT(string server, ShareCollection shares)
        {
            int level = 2;
            int entriesRead, totalEntries, nRet, hResume = 0;
            IntPtr pBuffer = IntPtr.Zero;

            try
            {
                nRet = NetShareEnum(server, level, out pBuffer, -1,
                    out entriesRead, out totalEntries, ref hResume);

                if (ERROR_ACCESS_DENIED == nRet)
                {
                    //Need admin for level 2, drop to level 1
                    level = 1;
                    nRet = NetShareEnum(server, level, out pBuffer, -1,
                        out entriesRead, out totalEntries, ref hResume);
                }

                if (NO_ERROR == nRet && entriesRead > 0)
                {
                    Type t = (2 == level) ? typeof(SHARE_INFO_2) : typeof(SHARE_INFO_1);
                    int offset = Marshal.SizeOf(t);

                    for (int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += offset)
                    {
                        IntPtr pItem = new IntPtr(lpItem);
                        if (1 == level)
                        {
                            SHARE_INFO_1 si = (SHARE_INFO_1)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, string.Empty, si.ShareType, si.Remark);
                        }
                        else
                        {
                            SHARE_INFO_2 si = (SHARE_INFO_2)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, si.Path, si.ShareType, si.Remark);
                        }
                    }
                }

            }
            finally
            {
                // Clean up buffer allocated by system
                if (IntPtr.Zero != pBuffer)
                    NetApiBufferFree(pBuffer);
            }
        }

        /// <summary>
        /// Enumerates the shares on Windows 9x
        /// </summary>
        /// <param name="server">The server name</param>
        /// <param name="shares">The ShareCollection</param>
        protected static void EnumerateShares9x(string server, ShareCollection shares)
        {
            int level = 50;
            int nRet = 0;
            ushort entriesRead, totalEntries;

            Type t = typeof(SHARE_INFO_50);
            int size = Marshal.SizeOf(t);
            ushort cbBuffer = (ushort)(MAX_SI50_ENTRIES * size);
            //On Win9x, must allocate buffer before calling API
            IntPtr pBuffer = Marshal.AllocHGlobal(cbBuffer);

            try
            {
                nRet = NetShareEnum(server, level, pBuffer, cbBuffer,
                    out entriesRead, out totalEntries);

                if (ERROR_WRONG_LEVEL == nRet)
                {
                    level = 1;
                    t = typeof(SHARE_INFO_1_9x);
                    size = Marshal.SizeOf(t);

                    nRet = NetShareEnum(server, level, pBuffer, cbBuffer,
                        out entriesRead, out totalEntries);
                }

                if (NO_ERROR == nRet || ERROR_MORE_DATA == nRet)
                {
                    for (int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += size)
                    {
                        IntPtr pItem = new IntPtr(lpItem);

                        if (1 == level)
                        {
                            SHARE_INFO_1_9x si = (SHARE_INFO_1_9x)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, string.Empty, si.ShareType, si.Remark);
                        }
                        else
                        {
                            SHARE_INFO_50 si = (SHARE_INFO_50)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, si.Path, si.ShareType, si.Remark);
                        }
                    }
                }
                else
                {
                    log.Warn("EnumerateShares9x: " + nRet);
                }

            }
            finally
            {
                //Clean up buffer
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        /// <summary>
        /// Enumerates the shares
        /// </summary>
        /// <param name="server">The server name</param>
        /// <param name="shares">The ShareCollection</param>
        protected static void EnumerateShares(string server, ShareCollection shares)
        {
            if (null != server && 0 != server.Length && !IsW2KUp)
            {
                server = server.ToUpper();

                // On NT4, 9x and Me, server has to start with "\\"
                if (!('\\' == server[0] && '\\' == server[1]))
                    server = @"\\" + server;
            }

            if (IsNT)
                EnumerateSharesNT(server, shares);
            else
                EnumerateShares9x(server, shares);
        }

        #endregion

        #endregion

        #region Public static methods

        /// <summary>
        /// Returns the UNC path for a mapped drive or local share.
        /// </summary>
        /// <param name="fileName">The path to map</param>
        /// <param name="includeSelfMaps">if true, return
        /// UNC names on shares we expose from this host,
        /// else do not.</param>
        /// <param name="includeAdminShares">if true, return
        /// UNC names include admin shares, else do not.
        /// </param>
        /// <returns>The UNC path (if available)</returns>
        public static string PathToUnc(string fileName,
            bool includeSelfMaps, bool includeAdminShares)
        {
            // The fileName is canonicalized by reference
            // If it is already a UNC name, return it.
            if (!FileUtility.IsMappedOrLocal(ref fileName))
                return fileName;

            int nRet = 0;
            UNIVERSAL_NAME_INFO rni = new UNIVERSAL_NAME_INFO();
            int bufferSize = Marshal.SizeOf(rni);

            nRet = WNetGetUniversalName(
                fileName, UNIVERSAL_NAME_INFO_LEVEL,
                ref rni, ref bufferSize);

            if (ERROR_MORE_DATA == nRet)
            {
                IntPtr pBuffer = Marshal.AllocHGlobal(bufferSize); ;
                try
                {
                    nRet = WNetGetUniversalName(
                        fileName, UNIVERSAL_NAME_INFO_LEVEL,
                        pBuffer, ref bufferSize);

                    if (NO_ERROR == nRet)
                    {
                        rni = (UNIVERSAL_NAME_INFO)Marshal.PtrToStructure(pBuffer,
                            typeof(UNIVERSAL_NAME_INFO));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pBuffer);
                }
            }

            switch (nRet)
            {
                case NO_ERROR:
                    // Found something not exposed by
                    // us.  Return it unless it's an
                    // admin share and admin share maps
                    // we not requested.
                    if (FileUtility.IsAdminShareName(rni.lpUniversalName) &&!includeAdminShares)
                        return fileName;
                    return rni.lpUniversalName;

                case ERROR_NOT_CONNECTED:
                    // fileName doesn't correspond to
                    // any share we know about from a
                    // remote server.
                    if (!includeSelfMaps)
                        return fileName;

                    // Check for a self-map
                    ShareCollection shi = LocalShares;
                    if (null != shi)
                    {
                        Share share = shi[fileName];
                        if (null != share)
                        {
                            // share is the "best-fit" share (the algorithm
                            // picks non-admin shares in preference to admin
                            // ones).  So if we have an admin share at this
                            // point, there's no other alternative; if admin
                            // shares were not requested, return the original
                            // filename.

                            if (FileUtility.IsAdminShareName(share.Path) && !includeAdminShares)
                                return fileName;

                            string path = share.Path;
                            if (null != path && 0 != path.Length)
                            {
                                int index = path.Length;
                                if (Path.DirectorySeparatorChar != path[path.Length - 1])
                                    index++;

                                if (index < fileName.Length)
                                    fileName = fileName.Substring(index);
                                else
                                    fileName = string.Empty;

                                fileName = Path.Combine(share.ToString(), fileName);
                            }
                        }
                    }

                    return fileName;

                default:
                    log.Warn(String.Format("PathToUnc: unknown return value: {0}", nRet));
                    return string.Empty;
            }
        }

#if false
        // This code does not appaer to solve any problem we currently
        // have at Serveron.  Feel free to uncomment it if needed.  It
        // has been modified while "commented out" so may easily be
        // incorrect.

        /// <summary>
        /// Returns the local <see cref="Share"/> object with the best match
        /// to the specified path.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Share PathToShare(string fileName)
        {
            if (!FileUtility.IsMappedOrLocal(fileName))
                return null;
            fileName = Path.GetFullPath(fileName);

            ShareCollection shi = LocalShares;
            if (null == shi)
                return null;
            else
                return shi[fileName];
        }
#endif

        #endregion

        #region Local shares

        /// <summary>The local shares</summary>
        private static ShareCollection _local = null;

        /// <summary>
        /// Return the local shares
        /// </summary>
        public static ShareCollection LocalShares
        {
            get
            {
                if (null == _local)
                    _local = new ShareCollection();

                return _local;
            }
        }

        /// <summary>
        /// Return the shares for a specified machine
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static ShareCollection GetShares(string server)
        {
            return new ShareCollection(server);
        }

        #endregion

        #region Private Data

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ShareCollection));

        /// <summary>The name of the server this collection represents</summary>
        private string _server;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor - local machine
        /// </summary>
        public ShareCollection()
        {
            _server = string.Empty;
            EnumerateShares(_server, this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Server"></param>
        public ShareCollection(string server)
        {
            _server = server;
            EnumerateShares(_server, this);
        }

        #endregion

        #region Add

        protected void Add(Share share)
        {
            InnerList.Add(share);
        }

        protected void Add(string netName, string path, ShareType shareType, string remark)
        {
            InnerList.Add(new Share(_server, netName, path, shareType, remark));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the name of the server this collection represents
        /// </summary>
        public string Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Returns the <see cref="Share"/> at the specified index.
        /// </summary>
        public Share this[int index]
        {
            get { return (Share)InnerList[index]; }
        }

        /// <summary>
        /// Returns the <see cref="Share"/> which matches a given local path
        /// </summary>
        /// <param name="path">The path to match</param>
        public Share this[string path]
        {
            get
            {
                // the path is canonicalized by reference
                if (!FileUtility.IsMappedOrLocal(ref path))
                    return null;

                Share match = null;

                for (int i = 0; i < InnerList.Count; i++)
                {
                    Share s = (Share)InnerList[i];

                    if (s.IsFileSystem && s.MatchesPath(path))
                    {
                        //Store first match
                        if (null == match)
                            match = s;

                        // If this has a longer path,
                        // and this is a disk share or match is a special share, 
                        // then this is a better match
                        else if (match.Path.Length < s.Path.Length)
                        {
                            if (ShareType.Disk == s.ShareType || ShareType.Disk != match.ShareType)
                                match = s;
                        }
                    }
                }

                return match;
            }
        }

        #endregion

        #region Implementation of ICollection

        /// <summary>
        /// Copy this collection to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Share[] array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        #endregion
    }

    #endregion
}
