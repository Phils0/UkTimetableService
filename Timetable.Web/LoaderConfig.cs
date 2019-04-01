using System.IO;
using Microsoft.Extensions.Configuration;

namespace Timetable.Web
{
    public interface ILoaderConfig
    {
        string TimetableArchiveFile { get; }
        bool IsRdgZip { get; }
    }
    
    internal class LoaderConfig : ILoaderConfig
    {
        private readonly IConfiguration _appSettings;

        internal LoaderConfig(IConfiguration appSettings)
        {
            _appSettings = appSettings;
        }
        
        public bool IsRdgZip => TimetableArchiveFile.Contains("ttis");    // Initially just simple file name check

        public string TimetableArchiveFile
        {
            get
            {
                var path = Path.Combine(@"Data", _appSettings["TimetableArchive"]);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }    

        public override string ToString()
        {
            return $"{TimetableArchiveFile}, IsRdgZip: {IsRdgZip}";
        }
    }
}