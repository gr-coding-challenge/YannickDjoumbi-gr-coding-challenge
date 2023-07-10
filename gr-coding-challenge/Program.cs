using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

public class SessionSummary
{
    public string SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Duration { get; set; }
    public bool ReturnedLate { get; set; }
    public bool DamagedOnReturn { get; set; }
}

public class Session
{
    [JsonProperty]
    public string Type { get; set; }
    [JsonProperty]
    public string Id { get; set; }
    [JsonProperty]
    public string Timestamp { get; set; }
    [JsonProperty]
    public string Comments { get; set; }

}


class Program
{
    static List<Session> GetAllDuplicates()
    {
        var result = new List<Session>();

        // JSON input and added records to handle large data
        string data = @"[
        {
          ""type"": ""START"",
          ""id"": ""ABC123"",
          ""timestamp"": ""1681722000"",
          ""comments"": ""No issues - brand new and shiny!""
        },
        {
          ""type"": ""END"",
          ""id"": ""ABC123"",
          ""timestamp"": ""1681743600"",
          ""comments"": ""Car is missing both front wheels!""
        },
        {
          ""type"": ""START"",
          ""id"": ""ABC456"",
          ""timestamp"": ""1680343200"",
          ""comments"": ""Small dent on passenger door""
        },
        {
          ""type"": ""END"",
          ""id"": ""1680382800"",
          ""timestamp"": ""0123499"",
          ""comments"": """"
        },
        {
          ""type"": ""START"",
          ""id"": ""168"",
          ""timestamp"": ""0123499"",
          ""comments"": """"
        },
        {
          ""type"": ""END"",
          ""id"": ""168"",
          ""timestamp"": ""0333499"",
          ""comments"": """"
        }

        ]";

        //Deserialize the json data to a list
        var listObj = JsonConvert.DeserializeObject<List<Session>>(data);

        //Define objects to be used 
        var sessionObj = new SessionSummary();
        var session1 = new Session();
        var session2 = new Session();

        //find duplicates and define the sessions
        var duplicates = listObj.GroupBy(x => x.Id).Where(y => y.Count() > 1).SelectMany(session => session).ToList();
        foreach (var duplicate in duplicates)
        {
            result.Add(duplicate);
        }

        return result;
    }

    static List<SessionSummary> GetAllSessions()
    {
        List<SessionSummary> result = new List<SessionSummary>();

        var getDuplicatesOp = GetAllDuplicates();

        //iterate over the data to map the session fields
        if (getDuplicatesOp.Count > 0)
        {
            for (int i = 0; i < getDuplicatesOp.Count; i = i + 2)
            {
                var session1 = getDuplicatesOp[i];
                var session2 = getDuplicatesOp[i + 1];

                var sessionObj = new SessionSummary();
                sessionObj.SessionId = session1.Id;
                sessionObj.StartTime = DateTimeOffset.FromUnixTimeSeconds(int.Parse(session1.Timestamp)).LocalDateTime;
                sessionObj.EndTime = DateTimeOffset.FromUnixTimeSeconds(int.Parse(session2.Timestamp)).LocalDateTime;
                sessionObj.Duration = (sessionObj.EndTime - sessionObj.StartTime).TotalHours;

                if (sessionObj.Duration > 24)
                {
                    sessionObj.ReturnedLate = true;
                }

                if (!string.IsNullOrWhiteSpace(session2.Comments))
                {
                    sessionObj.DamagedOnReturn = true;
                }
                result.Add(sessionObj);
            }
        }
        else
        {
            Console.WriteLine("No Duplicate Records found");
        }

        return result;
    }

    static void Main()
    {
        var result = GetAllSessions();

        foreach (var record in result)
        {
            //display the output to the console
            Console.WriteLine($"The session id is: {record.SessionId}");
            Console.WriteLine($"The session start time: {record.StartTime}");
            Console.WriteLine($"The session end time: {record.EndTime}");
            Console.WriteLine($"The session duration: {record.Duration} hours ");
            Console.WriteLine($"The Car was returned late: {record.ReturnedLate}");
            Console.WriteLine($"The Car is damaged: {record.DamagedOnReturn}");
            Console.WriteLine("----------------------------------------------");
        }
    }
}
