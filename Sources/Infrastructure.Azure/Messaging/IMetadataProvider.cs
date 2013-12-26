using System.Collections.Generic;

namespace Infrastructure.Azure.Messaging
{
	public interface IMetadataProvider
	{
		IDictionary<string, string> GetMetadata<T>(T payload);
	}

	public class DummyMetadataProvider : IMetadataProvider
	{
		public IDictionary<string, string> GetMetadata<T>(T payload)
		{
			return new Dictionary<string, string> {{"metadata", "metadata value"}};
		}
	}
}