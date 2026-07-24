namespace IPO.Correspondence.Models
{
    public enum NotificationStatus
    {
        SendingToNotify,
        SendingToProvider,
        DeliveredConfirmed,
        Unknown,
        DeliveredUnconfirmed, 
        SendingToRecipient,
        UserError,
        Rejected,
        SystemFailure,
        ValidationFailed,  
        VirusScanFailed,  
        Cancelled
    }
}
