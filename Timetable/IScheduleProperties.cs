namespace Timetable
{
    /// <summary>
    /// Service Characteristics
    /// </summary>
    public interface IScheduleProperties
    {
        /// <summary>
        /// 2 char\6 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// First 4 digits indicate the logical service
        /// with last 2 digits for splits and joins
        /// </remarks>
        string RetailServiceId { get; }
        /// <summary>
        /// 2 char\4 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// Indicates the retail service in NRS
        /// Use to not have to worry about splits and joins
        /// </remarks>
        string NrsRetailServiceId { get; }
        /// <summary>
        /// Train Identity - sometimes called HeadCode
        /// </summary>
        string TrainIdentity { get; }
        /// <summary>
        /// Toc
        /// </summary>
        Toc Operator { get; }
        /// <summary>
        /// Seat Accomodation class
        /// </summary>
        AccomodationClass SeatClass { get; }
        /// <summary>
        /// Sleeper Accomodation Class
        /// </summary>
        AccomodationClass SleeperClass { get; }
        /// <summary>
        ///  Reservation indicator
        /// </summary>
        ReservationIndicator ReservationIndicator { get; }
        /// <summary>
        /// Status - values incorporates transport mode and whether its permanent or STP
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Status </remarks>
        string Status { get; }
        /// <summary>
        /// Train Category
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Category </remarks>
        string Category { get; }
        /// <summary>
        /// Schedule has the retail service id
        /// </summary>
        /// <param name="retailServiceId"></param>
        /// <returns></returns>
        bool HasRetailServiceId(string retailServiceId);
        /// <summary>
        /// Is Operating Toc
        /// </summary>
        /// <param name="toc"></param>
        /// <returns></returns>
        bool IsOperatedBy(string toc);
    }
}