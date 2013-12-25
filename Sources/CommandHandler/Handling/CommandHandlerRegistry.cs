using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;

namespace CommandHandler.Handling
{
	class CommandHandlerRegistry
	{
		private readonly Dictionary<Type, ICommandHandler> handlers = new Dictionary<Type, ICommandHandler>();
		
		public void RegisterHandler(ICommandHandler handler)
		{
			var genericHandler = typeof(ICommandHandler<>);
			var supportedCommandTypes = handler.GetType()
				.GetInterfaces()
				.Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericHandler)
				.Select(iface => iface.GetGenericArguments()[0])
				.ToList();

			if (handlers.Keys.Any(supportedCommandTypes.Contains))
				throw new ArgumentException("The command handled by the received handler already has a registered handler");

			foreach (var commandType in supportedCommandTypes)
			{
				handlers.Add(commandType, handler);
			}
		}

		public void Handle(ICommand command)
		{
			var commandType = command.GetType();
			if (!handlers.ContainsKey(commandType))
				throw new ArgumentException("The command handler has not been registered");

			var handler = handlers[commandType];
			((dynamic)handler).Handle((dynamic)command);
		}
	}
}