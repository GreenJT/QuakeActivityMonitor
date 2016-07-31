# QuakeActivityMonitor
QuakeActivityMonitor is a desktop application which monitors and displays earthquake activity.

### Building and Running the application ###
1. Open and compile the project in Visual Studio 2015.
2. Ensure that the value for FileLocation in app.config contains the filepath for the worldcities csv.
3. The application can be run using Visual Studio or by opening the executable located in the bin folder.
4. Once the application is run it will display earthquakes that have occurred in the past hour and will continue to monitor for seismic activity until closed by the user.
5. The application can be ended by closing the command line or pressing the any key.

### The Configuration File ###
Changing the values in app.config gives some flexibility as far as what and how information is gathered and displayed.

**Interval** - this value dictates the interval with which to query for recent seismic activity.

**File Location** - Complete file path of worldcities csv file.

**Base Address** - This is the base url for querying the USGS API.

**Method** - Dictates the method that will be used with the API.

**Format** - Response format from the API.

**Parameter** - Query method parameter sent to the API.

### Dependencies and References ###
* CsvHelper
* NewtonSoft.Json
* System
* System.Data
* System.Linq
* System.Timers
* System.Net.Http
* System.Configuration
* System.Threading.Tasks
* System.Device.Location
* System.Collections.Generic
