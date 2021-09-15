using System;
using System.Numerics;

using Raylib_cs;

struct Player {

    public Vector2 position;
    public Texture2D texture;
    public Vector2 size;
    public Vector2 velocity;
    public float maxSpeed;
    public float maxSprintSpeed;
    public float accel;
    public float deaccel;

    public Vector2 knockback;

    public Player(  Vector2 newPosition, Texture2D newTexture, Vector2 newSize, float newMaxSpeed, float newMaxSprintSpeed, float newAccel, float newDeaccel) {

        position = newPosition;
        texture = newTexture;
        size = newSize;

        velocity = Vector2.Zero;

        maxSpeed = newMaxSpeed;
        maxSprintSpeed = newMaxSprintSpeed;

        accel = newAccel;
        deaccel = newDeaccel;

        knockback = Vector2.Zero;
    }

    public Rectangle Rect {
        get {return new Rectangle(position.X, position.Y, size.X, size.Y);}
    }

    public Vector2 Origin {
        get {return new Vector2(position.X + size.X / 2, position.Y + size.Y / 2);}
    }
}