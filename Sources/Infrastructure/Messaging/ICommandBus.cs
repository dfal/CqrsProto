namespace Infrastructure.Messaging
{
	public interface ICommandBus
	{
		void Send(Envelope<ICommand> command);
	}
}
