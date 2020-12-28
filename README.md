# UkTimetableService
A simple timetable service for GB rail using aspnetcore.

It loads a GB timetable in the CIF file format and provides a simple API to query.

[![Build Status](https://dev.azure.com/phils0oss/UkRailProjects/_apis/build/status/Phils0.UkTimetableService?branchName=master)](https://dev.azure.com/phils0oss/UkRailProjects/_build/latest?definitionId=2&branchName=master)

## API

* `/api/Timetable/service/C26193/2019-08-11`  - request a service for a date using the TimetableUID
* `/api/Timetable/retailService/GW622100/2019-07-15`   - request a service for a date using the Retail Service Id.  Can be either 4 or 6 digit RSID.  If 4 and has multiple services (generally associations) returns them all.
* `/api/Timetable/arrivals/SUR/2019-08-23T10:00:00`   - arrivals at a location around a specific date time
* `/api/Timetable/departures/SUR/2019-08-23T10:00:00`   - departures at a location around a specific date time
* `/api/Timetable/toc/GR/2019-07-30`   - services for a train operating company (toc) on a specific date

### Reference API

* `/api/Reference/location`   - locations grouped by CRS
* `/api/Reference/toc`   - tocs in timetable
* `/api/Reference/reasons/cancellation` - Darwin cancellation reasons
* `/api/Reference/reasons/late` - Darwin late reasons

The API has Swagger documentation which provides fuller details at `/swagger`

## Timetable Data

It loads a timetable in the CIF file format.  It can load an RDG timetable archive - zips named RJTTFnnn.ZIP and TTIS archives - zips named ttisnnn.zip where nnn is a 3 digit number.  It has a dependency on loading the Master Station List (MSN) file from these archives to get the set of locations. Therefore it currently does support the Network Rail version of the CIF which only has the timetable file. 

* RJTTFxxx.zip files are available from https://opendata.nationalrail.co.uk. Details on how to download it can be found at https://wiki.openraildata.com/index.php?title=DTD.

This repo has powershell scripts to download the data 
```bash
git clone https://github.com/phils0/RailOpenDataScripts
```
The data file needs to be placed in the `Timetable.Web\Data` directory

Edit `Timetable.Web/appsettings.json`, to set  `TimetableArchive` to match the name of the downloaded timetable file.

## Build and Test

Run `dotnet build` and `dotnet test` from the repos root directory (containing `TimetableService.sln`).

Alternatively open the solution `TimetableService.sln` in Visual Studio or Rider and it should just work.

### Dependencies

It has dependencies on various other rail related open source projects to load the various data files:

* CIF: https://github.com/Phils0/CifParser
* Darwin: https://github.com/Phils0/DarwinClient
* Knowledgebase: https://github.com/Phils0/NreKnowledgebaseClient
