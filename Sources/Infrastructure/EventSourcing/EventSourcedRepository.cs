using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		static EventSourcedRepository()
		{
			var sourceType = typeof (T);
			
			SourceType = sourceType.Name;

			EntityFactory = GetEntityFactory();
			
			if (typeof (IMementoOriginator).IsAssignableFrom(sourceType))
			{
				GetSnapshotFunc = (store, id, maxVersion) => store != null ? store.Get(id, maxVersion) : null;
			}
			else
			{
				GetSnapshotFunc = (store, id, maxValue) => null;
			}
		}

		public EventSourcedRepository(IEventStore eventStore)
		{
			Debug.Assert(eventStore != null);
			
			this.eventStore = eventStore;
		}

		public EventSourcedRepository(IEventStore eventStore, ISnapshotStore snapshotStore)
			: this(eventStore)
		{
			Debug.Assert(snapshotStore != null);

			this.snapshotStore = snapshotStore;
		}

		public T Find(Guid id)
		{
			var snapshot = GetSnapshotFunc(snapshotStore, id, int.MaxValue);

			var minVersion = 0;
			
			if (snapshot != null)
				minVersion = snapshot.Version + 1;

			var events = eventStore.Load(id, minVersion).AsCachedAnyEnumerable();

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
			Debug.Assert(eventSourced != null);
			Debug.Assert(!string.IsNullOrEmpty(correlationId));

			var events = eventSourced.Flush();
			
			eventStore.Save(eventSourced.Id, events, new Dictionary<string, string>
			{
				{"CorrelationId", correlationId},
				{"SourceType", SourceType},
				
			});
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
