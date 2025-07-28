using Backdash;
using Symbiosis;
using Symbiosis.Input;
using System;
using System.Net;

var inputSerializer = new StructBinarySerializer<PlayerInputs>();
var sessionBuilder = RollbackNetcode.WithInputType(t => t.Custom(inputSerializer));

Console.Write("(L)ocal or (R)emote: ");
var mode = Console.ReadLine();

if (mode.ToUpper().StartsWith("R")) {
    Console.Write("Port to use: ");
    var port = Convert.ToInt32(Console.ReadLine());
    Console.Write("Remote IP and port to connect to: ");
    var remoteAddress = Console.ReadLine();
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
