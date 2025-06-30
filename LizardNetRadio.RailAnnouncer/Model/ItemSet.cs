namespace LizardNetRadio.RailAnnouncer.Model;

using WeightedRandomLibrary;

public class ItemSet
{
    public List<ItemSetOption> Options { get; set; } = [];

    public string GetRandom(Random rnd, string? grammar = null)
    {
        if (grammar == null)
        {
            var forGrammar = this.Options.SelectMany(o => o.Files).ToList();

            return forGrammar[rnd.Next(forGrammar.Count)];
        }
        else
        {
            var forGrammar = this.Options
                .Where(o => o.Grammar == grammar || o.Grammar == null)
                .SelectMany(o => o.Files)
                .ToList();
            return forGrammar[rnd.Next(forGrammar.Count)];
        }
    }

    public string? PickWeightedGrammar(Random rnd)
    {
        var weights = this.Options.Select(x => new Option<string?>(x.Grammar, x.Files.Count)).ToArray();
        var weightedRandom = new WeightedRandomizer<string?>(weights, rnd.Next());
        return weightedRandom.Next().Value;
    }
}