using System;
using System.Threading;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Allows client to enable arbitrary method to be retried some number of times
	/// if an exception is encountered, with a random timeout in-between.
	/// </summary>
	/// <remarks>
	/// An instance of this class can be used to call an instance method and catch
	/// any exceptions. The exception is then examined to determine whether it indicates
	/// that the method can/should be retried. If so, a random amount of time is allowed
	/// to expire (between configurable limits) and then the method is called again,
	/// for up to a maximum number of times.
	/// </remarks>
	public class RetryableMethod
	{
		#region State, construction and disposal

		protected int _retryCount = 0;
		protected int _maxRetries = 0;
		protected int _maxRetryDelay = 0;
		protected object _callSite;
		protected Delegate _delegate;
		protected object[] _args;
		protected IsExceptionRetryable_Delegate _isRetryableException;

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RetryableMethod));

		/// <summary>
		/// Defines the method signature for the method that will be called to determine whether an exception
		/// that has been caught can/should be retried. Caller supplies such a method.
		/// </summary>
		public delegate bool IsExceptionRetryable_Delegate(Exception ex);

		/// <summary>
		/// Constructs an instance that can be used
		/// </summary>
		/// <param name="callSite">The instance upon which the method is to be called</param>
		/// <param name="del">Delegate representing the method to call</param>
		/// <param name="args">Arguments to pass to method</param>
		/// <param name="maxRetries">Maximum number of retries to attempt</param>
		/// <param name="maxRetryDelay">Maximum time, in millis, to wait before a retry is attempted. 
		/// Actual value will be a random number of millis up to this number</param>
		/// <param name="isRetryableException">Method for this instance to call when an exception is
		/// encountered, to determine whether the method should be retried in response.</param>
		public RetryableMethod(object callSite, Delegate del, object[] args, int maxRetries,
			int maxRetryDelay, IsExceptionRetryable_Delegate isRetryableException)	
		{
			this._callSite = callSite;
			this._delegate = del;
			this._args = args;
			this._maxRetries = maxRetries;
			this._maxRetryDelay = maxRetryDelay;	// max millis between retries
			this._isRetryableException = isRetryableException;
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Call here actually starts the process of calling the method and retrying if necessary
		/// </summary>
		/// <returns>Whatever the method called returns</returns>
		public object Execute()
		{
			while (true) 
			{
				try 
				{	
					return this._delegate.Method.Invoke(this._callSite, this._args);
				} 
				catch (Exception ex) 
				{
					// The exception here will always be a "targetinvocationexception"
					// (because of the invoke above) The real exception we want to look
					// for is the inner exception
					if (this._isRetryableException(ex.InnerException))
					{
						if (this._maxRetries > this._retryCount) 
						{
							log.Debug(String.Format("Retryable method {0} caught retryable exception",
								this._delegate.Method.Name), ex.InnerException);
							this._retryCount++;
							Random rnd = new Random(this._retryCount);
							int delay = rnd.Next(this._maxRetryDelay);
							AutoResetEvent poorMansTimeout = new AutoResetEvent(false);
							poorMansTimeout.WaitOne(delay, false);
							log.Debug(String.Format("Retryable method {0} being retried",
								this._delegate.Method.Name));
						}
						else 
						{
							ApplicationException wrappedException =
								new ApplicationException("Max retries exceeded", ex.InnerException);
							throw wrappedException;
						}
					}
					else 
					{
						// Un-retryable exception encountered; just throw
						throw ex.InnerException;
					}
				}
			}
		}

		#endregion
	}
}
