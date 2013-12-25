using System;
using System.IO;
using FluentAssertions;
using Infrastructure.Azure.EventSourcing;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.WindowsAzure.Storage;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Azure.IntegrationTests
{
	[TestFixture]
	public class EventStoreFixture
	{
		[Test]
		public void Should_store_first_commit_with_events()
		{
			const string tenant = "testTenant";
			const string correlationId = "testcorrelation";
			
			var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
			var serializer = new JsonSerializer();
			
			var eventStore = new EventStore(tenant, cloudStorageAccount, serializer, new Mock<IEventBus>().Object);

			var repository = new EventSourcedRepository<TestEventSourced>(eventStore);

			var source = new TestEventSourced(42, "forty-two");
			source.UpdateNumber(43);
			source.UpdateText("forty-three");
			repository.Save(source, correlationId);

			var blobClient = cloudStorageAccount.CreateCloudBlobClient();
			var headsContainer = blobClient.GetContainerReference(tenant.ToLower() + "-heads");
			
			var head = headsContainer.GetBlockBlobReference(source.Id.ToString());
			head.Exists().Should().BeTrue();

			Guid commitId;
			using (var stream = new MemoryStream())
			{
				head.DownloadToStream(stream);
				commitId = new Guid(stream.ToArray());
			}

			var commitsContainer = blobClient.GetContainerReference(tenant.ToLower() + "-commits");
			var commit = commitsContainer.GetBlockBlobReference(commitId.ToString());
			commit.Exists().Should().BeTrue();

			byte[] content;
			using (var stream = new MemoryStream())
			{
				commit.DownloadToStream(stream);
				content = stream.ToArray();
			}

			var events = (IEvent[])serializer.Deserialize(content);
			events.ShouldAllBeEquivalentTo(new IEvent[]
			{
				new TestEventCreated
				{
					SourceId = source.Id,
					SourceVersion = 1,
					Number = 42,
					Text = "forty-two"
				},
				new TestEventNumber
				{
					SourceId = source.Id,
					SourceVersion = 2,
					Number = 43
				}, 
				new TestEventText
				{
					SourceId = source.Id,
					SourceVersion = 3,
					Text = "forty-three"
				}, 
			});
			commit.Metadata["SourceId"].Should().Be(source.Id.ToString());
			commit.Metadata["SourceType"].Should().Be(source.GetType().Name);
			commit.Metadata["CorrelationId"].Should().Be(correlationId);
			commit.Metadata["CommitId"].Should().NotBeEmpty();
			commit.Metadata.ContainsKey("ParentId").Should().BeFalse();
		}
	}



	class TestEventSourced : EventSourced
	{
		public int Number { get; private set; }
		public string Text { get; private set; }

		public TestEventSourced(Guid id)
			: base(id)
		{
			Handles<TestEventCreated>(e => { Number = e.Number; Text = e.Text; });
			Handles<TestEventNumber>(e => Number = e.Number);
			Handles<TestEventText>(e => Text = e.Text);
		}

		public TestEventSourced(int number, string text)
			: this(Guid.NewGuid())
		{
			Apply(new TestEventCreated
			{
				Number = number,
				Text = text
			});
		}

		public void UpdateNumber(int number)
		{
			Apply(new TestEventNumber { Number = number });
		}

		public void UpdateText(string text)
		{
			Apply(new TestEventText { Text = text });
		}
	}

	class TestEventCreated : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public int Number { get; set; }
		public string Text { get; set; }
	}

	class TestEventNumber : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public int Number { get; set; }
	}

	class TestEventText : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string Text { get; set; }
	}
}
