using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Serveron.Utility.Core
{
	[Serializable]
	public class NetSerial : IDeserializationCallback, IDisposable
	{
		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
