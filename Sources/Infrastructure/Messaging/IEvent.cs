using System;

namespace Infrastructure.Messaging
{
	public interface IEvent
	{
		Guid SourceId { get; set; }

		int SourceVersion { get; set; }
	}
}
