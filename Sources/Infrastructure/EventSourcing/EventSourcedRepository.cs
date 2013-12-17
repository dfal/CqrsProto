using System;
using System.Linq;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Utils;

namespace Infrastructure.EventSourcing
{
	public class EventSourcedRepository<T> : IEventSourcedRepository<T>
		where T : class, IEventSourced
	{
		static readonly string SourceType;
		static readonly Func<Guid, T> EntityFactory;
		static readonly Func<ISnapshotStore, Guid, int, IMemento> GetSnapshotFunc;
		
		readonly IEventStore eventStore;
		readonly ISnapshotStore snapshotStore;
		readonly ISerializer serializer;

		static EventSourcedRepository()
		{
			SourceType = typeof (T).Name;

			EntityFactory = GetEntityFactory();
			
			if (typeof (IMementoOriginator).IsAssignableFrom(typeof (T)))
			{
				GetSnapshotFunc = (store, id, maxVersion) => store != null ? store.Get(id, maxVersion) : null;
			}
			else
			{
				GetSnapshotFunc = (store, id, maxValue) => null;
			}
		} 

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer)
			: this(eventStore, serializer, null)
		{}

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer, ISnapshotStore snapshotStore)
		{
			this.eventStore = eventStore;
			this.snapshotStore = snapshotStore;
			this.serializer = serializer;
		}

		public T Find(Guid id)
		{
			var snapshot = GetSnapshotFunc(snapshotStore, id, int.MaxValue);

			var minVersion = 0;
			
			if (snapshot != null)
				minVersion = snapshot.Version + 1;

			var events = eventStore.Load(id, minVersion)
				.Select(x => (IEvent) serializer.Deserialize(x.Payload))
				.AsCachedAnyEnumerable();

			if (snapshot == null && !events.Any()) return null;

			var entity = EntityFactory(id);
			
			if (snapshot != null) 
				((IMementoOriginator)entity).RestoreSnapshot(snapshot);
			
			if (events.Any())
				entity.Restore(events);
			
			return entity;
		}

		public T Get(Guid id)
		{
			var entity = Find(id);
			
			if (entity == null)
				throw new EntityNotFoundException(id, SourceType);

			return entity;
		}

		public void Save(T eventSourced, string correlationId)
		{
			var events = eventSourced.Flush();
			var serialized = events.Select(e => new EventData
			{
				SourceId = e.SourceId.ToString(),
				SourceVersion = e.SourceVersion,
				SourceType = SourceType,
				EventType = e.GetType().Name,
				CorrelationId = correlationId,
				Payload = serializer.Serialize(e),
			});

			eventStore.Save(eventSourced.Id, serialized);
		}

		static Func<Guid, T> GetEntityFactory()
		{
			var ctor = typeof(T).GetConstructor(new[] { typeof(Guid) });
			
			if (ctor == null)
				throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid)");

			return id => (T)ctor.Invoke(new object[] { id });
		}
	}
}
