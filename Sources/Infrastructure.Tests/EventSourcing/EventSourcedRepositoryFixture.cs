using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Tests.Annotations;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.EventSourcing
{
	[TestFixture]
	public class EventSourcedRepositoryFixture
	{
		[Test]
		public void Find_should_return_hydrated_entity_when_it_has_events()
		{
			var entityId = Guid.NewGuid();

			var events = new IEvent[]
			{
				new CorrectEventSourced.Created { SourceId = entityId, SourceVersion = 1, Value = 1 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = 2, Value = 20 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = 3, Value = 30 }
			};

			var commit = new Commit
			{
				Id = Guid.NewGuid(),
				SourceId = entityId,
				Changes = events
			};

			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, 0)).Returns(new [] { commit });

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(events.Last().SourceVersion);
			entity.Value.Should().Be(events.Cast<dynamic>().Last().Value);
		}

		[Test]
		public void Find_should_return_hydrated_entity_when_it_has_snapsot_and_events()
		{
			var entityId = Guid.NewGuid();

			var snapshot = new MementoOriginator.Memento
			{
				SourceId = entityId,
				Version = 3,
				Value = 30
			};

			var events = new IEvent[]
			{
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = snapshot.Version + 1, Value = 40 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = snapshot.Version + 2, Value = 50 }
			};

			var commit = new Commit
			{
				Id = Guid.NewGuid(),
				SourceId = entityId,
				Changes = events
			};

			var snapshotStoreMock = new Mock<ISnapshotStore>();
			snapshotStoreMock.Setup(snapshotStore => snapshotStore.Get(entityId, int.MaxValue)).Returns(() => snapshot);

			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, snapshot.Version + 1)).Returns(new[] { commit });

			var repository = new EventSourcedRepository<MementoOriginator>(eventStoreMock.Object, snapshotStoreMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(events.Last().SourceVersion);
			entity.Value.Should().Be(events.OfType<CorrectEventSourced.Updated>().Last().Value);
		}

		[Test]
		public void Find_should_return_hydrated_entity_when_it_has_snapsot_and_no_later_events()
		{
			var entityId = Guid.NewGuid();

			var snapshot = new MementoOriginator.Memento
			{
				SourceId = entityId,
				Version = 3,
				Value = 30
			};

			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, snapshot.Version + 1)).Returns(Enumerable.Empty<Commit>);

			var snapshotStoreMock = new Mock<ISnapshotStore>();
			snapshotStoreMock.Setup(snapshotStore => snapshotStore.Get(entityId, int.MaxValue)).Returns(() => snapshot);

			var repository = new EventSourcedRepository<MementoOriginator>(eventStoreMock.Object, snapshotStoreMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(snapshot.Version);
			entity.Value.Should().Be(snapshot.Value);
		}
		
		[Test]
		public void Find_should_return_null_when_there_are_no_evetns_with_specified_source_id()
		{
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(It.IsAny<Guid>(), 0)).Returns(Enumerable.Empty<Commit>);

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object);

			repository.Find(Guid.NewGuid()).Should().BeNull();
		}

		[Test]
		public void Get_should_throw_an_exception_when_there_are_no_evetns_with_specified_source_id()
		{
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(It.IsAny<Guid>(), 0)).Returns(Enumerable.Empty<Commit>);

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object);

			var id = Guid.NewGuid();
			Action action = () => repository.Get(id);

			action.ShouldThrow<EntityNotFoundException>()
				.Where(ex => ex.EntityId == id && ex.EntityType == typeof(CorrectEventSourced).Name);
		}

		[Test]
		public void Should_save_events_in_event_store()
		{
			var entity = new CorrectEventSourced(42);
			entity.Update(43);
			entity.Update(44);

			var correlationId = Guid.NewGuid().ToString();

			var expectedEvents = new IEvent[]
			{
				new CorrectEventSourced.Created
				{
					SourceId = entity.Id,
					SourceVersion = 1,
					Value = 42
				},
				new CorrectEventSourced.Updated
				{
					SourceId = entity.Id,
					SourceVersion = 2,
					Value = 43
				},
				new CorrectEventSourced.Updated
				{
					SourceId = entity.Id,
					SourceVersion = 3,
					Value = 44
				},
			};

			var expectedMetadata = new Dictionary<string, string>
			{
				{ "CorrelationId", correlationId }
			};

			var eventStoreMock = new Mock<IEventStore>();

			Commit storedCommit = null;
			eventStoreMock.Setup(eventStore => eventStore.Save(It.IsAny<Commit>()))
				.Callback<Commit>(commit => storedCommit = commit);
			
			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object);

			repository.Save(entity, correlationId);

			storedCommit.Changes.ShouldBeEquivalentTo(expectedEvents);
			storedCommit.Metadata.ShouldAllBeEquivalentTo(expectedMetadata);
			storedCommit.Id.Should().NotBeEmpty();
			storedCommit.SourceId.Should().Be(entity.Id);
			storedCommit.SourceType.Should().Be(typeof (CorrectEventSourced).Name);
		}

		[Test]
		public void Should_throw_an_exception_when_event_sourced_type_does_not_implement_required_constructor()
		{
			Action action = () =>
				new EventSourcedRepository<IncorrectEventSourced>(new Mock<IEventStore>().Object);

			action.ShouldThrow<TypeInitializationException>()
				.WithInnerException<InvalidCastException>()
				.WithInnerMessage("Type T must have a constructor with the following signature: .ctor(Guid)");
		}

		class CorrectEventSourced : EventSourced
		{
			public int Value { get; protected set; }

			[UsedImplicitly]
			public CorrectEventSourced(Guid id)
				: base(id)
			{
				Handles<Created>(OnCreated);
				Handles<Updated>(OnUpdated);
			}

			public CorrectEventSourced(int value) :
				this(Guid.NewGuid())
			{
				Apply(new Created { Value = value });
			}

			public void Update(int newValue)
			{
				if (Value == newValue) return;

				Apply(new Updated
				{
					Value = newValue
				});
			}

			void OnCreated(Created @event)
			{
				Value = @event.Value;
			}

			void OnUpdated(Updated @event)
			{
				Value = @event.Value;
			}

			public class Created : IEvent
			{
				public Guid SourceId { get; set; }
				public int SourceVersion { get; set; }

				public int Value { get; set; }
			}

			public class Updated : IEvent
			{
				public Guid SourceId { get; set; }
				public int SourceVersion { get; set; }

				public int Value { get; set; }
			}
		}

		[UsedImplicitly]
		class MementoOriginator : CorrectEventSourced, IMementoOriginator
		{
			public MementoOriginator(Guid id)
				: base(id)
			{ }

			public MementoOriginator(int value)
				: base(value)
			{ }

			public IMemento TakeSnapshot()
			{
				return new Memento
				{
					SourceId = this.Id,
					Version = this.Version,
					Value = this.Value
				};
			}

			public void RestoreSnapshot(IMemento snapshot)
			{
				Value = ((Memento)snapshot).Value;
				Version = snapshot.Version;
			}

			public class Memento : IMemento
			{
				public int Value { get; set; }
				public Guid SourceId { get; set; }
				public int Version { get; set; }
			}
		}

		[UsedImplicitly]
		class IncorrectEventSourced : IEventSourced
		{
			public Guid Id
			{
				get { return Guid.Empty; }
			}

			public int Version
			{
				get { return 0; }
			}

			public IEvent[] Flush()
			{
				return new IEvent[0];
			}

			public void Restore(IEnumerable<IEvent> history)
			{ }
		}
	}
}