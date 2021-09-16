using System;
using System.Numerics;

public struct Weapon {

    public Raylib_cs.Texture2D texture;
    public Vector2 size;

    public float angle;
    public Vector2 position;
    public int distance;

    public int projectileSpeed;
    public int projectileDamage;

    public float attackMaxCooldown;
    public float attackCooldown;

    public Weapon(int newDistance, int newProjectileSpeed, int newProjectileDamage, int newAttackMaxCooldown) {
        texture = Game.Instance.playerWeaponTexture;
        size = new Vector2(5, 5);
        angle = 0;
        position = Vector2.Zero;
        distance = newDistance;
        projectileSpeed = newProjectileSpeed;
        projectileDamage = newProjectileDamage;

        attackMaxCooldown = newAttackMaxCooldown;
        attackCooldown = newAttackMaxCooldown;
    }

    public Vector2 Origin {
        get {return position - (size / 2);}
    }

}