using System.Collections.Generic;

namespace Infrastructure.Azure.Messaging
{
	public interface IMetadataProvider
	{
		IDictionary<string, string> GetMetadata<T>(T payload);
	}
}