using Backdash;
using Symbiosis;
using Symbiosis.Input;

var inputSerializer = new StructBinarySerializer<PlayerInputs>();
var sessionBuilder = RollbackNetcode.WithInputType(t => t.Custom(inputSerializer)).ForLocal();
using var game = new Game1(sessionBuilder.Build());
game.Run();
