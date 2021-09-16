using System;
using System.Numerics;

using Raylib_cs;


struct Boss {

    public Vector2 position;
    public Vector2 size;

    public float attackTimer;
    public float attackInterval;

    public int projectileSpeed;

    public int phase;

    public int maxHealth;
    public int currentHealth;

    public Boss(Vector2 newPosition, Vector2 newSize, float newAttackInterval, int newProjectileSpeed,
                int newMaxHealth) {
        position = newPosition;
        size = newSize;

        attackTimer = 0;
        attackInterval = newAttackInterval;

        projectileSpeed = newProjectileSpeed;

        phase = 0;

        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
    }

    public Rectangle Rect {
        get {return new Rectangle(position.X, position.Y, size.X, size.Y);}
    }

    public Vector2 Origin {
        get {return new Vector2(position.X + size.X / 2, position.Y + size.Y / 2);}
        set {
            position.X = value.X - size.X / 2;
            position.Y = value.Y - size.Y / 2;
        }
    }
}