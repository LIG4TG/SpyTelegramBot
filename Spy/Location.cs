public class Location
{
    public int Id { get; }
    public string Name { get; }
    public IReadOnlyList<string> Cards { get; }

    public Location(int id, string name, List<string> cards)
    {
        Id = id;
        Name = name;
        Cards = cards;
    }

    public string GetRandomCard()
    {
        int index = Random.Shared.Next(Cards.Count);
        return Cards[index];
    }
}
