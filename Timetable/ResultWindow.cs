namespace Timetable
{
    /// <summary>
    /// How many services to return either side of the search pivot: up to <see cref="Before"/> before it and up to
    /// <see cref="After"/> at/after it. 
    /// </summary>
    public readonly struct ResultWindow
    {
        public int Before { get; }
        public int After { get; }

        public ResultWindow(int before, int after)
        {
            // Always request at least one service
            if (before == 0 && after == 0)
                after = 1;

            Before = before;
            After = after;
        }
    }
}
