using System;
using Infrastructure.EventSourcing;
using Proto.Domain;

namespace CommandHandler.Handling
{
	class CustomerCommandHandler :
		ICommandHandler<CreateCustomer>,
		ICommandHandler<RenameCustomer>,
		ICommandHandler<DeleteCustomer>
	{
		readonly Func<IEventSourcedRepository<Customer>> repositoryFactory;

		public CustomerCommandHandler(Func<IEventSourcedRepository<Customer>> repositoryFactory)
		{
			this.repositoryFactory = repositoryFactory;
		}

		public void Handle(CreateCustomer command)
		{
			var customer = new Customer(command.CustomerName, command.CustomerVatNumber, command.CustomerEmail);

			repositoryFactory().Save(customer, command.Id.ToString());
		}

		public void Handle(RenameCustomer command)
		{
			var repo = repositoryFactory();

			var customer = repo.Get(command.CustomerId);

			customer.Rename(command.NewName);

			repo.Save(customer, command.Id.ToString());
		}

		public void Handle(DeleteCustomer command)
		{
			var repo = repositoryFactory();

			var customer = repo.Get(command.CustomerId);
			customer.Delete();

			repo.Save(customer, command.Id.ToString());
		}
	}
}