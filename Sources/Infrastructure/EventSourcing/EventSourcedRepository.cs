using System;
using System.Collections.Specialized;
using System.Diagnostics;
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
		static readonly string SourceClrTypeName;
		static readonly Func<Guid, T> EntityFactory;
		static readonly Func<ISnapshotStore, Guid, int, IMemento> GetSnapshotFunc;

		const string SourceClrTypeHeader = "SourceClrTypeName";
		const string EventClrTypeHeader = "EventClrTypeName";
		
		readonly IEventStore eventStore;
		readonly ISnapshotStore snapshotStore;
		readonly ISerializer serializer;

		static EventSourcedRepository()
		{
			var sourceType = typeof (T);
			SourceType = sourceType.Name;
			SourceClrTypeName = sourceType.AssemblyQualifiedName;

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

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer)
		{
			Debug.Assert(eventStore != null);
			Debug.Assert(serializer != null);

			this.eventStore = eventStore;
			this.serializer = serializer;
		}

		public EventSourcedRepository(IEventStore eventStore, ISerializer serializer, ISnapshotStore snapshotStore)
			: this(eventStore, serializer)
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

			var events = eventStore.Load(id, minVersion)
				.Select(x => (IEvent) serializer.Deserialize(x.Payload, GetEventType(x.Metadata)))
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
			Debug.Assert(eventSourced != null);
			Debug.Assert(!string.IsNullOrEmpty(correlationId));

			var events = eventSourced.Flush();
			var serialized = events.Select(e => new EventData
			{
				SourceId = e.SourceId.ToString(),
				SourceVersion = e.SourceVersion,
				SourceType = SourceType,
				EventType = e.GetType().Name,
				CorrelationId = correlationId,
				Payload = serializer.Serialize(e),
				Metadata = serializer.Serialize(new NameValueCollection
				{
					{ SourceClrTypeHeader, SourceClrTypeName },
					{ EventClrTypeHeader, e.GetType().AssemblyQualifiedName }
				})
			});

			eventStore.Save(eventSourced.Id, serialized);
		}

		Type GetEventType(byte[] metadata)
		{
			var headers = (NameValueCollection)serializer.Deserialize(metadata, typeof (NameValueCollection));
			
			return Type.GetType(headers[EventClrTypeHeader]);
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
