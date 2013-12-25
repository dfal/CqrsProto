using Topshelf;

namespace CommandHandler
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service<Service>(s =>
				{
					s.ConstructUsing(name => new Service());
					s.WhenStarted(tc => tc.Start());
					s.WhenStopped(tc => tc.Stop());
				});

				x.RunAsLocalSystem();
				x.SetServiceName("Proto.CommandHandler");
			});
		}
	}
}
