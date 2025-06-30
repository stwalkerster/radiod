namespace LizardNetRadio.RailAnnouncer;

using LizardNetRadio.RailAnnouncer.Model;

public static class Extensions
{
    public static Station GetRandomStation(this DataFile data, Random rnd)
    {
        if (data.Stations.Count == 0)
        {
            throw new InvalidOperationException("No stations available in the data file.");
        }
        
        return data.Stations[rnd.Next(data.Stations.Count)];
    }
}