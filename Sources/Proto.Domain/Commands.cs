using System;
using Infrastructure.Messaging;

namespace Proto.Domain
{
	public class CreateCustomer : ICommand
	{
		public Guid Id { get; set; }
		
		public string CustomerName { get; set; }
		public string CustomerVatNumber { get; set; }
		public string CustomerEmail { get; set; }
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
