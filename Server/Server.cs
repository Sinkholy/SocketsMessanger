using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Server;
using System.Net.Mail;
using System.Text.Json.Serialization;
using System.Text.Json;
using Payload;

namespace Sender
{
	internal class Server : IDisposable
	{
		readonly ConnectionsListener connectionsListener;
		readonly List<Client> clients;

		internal Server(IPEndPoint ip) 
		{
			connectionsListener = new ConnectionsListener(ip);
			clients = new List<Client>();

			connectionsListener.NewConnection += OnNewConnection;
		}

		internal async Task Start()
		{
			connectionsListener.Start();
		}
		async void OnNewConnection(TcpClient connection)
		{
			var client = new Client(connection);
			client.ClientDisconnected += OnClientDisconnected;
			client.NewMessage += OnNewMessageRecieved;
			client.ClientInicialized += OnClientInicialized;

			client.StartListening();
			clients.Add(client);

			Console.WriteLine($"Клиент подключился, всего клиентов: {clients.Count}");
		}
		void OnClientInicialized(Client client)
		{
			Console.WriteLine($"Клиент {client.Username} инициализирован.");
			Broadcast(client, $"{client.Username} присоединился.");
		}
		void OnNewMessageRecieved(Client client, Payload.Message message)
		{
			Console.WriteLine("Новый запрос на отправу сообщения.");
			Broadcast(client, $"{client.Username} : {message.Content}");
		}
		void OnClientDisconnected(Client client)
		{
			clients.Remove(client);
			client.Dispose();
			Broadcast(client, $"{client.Username} отключился.");
			Console.WriteLine($"Клиент отключился, всего клиентов: {clients.Count}");
		}
		async void Broadcast(Client client, string message)
		{
			foreach (var _client in clients.Where(x => x.Initialized 
												&& x.Username != client.Username))
			{
				await _client.SendMessage(message);
			}
		}

		public void Dispose()
		{
			connectionsListener.Dispose();
			foreach (var client in clients)
			{
				client.Dispose();
			}
		}

		class ConnectionsListener : IDisposable
		{
			readonly TcpListener listener;
			readonly CancellationTokenSource cts;

			internal Action<TcpClient> NewConnection = delegate { };

			internal ConnectionsListener(IPEndPoint ip)
			{
				listener = new TcpListener(ip);
				cts = new CancellationTokenSource();
			}

			public void Dispose()
			{
				cts.Cancel();
				listener.Stop();
			}

			internal void Start()
			{
				Task.Run(Listen, cts.Token);
				
				async void Listen() // TODO: переименовать
				{
					try
					{
						listener.Start();

						while (true)
						{						
							NewConnection(await listener.AcceptTcpClientAsync());
						}
					}
					finally
					{
						listener.Stop();
					}
				}
			}
		}
		class Heart : IDisposable
		{
			readonly TimeSpan heartbeatRate;
			readonly Server server;
			readonly CancellationTokenSource cts;

			internal Heart(Server server, TimeSpan heartbeatRate)
			{
				cts = new CancellationTokenSource();
				this.server = server;
				this.heartbeatRate = heartbeatRate;
			}

			internal Action HeartbeatSent = delegate { };

			internal void Start()
			{
				Task.Run(Run, cts.Token);

				async void Run() // TODO: нейминг
				{
					foreach(var client in server.clients.Where(client => client.Initialized))
					{
						await client.SendHeartbeat();
						HeartbeatSent();
					}

					await Task.Delay(heartbeatRate);
				}
			}

			public void Dispose()
			{
				cts.Cancel();
			}
		}
	}
}
