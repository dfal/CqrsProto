using System;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace Proto.Domain
{
	public class Customer : EventSourced
	{
		bool deleted;
		string name;

		public Customer(Guid id)
			: base(id)
		{
			Handles<CustomerCreated>(OnCustomerCreated);
			Handles<CustomerDeleted>(OnCustomerDeleted);
			Handles<CustomerRenamed>(OnCustomerRenamed);
		}

		public Customer(string name, string vatNumber, string email)
			: this(Guid.NewGuid())
		{
			Apply(new CustomerCreated
			{
				Name = name,
				VatNumber = vatNumber,
				Email = email
			});
		}

		public void Delete()
		{
			Apply(new CustomerDeleted());
		}

		public void Rename(string newName)
		{
			if (deleted)
				throw new InvalidOperationException("Customer already deleted.");

			if (name != newName) Apply(new CustomerRenamed
			{
				OldName = name,
				NewName = newName
			});

		}

		void OnCustomerCreated(CustomerCreated @event)
		{
			name = @event.Name;
			deleted = false;
		}

		void OnCustomerRenamed(CustomerRenamed @event)
		{
			name = @event.NewName;
		}

		void OnCustomerDeleted(CustomerDeleted @event)
		{
			deleted = true;
		}
	}

	public class CustomerCreated : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string Name { get; set; }
		public string VatNumber { get; set; }
		public string Email { get; set; }
	}

	public class CustomerDeleted : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }
	}

	public class CustomerRenamed : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string NewName { get; set; }
		public string OldName { get; set; }
	}
}
