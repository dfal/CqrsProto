using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public class ServiceBusConfig
	{
		private readonly ServiceBusSettings settings;

		public ServiceBusConfig(ServiceBusSettings settings)
		{
			this.settings = settings;
		}

		public void Initialize()
		{
			var namespaceManager = NamespaceManager.CreateFromConnectionString(settings.ConnectionString);
			settings.Topics.ForEach(topic =>
			{
				CreateTopicIfNotExists(namespaceManager, topic);
				topic.Subscriptions.ForEach(subscription =>
					CreateSubscriptionIfNotExists(namespaceManager, topic.Path, subscription));
			});
		}

		private static void CreateTopicIfNotExists(NamespaceManager namespaceManager, TopicSettings topicSettings)
		{
			if(namespaceManager.TopicExists(topicSettings.Path)) return;

			var topicDescription = new TopicDescription(topicSettings.Path)
			{
				// Fill topic settings;
			};

			namespaceManager.CreateTopic(topicDescription);
		}


		private static void CreateSubscriptionIfNotExists(NamespaceManager namespaceManager, string topicPath, SubscriptionSettings subscriptionSettings)
		{
			if (namespaceManager.SubscriptionExists(topicPath, subscriptionSettings.Name)) return;

			var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionSettings.Name)
			{
				// Fill subscription settings;
				LockDuration = TimeSpan.FromSeconds(150),
			};

			namespaceManager.CreateSubscription(subscriptionDescription);
		}
	}
}