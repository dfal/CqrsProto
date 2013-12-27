using System;
using Infrastructure.Messaging;

namespace Proto.Domain
{
	public class CreateCustomer : ICommand
	{
		public Guid Id { get; set; }
		
		public string Name { get; set; }
		public string VatNumber { get; set; }
		public string Email { get; set; }
	}

	public class RenameCustomer : ICommand
	{
		public Guid Id { get; set; }

		public Guid CustomerId { get; set; }
		public string NewName { get; set; }
	}

	public class DeleteCustomer : ICommand
	{
		public Guid Id { get; set; }

		public Guid CustomerId { get; set; }
	}
}
