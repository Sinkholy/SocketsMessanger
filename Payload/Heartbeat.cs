using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payload
{
	public class Heartbeat : PayloadBase
	{
		public Heartbeat()
			: base(Type.Heartbeat) { }
	}
}
