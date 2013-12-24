using Owin;

namespace Proto
{
	class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			/*app.UseHandlerAsync((req, res) =>
			{
				res.ContentType = "text/plain";
				return res.WriteAsync("Hello world!");
			});*/

			app.UseNancy();
		}
	}
}
