using System;
using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Nancy;

namespace Proto
{
	public class DefaultModule : NancyModule
	{
		public DefaultModule()
		{
			Get["/"] = parameters => "Hello World";
		}
	}
}
