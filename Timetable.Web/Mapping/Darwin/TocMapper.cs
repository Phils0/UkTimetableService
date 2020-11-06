using DarwinClient.SchemaV16;

namespace Timetable.Web.Mapping.Darwin
{
    public static class TocMapper
    {
        public static Toc Map(TocRef toc)
        {
            return new Toc(toc.toc, toc.tocname)
            {
                NationalRailUrl = toc.url
            };
        }
    }
}