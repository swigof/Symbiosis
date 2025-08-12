using Microsoft.Xna.Framework;
using Symbiosis.Session;

namespace Symbiosis.Manager;

// Should only be used with a safely locked gamestate
public static class SpawnManager
{
    public static void Update(ref GameState gamestate)
    {
        
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

    private static Vector2 GetRandomPositionJustOutsideScreen()
    {
        const int padding = 16;
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