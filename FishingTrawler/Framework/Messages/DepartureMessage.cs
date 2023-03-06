namespace FishingTrawler.Messages
{
    internal class DepartureMessage
    {
        public long MainDeckhand { get; set; }

        public DepartureMessage(long mainDeckhand)
        {
            MainDeckhand = mainDeckhand;
        }
    }
}
