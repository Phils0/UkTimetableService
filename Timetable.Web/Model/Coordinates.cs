namespace Timetable.Web.Model
{
    /// <summary>
    /// Longitude/Latitude coordinates
    /// </summary>
    public class Coordinates
    {
        /// <summary>
        /// Longitude value
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Latitude value
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Estimated location
        /// </summary>
        public bool IsEstimate;

        public override string ToString()
        {
            return IsEstimate ? $"{Longitude},{Latitude} (E)" :  $"{Longitude},{Latitude}";
        }
    }
}