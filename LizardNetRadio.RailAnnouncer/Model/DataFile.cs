namespace LizardNetRadio.RailAnnouncer.Model;

public class DataFile
{
    public string BasePath { get; set; } = string.Empty;
    public Dictionary<string, ItemSet> ItemSets { get; set; } = new Dictionary<string, ItemSet>();
    
    public List<Station> Stations { get; set; } = [];
    
    public List<Carriage> Carriages { get; set; } = [];
    public List<CarriageSubset> CarriageSubsets { get; set; } = [];
}