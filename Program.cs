using Backdash;
using Symbiosis;

var session = RollbackNetcode.WithInputType<PlayerInputs>().ForLocal().Build();
using var game = new Game1(session);
game.Run();
