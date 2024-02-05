using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Payload;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common;

namespace Client
{
	internal class ClientNew : ClientBase
	{
		CancellationTokenSource cts;

		internal Action<Payload.Message> NewMessage = delegate { };

		public ClientNew(TcpClient connection) : base(connection)
		{
			cts = new CancellationTokenSource();

			this.NewMessageRecieved += OnNewMessageRecieved;
		}

		internal async Task ConnectNew(IPAddress ip, int port)
		{
			IPEndPoint ipEndPoint = new(ip, port);

			while (true)
			{
				try
				{
					await Connection.ConnectAsync(ipEndPoint);
					// Подключение произведено успешно, выходим из цикла.
					break;
				}
				catch (SocketException socketEx)
					when (socketEx.SocketErrorCode is SocketError.ConnectionRefused)
				{
					// Предполагается, что ConnectionRefused будет получено
					// при невозможности подключиться к удаленному серверу.
					await Task.Delay(1000); // TODO: в конфиг.
					continue;
				}
				catch
				{
					// TODO: обработка других сценариев. (нужна ли?)
					throw;
				}
			}



			Task.Run(Listen, cts.Token);

			async void Listen()
			{
				while (true)
				{
					var result = await this.ReceiveMessageNew();
					if (result.IsSuccessfull is false)
					{
						break;
					}
				}
				cts.Cancel();
			}
		}

		internal async Task Connect(IPAddress ip, int port)
		{
			IPEndPoint ipEndPoint = new(ip, port);

			while (true)
			{
				try
				{
					await Connection.ConnectAsync(ipEndPoint);
					// Подключение произведено успешно, выходим из цикла.
					break;
				}
				catch (SocketException socketEx)
					when (socketEx.SocketErrorCode is SocketError.ConnectionRefused)
				{
					// Предполагается, что ConnectionRefused будет получено
					// при невозможности подключиться к удаленному серверу.
					await Task.Delay(1000); // TODO: в конфиг.
					continue;
				}
				catch
				{
					// TODO: обработка других сценариев. (нужна ли?)
					throw;
				}
			}
		}
		internal async Task StartListening()
		{
			Task.Run(Listen, cts.Token);

			async void Listen()
			{
				while (true)
				{
					var result = await this.ReceiveMessageNew();
					if (result.IsSuccessfull)
					{
						OnNewMessageRecieved(result.Message!);
					}
					else
					{
						Console.WriteLine("Сервер не отвечает.");
						break;
					}
				}
				cts.Cancel();
			}
		}

		void OnNewMessageRecieved(byte[] message)
		{
			var payloadType = DeserializePayloadType();
			switch (payloadType)
			{
				case Payload.Type.Message:
					HandleMessagePayload();
					break;
				case Payload.Type.Heartbeat:
					break;
			}

			Payload.Type DeserializePayloadType()
			{
				//TODO: омагад каков костыль
				var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
				return Enum.Parse<Payload.Type>(payloadDict["PayloadType"].ToString());
			}
			void HandleMessagePayload()
			{
				var payload = JsonSerializer.Deserialize<Payload.Message>(message);
				NewMessage(payload);
			}
			void HandleHeartbeatPayload()
			{
				var payload = JsonSerializer.Deserialize<Payload.Heartbeat>(message);

			}
		}
		internal async Task SendInitialMessage(string username)
		{
			var payload = new Initial(username);
			await SendPayload(payload);
		}
		internal async Task SendMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			var payload = new Message(message);
			await SendPayload(payload);
		}
	}
}