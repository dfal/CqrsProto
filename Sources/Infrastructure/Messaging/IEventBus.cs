namespace Infrastructure.Messaging
{
	public interface IEventBus
	{
		void Publish(Envelope<IEvent> @event);
	}
}
