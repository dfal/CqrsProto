using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Infrastructure.Azure.EventSourcing
{
	public class EventStore : IEventStore
	{
		const string CommitsContainerSuffix = "-commits";
		const string HeadsContainerSuffix = "-heads";

		readonly ISerializer serializer;

		readonly CloudBlobContainer commitsContainer;
		readonly CloudBlobContainer headsContainer;

		readonly IEventBus eventBus;

		public EventStore(string tenant, string connectionString, ISerializer serializer, IEventBus eventBus)
			: this(tenant, CloudStorageAccount.Parse(connectionString), serializer, eventBus)
		{
		}

		public EventStore(string tenant, CloudStorageAccount account, ISerializer serializer, IEventBus eventBus)
		{
			this.serializer = serializer;
			
			var blobClient = account.CreateCloudBlobClient();
			
			this.commitsContainer = blobClient.GetContainerReference(tenant + CommitsContainerSuffix);
			this.commitsContainer.CreateIfNotExists();
			
			this.headsContainer = blobClient.GetContainerReference(tenant + HeadsContainerSuffix);
			this.headsContainer.CreateIfNotExists();

			this.eventBus = eventBus;
		}

		public void Save(Commit commit)
		{
			Debug.Assert(commit != null);
			Debug.Assert(commit.SourceId != Guid.Empty);
			Debug.Assert(commit.Id != Guid.Empty);
			Debug.Assert(commit.Changes != null);
			Debug.Assert(commit.Changes.Length > 0);

			var commitBlob = SaveCommit(commit);
			try
			{
				UpsertHead(commit);
			}
			catch (StorageException ex)
			{
				commitBlob.DeleteIfExists();
				
				if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
					throw new ConcurrencyException(commit.SourceId, commit.SourceType);

				throw;
			}
			catch
			{
				commitBlob.DeleteIfExists();
				throw;
			}

			foreach (var @event in commit.Changes)
			{
				eventBus.Publish(new Envelope<IEvent>(@event)
				{
					MessageId = Guid.NewGuid().ToString(),
					CorrelationId = commit.Metadata.ContainsKey("CorrelationId") ? commit.Metadata["CorrelationId"] : string.Empty
				});
			}
		}

		public IEnumerable<Commit> Load(Guid sourceId, int minVersion)
		{
			Debug.Assert(sourceId != Guid.Empty);

			var commitId = GetHead(sourceId);

			var commits = new List<Commit>();
			while (commitId.HasValue)
			{
				var commit = GetCommit(commitId.Value);
				commits.Add(commit);
				commitId = commit.ParentId;
			}

			return commits;
		}

		Guid? GetHead(Guid sourceId)
		{
			var headBlob = headsContainer.GetBlockBlobReference(sourceId.ToString());
			
			if (!headBlob.Exists()) return null;

			using (var stream = new MemoryStream())
			{
				headBlob.DownloadToStream(stream);
				return new Guid(stream.ToArray());
			}
		}

		CloudBlockBlob SaveCommit(Commit commit)
		{
			var commitBlob = commitsContainer.GetBlockBlobReference(commit.Id.ToString());

			var content = serializer.Serialize(commit.Changes);

			commitBlob.UploadFromByteArray(content, 0, content.Length);

			if (commit.Metadata != null) foreach (var pair in commit.Metadata)
				commitBlob.Metadata[pair.Key] = pair.Value;

			commitBlob.Metadata["CommitId"] = commit.Id.ToString();

			if (commit.ParentId.HasValue)
				commitBlob.Metadata["ParentId"] = commit.ParentId.ToString();

			commitBlob.SetMetadata();

			return commitBlob;
		}

		Commit GetCommit(Guid commitId)
		{
			var commitBlob = commitsContainer.GetBlockBlobReference(commitId.ToString());
			
			byte[] content;
			using (var stream = new MemoryStream())
			{
				commitBlob.DownloadToStream(stream);
				content = stream.ToArray();
			}

			return new Commit
			{
				Id = new Guid(commitBlob.Metadata["CommitId"]),
				SourceId = new Guid(commitBlob.Metadata["SourceId"]),
				ParentId = commitBlob.Metadata.ContainsKey("ParentId") ? new Guid?(new Guid(commitBlob.Metadata["ParentId"])) : null,
				SourceType = commitBlob.Metadata["SourceType"],
				Changes = (IEvent[])serializer.Deserialize(content),
			};
		}

		void UpsertHead(Commit commit)
		{
			var headBlob = headsContainer.GetBlockBlobReference(commit.SourceId.ToString());

			var headBytes = commit.Id.ToByteArray();
	
			headBlob.UploadFromByteArray(headBytes, 0, headBytes.Length, commit.AccessCondition());
		}
	}
}
