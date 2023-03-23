using System;
using System.Numerics;

using Raylib_cs;

public class Character {

    //=============================================================================

    public delegate void healthChangeHandler(int newHealth);
    public event healthChangeHandler healthChangeEvent;

    //=============================================================================

    protected Vector2 position;
    protected Vector2 size;
    protected Color color;

    protected int maxHealth;
    protected int currentHealth;

    //=============================================================================

    public Character(float x, float y, Vector2 newSize, int newMaxHealth) {
        position = new Vector2(x, y);
        size = newSize;
        color = Color.WHITE;

        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
    }

    //=============================================================================

    public Vector2 Position {
        get {return position;}
        set {position = value;}
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

    public Color GetColor() {
        return color;
    }

    public int CurrentHealth {
        get {return currentHealth;}
        set {
            currentHealth = Math.Max(value, 0);
            if (healthChangeEvent != null) healthChangeEvent.Invoke(currentHealth);
        }
    }

    //=============================================================================

}