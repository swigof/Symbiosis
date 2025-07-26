using Microsoft.Xna.Framework;

namespace Symbiosis;

public static class Collision
{
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

