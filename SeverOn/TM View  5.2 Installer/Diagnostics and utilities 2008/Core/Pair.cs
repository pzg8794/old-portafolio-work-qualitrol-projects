/// Serveron's Serveron.Utility.Core.Pair C# code
/// Copyright (c) 2005 Serveron Corporation. All rights reserved.
using System;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// contains a pair of objects
	/// </summary>
	/// <remarks>same as <c>System.Web.UI.Pair</c> class without the
	/// undesirable System.Web.UI namespace</remarks>
	public class Pair
	{
		/// <summary>
		/// construct an empty Pair class instance
		/// </summary>
		public Pair( )
			: this( null, null )
		{
		}

		/// <summary>
		/// construct a Pair class instance containing specified
		/// first and second objects
		/// </summary>
		/// <param name="first">first object</param>
		/// <param name="second">second object</param>
		public Pair( object first, object second )
		{
			First = first;
			Second = second;
		}

		/// <summary>
		/// first object contained in this Pair
		/// </summary>
		public object First;

		/// <summary>
		/// second object contained in this Pair
		/// </summary>
		public object Second;
	}
}
