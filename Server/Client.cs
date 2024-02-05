using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Common;

namespace Server
{
	internal class Client : ClientBase
	{
		readonly CancellationTokenSource cts;

		internal string Username { get; private set; } = string.Empty;
		internal bool Initialized
			=> !string.IsNullOrEmpty(Username);

		internal Action<Client, Payload.Message> NewMessage = delegate { };
		internal Action<Client> ClientInicialized = delegate { };
		internal Action<Client> ClientDisconnected = delegate { };
		internal Action<Client> HeartbeatReceived = delegate { };

		public Client(TcpClient connection)
			: base(connection)
		{
			this.NewMessageRecieved += OnNewMessageRecieved;
			cts = new CancellationTokenSource();
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
				case Payload.Type.Initial:
					HandleInitialPayload();
					break;
			}

			Payload.Type DeserializePayloadType()
			{
				//TODO: омагад каков костыль
				var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
				return Enum.Parse<Payload.Type>(payloadDict["PayloadType"].ToString());
			}
			void HandleInitialPayload()
			{
				var payload = JsonSerializer.Deserialize<Payload.Initial>(message);
				Username = payload.Username;
				ClientInicialized(this);
			}
			void HandleMessagePayload()
			{
				var payload = JsonSerializer.Deserialize<Payload.Message>(message);
				NewMessage(this, payload);
			}
			void HandleHeartbeatPayload()
			{
				var payload = JsonSerializer.Deserialize<Payload.Heartbeat>(message);

			}
		}

		internal async Task SendHeartbeat()
		{
			var payload = new Payload.Heartbeat();
			await SendPayload(payload);
		}
		internal async Task SendMessage(string message)
		{
			var payload = new Payload.Message(message);
			await SendPayload(payload);
		}
		
		internal async Task StartListeningNew()
		{
			Task.Run(Listen, cts.Token);

			async void Listen()
			{
				while (true)
				{
					var result = await this.ReceiveMessageNew();
					if (result.IsSuccessfull)
					{
						OnNewMessageRecieved(result.Message);
					}
					else if (result.Code is MessageReceivedResult.ErrorCode.Disconnected)
					{
						ClientDisconnected(this);
						break;
					}
					else
					{
						break;
					}
				}
				cts.Cancel();
			}
		}

		internal async Task StartListening()
		{
			Task.Run(Listen, cts.Token);

			async void Listen()
			{
				while (true)
				{
					try
					{
						await this.RecieveMessage();
					}
					catch (IOException ex)
						when (ex.InnerException is SocketException innerEx
							&& innerEx.SocketErrorCode is SocketError.ConnectionReset)
					{
						ClientDisconnected(this);
						break;
					}
					catch
					{
						// TODO: обработка других случаев.
						throw;
					}
				}
				cts.Cancel();
			}
		}
	}
}
