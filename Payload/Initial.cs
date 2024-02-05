using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payload
{
	public class Initial : PayloadBase
	{
		public string Username { get; private set; }

		public Initial(string username) 
			: base(Type.Initial) 
		{
			Username = username;
		}
	}
}
