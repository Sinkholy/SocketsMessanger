using System.Configuration;
using System.Net;
using System.Net.Sockets;

using Client;
using Payload;
using System.Text.Json;

internal class Program
{
	private static async Task Main(string[] args)
	{
		var client = new ClientNew(new TcpClient());
		client.NewMessage += OnNewMessageReceived;

		Console.WriteLine("Ожидание сервера...");
		await client.Connect(GetIpAddress(), GetPort());
		Console.WriteLine("Соединение с сервером установлено.");
		client.StartListening();
		Console.WriteLine("Введите ваше имя: ");
		await client.SendInitialMessage(Console.ReadLine());

		Console.WriteLine("Введите сообщение: ");
		while (true)
		{
			var message = Console.ReadLine();
			await client.SendMessage(message);
		}

		void OnNewMessageReceived(Payload.Message payload)
		{
			Console.WriteLine(payload.Content);
		}
	}

	static IPAddress GetIpAddress()
	{
		return IPAddress.Parse(ConfigurationManager.AppSettings["ipAddress"]);
	}
	static int GetPort()
	{
		return int.Parse(ConfigurationManager.AppSettings["port"]);
	}
}