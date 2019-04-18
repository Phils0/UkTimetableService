using System.Threading;

namespace Timetable.Web
{
    internal class Sequence
    {
        private int _nextId = 0;

        public int GetNext()
        {
            return Interlocked.Increment(ref _nextId);
        }
    }
}
