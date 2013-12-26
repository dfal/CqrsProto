using System;
using Proto.Domain;

namespace ProjectionHandler.Handling
{
	class CustomerEventHandler : IEventHandler<CustomerCreated>
	{
		public void Handle(CustomerCreated @event)
		{
			Console.WriteLine(@event.Name);
		}
	}
}