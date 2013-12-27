using System;
using System.IO;
using System.Text;
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
		public CustomersModule(ICommandBus commandBus, DocumentStore documentStore, Infrastructure.Serialization.ISerializer serializer)
			: base("/api/customers")
		{
			Get["/"] = _ =>
			{
				var customers = documentStore.GetAll<CustomerDocument>();
				return Encoding.UTF8.GetString(serializer.Serialize(customers));
			};

			Get["/customer/{id:guid}"] = parameters =>
			{
				var customer = documentStore.Find<CustomerDocument>(parameters.id);

				return Encoding.UTF8.GetString(serializer.Serialize(customer));
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