using System;
using System.Threading;
using Infrastructure.Azure.Messaging;
using Infrastructure.Serialization;
using Microsoft.Owin.Hosting;
using Proto.Domain;

namespace Proto
{
	class Program
	{
		static void Main(string[] args)
		{
/*
			var settings = new ServiceBusSettings();
			new ServiceBusConfig(settings).Initialize();

			var sender = new TopicSender(settings, "proto/commands");
			var bus = new CommandBus(sender, new DummyMetadataProvider(), new JsonSerializer());
			Console.WriteLine("Press key to send command");

			var name = Console.ReadLine();
			while (!string.IsNullOrEmpty(name))
			{
				bus.Send(new CreateCustomer
				{
					CustomerName = name,
					CustomerEmail = name + "@gmail.com",
					CustomerVatNumber = "123456"
				});

				name = Console.ReadLine();
			}
 */

			var stop = new ManualResetEvent(false);

			Console.CancelKeyPress += (sender, e) =>
			{
				Console.WriteLine("^C");
				stop.Set();

				e.Cancel = true;
			};

			const string url = "http://localhost:8880";
			
			using (WebApp.Start<Startup>(url))
			{
				Console.WriteLine("Listening on {0}; press Ctrl+C to quit.", url);
				stop.WaitOne();
			}
		}
	}
}
