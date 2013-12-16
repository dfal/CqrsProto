using System;
using System.Collections.Generic;
using FluentAssertions;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
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
			
		}

		[Test]
		public void Find_should_return_hydrated_entity_when_it_has_snapsot_and_events()
		{
			
		}

		[Test]
		public void Find_should_return_null_when_there_are_no_evetns_with_specified_source_id()
		{

		}

		[Test]
		public void Get_should_return_hydrated_entity_when_it_has_events()
		{
			
		}

		[Test]
		public void Get_should_throw_an_exception_when_there_are_no_evetns_with_specified_source_id()
		{

		}

		public void Should_save_events_in_event_store()
		{
			
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

			public CorrectEventSourced(Guid id) : base(id)
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
					OldValue = Value,
					NewValue = newValue
				});
			}

			void OnCreated(Created @event)
			{
				Value = @event.Value;
			}

			void OnUpdated(Updated @event)
			{
				Value = @event.NewValue;
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

				public int OldValue { get; set; }
				public int NewValue { get; set; }
			}
		}

		class MementoOriginator : CorrectEventSourced, IMementoOriginator
		{
			public MementoOriginator(Guid id) : base(id)
			{ }

			public MementoOriginator(int value) : base(value)
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
				Value = ((Memento) snapshot).Value;
			}

			class Memento : IMemento
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
			{}
		}
	}
}
