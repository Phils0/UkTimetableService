namespace Timetable
{
    public static class ServiceStatus
    {
        public const string PermanentBus = "B";
        public const string PermanentFreight = "F";
        public const string PermanentPassenger = "P";
        public const string PermanentShip = "S";
        public const string PermanentTrip = "T";
        public const string StpPassenger = "1";
        public const string StpFreight = "2";
        public const string StpTrip = "3";
        public const string StpShip = "4";
        public const string StpBus = "5";
    }
    
    public static class ServiceCategory
    {
        public const string LondonUndergroundService = "OL";
        public const string UnadvertisedOrdinaryPassenger = "OU";
        public const string OrdinaryPassenger = "OO";
        public const string ChannelTunnel = "XC";
        public const string UnadvertisedExpress = "XU";
        public const string ExpressPassenger = "XX";
        public const string Sleeper = "XZ";
        public const string BusReplacement = "BR";
        public const string BusPermanent = "BS";
        public const string Ship = "SS";    // Not used?        
    }
}