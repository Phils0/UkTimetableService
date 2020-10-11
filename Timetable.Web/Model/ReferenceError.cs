using System;

namespace Timetable.Web.Model
{
    public class ReferenceError
    {
        public DateTime GeneratedAt { get; } = DateTime.Now;
        /// <summary>
        /// Reason
        /// </summary>
        public string Reason { get;}

        public ReferenceError(string reason)
        {
            Reason = reason;
        }
    }
}