using System;
using System.Reflection;
using System.IO;

using log4net;
using log4net.Repository;
using log4net.Appender;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Support methods for log4net
	/// </summary>
	public class Log4NetSupport
	{
		/// <summary>
		/// Help in simulating a "Dump" level,
		/// one level below "Debug"
		/// </summary>
		private static bool DumpLevel = false;

		/// <summary>
		/// Static class
		/// </summary>
		private Log4NetSupport()
		{
		}

		/// <summary>
		/// Initialize the logging system, handling some platform dependencies.
		/// This method is suitable for use in Windows clients but not Services.
		/// </summary>
		public static void InitializeLogging()
		{
			log4net.Config.XmlConfigurator.Configure(new FileInfo(ServeronConfiguration.ApplicationConfiguration.FilePath));
			/// Sam Dahan 2/2009: the log is now in the common application data folder, under a Serveron folder.
			/// For that to work under all operating systems we have to do some shenanigans
			/// and change the path from the configuration file.
			ILoggerRepository []rep = LogManager.GetAllRepositories();
			IAppender [] appenders = rep[0].GetAppenders();
			Assembly assy = Assembly.GetEntryAssembly();
			string logName = assy == null ?
				"Serveron Unit Tests.log" : Path.GetFileNameWithoutExtension(assy.Location) + ".log";
			foreach (IAppender appender in appenders)
			{
				if (appender is RollingFileAppender)
				{
					RollingFileAppender logger = (RollingFileAppender)appender;
					if (Path.GetFileName(logger.File).ToLower().Contains("client"))
					{
						if (LogFile == null)
						{
							LogFile = logger.File;
							logger.File = FileUtility.LocateConfigFile(logName);
						}
					}
					else
					{
						logger.File = FileUtility.LocateConfigFile(Path.GetFileName(logger.File));
					}
					logger.ActivateOptions();
				}
			}
		}

		/// <summary>
		/// AppDomain-wide property indicating whether dump-
		/// level debugging is enabled.
		/// </summary>
		public static bool Dump
		{
			get
			{
				return DumpLevel;
			}
			set
			{
				DumpLevel = value;
			}
		}

		static public string LogFile { get; private set; }
	}
}
