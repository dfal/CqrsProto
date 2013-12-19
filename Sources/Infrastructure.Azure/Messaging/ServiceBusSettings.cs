using System.Collections.Generic;

namespace Infrastructure.Azure.Messaging
{
	// TODO: Implement loading settings from xml.
	public class ServiceBusSettings
	{
		public string ConnectionString = "[ConnectionString from Azure]";

		public List<TopicSettings> Topics = new List<TopicSettings>
		{
			new TopicSettings
			{
				Path = "Events",
				Subscriptions = new List<SubscriptionSettings> { new SubscriptionSettings { Name = "AllEvents" }}
			}
		};
	}

	public class TopicSettings
	{
		public string Path;

		public List<SubscriptionSettings> Subscriptions;
	}

	public class SubscriptionSettings
	{
		public string Name;
	}
}