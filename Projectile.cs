using System;
using System.Numerics;

struct Projectile {
    public Vector2 position;
    public Vector2 direction;
    public int speed;

    public Projectile(Vector2 newPos, Vector2 newDir, int newSpeed) {
        position = newPos;
        direction = newDir;
        speed = newSpeed;
    }
}