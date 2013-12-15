namespace Infrastructure.Serialization
{
	public interface ISerializer
	{
		byte[] Serialize(object graph);
		object Deserialize(byte[] serilized);
	}
}