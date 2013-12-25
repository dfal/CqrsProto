using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;

namespace ProjectionHandler.Handling
{
	internal class EventHandlerRegistry
	{
		private readonly Dictionary<Type, IEventHandler> handlers = new Dictionary<Type, IEventHandler>();

		public void RegisterHandler(IEventHandler handler)
		{
			var genericHandler = typeof(IEventHandler<>);
			var supportedEventTypes = handler.GetType()
				.GetInterfaces()
				.Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericHandler)
				.Select(iface => iface.GetGenericArguments()[0])
				.ToList();

			if (handlers.Keys.Any(supportedEventTypes.Contains))
				throw new ArgumentException("The event handled by the received handler already has a registered handler");

			foreach (var eventType in supportedEventTypes)
			{
				handlers.Add(eventType, handler);
			}
		}

		public void Handle(IEvent @event)
		{
			var eventType = @event.GetType();
			if (!handlers.ContainsKey(eventType))
				throw new ArgumentException("The event handler has not been registered");

			var handler = handlers[eventType];
			((dynamic)handler).Handle((dynamic)@event);
		}		
	}
}