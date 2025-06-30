namespace LizardNetRadio.RailAnnouncer.Model;

using WeightedRandomLibrary;

public class Carriage
{
    public string File { get; set; }
    public int Weight { get; set; }
    public int Number { get; set; }
    public Option<Carriage> Option => new Option<Carriage>(this, this.Weight);
}