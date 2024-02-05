using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payload
{
	public class PayloadBase
	{
		public Type PayloadType { get; protected set; }

		public PayloadBase(Type payloadType)
		{
			PayloadType = payloadType;
		}
	}

	public enum Type : byte
	{
		Message,
		Heartbeat,
		Initial
	}

	public class NewPayload<T>
	{
		public Type Type { get; set; }
		public T Data { get; set; }
	}
}
