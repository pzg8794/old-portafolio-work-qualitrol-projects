using System;


namespace Serveron.Utility.Core
{
	/// <summary>
	/// The base context is known throughout the application.
	/// </summary>
	public interface IBaseContext
	{
		/// <summary>
		/// Get the active credentials
		/// </summary>
		Credentials Credentials{get;}

		/// <summary>
		/// Get the "properties", a hashtable into which we put all kinds
		/// of configuration stuff including all the appSettings.
		/// </summary>
		System.Collections.IDictionary ScratchPad { get; }
	}

	/// <summary>
	/// Global exposure of the base context as Frame.Context
	/// </summary>
	public class Frame
	{
		private static IBaseContext _context;
        private static bool _shutdown;
        private static object _sync = new object();

		/// <summary>
		/// Global exposure of the base context.
		/// </summary>
		public static IBaseContext Context
		{
			get
			{
                lock (_sync)
                {
                    if (_context == null)
                        throw new InvalidOperationException("Frame.Context: not available during initialization");
                    return _context;
                }
			}

			set
			{
                lock (_sync)
                {
                    if (_context != null)
                        throw new InvalidOperationException("Frame.Context: may only be set once");
                    _context = value;
                }
			}
		}

        /// <summary>
        /// The Shutdown flag is set to true early in shutdown
        /// Callers that may run asynchronously during the
        /// shutdown process (e.g. timed calledbacks) should
        /// check the flag before touching the Context property
        /// in order to avoid ObjectDisposedException on the
        /// object that implements the context.
        /// </summary>
        public static bool Shutdown
        {
            get
            {
                lock (_sync)
                {
                    return _shutdown;
                }
            }

            set
            {
                lock (_sync)
                {
                    if (!value)
                        throw new InvalidOperationException("Frame.Shutdown: may not be revoked");
                    _shutdown = true;
                }
            }
        }
	}
}
