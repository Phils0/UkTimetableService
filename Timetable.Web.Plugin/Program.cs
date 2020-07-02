using System;
using Serilog;

namespace Timetable.Web
{
    /// <summary>
    /// Dummy Main method, required due to referencing Microsoft.NET.Sdk.Web
    /// </summary>
    /// <remarks>
    /// Reference Microsoft.NET.Sdk.Web instead of Microsoft.NET.Sdk
    /// to avoid reference problems required in IPlugin
    /// </remarks>
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Fatal("Dummy Main.  Do not run");
            return 0;
        }
    }
}
