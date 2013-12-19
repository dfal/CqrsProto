using System;
using Infrastructure.Messaging;

namespace Infrastructure.Azure.Messaging
{
	class CommandBus : ICommandBus
	{
		public void Send(Envelope<ICommand> command)
		{
			throw new NotImplementedException();
		}
	}
}