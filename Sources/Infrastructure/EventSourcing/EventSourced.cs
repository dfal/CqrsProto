using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;

namespace Infrastructure.EventSourcing
{
	public abstract class EventSourced : IEventSourced
	{
		readonly IDictionary<Type, Action<IEvent>> handlers = new Dictionary<Type, Action<IEvent>>();
		readonly IList<IEvent> pendingEvents = new List<IEvent>();

		protected EventSourced(Guid id)
		{
			Id = id;
		}
		
		public Guid Id { get; private set; }
		
		public int Version { get; private set; }

		public IEvent[] Flush()
		{
			var events = pendingEvents.ToArray();
			pendingEvents.Clear();
			
			Version += events.Length;
			
			return events;
		}

		public void Restore(IEnumerable<IEvent> history)
		{
			foreach (var @event in history) Raise(@event);
		}

		protected void Handles<T>(Action<T> handler) where T : IEvent
		{
			handlers.Add(typeof(T), @event => handler((T)@event));
		}

		protected void Apply(IEvent @event)
		{
			@event.SourceId = Id;
			@event.SourceVersion = Version + 1;
			
			Raise(@event);
			
			pendingEvents.Add(@event);
		}

		void Raise(IEvent @event)
		{
			Action<IEvent> handler;
			
			if (handlers.TryGetValue(@event.GetType(), out handler))
				handler.Invoke(@event);

			Version = @event.SourceVersion;
		}
	}
}
