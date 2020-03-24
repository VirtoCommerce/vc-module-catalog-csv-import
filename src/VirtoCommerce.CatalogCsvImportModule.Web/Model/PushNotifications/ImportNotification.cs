namespace VirtoCommerce.CatalogCsvImportModule.Web.Model.PushNotifications
{
    /// <summary>
    ///  Notification for catalog data import job.
    /// </summary>
	public class ImportNotification : JobNotificationBase
	{
		public ImportNotification(string creator)
			: base(creator)
		{
			NotifyType = "CatalogCsvImport";
		}
	}
}
