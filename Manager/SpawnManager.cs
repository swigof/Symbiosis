using Microsoft.Xna.Framework;
using Symbiosis.Session;

namespace Symbiosis.Manager;

public static class SpawnManager
{
    public static void Update(GameState gamestate)
    {
        
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