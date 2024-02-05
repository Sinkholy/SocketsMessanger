using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payload
{
	public class Message : PayloadBase
	{
		public Message(string content) 
			: base(Type.Message)
		{
			Content = content;
		}

		public string Content { get; protected set; }
	}
}
