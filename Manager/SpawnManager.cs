using Microsoft.Xna.Framework;
using Symbiosis.Session;

namespace Symbiosis.Manager;

// Should only be used with a safely locked gamestate
public static class SpawnManager
{
    const int _roundDuration = 7200;
    const int _firstSpawnFrame = 60;
    const int _startDelay = 600;
    const int _maxSpawnFrame = (int)(_roundDuration * 0.8f);
    const float _framesPerEggEnemyStart = 600;
    const float _framesPerEggEnemyEnd = 300;
    const float _framesPerFrogEnemyStart = 300;
    const float _framesPerFrogEnemyEnd = 60;

    public static void Update(ref GameState gamestate)
    {
        if (gamestate.RoundFrame == _firstSpawnFrame)
        {
            SpawnEggEnemy(ref gamestate);
            SpawnFrogEnemy(ref gamestate);
        }
        
        if (gamestate.RoundFrame < _startDelay || gamestate.RoundFrame > _roundDuration) return;

        var lerpValue = (float)gamestate.RoundFrame / _maxSpawnFrame;
        var eggEnemyDelay = MathHelper.Lerp(_framesPerEggEnemyStart, _framesPerEggEnemyEnd, lerpValue);
        var frogEnemyDelay = MathHelper.Lerp(_framesPerFrogEnemyStart, _framesPerFrogEnemyEnd, lerpValue);

        if (gamestate.RoundFrame - gamestate.LastEggEnemySpawn > eggEnemyDelay)
            SpawnEggEnemy(ref gamestate);
        if (gamestate.RoundFrame - gamestate.LastFrogEnemySpawn > frogEnemyDelay)
            SpawnFrogEnemy(ref gamestate);
    }

    private static bool SpawnEggEnemy(ref GameState gamestate)
    {
        if (gamestate.NextEggEnemyIndex != -1)
        {
            gamestate.Clusters[gamestate.NextEggEnemyIndex].Init(GetRandomPositionJustOutsideScreen(16));
            gamestate.NextEggEnemyIndex = GetNextEggEnemyIndex(gamestate);
            gamestate.LastEggEnemySpawn = gamestate.RoundFrame;
            return true;
        }
        return false;
    }
    
    private static bool SpawnFrogEnemy(ref GameState gamestate)
    {
        if (gamestate.NextFrogEnemyIndex != -1)
        {
            gamestate.FrogEnemies[gamestate.NextFrogEnemyIndex].Init(GetRandomPositionJustOutsideScreen(64));
            gamestate.NextFrogEnemyIndex = GetNextFrogEnemyIndex(gamestate);
            gamestate.LastFrogEnemySpawn = gamestate.RoundFrame;
            return true;
        }
        return false;
    }

    private static int GetNextEggEnemyIndex(GameState gamestate)
    {
        for (var i = 0; i < gamestate.Clusters.Length; i++)
        {
            if (!gamestate.Clusters[i].Active)
                return i;
        }
        return -1;
    }

    private static int GetNextFrogEnemyIndex(GameState gamestate)
    {
        for (var i = 0; i < gamestate.FrogEnemies.Length; i++)
        {
            if (!gamestate.FrogEnemies[i].Active)
                return i;
        }
        return -1;
    }

    private static Vector2 GetRandomPositionJustOutsideScreen(int padding)
    {
        var width = Game1.ScreenBounds.Width + padding;
        var height = Game1.ScreenBounds.Height + padding;
        var perimeterPoint = Game1.Random.NextInt(0, width * 2 + height * 2);
        if (perimeterPoint < width + height)
        {
            if (perimeterPoint < width)
                return new Vector2(perimeterPoint - padding, -padding);
            else
                return new Vector2(width - padding, perimeterPoint - width - padding);
        }
        else
        {
            perimeterPoint = perimeterPoint - (width + height);
            if (perimeterPoint < width)
                return new Vector2(width - perimeterPoint - padding, height - padding);
            else
                return new Vector2(-padding, height - (perimeterPoint - width) - padding);
        }
    }
}