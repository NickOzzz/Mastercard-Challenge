using Newtonsoft.Json;

namespace monarchs;

class Program
{
    public static async Task<int> Main()
    {

        var kingsDetails = await GetKings();

        Console.WriteLine($"1. Monarchs in the list: {kingsDetails.Count}");

        var kingProperties = kingsDetails.Select(king => new { king.Id, RulingYears = CalculateTheRulingPeriod(king.Yrs) });
        var sortedKingsByRulingYears = kingProperties.ToList().OrderBy(x => x.RulingYears);

        var mostServingMonarch = kingsDetails.Find(monarch => monarch.Id == sortedKingsByRulingYears.Last().Id) ?? throw new Exception("No most serving monarch found");
        var longestServingMonarchInYears = sortedKingsByRulingYears.Last();

        Console.WriteLine($"2. Monarch: {mostServingMonarch.Nm} ruled the longest. Ruled for amount of years: {longestServingMonarchInYears.RulingYears}");

        var orderedHouses = CountRepetitiveHouses(kingsDetails);
        var longestRulingHouse = orderedHouses.Last();

        Console.WriteLine($"3. House that ruled the longest: {longestRulingHouse.Key}. Ruled for amount of years: {longestRulingHouse.Value.RulingYears}");

        var sortedListByRepetitiveNames = CountRepetitiveNames(kingsDetails);
        var mostRepetitiveName = sortedListByRepetitiveNames.Last();

        Console.WriteLine($"4. The most repetitive name is {mostRepetitiveName.Key}. The name was repeated: {mostRepetitiveName.Value} times");

        return 0;
    }

    private static async Task<List<King>> GetKings() 
    {
        var baseAddres = "https://gist.githubusercontent.com/";
        var uriSuffix = "/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";
        var client = new HttpClient() { BaseAddress = new Uri(baseAddres, UriKind.RelativeOrAbsolute) };

        var response = await client.SendAsync(new HttpRequestMessage() { RequestUri = new Uri(uriSuffix, UriKind.RelativeOrAbsolute), Method = HttpMethod.Get });
        var responseContent = await response.Content.ReadAsStringAsync();

        var kingsDetails = JsonConvert.DeserializeObject<List<King>>(responseContent);

        return kingsDetails ?? throw new Exception("No kings found");
    }

    private static int CalculateTheRulingPeriod(string yearsOfRuling)
    {
        if (yearsOfRuling.Contains("-"))
        {
            var splittedYears = yearsOfRuling.Split("-");
            if (splittedYears.Count() > 1
                && !string.IsNullOrEmpty(splittedYears[0])
                && !string.IsNullOrEmpty(splittedYears[1]))
            {
                return Int32.Parse(yearsOfRuling.Split("-")[1]) - Int32.Parse(yearsOfRuling.Split("-")[0]);
            }
            else if (splittedYears.Count() > 1
                && string.IsNullOrEmpty(splittedYears[1])
                && !string.IsNullOrEmpty(splittedYears[0]))
            {
                return DateTime.UtcNow.Year - Int32.Parse(splittedYears[0]);
            }
            return 0;
        }
        return 1;
    }

    private static List<KeyValuePair<string, int>> CountRepetitiveNames(List<King> kingsDetails)
    {
        Dictionary<string, int> dictOfNames = new();
        foreach (var fullName in kingsDetails.Select(king => king.Nm))
        {
            var firstName = fullName.Split(" ")[0];
            if (!dictOfNames.TryGetValue(firstName, out var count))
            {
                dictOfNames[firstName] = 1;
            }
            else
            {
                dictOfNames[firstName] = count + 1;
            }
        }
        return dictOfNames.OrderBy(x => x.Value).ToList();
    }

    private static List<KeyValuePair<string, HouseProperties>> CountRepetitiveHouses(List<King> kings)
    {
        Dictionary<string, HouseProperties> dictOfKings = new();
        foreach (King king in kings)
        {
            if (!dictOfKings.TryGetValue(king.Hse, out HouseProperties houseObj))
            {
                dictOfKings[king.Hse] = new HouseProperties(1, CalculateTheRulingPeriod(king.Yrs));
            }
            else
            {
                dictOfKings[king.Hse] = new HouseProperties(houseObj.Counter + 1, houseObj.RulingYears + CalculateTheRulingPeriod(king.Yrs));
            }
        }
        return dictOfKings.OrderBy(x => x.Value.RulingYears).ToList();
    }

    private record HouseProperties(int Counter, int RulingYears);

    private record King(string Id, string Nm, string Cty, string Hse, string Yrs);
}
