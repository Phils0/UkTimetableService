using System;

namespace Timetable
{
    /// <summary>
    /// Accomodation classes supported
    /// </summary>
    public enum AccomodationClass
    {
        None, // Not available (Sleepers only)
        Both, // B Both First and Standard
        Standard, // S Standard only
        First // F First only (Sleepers only)
    }

    /// <summary>
    /// Possible reservation settings, making the ARSE mnemonic
    /// </summary>
    public enum ReservationIndicator
    {
        None, // Not supported
        Mandatory, // A Always - Manadatory
        Recommended, // R Recommended
        Supported, // S Supported
        EssentialBikes // E Essential for bicycles - never seen this value set
    }

    /// <summary>
    /// Possible catering values, can have 2 
    /// </summary>
    [Flags]
    public enum Catering
    {
        None,
        Buffet,         // C Buffet Service
        RestaurantCar,  // F Restaurant Car available for First Class passengers
        HotFood,        // H Service of hot food available
        FirstClass,     // M Meal included for First Class passengers
        Restaurant,     // R Restaurant
        Trolley         // T Trolley Service
    }
    
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
        /// Catering
        /// </summary>
        /// <remarks>Can have multiple values.  For values: https://wiki.openraildata.com/index.php?title=CIF_Schedule_Records </remarks>
        // Catering Catering { get; }
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