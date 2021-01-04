using DarwinClient.SchemaV16;

namespace Timetable.Web.Test.Data
{
    public static class Darwin
    {
        public static TocRef Avanti => new TocRef()
            {
                toc = "VT",
                tocname = "Avanti West Coast",
                url = "http://www.nationalrail.co.uk/tocs_maps/tocs/VT.aspx"
            };

        public static LocationRef WaterlooMain => new LocationRef()
        {
            tpl = "WATRLMN",
            crs = "WAT",
            toc = "NR",
            locname = "London Waterloo",
        };
        
        public static LocationRef Waterloo => new LocationRef()
        {
            tpl = "WATRLOO",
            crs = "WAT",
            toc = "NR",
            locname = "London Waterloo",
        };
        
        public static LocationRef WaterlooWindsor => new LocationRef()
        {
            tpl = "WATRLOW",
            crs = "WAT",
            toc = "NR",
            locname = "London Waterloo",
        };
        
        public static LocationRef Vauxhall => new LocationRef()
        {
            tpl = "VAUXHAL",
            crs = "VXH",
            toc = "SW",
            locname = "Vauxhall",
        };
        
        public static PportTimetableRef Reference
        {
            get
            {
                return new PportTimetableRef()
                {
                    File = "PPTimetable/20201023020846_ref_v3.xml.gz",
                    timetableId = "20201023020846",
                    LocationRef = new []
                    {
                        new LocationRef()
                        {
                            tpl = "WATRINT",
                            crs = "WAT",
                            toc = "NR",
                            locname = "London Waterloo",
                        },
                        Waterloo,
                        WaterlooMain,
                        WaterlooWindsor,
                        new LocationRef()
                        {
                            tpl = "WATR",
                            crs = "WAT",
                            toc = "NR",
                            locname = "London Waterloo",
                        },
                        Vauxhall,
                        new LocationRef()
                        {
                            tpl = "VAUXHLM",
                            crs = "VXH",
                            toc = "SW",
                            locname = "Vauxhall",
                        },
                        new LocationRef()
                        {
                            tpl = "VAUXHLW",
                            crs = "VXH",
                            toc = "SW",
                            locname = "Vauxhall",
                        },
                        new LocationRef()
                        {
                            tpl = "SURBITN",
                            crs = "SUR",
                            toc = "SW",
                            locname = "Surbiton",
                        },
                        new LocationRef()
                        {
                            tpl = "GUILDFD",
                            crs = "GLD",
                            toc = "RT",
                            locname = "Guildford",
                        },
                        new LocationRef()
                        {
                            tpl = "WOKING",
                            crs = "WOK",
                            toc = "SW",
                            locname = "Woking",
                        }                    },
                    TocRef = new []
                    {
                        Avanti,
                        new TocRef()
                        {
                            toc = "CC",
                            tocname = "c2c",
                            url = "http://www.nationalrail.co.uk/tocs_maps/tocs/CC.aspx"
                        }
                    },
                    LateRunningReasons = new []
                    {
                        new Reason() { code = 100, reasontext = "This train has been delayed by a broken down train"},
                        new Reason() { code = 101, reasontext = "This train has been delayed by a delay on a previous journey"},
                        new Reason() { code = 501, reasontext = "This train has been delayed by a broken down train"},
                        new Reason() { code = 502, reasontext = "This train has been delayed by a broken windscreen on the train"},
                        new Reason() { code = 919, reasontext = "This train has been delayed by misuse of a level crossing"},
                    },
                    CancellationReasons = new []
                    {
                        new Reason() { code = 100, reasontext = "This train has been cancelled by a broken down train"},
                        new Reason() { code = 101, reasontext = "This train has been cancelled by a delay on a previous journey"},
                        new Reason() { code = 501, reasontext = "This train has been cancelled by a broken down train"},
                        new Reason() { code = 502, reasontext = "This train has been cancelled by a broken windscreen on the train"},
                        new Reason() { code = 919, reasontext = "This train has been cancelled by misuse of a level crossing"},
                    },
                    Via = new []
                    {
                        new Via()
                        {
                            at = "SUR",
                            dest = "BRGHTN",
                            loc1 = "BSNGSTK",
                            loc2 = null,
                            viatext = "via Basingstoke "
                        },
                        new Via()
                        {
                            at = "SUR",
                            dest = "GUILDFD",
                            loc1 = "CBHMSDA",
                            loc2 = null,
                            viatext = "via Cobham"
                        },
                        new Via()
                        {
                            at = "SUR",
                            dest = "GUILDFD",
                            loc1 = "WOKING",
                            loc2 = null,
                            viatext = "via Woking "
                        },                    
                        new Via()
                        {
                            at = "SUR",
                            dest = "PHBR",
                            loc1 = "BSNGSTK",
                            loc2 = null,
                            viatext = "via Basingstoke "
                        },                     
                        new Via()
                        {
                            at = "SUR",
                            dest = "PSEA",
                            loc1 = "BSNGSTK",
                            loc2 = null,
                            viatext = "via Basingstoke "
                        },
                        new Via()
                        {
                            at = "WAT",
                            dest = "GUILDFD",
                            loc1 = "RICHMND",
                            loc2 = "ALDRSHT",
                            viatext = "via Richmond \u0026 Aldershot"
                        },
                        new Via()
                        {
                            at = "WAT",
                            dest = "GUILDFD",
                            loc1 = "ASCOT",
                            loc2 = null,
                            viatext = "via Ascot"
                        },
                        new Via()
                        {
                            at = "WAT",
                            dest = "GUILDFD",
                            loc1 = "EPSM",
                            loc2 = null,
                            viatext = "via Epsom"
                        },
                        new Via()
                        {
                            at = "WAT",
                            dest = "GUILDFD",
                            loc1 = "CBHMSDA",
                            loc2 = null,
                            viatext = "via Cobham"
                        },
                        new Via()
                        {
                            at = "WAT",
                            dest = "GUILDFD",
                            loc1 = "WOKING",
                            loc2 = null,
                            viatext = "via Woking"
                        },
                    },
                    CISSource = new []
                    {
                        new CISSource() {code = "AM01", name = "Southern Metropolitan"}, 
                        new CISSource() {code = "AM02", name = "Southern Suburban"}, 
                        new CISSource() {code = "AMO1", name = "Southern Metropolitan  (Legacy)"}, 
                        new CISSource() {code = "AMO2", name = "Southern Suburban  (Legacy)"},  
                        new CISSource() {code = "at07", name = "East Midlands"},  
                    }
                };
            }
        }
    }
}