namespace Infrastructure.Messaging
{
	interface IEventBus
	{
		void Publish(Envelope<IEvent> @event);
	}
}
