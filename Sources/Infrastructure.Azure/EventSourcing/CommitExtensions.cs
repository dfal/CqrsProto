using System.Diagnostics;
using System.Linq;
using Infrastructure.EventSourcing;
using Microsoft.WindowsAzure.Storage;

namespace Infrastructure.Azure.EventSourcing
{
	public static class CommitExtensions
	{
		public static AccessCondition AccessCondition(this Commit commit)
		{
			Debug.Assert(commit != null && commit.Changes != null && commit.Changes.Length > 0);
			
			var expectedVersion = commit.Changes.Last().SourceVersion - commit.Changes.Length;
			return expectedVersion == 0
				? Microsoft.WindowsAzure.Storage.AccessCondition.GenerateIfNoneMatchCondition("*")
				: Microsoft.WindowsAzure.Storage.AccessCondition.GenerateIfSequenceNumberEqualCondition(expectedVersion);
		}
	}
}
