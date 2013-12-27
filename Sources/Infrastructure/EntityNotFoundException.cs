using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Infrastructure
{
	[Serializable]
	public class EntityNotFoundException : Exception
	{
		private readonly string entityId;
		private readonly string entityType;

		public EntityNotFoundException()
		{
		}

		public EntityNotFoundException(string entityId)
			: base(entityId)
		{
			this.entityId = entityId;
		}

		public EntityNotFoundException(string entityId, string entityType)
			: base(entityType + ": " + entityId)
		{
			this.entityId = entityId;
			this.entityType = entityType;
		}

		public EntityNotFoundException(string entityId, string entityType, string message, Exception inner)
			: base(message, inner)
		{
			this.entityId = entityId;
			this.entityType = entityType;
		}

		protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			this.entityId = info.GetString("entityId");
			this.entityType = info.GetString("entityType");
		}

		public string EntityId
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
			info.AddValue("entityId", this.entityId);
			info.AddValue("entityType", this.entityType);
		}
	}
}