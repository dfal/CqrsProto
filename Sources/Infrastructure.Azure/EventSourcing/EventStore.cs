using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Infrastructure.Azure.EventSourcing
{
	public class EventStore : IEventStore
	{
		const string commitsContainerSuffix = "-commits";
		const string headsContainerSuffix = "-heads";

		readonly ISerializer serializer;
		readonly CloudBlobClient blobClient;

		readonly CloudBlobContainer commitsContainer;
		readonly CloudBlobContainer headsContainer;


		public EventStore(string tenant, string connectionString, ISerializer serializer)
			: this(tenant, CloudStorageAccount.Parse(connectionString), serializer)
		{
		}

		public EventStore(string tenant, CloudStorageAccount account, ISerializer serializer)
		{
			this.serializer = serializer;
			this.blobClient = account.CreateCloudBlobClient();
			
			this.commitsContainer = blobClient.GetContainerReference(tenant + commitsContainerSuffix);
			this.commitsContainer.CreateIfNotExists();
			
			this.headsContainer = blobClient.GetContainerReference(tenant + headsContainerSuffix);
			this.headsContainer.CreateIfNotExists();
		}
		
		public IEnumerable<IEvent> Load(Guid sourceId, int minVersion)
		{
			throw new NotImplementedException(); 

		}

		public async void SaveAsync(Guid sourceId, IEnumerable<IEvent> events, IDictionary<string, string> metadata = null)
		{
			Debug.Assert(sourceId != Guid.Empty);
			Debug.Assert(events != null);

			var commitId = Guid.NewGuid();
			var commitBlob = commitsContainer.GetBlockBlobReference(commitId.ToString());

			var content = serializer.Serialize(events);
	
			await commitBlob.UploadFromByteArrayAsync(content, 0, content.Length);
			
			if (metadata != null) foreach (var pair in metadata)
					commitBlob.Metadata[pair.Key] = pair.Value;

			commitBlob.Metadata["CommitId"] = commitId.ToString();

			var headBlob = headsContainer.GetBlockBlobReference(sourceId.ToString());
		}
	}
}
