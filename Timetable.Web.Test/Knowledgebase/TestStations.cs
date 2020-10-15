using System;
using NreKnowledgebase.SchemaV4;

namespace Timetable.Web.Test.Knowledgebase
{
    public static class TestStations
    {
        public static StationList Stations
        {
            get
            {
                var tocs = new StationList();
                tocs.Station = new[]
                {
                    new StationStructure()
                    {
                        CrsCode = "WAT",
                        Name = "Waterloo",
                        AlternativeIdentifiers = new AlternativeIdentifiersStructure()
                        {
                            NationalLocationCode = "559800"
                        },
                        Longitude = new decimal(-0.113897),
                        Latitude = new decimal(51.503507)
                    },
                    new StationStructure()
                    {
                        CrsCode = "VXH",
                        Name = "Vauxhall",
                        AlternativeIdentifiers = new AlternativeIdentifiersStructure()
                        {
                            NationalLocationCode = "559700"
                        }
                    },
                    new StationStructure()
                    {
                        CrsCode = "SUR",
                        Name = "Surbiton",
                        AlternativeIdentifiers = new AlternativeIdentifiersStructure()
                        {
                            NationalLocationCode = "557100"
                        }
                    }
                };
                return tocs;
            }
        }
    }
}