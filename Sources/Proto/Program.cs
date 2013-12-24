using System;
using System.Threading;
using Microsoft.Owin.Hosting;

namespace Proto
{
	class Program
	{
		static void Main(string[] args)
		{
			var stop = new ManualResetEvent(false);

			Console.CancelKeyPress += (sender, e) =>
			{
				Console.WriteLine("^C");
				stop.Set();

				e.Cancel = true;
			};

			const string url = "http://localhost:8888";
			
			using (WebApp.Start<Startup>(url))
			{
				Console.WriteLine("Listening on {0}; press Ctrl+C to quit.", url);
				stop.WaitOne();
			}
		}
	}
}
