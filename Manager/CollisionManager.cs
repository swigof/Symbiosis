using Microsoft.Xna.Framework;
using Symbiosis.Entity;
using Symbiosis.Session;

namespace Symbiosis.Manager;

// Should only be used with a safely locked gamestate
public static class CollisionManager
{
    public static void Update(ref GameState gamestate)
    {
        CheckFrogTongue(ref gamestate);
        CheckSpider(ref gamestate);
        CheckFrog(ref gamestate);
        CheckEggs(ref gamestate);
    }

    private static void CheckFrogTongue(ref GameState gamestate)
    {
        if (!gamestate.Frog.Tonguing) return;
        var tongueBounds = gamestate.Frog.TongueBoundingCircle;
        var tonguePartBounds = gamestate.Frog.GetTongueBoundingCircles();
        for (var i = 0; i < gamestate.Clusters.Length; i++)
        {
            if (!gamestate.Clusters[i].Active) continue;
            if (!gamestate.Clusters[i].BoundingCircle.Intersects(tongueBounds)) continue;
            var eggEnemyBounds = gamestate.Clusters[i].GetEggEnemyBoundingCircles();
            var hasActive = false;
            for (var j = 0; j < eggEnemyBounds.Length; j++)
            {
                if (!gamestate.Clusters[i].EggEnemies[j].Active) continue;
                for (var k = 0; k < tonguePartBounds.Length; k++)
                {
                    if (eggEnemyBounds[j].Intersects(tonguePartBounds[k]))
                        gamestate.Clusters[i].EggEnemies[j].Active = false;
                    else
                        hasActive = true;
                }
            }
            if (hasActive) continue;
            gamestate.Clusters[i].Active = false;
            gamestate.NextEggEnemyIndex = i;
        }
    }

    private static void CheckSpider(ref GameState gamestate)
    {
        if (gamestate.Spider.Movement != SpiderMovement.Attacking) return;
        if (gamestate.Spider.AttackFrame != 10) return;
        var spiderBounds = gamestate.Spider.BoundingCircle;
        for (var i = 0; i < gamestate.FrogEnemies.Length; i++)
        {
            if (!gamestate.FrogEnemies[i].Active) continue;
            if (!gamestate.FrogEnemies[i].MainBoundingCircle.Intersects(spiderBounds)) continue;
            var boundingCircles = gamestate.FrogEnemies[i].GetBoundingCircles();
            for (var j = 0; j < boundingCircles.Length; j++)
            {
                if (!boundingCircles[j].Intersects(spiderBounds)) continue;
                gamestate.FrogEnemies[i].Active = false;
                gamestate.NextFrogEnemyIndex = i;
                return;
            }
        }
    }

    private static void CheckFrog(ref GameState gamestate)
    {
        for (var i = 0; i < gamestate.FrogEnemies.Length; i++)
        {
            if (!gamestate.FrogEnemies[i].Active) continue;
            if (!gamestate.FrogEnemies[i].HeadBounds.Intersects(gamestate.Frog.BoundingCircle)) continue;
            gamestate.Frog.Respawning = true;
        }
    }

    private static void CheckEggs(ref GameState gamestate)
    {
        for (var i = 0; i < gamestate.Clusters.Length; i++)
        {
            if (!gamestate.Clusters[i].Active) continue;
            if (!gamestate.Clusters[i].BoundingCircle.Intersects(Spider.HomeBoundingCircle)) continue;
            var eggEnemyBounds = gamestate.Clusters[i].GetEggEnemyBoundingCircles();
            var hasActive = false;
            for (var j = 0; j < gamestate.Clusters[i].EggEnemies.Length; j++)
            {
                if (!gamestate.Clusters[i].EggEnemies[j].Active) continue;
                if (eggEnemyBounds[j].Intersects(Spider.HomeBoundingCircle))
                {
                    gamestate.Clusters[i].EggEnemies[j].Active = false;
                    gamestate.EggCount--;
                    if (gamestate.EggCount <= 0)
                    {
                        gamestate.EggCount = 0;
                        if (gamestate.EndedOnFrame == 0)
                            gamestate.EndedOnFrame = gamestate.FrameNumber;
                    }
                }
                else
                    hasActive = true;
            }
            if (hasActive) continue;
            gamestate.Clusters[i].Active = false;
            gamestate.NextEggEnemyIndex = i;
        }
    }
    
    public struct Circle
    {
        public Vector2 Center;
        public float Radius;
    }

    public static bool Intersects(this Circle c1, Circle c2)
    {
        float distanceSquared = (c1.Center - c2.Center).LengthSquared();
        return distanceSquared < (c1.Radius + c2.Radius) * (c1.Radius + c2.Radius);
    }

    public static bool Intersects(this Circle c, Rectangle r)
    {
        Vector2 nearestPoint = new Vector2(
            MathHelper.Clamp(c.Center.X, r.Left, r.Right),
            MathHelper.Clamp(c.Center.Y, r.Top, r.Bottom)
        );
        float distanceSquared = (c.Center - nearestPoint).LengthSquared();
        return distanceSquared < c.Radius * c.Radius;
    }

    public static bool Intersects(this Rectangle r, Circle c)
    {
        return Intersects(c, r);
    }

    public static bool Intersects(this Rectangle r1, Rectangle r2)
    {
        bool overlapX = r1.Right > r2.Left && r1.Left < r2.Right;
        bool overlapY = r1.Top < r2.Bottom && r1.Bottom > r2.Top;
        return overlapX && overlapY;
    }
}

