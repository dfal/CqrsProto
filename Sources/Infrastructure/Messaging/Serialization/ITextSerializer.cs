using System.IO;

namespace Infrastructure.Azure.Messaging
{
	public interface ITextSerializer
	{
		void Serialize(TextWriter writer, object graph);

		object Deserialize(TextReader reader);
	}
}