using System.Collections.Generic;
using Infrastructure.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public interface IMetadataProvider
	{
		IDictionary<string, string> GetMetadata(IEvent payload);
	}
}