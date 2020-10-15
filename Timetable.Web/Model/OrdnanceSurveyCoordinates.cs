namespace Timetable.Web.Model
{
    /// <summary>
    /// Ordnance Survey coordinates
    /// </summary>
    public class OrdnanceSurveyCoordinates
    {
        /// <summary>
        /// OS Easting value
        /// </summary>
        public int Eastings { get; set; }
        /// <summary>
        /// ES Nothings value
        /// </summary>
        public int Northings { get; set; }
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