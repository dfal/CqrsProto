using Infrastructure.Messaging;

namespace ProjectionHandler.EventHandling
{
	public interface IEventHandler { }

	public interface IEventHandler<T> : IEventHandler where T : IEvent
	{
		void Handle(T @event);
	}
}