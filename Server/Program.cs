using System.Net;

internal class Program
{
	private static async Task Main(string[] args)
	{
		var server = new Sender.Server(new IPEndPoint(IPAddress.Parse("26.25.166.240"), 5005)); // TODO: app config
		await server.Start();

		Console.Read();
		server.Dispose();
	}
}