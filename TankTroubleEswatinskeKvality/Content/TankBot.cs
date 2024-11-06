using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankTroubleEswatinskeKvality.Content;

public class TankBot
{
    public Vector2 Position;
    private float _rotation;
    private float _speed = Game1.Speed;
    private int _moveTimer = 10;
    private int _shootTimer = 50;
    private Texture2D _texture;
    private Texture2D _bulletTexture;
    public List<Bullet> Bullets;
    private readonly Rectangle _botRectangle = new Rectangle(0, 0, 60, 35);
    public int Health = Game1.NumberOfHealth;

    public TankBot(Vector2 startPosition, Texture2D texture, Texture2D bulletTexture)
    {
        Position = startPosition;
        _texture = texture;
        _bulletTexture = bulletTexture;
        _rotation = 0f;
        Bullets = new List<Bullet>();
    }

    public void Update(GameTime gameTime, Viewport viewport)
    {
        _moveTimer--;
        _shootTimer--;
        
        if (_moveTimer <= 0)
        {
            float turnDirection = (float)(new Random().NextDouble() * 0.2 - 0.1);
            _rotation += turnDirection;
            _moveTimer = 10; 
        }
        
        Vector2 movement = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation)) * _speed;
        Vector2 newPosition = Position + movement;
        
        if (!IsOffScreen(newPosition, viewport)) Position = newPosition;
        if (IsOffScreen(newPosition, viewport)) _rotation += 180;
        
        if (_shootTimer <= 0)
        {
            Vector2 targetPosition = Position + movement  * 100; 
            Bullet newBullet = new Bullet(Position, targetPosition, Game1.Speed * 100);
            Bullets.Add(newBullet);
            _shootTimer = 120; 
        }
        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            Bullets[i].Update(gameTime);
            if (IsOffScreen(Bullets[i].Position, viewport))
            {
                Bullets.RemoveAt(i);
            }
        }
        Console.WriteLine($"Enemy HP: {Health}");
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(_texture, Position, new Rectangle(0, 0, 60, 35), Color.Red, _rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), 1.0f, SpriteEffects.None, 0f);
        foreach (var bullet in Bullets)
        {
            bullet.Draw(spriteBatch, _bulletTexture);
        }
        spriteBatch.End();
    }
    
    private bool IsOffScreen(Vector2 position, Viewport viewport)
    {
        float halfWidth = _botRectangle.Width / 2;
        float halfHeight = _botRectangle.Height / 2;
        
        Vector2 topLeft = RotatePoint(position, new Vector2(-halfWidth, -halfHeight), _rotation);
        Vector2 topRight = RotatePoint(position, new Vector2(halfWidth, -halfHeight), _rotation);
        Vector2 bottomLeft = RotatePoint(position, new Vector2(-halfWidth, halfHeight), _rotation);
        Vector2 bottomRight = RotatePoint(position, new Vector2(halfWidth, halfHeight), _rotation);
        
        return IsOutsideScreen(topLeft, viewport) || IsOutsideScreen(topRight, viewport) || IsOutsideScreen(bottomLeft, viewport) || IsOutsideScreen(bottomRight, viewport);
    }

    private Vector2 RotatePoint(Vector2 origin, Vector2 point, float rotation)
    {
        float cos = (float)Math.Cos(rotation);
        float sin = (float)Math.Sin(rotation);
        float rotatedX = point.X * cos - point.Y * sin;
        float rotatedY = point.X * sin + point.Y * cos;
        return new Vector2(origin.X + rotatedX, origin.Y + rotatedY);
    }

    private bool IsOutsideScreen(Vector2 point, Viewport viewport)
    {
        return point.X < 0 || point.X > viewport.Width || point.Y < 0 || point.Y > viewport.Height;
    }
    
    public bool CheckBulletCollision(Bullet bullet)
    {
        return GetBoundingBox().Intersects(bullet.GetBoundingBox());
    }

    private Rectangle GetBoundingBox()
    {
        return new Rectangle((int)Position.X - _botRectangle.Width / 2, (int)Position.Y - _botRectangle.Height / 2, _botRectangle.Width, _botRectangle.Height);
    }
    
    
}