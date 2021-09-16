using System;
using System.Numerics;

using Raylib_cs;

struct Player {

    public Vector2 position;
    public Texture2D texture;
    public Vector2 size;
    public int maxHealth;
    public int currentHealth;
    
    public Vector2 velocity;
    public float maxSpeed;
    public float maxSprintSpeed;
    public float accel;
    public float deaccel;

    public Vector2 knockback;
    public float projectilePadding;
    public float attackMaxCooldown;
    public float attackCooldown;

    public bool dodgePreview;
    public int dodgeDistance;
    public int dodgeSpeed;
    public bool isDodging;
    public Vector2 dodgeTarget;


    public Player(  Vector2 newPosition, Texture2D newTexture, Vector2 newSize, int newMaxHealth,
                    float newMaxSpeed, float newMaxSprintSpeed, float newAccel, float newDeaccel,
                    float newAttackMaxCooldown,
                    float newProjectilePadding, int newDodgeDistance, int newDodgeSpeed) {

        position = newPosition;
        texture = newTexture;
        size = newSize;

        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;

        velocity = Vector2.Zero;

        maxSpeed = newMaxSpeed;
        maxSprintSpeed = newMaxSprintSpeed;

        accel = newAccel;
        deaccel = newDeaccel;

        knockback = Vector2.Zero;
        projectilePadding = newProjectilePadding;

        attackMaxCooldown = newAttackMaxCooldown;
        attackCooldown = newAttackMaxCooldown;

        dodgePreview = false;
        dodgeDistance = newDodgeDistance;
        dodgeSpeed = newDodgeSpeed;
        isDodging = false;
        dodgeTarget = newPosition;
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