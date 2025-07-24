using Backdash;
using Symbiosis;
using System;
using System.Net;

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

var session = RollbackNetcode
        .WithInputType<PlayerInputs>()
        .WithPort(port)
        .WithPlayers(players)
        .ForRemote()
        .Build();

using var game = new Game1(session);
game.Run();
