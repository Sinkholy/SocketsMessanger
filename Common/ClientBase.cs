using System.IO;
using System.Net.Sockets;
using System.Text.Json;

using Payload;

namespace Common
{
	public abstract class ClientBase : IDisposable
	{
		protected TcpClient Connection { get; private set; }

		protected Action<byte[]> NewMessageRecieved = delegate { };

		protected ClientBase(TcpClient connection)
		{
			Connection = connection;
		}

		protected async Task<MessageReceivedResult> ReceiveMessageNew()
		{
			try
			{
				var stream = Connection.GetStream();
				var dataLength = await GetDataLength(stream);
				byte[] recievedData = await GetData(stream, dataLength);

				return MessageReceivedResult.Successfull(recievedData);
			}
			catch (IOException ex)
				when (ex.InnerException is SocketException innerEx
					&& innerEx.SocketErrorCode is SocketError.ConnectionReset)
			{
				return MessageReceivedResult.Failed(MessageReceivedResult.ErrorCode.Disconnected);
			}
			catch
			{
				// TODO: обработка других случаев.
				throw;
			}

			async Task<int> GetDataLength(Stream stream)
			{
				var buffer = new byte[sizeof(int)];
				var received = 0;

				while (received < sizeof(int))
				{
					received += await stream.ReadAsync(buffer);
				}

				return BitConverter.ToInt32(buffer);
			}
			async Task<byte[]> GetData(Stream stream, int dataLength)
			{
				var buffer = new byte[dataLength];
				var received = 0;

				while (received < dataLength)
				{
					received += await stream.ReadAsync(buffer);
				}

				return buffer;
			}
		}
		protected async Task RecieveMessage()
		{
			var stream = Connection.GetStream();
			var dataLength = await GetDataLength();
			byte[] recievedData = await GetData();
			NewMessageRecieved(recievedData);

			async Task<int> GetDataLength()
			{
				var buffer = new byte[sizeof(int)];
				var received = 0;

				while (received < sizeof(int))
				{
					received += await stream.ReadAsync(buffer);
				}

				return BitConverter.ToInt32(buffer);
			}
			async Task<byte[]> GetData()
			{
				var buffer = new byte[dataLength];
				var received = 0;

				while (received < dataLength)
				{
					received += await stream.ReadAsync(buffer);
				}

				return buffer;
			}
		}
		protected async Task SendPayload<PayloadType>(PayloadType payload)
			where PayloadType : PayloadBase
		{
			var serializedPayload = SerializePayload(payload);
			var preparedPayload = AppendMessageLengthPrefix(serializedPayload);
			await SendMessage(preparedPayload);

			byte[] SerializePayload(PayloadType payload)
			{
				// Мы сериализуем напрямую в байты
				// т.к. нет смысла сериализовывать в строки 
				// ибо передавать по сокету мы будем именно байты.
				return JsonSerializer.SerializeToUtf8Bytes(payload);
			}
		}
		byte[] AppendMessageLengthPrefix(byte[] message)
		{
			// https://blog.stephencleary.com/2009/04/message-framing.html

			var messageLength = BitConverter.GetBytes(message.Length);
			var preparedMessage = new byte[message.Length + messageLength.Length];

			messageLength.CopyTo(preparedMessage, 0);
			message.CopyTo(preparedMessage, messageLength.Length);

			return preparedMessage;
		}
		async Task SendMessage(byte[] message)
		{
			try
			{
				NetworkStream stream = Connection.GetStream();
				await stream.WriteAsync(message);
			}
			catch (Exception ex)
			{

			}
		}

		public void Dispose()
		{
			//TODO: оставить что-то одно?
			Connection.Close();
			Connection.Dispose();
		}
	}
	public struct MessageReceivedResult
	{
		public bool IsSuccessfull
			=> Code == ErrorCode.None;
		public byte[]? Message { get; init; }
		public ErrorCode Code { get; init; }

		public static MessageReceivedResult Successfull(byte[] message)
		{
			return new MessageReceivedResult
			{
				Message = message,
				Code = ErrorCode.None
			};
		}
		public static MessageReceivedResult Failed(ErrorCode errorCode)
		{
			return new MessageReceivedResult
			{
				Code = errorCode
			};
		}

		public enum ErrorCode : byte
		{
			None,
			Disconnected
		}
	}
}
