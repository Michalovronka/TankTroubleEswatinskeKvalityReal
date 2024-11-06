using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankTroubleEswatinskeKvality.Content;

public class Bullet
{
    public Vector2 Position;
    private Vector2 _velocity;
    private readonly int _speed;
    private int bulletSize = 10;

    public Bullet(Vector2 startPosition, Vector2 targetPosition, int speed)
    {
        Position = startPosition;
        _speed = speed;
        Vector2 direction = targetPosition - startPosition;
        direction.Normalize();
        _velocity = direction * _speed;
    }

    public void Update(GameTime gameTime)
    {
        Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public Rectangle GetBoundingBox()
    {
        return new Rectangle((int)Position.X , (int)Position.Y, bulletSize, bulletSize);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        Rectangle bulletRectangle = new Rectangle((int)Position.X, (int)Position.Y, bulletSize, bulletSize);
        spriteBatch.Draw(texture, bulletRectangle, Color.White);
    }
}