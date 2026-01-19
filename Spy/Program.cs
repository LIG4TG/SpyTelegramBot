using System.ComponentModel;
using Spy;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// ========= LOCATIONS ======================
var clashRoyaleCards = new List<string>
{
    "Рыцарь",
    "Лучницы",
    "Гоблины",
    "Гигант",
    "П.Е.К.К.А",
    "Миньоны",
    "Воздушный шар",
    "Ведьма",
    "Варвары",
    "Голем",
    "Скелеты",
    "Валькирия",
    "Армия скелетов",
    "Подрывник",
    "Мушкетёр",
    "Младший дракон",
    "Принц",
    "Маг",
    "Мини П.Е.К.К.А",
    "Гоблины-копейщики",
    "Гигантский скелет",
    "Всадник на кабане",
    "Орда миньонов",
    "Ледяной маг",
    "Королевский гигант",
    "Стражи",
    "Принцесса",
    "Тёмный принц",
    "Три мушкетёра",
    "Лавовый пёс",
    "Ледяной дух",
    "Огненные духи",
    "Шахтёр",
    "Спарки",
    "Боулер",
    "Дровосек",
    "Таран",
    "Инферно-дракон",
    "Ледяной голем",
    "Мегаминьон",
    "Гоблин с дротиками",
    "Банда гоблинов",
    "Электромаг",
    "Элитные варвары",
    "Охотник",
    "Палач",
    "Бандитка",
    "Королевские рекруты",
    "Ночная ведьма",
    "Летучие мыши",
    "Королевский призрак",
    "Зэппи",
    "Хулиганы",
    "Тележка с пушкой",
    "Мега-рыцарь",
    "Бочка со скелетами",
    "Летательная машина",
    "Подрывники стен",
    "Королевские кабаны",
    "Гоблин-гигант",
    "Рыбак",
    "Магический лучник",
    "Электродракон",
    "Фейерверк",
    "Могучий шахтёр",
    "Эликсирный голем",
    "Боевой лекарь",
    "Король скелетов",
    "Золотой рыцарь",
    "Королева лучниц",
    "Монах",
    "Феникс",

    // Заклинания
    "Стрелы",
    "Огненный шар",
    "Молния",
    "Ракета",
    "Заморозка",
    "Яд",
    "Зеркало",
    "Клон",
    "Ярость",
    "Лечение",
    "Торнадо",
    "Бревно",
    "Снежок",
    "Землетрясение",
    "Вспышка",
    "Стрелы гоблинов",

    // Здания
    "Пушка",
    "Адская башня",
    "Башня-бомба",
    "Мортира",
    "Хижина варваров",
    "Хижина гоблинов",
    "Надгробие",
    "Хижина эликсира",
    "Печь",
    "Гоблинская клетка",
    "Башня Теслы",
    "Спавнер гоблинов",
    "Гоблинский бур"
};
Locations.AddLocation(
    new Location(
        id: 1,
        name: "Clash Royale",
        cards: clashRoyaleCards
    )
);


var botToken = "8314075691:AAHkdSQ_x1vF-lUaAA8RWZEae-vBAJ0_vWU";
var bot = new TelegramBotClient(botToken);

Console.WriteLine("Bot started");

using var cts = new CancellationTokenSource();

bot.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    cancellationToken: cts.Token
);

Console.ReadLine();
cts.Cancel();


// ================= UPDATE HANDLER =================

async Task HandleUpdateAsync(
    ITelegramBotClient botClient,
    Update update,
    CancellationToken cancellationToken)
{
    if (update.Type != UpdateType.Message)
        return;

    var message = update.Message;
    if (message == null || message.Type != MessageType.Text)
        return;

    var text = message.Text!.Trim();

    if (text.StartsWith("/start"))
        await HandleStart(botClient, message);

    else if (text == "/create")
        await HandleCreate(botClient, message);

    else if (text == "/lobby")
        await HandleLobby(botClient, message);
    else if (text == "/players")
        await HandlePlayers(botClient, message);
    else if (text == "/ready")
        await HandleStartGame(botClient, message);
}


// ================= COMMANDS =================

async Task HandleCreate(ITelegramBotClient botClient, Message message)
{
    var host = new Player
    {
        UserId = message.From!.Id,
        FirstName = message.From.FirstName,
        ChatId = message.Chat.Id
    };

    LobbyStorage.RemovePlayerFromAnyLobby(message.From!.Id);

    var lobby = new Lobby(host);
    LobbyStorage.Add(lobby);

    var inviteLink = $"https://t.me/hearsayogbot?start={lobby.LobbyId}";

    await botClient.SendMessage(
        chatId: message.Chat.Id,
        text: $"Лобби создано 🎉\n\nСсылка для входа:\n{inviteLink}"
    );
}

async Task HandleStart(ITelegramBotClient botClient, Message message)
{
    var parts = message.Text!.Split(' ');

    if (parts.Length == 1)
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Привет! Перейди по ссылке приглашения, чтобы войти в лобби."
        );
        return;
    }

    if (!Guid.TryParse(parts[1], out var lobbyId))
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Некорректная ссылка 😕"
        );
        return;
    }

    var lobby = LobbyStorage.Get(lobbyId);
    if (lobby == null)
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Лобби не найдено 😢"
        );
        return;
    }
    
    var player = new Player
    {
        UserId = message.From!.Id,
        FirstName = message.From.FirstName,
        ChatId = message.Chat.Id
    };
    
    LobbyStorage.RemovePlayerFromAnyLobby(player.UserId);
    
    if (!lobby.AddPlayer(player))
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Ты уже в этом лобби 🙂"
        );
        return;
    }
    
    await botClient.SendMessage(
        message.Chat.Id,
        $"Ты вошёл в лобби!\nИгроков: {lobby.Players.Count}"
    );

}


async Task HandleLobby(ITelegramBotClient botClient, Message message)
{
    var lobby = LobbyStorage.FindByPlayer(message.From!.Id);

    if (lobby == null)
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Ты не состоишь в лобби ❌"
        );
        return;
    }

    var playersText = string.Join(
        "\n",
        lobby.Players.Select(p =>
            p.UserId == lobby.HostId
                ? $"👑 {p.FirstName}"
                : $"• {p.FirstName}"
        )
    );

    await botClient.SendMessage(
        message.Chat.Id,
        $"Лобби:\n\n{playersText}"
    );
}

async Task HandlePlayers(ITelegramBotClient botClient, Message message)
{
    var lobby = LobbyStorage.FindByPlayer(message.From!.Id);

    if (lobby == null)
    {
        await botClient.SendMessage(
            message.Chat.Id,
            "Ты не состоишь в лобби ❌"
        );
        return;
    }

    var playersText = string.Join(
        "\n",
        lobby.Players.Select((p, index) =>
            p.UserId == lobby.HostId
                ? $"{index + 1}. {p.FirstName}👑"
                : $"{index + 1}. {p.FirstName}"
        )
    );

    await botClient.SendMessage(
        chatId: message.Chat.Id,
        text: $"👥 Участники лобби:\n\n{playersText}"
    );
}

async Task HandleStartGame(ITelegramBotClient botClient, Message message)
{
    var lobby = LobbyStorage.FindByPlayer(message.From!.Id);
    
    
    if (lobby.Current_Spy != null)
    {
        foreach (var player in lobby.Players)
        {
            if (player.IsSpy)
            {
                await botClient.SendMessage(
                    chatId: player.ChatId,
                    text: $"Локация была: {lobby.Current_Location}");
                    player.IsSpy = false;
            }
            else if (!player.IsSpy)
            {
                await botClient.SendMessage(
                    chatId: player.ChatId,
                    text: $"шпионом был: {lobby.Current_Spy.FirstName}");
            }
        }
        lobby.Current_Spy = null;
    }

    var spies = lobby.ChooseRandomSpies(lobby.Players, lobby.NumOfSpies);   
    var location = Locations.GetRandomLocation();
    string card = location.GetRandomCard();
    lobby.Current_Location = card;

    foreach (var player in spies)
    {
        player.IsSpy = true;
        lobby.Current_Spy = player;
        Console.WriteLine($"{player.UserId}, {player.FirstName}");
        
    }
    foreach (var player in lobby.Players)
    {

        if (player.IsSpy)
        {
            await botClient.SendMessage(
                chatId: player.ChatId,
                text: "Ты шпион/пидорас");
        }
        else
        {
            await botClient.SendMessage(
                chatId: player.ChatId,
                text: $"Локация: {card}");
        }
    }
    
    
}


// ================= ERROR =================

Task HandleErrorAsync(
    ITelegramBotClient botClient,
    Exception exception,
    CancellationToken cancellationToken)
{
    Console.WriteLine(exception);
    return Task.CompletedTask;
}


// ================= STORAGE =================

static class LobbyStorage
{
    private static readonly Dictionary<Guid, Lobby> _lobbies = new();

    public static void Add(Lobby lobby)
    {
        _lobbies[lobby.LobbyId] = lobby;
    }

    public static Lobby? Get(Guid lobbyId)
    {
        _lobbies.TryGetValue(lobbyId, out var lobby);
        return lobby;
    }

    public static Lobby? FindByPlayer(long userId)
    {
        return _lobbies.Values
            .FirstOrDefault(l => l.Players.Any(p => p.UserId == userId));
    }
    public static void RemovePlayerFromAnyLobby(long userId)
    {
        foreach (var lobby in _lobbies.Values.ToList())
        {
            if (lobby.RemovePlayer(userId))
            {
                // если хост ушёл или лобби пустое — удаляем
                if (!lobby.Players.Any() || lobby.HostId == userId)
                {
                    _lobbies.Remove(lobby.LobbyId);
                }

                return;
            }
        }
    }
}



public static class Locations
{
    private static readonly List<Location> _locations = new();

    public static void AddLocation(Location location)
    {
        _locations.Add(location);
    }

    public static Location GetRandomLocation()
    {
        int index = Random.Shared.Next(_locations.Count);
        return _locations[index];
    }
    
}




