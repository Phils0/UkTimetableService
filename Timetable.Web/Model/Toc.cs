namespace Timetable.Web.Model
{
    /// <summary>
    /// Train Operation Company
    /// </summary>
    public class Toc
    {
        /// <summary>
        /// RDG Toc Service
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// NRE url
        /// </summary>
        public string NationalRailUrl { get; set; }
    }
}