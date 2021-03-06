﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Infrastructure
{
	[Serializable]
	public class ConcurrencyException : Exception
	{
		private readonly Guid entityId;
		private readonly string entityType;

		public ConcurrencyException()
		{
		}

		public ConcurrencyException(Guid entityId)
			: base(entityId.ToString())
		{
			this.entityId = entityId;
		}

		public ConcurrencyException(Guid entityId, string entityType)
			: base(entityType + ": " + entityId)
		{
			this.entityId = entityId;
			this.entityType = entityType;
		}

		public ConcurrencyException(Guid entityId, string entityType, string message, Exception inner)
			: base(message, inner)
		{
			this.entityId = entityId;
			this.entityType = entityType;
		}

		protected ConcurrencyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			this.entityId = Guid.Parse(info.GetString("entityId"));
			this.entityType = info.GetString("entityType");
		}

		public Guid EntityId
		{
			get { return this.entityId; }
		}

		public string EntityType
		{
			get { return this.entityType; }
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("entityId", this.entityId.ToString());
			info.AddValue("entityType", this.entityType);
		}
	}
}
