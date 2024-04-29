using System.Net.Http.Json;


internal class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly List<Root> results = new List<Root>();  
    private static int pendingOperations = 0;
    private static readonly ManualResetEvent doneEvent = new ManualResetEvent(false);

    private async static Task Main(string[] args)
    {
        client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
        var cities = new[] { "Lille", "Paris", "Marseille", "xuxuyolo" };

        pendingOperations = cities.Length;

        foreach (var city in cities)
        {
            ThreadPool.QueueUserWorkItem(_ => GetWeatherAsync(city).Wait());
        }

        doneEvent.WaitOne(); 

        foreach (var data in results)
        {
            if (data != null)
            {
                Console.WriteLine($"City: {data.Location.Name}, Temperature: {data.Current.Temp_C}C");
            }
        }

        Console.ReadLine();
    }

    private static async Task GetWeatherAsync(string city)
    {
        int maxRetries = 3;
        int attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                string apiKey = "0cb155a91c6c448c99e122516242904"; 
                string requestUri = $"current.json?q={city}&key={apiKey}";
                var response = await client.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Root>();
                    lock (results)
                    {
                        results.Add(result);
                    }
                    break;
                }
                else
                {
                    Console.WriteLine($"Erreur lors de l'optention de la ville : {city}. Status Code: {response.StatusCode}. On Rééssaye Chef...");
                    attempts++;
                    await Task.Delay(1000); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception lors de la récupération des données météo pour {city} : {e.Message}. Nouvelle tentative...");
                attempts++;
                await Task.Delay(1000);
            }
        }

        if (attempts == maxRetries)
        {
            Console.WriteLine($"Échec de la récupération des données pour {city} après {maxRetries} tentatives.");
        }

        if (Interlocked.Decrement(ref pendingOperations) == 0)
        {
            doneEvent.Set(); 
        }
    }
}



public class Root
{
    public Location Location { get; set; }
    public Current Current { get; set; }
}

public class Location
{
    public string Name { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Tz_Id { get; set; }
    public long Localtime_Epoch { get; set; }
    public string Localtime { get; set; }
}

public class Current
{
    public long Last_Updated_Epoch { get; set; }
    public string Last_Updated { get; set; }
    public double Temp_C { get; set; }
    public double Temp_F { get; set; }
    public int Is_Day { get; set; }
    public Condition Condition { get; set; }
    public double Wind_Mph { get; set; }
    public double Wind_Kph { get; set; }
    public int Wind_Degree { get; set; }
    public string Wind_Dir { get; set; }
    public double Pressure_Mb { get; set; }
    public double Pressure_In { get; set; }
    public double Precip_Mm { get; set; }
    public double Precip_In { get; set; }
    public int Humidity { get; set; }
    public int Cloud { get; set; }
    public double Feelslike_C { get; set; }
    public double Feelslike_F { get; set; }
    public double Vis_Km { get; set; }
    public double Vis_Miles { get; set; }
    public double Uv { get; set; }
    public double Gust_Mph { get; set; }
    public double Gust_Kph { get; set; }
}

public class Condition
{
    public string Text { get; set; }
    public string Icon { get; set; }
    public int Code { get; set; }
}

