using Microsoft.WindowsAzure.Storage.Table;

namespace ProjectionHandler.Documents
{
	public class CustomerDocument : TableEntity
	{
		public string Name { get; set; }
		public string VatNumber { get; set; }
		public string Email { get; set; }
	}
}
