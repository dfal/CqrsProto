using System;
using Infrastructure.Azure.Documents;
using ProjectionHandler.Documents;
using Proto.Domain;


namespace ProjectionHandler.EventHandling
{
	class CustomerEventHandler : 
		IEventHandler<CustomerCreated>,
		IEventHandler<CustomerRenamed>,
		IEventHandler<CustomerDeleted>
	{
		readonly Func<DocumentStore> documentStoreFactory;

		public CustomerEventHandler(Func<DocumentStore> documentStoreFactory)
		{
			this.documentStoreFactory = documentStoreFactory;
		}

		public void Handle(CustomerCreated @event)
		{
			documentStoreFactory().InsertOrReplace(new CustomerDocument
			{
				RowKey = @event.SourceId.ToString(),
				Name = @event.Name,
				VatNumber = @event.VatNumber,
				Email = @event.Email
			});
		}

		public void Handle(CustomerRenamed @event)
		{
			var documentStore = documentStoreFactory();
			var customer = documentStore.Get<CustomerDocument>(@event.SourceId.ToString());
			
			customer.Name = @event.NewName;
			
			documentStoreFactory().InsertOrReplace(customer);
		}

		public void Handle(CustomerDeleted @event)
		{
			documentStoreFactory().Delete<CustomerDocument>(@event.SourceId.ToString());
		}
	}
}