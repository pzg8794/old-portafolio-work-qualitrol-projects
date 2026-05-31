using System;
using System.Collections;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Base interface of all Proxies.  This interface
	/// allows methods returning proxies to be declared
	/// with a return type more specific than "object".
	/// The actual return type is
	/// an implementation of IAdminProxy, IDataProxy, etc.
	/// </summary>
	public interface IProxy
	{
		// No methods
	}

	#region AsyncStateHolder class

	/// <summary>
	/// An instance of AsyncStateHolder becomes the AsyncState argument
	/// passed to the begin call and passed back to the callback.
	/// </summary>
	public class AsyncStateHolder
	{
		private Guid _id;
		private IDictionary _pendingRequests;
		private TimeSpan _untilNextRequest;
		IProxy _proxy;
		private object _state;

		/// <summary>
		/// Our identity, also caller's completion handle
		/// </summary>
		public Guid ID { get { return _id; } }

		/// <summary>
		/// Caller's dictionary into which we store our handle at request time
		/// </summary>
		public IDictionary PendingRequests { get { return _pendingRequests; } }

		/// <summary>
		/// Time between requests on this stream
		/// </summary>
		public TimeSpan UntilNextRequest { get { return _untilNextRequest; } }

		/// <summary>
		/// Proxy we made the request upon
		/// </summary>
		public IProxy Proxy { get { return _proxy; } }

		/// <summary>
		/// Additional state.  Might be used, for example, to
		/// hold a retry count.
		/// </summary>
		public object State { get { return _state; } }

		/// <summary>
		/// Derived property: return the completion type for this asynchronous
		/// request, or null if it's not in the table.
		/// </summary>
		public string CompletionType { get { return (string)_pendingRequests[_id]; } }

		/// <summary>
		/// Construct an AsyncStateHolder without extra state
		/// </summary>
		/// <param name="id">identity</param>
		/// <param name="pendingRequests">caller's dictionary</param>
		/// <param name="untilNextRequest">inter-call delay</param>
		/// <param name="proxy">web proxy on which request was made</param>
		public AsyncStateHolder(Guid id, IDictionary pendingRequests, TimeSpan untilNextRequest, IProxy proxy) :
			this(id, pendingRequests, untilNextRequest, proxy, null)
		{
		}

		/// <summary>
		/// Construct an AsyncStateHolder with an optional state object
		/// </summary>
		/// <param name="id">identity</param>
		/// <param name="pendingRequests">caller's dictionary</param>
		/// <param name="untilNextRequest">inter-call delay</param>
		/// <param name="proxy">web proxy on which request was made</param>
		/// <param name="state">uncommitted state, e.g. retry count</param>
		public AsyncStateHolder(Guid id, IDictionary pendingRequests, TimeSpan untilNextRequest, IProxy proxy, object state)
		{
			_id = id;
			_pendingRequests = pendingRequests;
			_untilNextRequest = untilNextRequest;
			_proxy = proxy;
			_state = state;
		}
	}

	#endregion

}