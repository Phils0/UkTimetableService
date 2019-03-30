namespace Timetable.Web.Model
{
    /// <summary>
    /// Location coordinates
    /// </summary>
    public struct Coordinates
    {
        /// <summary>
        /// OS Easting value
        /// </summary>
        public int Eastings;
        /// <summary>
        /// ES Nothings value
        /// </summary>
        public int Northings;
        /// <summary>
        /// Estimated location
        /// </summary>
        public bool IsEstimate;

        public override string ToString()
        {
            return IsEstimate ? $"{Eastings},{Northings} (E)" :  $"{Eastings},{Northings}";
        }
    }
}