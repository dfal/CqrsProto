using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Infrastructure.Azure.Documents
{
	public class DocumentStore
	{
		readonly string tenant;
		readonly CloudTableClient tableClient;

		public DocumentStore(string tenant, string connectionString)
			: this(tenant, CloudStorageAccount.Parse(connectionString))
		{
		}

		public DocumentStore(string tenant, CloudStorageAccount account)
		{
			this.tenant = tenant;
			this.tableClient = account.CreateCloudTableClient();
		}

		public T Find<T>(string key) where T : class, ITableEntity, new()
		{
			var table = tableClient.GetTableReference(typeof (T).Name);
			if (!table.Exists()) return null;

			var retrieveOperation = TableOperation.Retrieve<T>(tenant, key);

			return table.Execute(retrieveOperation).Result as T;
		}

		public IEnumerable<T> GetAll<T>() where T : class, ITableEntity, new()
		{
			var table = tableClient.GetTableReference(typeof(T).Name);
			if (!table.Exists()) return Enumerable.Empty<T>();

			var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tenant));

			return table.ExecuteQuery(query);
		}

		public void InsertOrReplace<T>(T document) where T : class, ITableEntity, new()
		{
			Debug.Assert(document != null);

			var table = tableClient.GetTableReference(typeof (T).Name);

			document.PartitionKey = tenant;

			var insertOrReplace = TableOperation.InsertOrReplace(document);

			table.Execute(insertOrReplace);
		}
	}
}
