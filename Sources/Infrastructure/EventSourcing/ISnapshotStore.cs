using System;

namespace Infrastructure.EventSourcing
{
	public interface ISnapshotStore
	{
		IMemento Get(Guid sourceId, int maxVersion);
		void Save(IMemento memento);
	}
}
