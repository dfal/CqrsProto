namespace Infrastructure.EventSourcing
{
	public interface IMementoOriginator
	{
		IMemento TakeSnapshot();
		void RestoreSnapshot(IMemento snapshot);
	}
}
