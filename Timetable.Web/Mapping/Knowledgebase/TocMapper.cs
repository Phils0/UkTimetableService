using NreKnowledgebase.SchemaV4;

namespace Timetable.Web.Mapping.Knowledgebase
{
    public static class TocMapper
    {
        public static Toc Map(TrainOperatingCompanyStructure toc)
        {
            return new Toc(toc.AtocCode, toc.Name);
        }
    }
}