using System;
using System.Linq;
using System.Reflection;
using Infrastructure.Messaging;
using Infrastructure.Serialization;

namespace Infrastructure.EventSourcing
{
	public class EventSourcedRepository<T> : IEventSourcedRepository<T>
		where T : class, IEventSourced
	{
		readonly IEventStore eventStore;
		readonly ISnapshotStore snapshotStore;
		readonly ISerializer serializer;

		readonly Func<Guid, int, IMemento> getSnapshot;

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer)
			: this(eventStore, serializer, null)
		{}

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer, ISnapshotStore snapshotStore)
		{
			this.eventStore = eventStore;
			this.snapshotStore = snapshotStore;
			this.serializer = serializer;

			if (snapshotStore != null && typeof (IMementoOriginator).IsAssignableFrom(typeof (T)))
			{
				this.getSnapshot = snapshotStore.Get;
			}
			else
			{
				this.getSnapshot = (id, maxVersion) => null;
			}
		}

		public T Find(Guid id)
		{
			throw new NotImplementedException();
			/*var events = eventStore.Load(id, 0).Select(x => (IEvent) serializer.Deserialize(x.Payload));

			if (!events.Any()) return null;

			if (mementoSupported)
				return CreateMementoOriginator*/
		}

		public T Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public void Save(T eventSourced, string correlationId)
		{
			throw new NotImplementedException();
		}
	}
}
