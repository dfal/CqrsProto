namespace Infrastructure.Messaging
{
	interface ICommandBus
	{
		void Send(Envelope<ICommand> command);
	}
}
