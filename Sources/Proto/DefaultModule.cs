using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;

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
