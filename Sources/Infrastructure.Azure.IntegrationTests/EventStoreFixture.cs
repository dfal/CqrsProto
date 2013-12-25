using System;
using System.IO;
using FluentAssertions;
using Infrastructure.Azure.EventSourcing;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Azure.IntegrationTests
{
	[TestFixture]
	public class EventStoreFixture
	{
		const string Tenant = "testTenant";

		CloudBlobContainer headsContainer;
		CloudBlobContainer commitsContainer;
		
		EventSourcedRepository<TestEventSourced> repository;
		ISerializer serializer;

		[SetUp]
		public void SetUp()
		{
			serializer = new JsonSerializer();

			var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
			
			var eventStore = new EventStore(Tenant, cloudStorageAccount, serializer, new Mock<IEventBus>().Object);

			repository = new EventSourcedRepository<TestEventSourced>(eventStore);

			var blobClient = cloudStorageAccount.CreateCloudBlobClient();
			this.headsContainer = blobClient.GetContainerReference(Tenant.ToLower() + "-heads");
			this.commitsContainer = blobClient.GetContainerReference(Tenant.ToLower() + "-commits");
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			headsContainer.DeleteIfExists();
			commitsContainer.DeleteIfExists();
		}

		[Test]
		public void Should_store_first_commit_with_events()
		{
			const string correlationId = "testcorrelation";
			
			var source = new TestEventSourced(42, "forty-two");
			source.UpdateNumber(43);
			source.UpdateText("forty-three");
			repository.Save(source, correlationId);

			var head = headsContainer.GetBlockBlobReference(source.Id.ToString());
			head.Exists().Should().BeTrue();

			Guid commitId;
			using (var stream = new MemoryStream())
			{
				head.DownloadToStream(stream);
				commitId = new Guid(stream.ToArray());
			}

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

		[Test]
		public void Shoud_throw_concurrency_exception_when_there_was_an_attempt_to_save_changes_of_outdated_entity()
		{
			var source = new TestEventSourced(42, "forty-two");
			repository.Save(source, Guid.NewGuid().ToString());

			var source1 = repository.Get(source.Id);
			source1.UpdateNumber(43);

			repository.Save(source1, Guid.NewGuid().ToString());

			source.UpdateNumber(44);

			Action action = () => repository.Save(source, Guid.NewGuid().ToString());
			action.ShouldThrow<ConcurrencyException>()
				.Where(ex => ex.EntityId == source.Id && ex.EntityType == source.GetType().Name);
		}

		[Test]
		public void Shoud_be_able_to_save_the_same_instance_multiple_times()
		{
			var source = new TestEventSourced(42, "forty-two");
			repository.Save(source, Guid.NewGuid().ToString());

			source.UpdateNumber(44);

			repository.Save(source, Guid.NewGuid().ToString());
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

	public class TestEventCreated : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public int Number { get; set; }
		public string Text { get; set; }
	}

	public class TestEventNumber : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public int Number { get; set; }
	}

	public class TestEventText : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string Text { get; set; }
	}
}
