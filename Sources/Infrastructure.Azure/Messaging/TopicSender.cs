using System;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public class TopicSender : IMessageSender
	{
		private readonly TopicClient topicClient;
		private readonly RetryPolicy<ServiceBusTransientErrorDetectionStrategy> retryPolicy;

		public TopicSender(ServiceBusSettings settings, string topic)
		{
			retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(RetryStrategy.DefaultFixed);
			topicClient = TopicClient.CreateFromConnectionString(settings.ConnectionString, topic);
		}

		public void SendAsync(Func<BrokeredMessage> messageFactory)
		{
			retryPolicy.ExecuteAsync(() => topicClient.SendAsync(messageFactory()).ContinueWith(task =>
			{
				if (task.Exception != null)
				{
					// A non-transient exception occurred or retry limit has been reached;
					// TODO:Put message logging here;
					throw task.Exception;
				}
			}));
		}

		public void Send(Func<BrokeredMessage> messageFactory)
		{
			try
			{
				retryPolicy.ExecuteAction(() => topicClient.Send(messageFactory()));
			}
			catch(Exception)
			{
				// A non-transient exception occurred or retry limit has been reached;
				// TODO: Put message logging here;
				throw;
			}
		}
	}
}