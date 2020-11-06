using System;
using NreKnowledgebase.SchemaV4;

namespace Timetable.Web.Test.Knowledgebase
{
    public static class TestTocs
    {
        public static TrainOperatingCompanyList Tocs
        {
            get
            {
                var tocs = new TrainOperatingCompanyList();
                tocs.TrainOperatingCompany = new[]
                {
                    new TrainOperatingCompanyStructure()
                    {
                        AtocCode = "VT",
                        Name = "Avanti West Coast",
                        OperatingPeriod = new OperatingPeriodStructure()
                        {
                            StartDate = new DateTime(2019, 12, 08)
                        }
                    },
                    new TrainOperatingCompanyStructure()
                    {
                        AtocCode = "VT",
                        Name = "Virgin Trains",
                        OperatingPeriod = new OperatingPeriodStructure()
                        {
                            StartDate = new DateTime(2018, 12, 08),
                            EndDate = new DateTime(2019, 12, 07)
                        }
                    },
                    new TrainOperatingCompanyStructure()
                    {
                        AtocCode = "CC",
                        Name = "c2c",
                        OperatingPeriod = new OperatingPeriodStructure()
                        {
                            StartDate = new DateTime(2005, 5, 18),
                            EndDate = new DateTime(2029, 11, 10)
                        }
                    }
                };
                return tocs;
            }
        }
    }
}