using System;

namespace Infrastructure.EventSourcing
{
	public interface IMemento
	{
		Guid SourceId { get; }
		int Version { get; }
	}
}
