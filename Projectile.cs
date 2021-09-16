using System;
using System.Numerics;

struct Projectile {
    public Vector2 position;
    public Vector2 direction;
    public int speed;
    public int damage;
    public float expire;

    public Projectile(Vector2 newPos, Vector2 newDir, int newSpeed, int newDamage, float newExpire) {
        position = newPos;
        direction = newDir;
        speed = newSpeed;
        damage = newDamage;
        expire = newExpire;
    }
}