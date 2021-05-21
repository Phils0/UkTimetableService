namespace Timetable
{
    /// <summary>
    /// An individual schedule
    /// </summary>
    public class CifScheduleProperties : IScheduleProperties
    {
        /// <summary>
        /// 2 char\6 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// First 4 digits indicate the logical service
        /// with last 2 digits for splits and joins
        /// </remarks>
        public string RetailServiceId { get; set; }

        /// <summary>
        /// 2 char\4 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// Indicates the retail service in NRS
        /// Use to not have to worry about splits and joins
        /// </remarks>
        public string ShortRetailServiceId
        {
            get { return string.IsNullOrEmpty(RetailServiceId) ? "" : RetailServiceId.Substring(0, 6); }
        }
        
        /// <summary>
        /// Train Identity - sometimes called HeadCode
        /// </summary>
        public string TrainIdentity { get; set; }

        /// <summary>
        /// Toc
        /// </summary>
        public Toc Operator { get; set; }
        /// <summary>
        /// Seat Accomodation class
        /// </summary>
        public AccomodationClass SeatClass { get; set; }
        /// <summary>
        /// Sleeper Accomodation Class
        /// </summary>
        public AccomodationClass SleeperClass { get; set; }
        /// <summary>
        ///  Reservation indicator
        /// </summary>
        public ReservationIndicator ReservationIndicator { get; set; }

        /// <summary>
        /// Catering
        /// </summary>
        /// <remarks>Can have multiple values.  For values: https://wiki.openraildata.com/index.php?title=CIF_Schedule_Records </remarks>
        public Catering Catering { get; set; } = Catering.None;
        
        /// <summary>
        /// Train Category
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Category </remarks>
        public string Category { get; set; }
        
        public bool HasRetailServiceId(string retailServiceId)
        {
            return !string.IsNullOrEmpty(RetailServiceId) && 
                   retailServiceId != null && 
                   retailServiceId.StartsWith(ShortRetailServiceId);
        }

        public bool IsOperatedBy(string toc)
        {
            return Operator.Equals(toc);
        }
        
        public override string ToString()
        {
            return $"{RetailServiceId} - {SeatClass}|{SleeperClass}|{ReservationIndicator}";
        }
    }
}