using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Infrastructure.Serialization
{
	public class JsonSerializer : ISerializer
	{
		static readonly Newtonsoft.Json.JsonSerializer Serializer;

		static JsonSerializer()
		{
			Serializer = new Newtonsoft.Json.JsonSerializer
			{
				TypeNameHandling = TypeNameHandling.Auto,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};
		}

		public byte[] Serialize(object graph)
		{
			using (var stream = new MemoryStream())
			{
				Serialize(stream, graph);
				return stream.ToArray();
			}
		}

		public void Serialize(Stream output, object graph)
		{
			using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(output, Encoding.UTF8)))
			{
				Serializer.Serialize(jsonWriter, graph);
			}
		}

		public object Deserialize(byte[] serilized)
		{
			using (var stream = new MemoryStream(serilized))
			{
				return Deserialize(stream);
			}
		}

		public object Deserialize(Stream input)
		{
			using (var jsonReader = new JsonTextReader(new StreamReader(input, Encoding.UTF8)))
			{
				return Serializer.Deserialize(jsonReader);
			}
		}
	}
}
