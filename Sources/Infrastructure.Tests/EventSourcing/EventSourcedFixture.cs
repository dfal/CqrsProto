using System;
using System.Linq;
using FluentAssertions;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using NUnit.Framework;

namespace Infrastructure.Tests.EventSourcing
{
	[TestFixture]
	public class EventSourcedFixture
	{
		[Test]
		public void Should_increase_version_when_applying_event()
		{
			var test = new TestEventSourced(42, "forty-two");

			test.Version.Should().Be(1);

			test.RaiseTestEventNumber(43);

			test.Version.Should().Be(2);
		}

		[Test]
		public void Should_restore_from_history()
		{
			var entityId = Guid.NewGuid();
			var test = new TestEventSourced(entityId);
			
			var events = new IEvent[]
			{
				new TestEventCreated { SourceVersion = 1 }, 
				new TestEventNumber { Number = 1, SourceVersion = 2 }, 
				new TestEventNumber { Number = 1, SourceVersion = 3 }, 
				new TestEventText { Text = "2", SourceVersion = 4 }, 
				new TestEventText { Text = "3", SourceVersion = 5 }, 
				new TestEventNumber { Number = 5, SourceVersion = 6 }, 
				new TestEventText { Text = "8", SourceVersion = 7 }
			};

			test.Restore(events);

			test.Version.Should().Be(events.Length);
			test.Number.Should().Be(events.OfType<TestEventNumber>().Last().Number);
			test.Text.Should().Be(events.OfType<TestEventText>().Last().Text);
		}

		[Test]
		public void Should_return_pending_events_when_flushed()
		{
			var test = new TestEventSourced(1, "one"); //Created event
			test.RaiseTestEventNumber(2);
			test.RaiseTestEventNumber(3);
			test.RaiseTestEventNumber(4);

			var pendingEvents = test.Flush();

			pendingEvents.ShouldBeEquivalentTo(new IEvent[] 
			{
				new TestEventCreated { SourceId = test.Id, SourceVersion = 1, Number = 1, Text = "one"},
				new TestEventNumber { SourceId = test.Id, SourceVersion = 2, Number = 2 },
				new TestEventNumber { SourceId = test.Id, SourceVersion = 3, Number = 3 },
				new TestEventNumber { SourceId = test.Id, SourceVersion = 4, Number = 4 }
			});

			test.Version.Should().Be(pendingEvents.Length);

			test.Flush().Length.Should().Be(0);
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

		public void RaiseTestEventNumber(int number)
		{
			Apply(new TestEventNumber { Number = number });
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
