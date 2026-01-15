namespace Spy;

public class Lobby
{
    public Guid LobbyId { get; set; }
    public List<Player> Players { get; set; }
    public int HostId { get; set; }
    public int Location { get; set; }
}