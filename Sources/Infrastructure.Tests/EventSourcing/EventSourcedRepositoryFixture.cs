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
			public CorrectEventSourced(Guid id) : base(id)
			{}
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
