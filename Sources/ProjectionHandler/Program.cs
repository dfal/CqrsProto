using Topshelf;

namespace ProjectionHandler
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service<ProjectionHandlerService>(s =>
				{
					s.ConstructUsing(name => new ProjectionHandlerService());
					s.WhenStarted(tc => tc.Start());
					s.WhenStopped(tc => tc.Stop());
				});

				x.RunAsLocalSystem();
				x.SetServiceName("Proto.ProjectionHandler");
			});
		}
	}
}
