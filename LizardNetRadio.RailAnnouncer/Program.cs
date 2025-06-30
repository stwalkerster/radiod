namespace LizardNetRadio.RailAnnouncer;

using System.Diagnostics;
using FFMpegCore;
using LizardNetRadio.RailAnnouncer.Model;
using WeightedRandomLibrary;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

class Program
{
    static Random rnd = new Random();
    
    static void Main(string[] args)
    {
        string[] paTest = ["0156", "0157", "0158", "0159"];
        string[] displayTest = ["0049", "0050", "0051"];
        string[] evacuate = ["1522","1523"];
        
        var outputFile = "output.mp3";
        var dataFile = "data.yml";
        
        if (args.Length >= 2)
        {
            dataFile = args[0];
            outputFile = args[1];
        }
        
        // Read from the data.yml file into a new instance of the DataFile class using YAML deserialization
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlContent = File.ReadAllText(dataFile);
        var data = deserializer.Deserialize<DataFile>(yamlContent);

        var files = new List<string>();
        var probability = rnd.NextDouble();
        if (probability > 0.99)
        {
            files.AddRange(evacuate);
        }
        else if (probability > 0.97)
        {
            files.AddRange(paTest);
        }
        else if (probability > 0.96)
        {
            files.AddRange(displayTest);
        }
        else if (probability > 0.48)
        {
            files = GenerateNextTrainAnnouncement(data, outputFile);
        }
        else
        {
            files = GenerateApologyAnnouncement(data, outputFile);
        }
        
        ConcatenateFiles(files, data.BasePath, outputFile);

        if (args.Length == 0)
        {
            Process.Start("/usr/bin/cvlc", $"{outputFile} vlc://quit").WaitForExit();
        }
    }

    private static List<string> GenerateApologyAnnouncement(DataFile data, string outputFile)
    {
        var files = new List<string>();
        var hourGrammar = data.ItemSets["hours"].PickWeightedGrammar(rnd);

        files.Add(data.ItemSets["sorryToAnnounce"].GetRandom(rnd, hourGrammar));
        
        // Hour
        files.Add(data.ItemSets["hours"].GetRandom(rnd, hourGrammar));
        
        // Minute
        files.Add(data.ItemSets["minutes"].GetRandom(rnd));
        
        // TOC service to
        files.Add(data.ItemSets["tocServiceTo"].GetRandom(rnd));
        
        // Destination
        var destination = data.GetRandomStation(rnd);
        files.Add(destination.End);
        
        var nextDouble = rnd.NextDouble();
        if (nextDouble > 0.7)
        {
            files.Add(data.ItemSets["isDelayed"].GetRandom(rnd));
        }
        else
        {
            // has been cancelled
            files.Add("1533");
        }

        nextDouble = rnd.NextDouble();
        if (nextDouble > 0.3)
        {
            // due to
            files.Add("1528");
            
            files.Add(data.ItemSets["reason"].GetRandom(rnd));
        }

        // please listen for further announcements
        files.Add("1738");

        return files;
    }

    private static List<string> GenerateNextTrainAnnouncement(DataFile data, string outputFile)
    {
        var files = new List<string>();
        
        var hourGrammar = data.ItemSets["hours"].PickWeightedGrammar(rnd);

        
        // The next train at platform
        files.Add("0011");
        
        // Platform number
        files.Add(data.ItemSets["platformNumbers"].GetRandom(rnd));
        
        var delayed = rnd.NextDouble() > 0.7;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (delayed)
        {
            // Is the delayed
            files.Add("1549");
        }
        else
        {
            // Is the
            files.Add(data.ItemSets["isThe"].GetRandom(rnd, hourGrammar));    
        }
        
        // Hour
        files.Add(data.ItemSets["hours"].GetRandom(rnd, hourGrammar));
        
        // Minute
        files.Add(data.ItemSets["minutes"].GetRandom(rnd));
        
        // TOC service to
        files.Add(data.ItemSets["tocServiceTo"].GetRandom(rnd));
        
        // Destination
        var destination = data.GetRandomStation(rnd);
        files.Add(destination.End);

        // Calling at
        files.Add("1845");
        
        // Calling at stations
        var stationListCount = rnd.Next(10);
        var intermediates = new List<Station>();
        
        for (int i = 0; i < stationListCount; i++)
        {
            var randomStation = data.GetRandomStation(rnd);
            intermediates.Add(randomStation);
            files.Add(randomStation.Mid);
        }

        if (stationListCount > 0)
        {
            // and
            files.Add("1908");
            // Destination
            files.Add(destination.End);
        }
        else
        {
            // Destination
            files.Add(destination.Mid);
            // only
            files.Add("2362");
        }
        
        var formation = false;
        var shortPlatforms = false;
        Carriage? carriages = null;
        if (formation)
        {
            carriages = GenerateCarriageCount(data, files);

            shortPlatforms = GenerateShortPlatforms(data, carriages, intermediates, files);
        }

        var notCallingAt = false;
        if (!shortPlatforms && rnd.NextDouble() > 0.8)
        {
            notCallingAt = true;
            
            // please note that this train today will not call at
            files.Add("1508");
        
            var notCallingAtCount = rnd.Next(1, 6);
            var notCallingAtStations = new List<Station>();
            for (int i = 0; i < notCallingAtCount; i++)
            {
                var randomStation = data.GetRandomStation(rnd);
                
                if(intermediates.Contains(randomStation) || notCallingAtStations.Contains(randomStation))
                {
                    // If the station is already in the list, skip it
                    i--;
                    continue;
                }
                
                notCallingAtStations.Add(randomStation);
                
                if (i != 0 && i == notCallingAtCount - 1)
                {
                    // and
                    files.Add("1908");
                }
                
                files.Add(randomStation.Mid);
            }

            // due to
            files.Add("1528");
            
            files.Add(data.ItemSets["reason"].GetRandom(rnd));
        }

        // first class
        if (!shortPlatforms && formation && carriages?.Number > 4 && rnd.NextDouble() > 0.6)
        {
            // First class accommodation is situated towards the X
            files.Add(rnd.Next(1) == 0 ? "1505" : "1510");
            
            // of this train
            files.Add("0493");
        }
        
        // trolley
        if (rnd.NextDouble() > 0.7)
        {
            // A trolley service
            files.Add("0002");
            // of drinks and light refreshments
            files.Add("0084");
            // is available on this train
            files.Add("0351");
        }

        return files;
    }
    
    private static bool GenerateShortPlatforms(DataFile data, Carriage carriages, List<Station> intermediates, List<string> files)
    {
        var requiredCoaches = rnd.Next(4, 12);
        Console.WriteLine($"{carriages.Number}/{requiredCoaches} carriages, {intermediates.Count} intermediates");
        if (carriages.Number >= requiredCoaches && intermediates.Count > 1)
        {
            // This train will be calling at stations with short platforms
            files.Add("1514");
                
            // customers for
            files.Add("1298");
                
            // station list
            var shortPlatforms = new List<Station>(intermediates);
                
            var numShortPlatforms = rnd.Next(1, Math.Max(4, shortPlatforms.Count));
            for (int i = 0; i < numShortPlatforms; i++)
            {
                var station = shortPlatforms[rnd.Next(shortPlatforms.Count)];
                shortPlatforms.Remove(station);

                if (i != 0 && i == numShortPlatforms - 1)
                {
                    // and
                    files.Add("1908");
                }
                    
                files.Add(station.Mid);
            }
                
            // Please join the (front|middle|rear) X coaches of this train
            var validSubsets = data.CarriageSubsets.Where(x => x.Min <= carriages.Number).ToList();
            Console.WriteLine("Subsets: " + string.Join(',',validSubsets.Select(x => x.File)));
            var subset = validSubsets[rnd.Next(validSubsets.Count)];
                
            files.Add(subset.File);
            
            return true;
        }
        
        return false;
    }

    private static Carriage GenerateCarriageCount(DataFile data, List<string> files)
    {
        // This train is formed of
        files.Add("1162");
            
        // Carriage formation
        var weights = data.Carriages.Select(x => x.Option).ToArray();
        var weightedRandom = new WeightedRandomizer<Carriage>(weights, rnd.Next());
        var carriageCount = weightedRandom.Next().Value;

        files.Add(carriageCount.File);

        if (carriageCount.Number == 1)
        {
            // coach
            files.Add("0492");
        }
        else
        {
            // coaches
            files.Add("2213");
        }
        
        return carriageCount;
    }


    private static void ConcatenateFiles(IEnumerable<string> files, string basePath, string outputFile)
    {
        var fullPaths = files.Select(file => $"{basePath}{file}.mp3").ToArray();

        foreach (var path in fullPaths)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }
        }
        
        FFMpegArguments.FromConcatInput(fullPaths)
            .OutputToFile(outputFile, true)
            .ProcessSynchronously();
    }
}