using System;
using System.IO;

namespace Infrastructure.Serialization
{
	public interface ISerializer
	{
		byte[] Serialize(object graph);

		void Serialize(Stream output, object graph);

		object Deserialize(byte[] serilized, Type objecType);

		object Deserialize(Stream input, Type objectType);
	}
}