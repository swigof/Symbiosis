using Backdash;
using Symbiosis;
using Symbiosis.Input;
using System;
using System.Net;

var inputSerializer = new StructBinarySerializer<PlayerInputs>();
var sessionBuilder = RollbackNetcode.WithInputType(t => t.Custom(inputSerializer));

Console.Write("(L)ocal, (R)emote, (C)ustom, or (T)esting: ");
var mode = Console.ReadLine().ToUpper();

if (mode.StartsWith("C") || mode.StartsWith("R")) 
{
    var port = 34345;
    Console.Write("Remote IP to connect to: ");
    var ip = Console.ReadLine();
    if (ip == "")
        ip = "127.0.0.1";
    var remoteAddress = ip + ":" + port.ToString();

    if (mode.StartsWith("C"))
    {
        Console.Write("Port to use: ");
        port = Convert.ToInt32(Console.ReadLine());
        Console.Write("Remote port to connect to: ");
        remoteAddress = ip + ":" + Console.ReadLine();
    }

    Console.Write("Connect as player 1 or 2: ");
    var player = Convert.ToInt32(Console.ReadLine());

    IPEndPoint endPoint;
    IPEndPoint.TryParse(remoteAddress, out endPoint);

    NetcodePlayer[] players;
    if (player == 1)
        players = [NetcodePlayer.CreateLocal(), NetcodePlayer.CreateRemote(endPoint)];
    else
        players = [NetcodePlayer.CreateRemote(endPoint), NetcodePlayer.CreateLocal()];

    sessionBuilder
        .WithPort(port)
        .WithPlayers(players)
        .ForRemote();
}
else if(mode.StartsWith("T"))
{
    sessionBuilder
        .ForSyncTest(options => options
            .UseJsonStateParser()
            .UseDesyncHandler<DiffPlexDesyncHandler>()
        );
}
else
{
    sessionBuilder
        .ForLocal();
}

using var game = new Game1(sessionBuilder.Build());
game.Run();
