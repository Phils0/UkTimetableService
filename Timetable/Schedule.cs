namespace Timetable
{
    /// <summary>
    /// Accomodation classes supported
    /// </summary>
    public enum AccomodationClass
    {
        None,   // Not available (Sleepers only)
        B,      // Both First and Standard
        S,      // Standard only
        F       // First only (Sleepers only)
    }

    /// <summary>
    /// Possible reservation settings, making the ARSE mnemonic
    /// </summary>
    public enum ReservationIndicator
    {
        None,   // Not supported
        A,      // Always - Manadatory
        R,      // Recommended
        S,      // Supported
        E       // Essential for bicycles - never seen this value set
    }

    /// <summary>
    /// Short Term Plan (STP) 
    /// </summary>
    public enum StpIndicator
    {
        P,  // Permanent schedule
        C,  // STP Cancellation of Permanent schedule
        N,  // New STP schedule (not an overlay)
        O,  // STP overlay of Permanent schedule
    }
    
    /// <summary>
    /// An indivdual schedule
    /// </summary>
    public class Schedule
    {
        public string TimetableUid { get; set; }
        
        public StpIndicator StpIndicator { get; set; }
        
        public Calendar On { get; set; }
        
        public string RetailServiceId { get; set; }
        
        public Toc Toc { get; set; }
        
        public AccomodationClass SeatClass { get; set; }
        
        public AccomodationClass SleeperClass { get; set; }
        
        public ReservationIndicator ReservationIndicator { get; set; }

        public override string ToString()
        {
            return $"{TimetableUid} -{StpIndicator} {On}";
        }
    }
}