using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Tests.Annotations;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.EventSourcing
{
	// ReSharper disable ImplicitlyCapturedClosure

	[TestFixture]
	public class EventSourcedRepositoryFixture
	{
		[Test]
		public void Find_should_return_hydrated_entity_when_it_has_events()
		{
			var entityId = Guid.NewGuid();

			var events = new[]
			{
				new EventData
				{
					SourceId = entityId.ToString(),
					SourceVersion = 1,
					SourceType = "CorrectEventSourced",
					EventType = "CorrectEventSourced.Created",
					Payload = new byte[] {0}
				},
				new EventData
				{
					SourceId = entityId.ToString(),
					SourceVersion = 2,
					SourceType = "CorrectEventSourced",
					EventType = "CorrectEventSourced.Updated",
					Payload = new byte[] {1}
				},
				new EventData
				{
					SourceId = entityId.ToString(),
					SourceVersion = 3,
					SourceType = "CorrectEventSourced",
					EventType = "CorrectEventSourced.Updated",
					Payload = new byte[] {2}
				},
			};
			
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, 0)).Returns(events);

			var deserializedEvents = new IEvent[]
			{
				new CorrectEventSourced.Created { SourceId = entityId, SourceVersion = events[0].SourceVersion, Value = 1 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = events[1].SourceVersion, Value = 20 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = events[2].SourceVersion, Value = 30 }
			};

			var serializerMock = new Mock<ISerializer>();
			serializerMock.Setup(serializer => serializer.Deserialize(It.IsAny<byte[]>())).Returns((byte[] bytes) => deserializedEvents[bytes.First()]);

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object, serializerMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(deserializedEvents.Last().SourceVersion);
			entity.Value.Should().Be(deserializedEvents.Cast<dynamic>().Last().Value);
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

			var events = new[]
			{
				new EventData
				{
					SourceId = entityId.ToString(),
					SourceVersion = snapshot.Version + 1,
					SourceType = "CorrectEventSourced",
					EventType = "CorrectEventSourced.Updated",
					Payload = new byte[] {0}
				},
				new EventData
				{
					SourceId = entityId.ToString(),
					SourceVersion = snapshot.Version + 2,
					SourceType = "CorrectEventSourced",
					EventType = "CorrectEventSourced.Updated",
					Payload = new byte[] {1}
				}
			};

			var deserializedEvents = new []
			{
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = events[0].SourceVersion, Value = 40 },
				new CorrectEventSourced.Updated { SourceId = entityId, SourceVersion = events[1].SourceVersion, Value = 50 }
			};

			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, snapshot.Version + 1)).Returns(events);

			var serializerMock = new Mock<ISerializer>();
			serializerMock.Setup(serializer => serializer.Deserialize(It.IsAny<byte[]>())).Returns((byte[] bytes) => deserializedEvents[bytes.First()]);

			var snapshotStoreMock = new Mock<ISnapshotStore>();
			snapshotStoreMock.Setup(snapshotStore => snapshotStore.Get(entityId, int.MaxValue)).Returns(() => snapshot);

			var repository = new EventSourcedRepository<MementoOriginator>(eventStoreMock.Object, serializerMock.Object, snapshotStoreMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(deserializedEvents.Last().SourceVersion);
			entity.Value.Should().Be(deserializedEvents.Last().Value);
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
			eventStoreMock.Setup(eventStore => eventStore.Load(entityId, snapshot.Version + 1)).Returns(Enumerable.Empty<EventData>);

			var snapshotStoreMock = new Mock<ISnapshotStore>();
			snapshotStoreMock.Setup(snapshotStore => snapshotStore.Get(entityId, int.MaxValue)).Returns(() => snapshot);

			var repository = new EventSourcedRepository<MementoOriginator>(eventStoreMock.Object, Mock.Of<ISerializer>(), snapshotStoreMock.Object);

			var entity = repository.Find(entityId);

			entity.Id.Should().Be(entityId);
			entity.Version.Should().Be(snapshot.Version);
			entity.Value.Should().Be(snapshot.Value);
		}
		
		[Test]
		public void Find_should_return_null_when_there_are_no_evetns_with_specified_source_id()
		{
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(It.IsAny<Guid>(), 0)).Returns(Enumerable.Empty<EventData>);

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object, Mock.Of<ISerializer>());

			repository.Find(Guid.NewGuid()).Should().BeNull();
		}

		[Test]
		public void Get_should_throw_an_exception_when_there_are_no_evetns_with_specified_source_id()
		{
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Load(It.IsAny<Guid>(), 0)).Returns(new EventData[0]);

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object, Mock.Of<ISerializer>());

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

			var expectedEvents = new[]
			{
				new EventData
				{
					SourceId = entity.Id.ToString(),
					SourceVersion = 1,
					EventType = typeof(CorrectEventSourced.Created).Name,
					SourceType = typeof(CorrectEventSourced).Name,
					Payload = new byte[] {42},
					CorrelationId = correlationId
				},
				new EventData
				{
					SourceId = entity.Id.ToString(),
					SourceVersion = 2,
					EventType = typeof(CorrectEventSourced.Updated).Name,
					SourceType = typeof(CorrectEventSourced).Name,
					Payload = new byte[] {43},
					CorrelationId = correlationId
				},
				new EventData
				{
					SourceId = entity.Id.ToString(),
					SourceVersion = 3,
					EventType = typeof(CorrectEventSourced.Updated).Name,
					SourceType = typeof(CorrectEventSourced).Name,
					Payload = new byte[] {44},
					CorrelationId = correlationId
				},
			};

			var serializerMock = new Mock<ISerializer>();
			serializerMock.Setup(serializer => serializer.Serialize(It.IsAny<IEvent>()))
				.Returns((dynamic @event) => new[] { (byte)@event.Value });

			EventData[] storedEvents = null;
			var eventStoreMock = new Mock<IEventStore>();
			eventStoreMock.Setup(eventStore => eventStore.Save(entity.Id, It.IsAny<IEnumerable<EventData>>()))
				.Callback<Guid, IEnumerable<EventData>>((id, events) => storedEvents = events.ToArray());

			var repository = new EventSourcedRepository<CorrectEventSourced>(eventStoreMock.Object, serializerMock.Object);

			repository.Save(entity, correlationId);

			storedEvents.ShouldBeEquivalentTo(expectedEvents);
		}

		[Test]
		public void Should_throw_an_exception_when_event_sourced_type_does_not_implement_required_constructor()
		{
			Action action = () =>
				new EventSourcedRepository<IncorrectEventSourced>(new Mock<IEventStore>().Object, new Mock<ISerializer>().Object);

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

		// ReSharper restore ImplicitlyCapturedClosure
	}
}