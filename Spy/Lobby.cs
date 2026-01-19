namespace Spy;

public class Lobby
{
    public Guid LobbyId { get; }
    public List<Player> Players { get; set; } = new();
    public long HostId { get; set; }
    public string Current_Location { get; set; } = "";
    public Player Current_Spy { get; set; } = null;
    public int Location { get; private set; } = 1;
    public int NumOfSpies { get; private set; } = 1;
    // создание лобби
    public Lobby(Player host)
    {
        LobbyId = Guid.NewGuid();
        HostId = host.UserId;
        Players.Add(host);
    }
    //добавить игрока
    public bool AddPlayer(Player player)
    {
        if (Players.Any(p => p.UserId == player.UserId)) return false;
        Players.Add(player);
        return true;
    }
    //кикать игрока
    
    public bool KickPlayer(long requesterId, long targetId)
    {
        if (requesterId != HostId)
            return false;

        if (targetId == HostId)
            return false;

        return Players.RemoveAll(p => p.UserId == targetId) > 0;
    }
    public bool RemovePlayer(long userId)
    {
        return Players.RemoveAll(p => p.UserId == userId) > 0;
    }

    public bool SetSpies(int count)
    {
        if (count < 1 || count >= Players.Count) return false;

        NumOfSpies = count;
        return true;
    }
    public bool SetLocation(int location)
    {
        Location = location;
        return true;
    }
    
    public bool StartGame(long requesterId)
    {
        if (requesterId != HostId) return false;
        if (Players.Count < 3) return false;
        return true;
    }
    public List<Player> ChooseRandomSpies(List<Player> players, int numOfSpies)
    {
        if (players == null || players.Count <= numOfSpies )
            throw new InvalidOperationException("Некорректное количество шпионов");

        return players
            .OrderBy(_ => Random.Shared.Next())
            .Take(numOfSpies)
            .ToList();
    }
    
}