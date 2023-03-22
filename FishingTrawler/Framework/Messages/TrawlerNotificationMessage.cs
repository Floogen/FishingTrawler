namespace FishingTrawler.Messages
{
    internal class TrawlerNotificationMessage
    {
        public string Notification { get; set; }

        public TrawlerNotificationMessage()
        {

        }

        public TrawlerNotificationMessage(string notification)
        {
            Notification = notification;
        }
    }
}
