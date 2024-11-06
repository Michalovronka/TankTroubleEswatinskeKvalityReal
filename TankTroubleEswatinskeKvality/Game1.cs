using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TankTroubleEswatinskeKvality.Content;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace TankTroubleEswatinskeKvality;

public class Game1 : Game
{
    public const int NumberOfHealth = 10;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _playerTexture;
    private Vector2 _playerPosition;
    private readonly Rectangle _playerRectangle = new Rectangle(0, 0, 60, 35);
    private Texture2D _bulletTexture;
    private List<Bullet> _bullets = []; 
    private float _rotation = 0f;
    private TankBot _tankBot;
    private int _playerHealth = NumberOfHealth;
    
    public const int Speed = 2;
    private float shootCooldown = 0.5f; 
    private float timeSinceLastShot = 0f;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _playerPosition = new Vector2(100, 100);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _playerTexture = new Texture2D(GraphicsDevice, 1, 1);
        _playerTexture.SetData(new[] { Color.White });
        _bulletTexture = new Texture2D(GraphicsDevice, 1, 1);
        _bulletTexture.SetData(new[] { Color.Red });
        
        _tankBot = new TankBot(new Vector2(300, 300), _playerTexture, _bulletTexture);
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        KeyboardState state = Keyboard.GetState();
        Vector2 movement = Vector2.Zero;
        
        if (state.IsKeyDown(Keys.A))
            _rotation -= 0.05f;
        if (state.IsKeyDown(Keys.D))
            _rotation += 0.05f;

        if (state.IsKeyDown(Keys.W) && !IsOffScreen(_playerPosition + new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation)) * Speed))
            movement = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation)) * Speed;
        if (state.IsKeyDown(Keys.S) && !IsOffScreen(_playerPosition - new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation)) * Speed))
            movement = -new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation)) * Speed;
        
        if (movement.Length() > 0)
        {
            movement.Normalize(); 
        }
        _playerPosition += movement;
        
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && timeSinceLastShot >= shootCooldown)
        {
            Vector2 startPosition = _playerPosition;
            Vector2 targetPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            
            Bullet newBullet = new Bullet(startPosition, targetPosition, Speed * 100);
            _bullets.Add(newBullet);

            timeSinceLastShot = 0f;
        }

        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            _bullets[i].Update(gameTime);
            if (IsOutsideScreen(_bullets[i].Position))
            {
                _bullets.RemoveAt(i);
            }
        }
        
        _tankBot.Update(gameTime, GraphicsDevice.Viewport);
        
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            if (_tankBot.CheckBulletCollision(_bullets[i]))
            {
                _bullets.RemoveAt(i);
                _tankBot.Health--;

                if (_tankBot.Health <= 0)
                {
                    Console.WriteLine("Player wins!");
                    Exit(); 
                    return;
                }
            }
        }

        for (int i = _tankBot.Bullets.Count - 1; i >= 0; i--)
        {
            if (_tankBot.Bullets[i].GetBoundingBox().Intersects(GetPlayerBoundingBox()))
            {
                _tankBot.Bullets.RemoveAt(i);
                _playerHealth--;

                if (_playerHealth <= 0)
                {
                    Console.WriteLine("Bot wins!");
                    Exit();
                    return;
                }
            }
        }

        Console.WriteLine($"Your HP: {_playerHealth}");
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_playerTexture, _playerPosition, _playerRectangle, Color.White, _rotation, new Vector2(_playerRectangle.Width / 2, _playerRectangle.Height / 2), 1.0f, SpriteEffects.None, 0f);
        
        foreach (var bullet in _bullets)
        {
            bullet.Draw(_spriteBatch, _bulletTexture);
        }

        _spriteBatch.End();
        
        _tankBot.Draw(_spriteBatch, _bulletTexture);
        base.Draw(gameTime);
    }
    
    private bool IsOffScreen(Vector2 position)
    {
        float halfWidth = _playerRectangle.Width / 2;
        float halfHeight = _playerRectangle.Height / 2;
        Vector2 topLeft = RotatePoint(position, new Vector2(-halfWidth, -halfHeight), _rotation);
        Vector2 topRight = RotatePoint(position, new Vector2(halfWidth, -halfHeight), _rotation);
        Vector2 bottomLeft = RotatePoint(position, new Vector2(-halfWidth, halfHeight), _rotation);
        Vector2 bottomRight = RotatePoint(position, new Vector2(halfWidth, halfHeight), _rotation);
        
        if (IsOutsideScreen(topLeft) || IsOutsideScreen(topRight) || IsOutsideScreen(bottomLeft) || IsOutsideScreen(bottomRight)) return true;
        return false;
    }
    private Vector2 RotatePoint(Vector2 origin, Vector2 point, float rotation)
    {
        float rotatedX = point.X * (float)Math.Cos(rotation) - point.Y * (float)Math.Sin(rotation);
        float rotatedY = point.X * (float)Math.Sin(rotation) + point.Y * (float)Math.Cos(rotation);
        return new Vector2(origin.X + rotatedX, origin.Y + rotatedY);
    }

    private bool IsOutsideScreen(Vector2 point)
    {
        return point.X < 0 || point.X > GraphicsDevice.Viewport.Width || point.Y < 0 || point.Y > GraphicsDevice.Viewport.Height;
    }
    
    private Rectangle GetPlayerBoundingBox()
    {
        return new Rectangle((int)_playerPosition.X - _playerRectangle.Width / 2, (int)_playerPosition.Y - _playerRectangle.Height / 2, _playerRectangle.Width, _playerRectangle.Height);
    }
    
}