using Backdash;
using Symbiosis;
using Symbiosis.Input;
using System;
using System.Net;

var inputSerializer = new StructBinarySerializer<PlayerInputs>();
var sessionBuilder = RollbackNetcode.WithInputType(t => t.Custom(inputSerializer));

Console.Write("(L)ocal, (R)emote, or (C)ustom: ");
var mode = Console.ReadLine().ToUpper();

if (mode.StartsWith("C") || mode.StartsWith("R")) 
{
    var port = 34345;
    var remoteAddress = "";
    if (mode.StartsWith("C"))
    {
        Console.Write("Port to use: ");
        port = Convert.ToInt32(Console.ReadLine());
        Console.Write("Remote IP and port to connect to: ");
        remoteAddress = Console.ReadLine();
    }
    else
    {
        Console.Write("Remote IP to connect to: ");
        remoteAddress = Console.ReadLine() + ":" + port.ToString();
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
else
{
    sessionBuilder
        .ForLocal();
}

using var game = new Game1(sessionBuilder.Build());
game.Run();
