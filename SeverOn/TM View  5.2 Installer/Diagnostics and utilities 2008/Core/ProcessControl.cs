// Copyright (c) Serveron Corporation 2005.  All Rights Reserved.

using System;
using System.Diagnostics;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Wrapper to simplify running subprocesses
	/// </summary>
	public class ProcessControl
	{
		private ProcessControl()
		{
		}

		/// <summary>
		/// Run a process synchronously with the current thread.
		/// </summary>
		/// <param name="path">exe path</param>
		/// <param name="args">command line args</param>
		/// <returns>exit status</returns>
		public static int RunProcessWait(string path, string args)
		{
			Process p = StartProcess(path, args, 0);
			return WaitProcess(p, TimeSpan.Zero);
		}

		/// <summary>
		/// Run a process synchronously with the current thread.
		/// </summary>
		/// <param name="path">exe path</param>
		/// <param name="args">command line args</param>
		/// <param name="timeout">maximum execution time for process.
		/// If 0 is passed, waits forever.  These are not the same
		/// semantics as the argument to Process.WaitForExit().</param>
		/// <returns>exit status</returns>
		public static int RunProcessWait(string path, string args, TimeSpan timeout)
		{
			Process p = StartProcess(path, args, 0);
			return WaitProcess(p, timeout);
		}

		/// <summary>
		/// Start an external tool, returning the Process instance.
		/// </summary>
		/// <param name="path">Exe path</param>
		/// <param name="args">command line argument string</param>
		public static Process StartProcess(string path, string args, int wait)
		{
			Process proc = new Process();
			ProcessStartInfo info = new ProcessStartInfo(path, args);
			info.UseShellExecute = true;
			info.RedirectStandardOutput = false;
			
			proc.StartInfo = info;
			proc.EnableRaisingEvents = false;

			proc.Start();
			if (wait != 0)
				System.Threading.Thread.Sleep(wait);
			return proc;
		}

		/// <summary>
		/// Block until process completes.
		/// </summary>
		/// <param name="proc">the proc</param>
		/// <param name="timeout">timeout</param>
		/// <returns>exit code</returns>
		public static int WaitProcess(Process proc, TimeSpan timeout)
		{
			if (timeout == TimeSpan.Zero)
				proc.WaitForExit();
			else
				proc.WaitForExit((int)timeout.TotalMilliseconds);

			return proc.ExitCode;
		}

		/// <summary>
		/// Start a document viewer, returning the Process instance.
		/// </summary>
		/// <param name="doc">Exe path</param>
		public static Process StartDocument(string doc)
		{
			Process proc = new Process();
			ProcessStartInfo info = new ProcessStartInfo(doc, "");
			info.UseShellExecute = true;
			info.RedirectStandardOutput = false;
			info.ErrorDialog = true;

			proc.StartInfo = info;
			proc.EnableRaisingEvents = false;

			proc.Start();
			return proc;
		}
	}
}
