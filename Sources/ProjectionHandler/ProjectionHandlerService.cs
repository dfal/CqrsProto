﻿using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;
using ProjectionHandler.Handling;

namespace ProjectionHandler
{
	class ProjectionHandlerService {
		static bool stopped;
		private TopicConsumer consumer;
		private EventHandlerRegistry handlerRegistry;
		readonly ServiceBusSettings settings = new ServiceBusSettings();

		public ProjectionHandlerService()
		{
			InitializeConsumer();
			InitializeEventHandlers();
		}

		public void Start()
		{
			stopped = false;
			while (!stopped)
			{
				consumer.Consume<IEvent>(handlerRegistry.Handle);
			}
		}

		public void Stop()
		{
			stopped = true;
		}

		private void InitializeConsumer()
		{
			// TODO: Get settings from config file.
			var serializer = new JsonSerializer();
			var client = SubscriptionClient.CreateFromConnectionString(settings.ConnectionString, "proto/events", "AllEvents");
			consumer = new TopicConsumer(client, serializer);
		}

		private void InitializeEventHandlers()
		{
			handlerRegistry = new EventHandlerRegistry();
			handlerRegistry.RegisterHandler(new CustomerEventHandler());
		}
	}
}