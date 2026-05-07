namespace EventEase.Models
{
    public class BookingViewModel
    {
        public int BookingID { get; set; }
        public DateTime BookingDate { get; set; }

        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }

        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
    }
}