using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common
{
	public static class Serializer
	{
		public static byte[] Serialize<T>(T toSerialize)
		{
			return JsonSerializer.SerializeToUtf8Bytes(toSerialize);
		} 
		public static T? Deserialize<T>(byte[] toDeserialize)
		{
			return JsonSerializer.Deserialize<T>(toDeserialize);
		}
	}
}
