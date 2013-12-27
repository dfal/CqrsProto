using System;
using Infrastructure.Messaging;
using Nancy;
using Nancy.ModelBinding;
using Proto.Domain;

namespace Proto.Api
{
	public class CustomersModule : NancyModule
	{
		public CustomersModule(ICommandBus commandBus)
			: base("/api/customers")
		{
			Get["/"] = _ =>
			{
				return "Customer list from projection store";
			};

			Get["/customer/{id:guid}"] = _ =>
			{
				return string.Format("Customer '{0}' from projection store", _.id);
			};

			Post["/new"] = _ =>
			{
				var command = this.Bind<CreateCustomer>();
				command.Id = Guid.NewGuid();

				commandBus.Send(command);

				return HttpStatusCode.OK;
			};
		}
	}
}