using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExportHorseRaceData
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // set the date of the race
            string date = "2025-02-11";

            // csv output filename
            string outputPath = "racing_data.csv";

            // use HttpClient to interact to the API
            using var client = new HttpClient();

            // requesting a json string from the api
            var stringResponse = await client.GetStringAsync($"https://www.sportsbet.com.au/apigw/sportsbook-racing/Sportsbook/Racing/AllRacing/{date}");

            // parse the string to jsonobject
            var jsonData = JsonNode.Parse(stringResponse);

            var dates = jsonData["dates"]?.AsArray();

            // initialize csv generator
            using StreamWriter writer = new StreamWriter(outputPath);
            
            // setup a title of the column
            writer.WriteLine("MeetingDate,RaceName,RaceNumber,RaceTime");

            // start looping up to the 'events' list
            foreach (var d in dates)
            {
                foreach (var section in d["sections"]?.AsArray())
                {
                    foreach (var meeting in section["meetings"]?.AsArray())
                    {
                        // get the list of events
                        var listEvents = meeting["events"]?.AsArray();
                        foreach (var ev in listEvents)
                        {
                            string raceName = ev?["displayName"]?.ToString() ?? "N/A";
                            string raceNumber = ev?["raceNumber"]?.ToString() ?? "N/A";
                            int unixStartTime = int.Parse(ev?["startTime"]?.ToString());

                            // since the startTime uses unix timestamp, we need to convert it to DateTime object
                            DateTime raceDateTime = DateTimeOffset.FromUnixTimeSeconds(unixStartTime).DateTime;

                            // write the value to the csv file
                            writer.WriteLine($"{raceDateTime.ToString("yyyy-MM-dd")},{raceName},{raceNumber},{raceDateTime.ToString("hh:mm tt")}");

                        }
                    }
                }
            }

            // finally, retreive the full path of the csv file and print it to the console
            var fullPathResult = Path.GetFullPath(outputPath);
            Console.WriteLine("CSV File has been generated successfully.");
            Console.WriteLine($"Navigate to: {fullPathResult}");
        }
    }
}
