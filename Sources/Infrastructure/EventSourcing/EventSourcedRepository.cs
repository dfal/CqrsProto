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

		readonly IDictionary<T, Head> loadedHeads;

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
			this.loadedHeads = new Dictionary<T, Head>();
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

			var commits = eventStore.Load(id, minVersion).AsCachedAnyEnumerable();

			if (snapshot == null && !commits.Any()) return null;

			var entity = EntityFactory(id);
			
			if (snapshot != null) 
				((IMementoOriginator)entity).RestoreSnapshot(snapshot);

			RestoreCommits(entity, commits);
			
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

			var changes = eventSourced.Flush();
			if (changes.Length == 0) return;

			var isNew = eventSourced.Version == changes.Length;

			if (!isNew && !loadedHeads.ContainsKey(eventSourced))
				throw new InvalidOperationException("Entity wasn't loaded by this instance of repository.");

			var commit = new Commit
			{
				Id = Guid.NewGuid(),
				ParentId = isNew ? null : new Guid?(loadedHeads[eventSourced].CommitId),
				SourceId = eventSourced.Id,
				SourceType = SourceType,
				SourceETag = isNew ? null : loadedHeads[eventSourced].ETag,
				Changes = changes,
				Metadata = new Dictionary<string, string>
				{
					{"CorrelationId", correlationId}
				}
			};
			
			eventStore.Save(commit);

			loadedHeads[eventSourced] = new Head
			{
				CommitId = commit.Id,
				ETag = commit.SourceETag
			};
		}

		void RestoreCommits(T entity, IEnumerable<Commit> commits)
		{
			foreach (var commit in commits)
			{
				entity.Restore(commit.Changes);
				loadedHeads[entity] = new Head
				{
					CommitId = commit.Id,
					ETag = commit.SourceETag
				};
			}
		}

		static Func<Guid, T> GetEntityFactory()
		{
			var ctor = typeof(T).GetConstructor(new[] { typeof(Guid) });
			
			if (ctor == null)
				throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid)");

			return id => (T)ctor.Invoke(new object[] { id });
		}

		struct Head
		{
			public Guid CommitId;
			public string ETag;
		}
	}
}
