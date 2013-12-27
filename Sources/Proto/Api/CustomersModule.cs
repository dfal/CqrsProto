using System;
using Infrastructure.Azure.Documents;
using Infrastructure.Messaging;
using Nancy;
using Nancy.ModelBinding;
using ProjectionHandler.Documents;
using Proto.Domain;

namespace Proto.Api
{
	public class CustomersModule : NancyModule
	{
		public CustomersModule(ICommandBus commandBus, DocumentStore documentStore)
			: base("/api/customers")
		{
			Get["/"] = _ =>
			{
				return documentStore.GetAll<CustomerDocument>();
			};

			Get["/customer/{id:guid}"] = parameters =>
			{
				var customer = documentStore.Find<CustomerDocument>(parameters.id);

				return customer;
				//return string.Format("Customer '{0}' from projection store", parameters.id);
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