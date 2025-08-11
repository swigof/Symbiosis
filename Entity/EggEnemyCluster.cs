using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Symbiosis.Entity;

internal struct EggEnemy
{
    public bool Active;
    public Vector2 RelativePosition;
}

public struct EggEnemyCluster : IBinarySerializable
{
    // Game state
    Vector2 _position = Vector2.Zero;
    EggEnemy[] _eggEnemies = new EggEnemy[8];
    public bool Active { get; private set; } = false;

    const int _radius = 8;
    const int _eggEnemyRadius = 2;
    const int _speed = 1;
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("8pxcircle");
    static readonly Vector2 _spriteCenter = new Vector2(1, 1);

    public EggEnemyCluster() { }

    public void Init()
    {
        Active = true;
        for (var i = 0; i < _eggEnemies.Length; i++)
        {
            _eggEnemies[i].Active = true;
            _eggEnemies[i].RelativePosition.X = Game1.Random.NextFloat() * (_radius - _eggEnemyRadius);
            _eggEnemies[i].RelativePosition.Y = Game1.Random.NextFloat() * (_radius - _eggEnemyRadius);
        }
    }

    public void Update()
    {
        _position += Vector2.Normalize(Spider.Home - _position) * _speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (var i = 0; i < _eggEnemies.Length; i++)
        {
            if (_eggEnemies[i].Active)
            {
                spriteBatch.Draw(
                    _idleTexture,
                    _position + _eggEnemies[i].RelativePosition,
                    null,
                    Color.Red,
                    0,
                    _spriteCenter,
                    0.25f,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref _position);
        for (var i = 0; i < _eggEnemies.Length; i++)
        {
            reader.Read(ref _eggEnemies[i].Active);
            reader.Read(ref _eggEnemies[i].RelativePosition);
        }
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in _position);
        for (var i = 0; i < _eggEnemies.Length; i++)
        {
            writer.Write(in _eggEnemies[i].Active);
            writer.Write(in _eggEnemies[i].RelativePosition);
        }
    }
}
