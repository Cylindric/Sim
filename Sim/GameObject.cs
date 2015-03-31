﻿using OpenTK;
namespace Sim
{
    public abstract class GameObject
    {
        // PUBLIC PROPERTIES
        public Vector2 Position
        {
            get { return _position; }
            set {
                _position = value;
                Hitbox = new Vector4(_position.X, _position.Y, Size.X, Size.Y);
            }
        }

        public Vector2 Size { get; set; }

        public double TimeToLive
        {
            get { return _timeToLive; }
            set
            {
                _timeToLive = value;
                _expires = true;
            }
        }

        public bool IsDead
        {
            get
            {
                return (_expires && _timeToLive <= 0);
            }
        }

        public Vector4 Hitbox { get; private set; }

        // PUBLIC METHODS
        public virtual void Update(double timeDelta)
        {
            if (_expires)
            {
                TimeToLive -= timeDelta;
            }
        }

        public abstract void Render();

        
        // PROTECTED PROPERTIES AND VARIABLES
        protected GraphicsController Graphics;
        protected Vector2 _position;
        protected Vector4 _hitbox;
        protected SpritesheetController _spritesheet;
        protected bool _expires = false;
        protected double _timeToLive = 0;

        // PROTECTED CONSTRUCTORS
        protected GameObject(GraphicsController graphics)
        {
            Graphics = graphics;
        }

        // PROTECTED METHODS
        protected void LoadSpritesheet(string spritesheetName)
        {
            _spritesheet = new SpritesheetController(spritesheetName, Graphics);
            Size = new Vector2(_spritesheet.SpriteWidth, _spritesheet.SpriteHeight);
        }

    }
}