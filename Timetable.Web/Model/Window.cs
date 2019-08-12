using System;

namespace Timetable.Web.Model
{
    public class Window
    {
        /// <summary>
        /// Request time for board
        /// </summary>
        public DateTime At { get; set; }
        /// <summary>
        /// Number of services arriving\departing before
        /// </summary>
        public ushort Before { get; set; }
        /// <summary>
        /// Number of services arriving\departing after
        /// </summary>
        public ushort After { get; set; }

        public override string ToString()
        {
            return At.ToString("s");
        }      
    }
}