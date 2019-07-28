using System.IO;
using Microsoft.Extensions.Configuration;

namespace Timetable.Web
{
    public interface ILoaderConfig
    {
        string TimetableArchiveFile { get; }
    }
    
    internal class LoaderConfig : ILoaderConfig
    {
        private readonly IConfiguration _appSettings;

        internal LoaderConfig(IConfiguration appSettings)
        {
            _appSettings = appSettings;
        }
        
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
            return $"{TimetableArchiveFile}";
        }
    }
}