namespace Timetable.Web
{
    public static class ServiceConfigurationFactory
    {
        public  static Model.Configuration Create(string dataArchive)
        {
            var t = typeof(ServiceConfigurationFactory);
            return new Model.Configuration()
            {
                // ReSharper disable once PossibleNullReferenceException
                Version = t.Assembly.GetName().Version.ToString(),
                Data = dataArchive
            };
        }
    }
}