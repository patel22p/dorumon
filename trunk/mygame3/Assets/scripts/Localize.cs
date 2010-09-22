public class Localize
{
        
    public LcString ipaddress = "Ipaddress:   port:5300";
    public LcString mustlogin = "You must login in first to play multiplayer";
    public LcString timelimit = "You can play in multiplayer only above specified time";
    public LcString firstname = "Enter username first";
    public LcString connectingto = "connecting to ";
    public LcString gameInstalled = "Game Installed";
    public LcString addfr = "Add Friend";
    public LcString playgame = " wants to play with u in Physx Wars Game";
    public LcString name = "Name: ";
    public LcString status = "Status:";
    public LcString refresh = "refresh friend list";
    public LcString scoreboard = "Score Board";
    public LcString chat = "Chat";
    public LcString logout = "Log Out";
    public LcString usersonline = "Users Online: ";
    public LcString vkontakte = "Vkontakte";
    public LcString vkconnected = "vkontakte connected :240";
    public LcString auth = "Authorization at Vkontakte";
    public LcString authhelp = "1. press Authorization 2. copy url from browser 3. paste url here";
    public LcString connect = "Connect";    
    public LcString time = "beta test start at {0}:00 end at {1}:00, time left {2} hours";
    public LcString loginasguest = "login as guest";
    public LcString guest = "Guest ";
    public LcString physxwarsver = "Physx Wars Версия ";
    public LcString url = "Url:";
    public LcString login = "Login";
    public LcString MasterServer = "MasterServer:";
    public LcString enternickname = "Enter Nick Name";
    public LcString yourversionis = "your version is: ";
    public LcString refreshserver = "refresh server list";
    public LcString version = " Version ";
    public LcString ping = "ping:";
    public LcString copyip = "Copy IP";
    public LcString copyright = "Copyright © 2010 Левочкин игорь";
    public LcString beginthegame = "Начать игру";
    public LcString fraglimit = "Frags Limit:";
    public LcString redteamscore = "Red Team Score:";
    public LcString blueteamscore = "Blue Team Score:";
    public LcString spectator = "Spectator";
    public LcString joingame = "Join Game";
    public LcString redteam = "Red Team";
    public LcString blueteam = "Blue Team";
    public LcString zombiefrag = "Zombie/Frag limit:";
    public LcString deathmatch = "Death Match";
    public LcString teamdeathmatch = "Team Death Match";
    public LcString teamzombiesurive = "Team Zombie Survive";
    public LcString zombisr = "Zombie Survive";
    public LcString startgame = "Start Game";
    public LcString notall = "not all users loaded map";
    public LcString fps = "фпс: ";
    public LcString quality = "quality:",
        fastest = "casc", fast = "fast", simple = "simple", good = "good", beateful = "beateful", fantastic = "fantastic";
    public LcString setquality = "Set Quality", setres = "screen resolution:";
    public LcString options = "Options", xof = "Camera X Offset:", yof = "Camera Y Offset:", camf = "Camera fieldOfView:";
    public LcString close = "close", lf = "Жизни: ", fg = "Фраги: ", ntr = "Нитро: ", fz = "FrozenTime: ", stg = "Раунд: ", zl = "Осталось зомби: ", clnt = "Could not connect to server: ", gald = "Game Already Started"
        , clnm = "Could not connect to master server: ", vklr = "Login vkontakte failed, make sure you give the application all the rights\r\n", errn = "error message not sended to ", cerr = "chat message not sended", host = "host", disc = "Disconnect", kick = "Kick", kills = "Kills", deaths = "deaths", kicked = " kicked"
        , zkills = "ZKills", zdeaths = "ZDeaths", maploaded = "Map Loaded", playerdisc = "Player disconnected ", dfg = "Disconnected from game:", plj = "Player joined ",
        onload = @" Задача игры - уничтожать зомби
      WASD - движение,
      ПРОБЕЛ - тормоз,
      SHIFT - ускорение,
      F - использование техники,
      цифры 1-3 - выбор оружия,
      мышь - обзор,
      ЛКМ - огонь,
      ПКМ - меню.
";
//    alt enter - fullscreen
//tab - close/open console
//f - go in/out car
//shift - nitro
//a,s,d,w move keys
//1 - machinegun 
//2 - rocketlauncher
//3 - physxgun
//4 - healthgun
    public LcString connwin = "Connection";
    public LcString serv = "Servers";
    public LcString baz = "Базука";
    public LcString heathgun = "Регенератор";
    public LcString gunmini = "Миниган";
    public LcString physxgun = "Телекинез", onlin = "online", offline = "offline", tpz = "Top Zombie Kill", tk = "TopKill", sb = "Stats Board", kls = "kills", dths = "Deaths", cw = "Chat Window", scw = " Погиб", ff = " friendly fired ", kld = " killed ", dbsf = " died byself", udl = "Вы умерли дожив до {0} раунда";
    public LcString hostwind = "host settings";
    public LcString enablemusci = "Enable Music";
}
public class LcString
{
    public string rus = " ";
    public string eng = " ";
    public override string ToString()
    {
        return OptionsWindow.ruslang && rus != " " ? rus : eng;
    }
    public static implicit operator LcString(string str)
    {
        LcString lc = new LcString();
        lc.eng = str;
        return lc;
    }
}