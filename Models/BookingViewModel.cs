namespace EventEase.Models
{
    public class BookingViewModel
    {
        public int BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string VenueName { get; set; }
        public string VenueLocation { get; set; }
        public int Capacity { get; set; }
    }
}  