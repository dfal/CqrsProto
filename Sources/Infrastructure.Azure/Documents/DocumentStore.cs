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
			var table = tableClient.GetTableReference(TableName<T>());
			if (!table.Exists()) return null;

			var retrieveOperation = TableOperation.Retrieve<T>(tenant, key);

			return table.Execute(retrieveOperation).Result as T;
		}

		public T Get<T>(string key) where T : class, ITableEntity, new()
		{
			var entity = Find<T>(key);

			if (entity == null)
				throw new EntityNotFoundException(key, typeof (T).Name);

			return entity;
		}

		public IEnumerable<T> GetAll<T>() where T : class, ITableEntity, new()
		{
			var table = tableClient.GetTableReference(TableName<T>());
			if (!table.Exists()) return Enumerable.Empty<T>();

			var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tenant));

			return table.ExecuteQuery(query);
		}

		public void InsertOrReplace<T>(T document) where T : class, ITableEntity, new()
		{
			Debug.Assert(document != null);

			var table = tableClient.GetTableReference(TableName<T>());

			table.CreateIfNotExists();

			document.PartitionKey = tenant;

			var insertOrReplace = TableOperation.InsertOrReplace(document);

			table.Execute(insertOrReplace);
		}

		public void Delete<T>(string key) where T : class, ITableEntity, new()
		{
			Debug.Assert(!string.IsNullOrEmpty(key));

			var table = tableClient.GetTableReference(TableName<T>());
			
			if (!table.Exists()) return;

			var retrieve = TableOperation.Retrieve<T>(tenant, key);

			var deleteEntity = table.Execute(retrieve).Result as T;


			if (deleteEntity == null)
				return;

			var delete = TableOperation.Delete(deleteEntity);

			table.Execute(delete);
		}

		static string TableName<T>()
		{
			return typeof(T).Name.Replace(".", "").Replace("_", "");
		}
	}
}
