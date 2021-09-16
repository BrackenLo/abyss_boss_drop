using System;
using System.Numerics;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class Player : Character {

    //=======================================================================================

    private Vector2 velocity = Vector2.Zero;
    private float maxSpeed = 0.6f;
    private float accel = 20;
    private float deaccel = 10;

    public Vector2 knockback = Vector2.Zero;
    private float knockbackResistance = 0.5f;

    private bool dodgePreview = false;
    private int dodgeDistance = 30;
    private int dodgeSpeed = 10;
    private bool isDodging = false;
    private Vector2 dodgeTarget = Vector2.Zero;

    private Weapon weapon;

    //=======================================================================================

    public Player(float x, float y) : base(x, y, Game.Instance.playerTexture, new Vector2(8, 8), 60) {
        color = new Color(239, 0, 183, 255);
        weapon = new Weapon(8, 60, 10, 1);
    }

    //=======================================================================================


    public bool IsDodging {get {return isDodging;}}
    public float KnockbackResistance {get {return knockbackResistance;}}

    //=======================================================================================

    public void update(float delta) {

        weapon.attackCooldown += delta;
        mouseUpdate(delta);

        if (isDodging) {
            Origin = Vector2.Lerp(Origin, dodgeTarget, dodgeSpeed * delta);
            if (Vector2.Distance(Origin, dodgeTarget) < 2) {
                isDodging = false;
                velocity = Vector2.Zero;
            }
        }
        else {
            keyboardUpdate(delta);
            fallCheck();
        }

    }

    //=======================================================================================

    private void mouseUpdate(float delta) {
        Vector2 mousePos = Game.Instance.camera.GetScreenToWorld(GetMousePosition());
        float angleRad = (float)Math.Atan2(mousePos.Y - Origin.Y, mousePos.X - Origin.X);
        weapon.angle = (float)(angleRad * (180 / Math.PI));

        Vector2 normalizedDirToMouse = Vector2.Normalize(mousePos - Origin);

        weapon.position = Origin + (normalizedDirToMouse * weapon.distance);

        if (!isDodging) {

            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) && weapon.attackCooldown >= weapon.attackMaxCooldown) {
                Projectile.PlayerProjectiles.Add(new Projectile(weapon.position, normalizedDirToMouse, weapon.projectileSpeed, weapon.projectileDamage));
                weapon.attackCooldown = 0;
            }
            
            if (IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON)) {
                dodgePreview = true;
                dodgeTarget = Origin + (normalizedDirToMouse * dodgeDistance);
            }
            if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON)) {
                dodgePreview = false;
                isDodging = true;
            }
        }
    }

    //=======================================================================================

    private void keyboardUpdate(float delta) {
        Vector2 moveDir = Vector2.Zero;

        if (IsKeyDown(KeyboardKey.KEY_W))   moveDir.Y -= 1;
        if (IsKeyDown(KeyboardKey.KEY_S))   moveDir.Y += 1;
        if (IsKeyDown(KeyboardKey.KEY_A))   moveDir.X -= 1;
        if (IsKeyDown(KeyboardKey.KEY_D))   moveDir.X += 1;

        if (knockback != Vector2.Zero) {
            velocity += knockback;
            knockback = Vector2.Zero;
        }

        if (moveDir != Vector2.Zero)
            velocity = Vector2.Lerp(velocity, moveDir * maxSpeed, accel * delta);
        else
            velocity = Vector2.Lerp(velocity, Vector2.Zero, deaccel * delta);

        position += velocity;
    }

    private void fallCheck() {
        bool playerStanding = false;
        foreach (Rectangle tile in Game.Instance.map.allTiles.Values) {
            if (CheckCollisionRecs(tile, Rect)) {
                playerStanding = true;
            }
        }
        if (!playerStanding) {
            Console.WriteLine("Player has fallen!");
            Game.Instance.playerFallen = true;
        }
    }

    //=======================================================================================

    public void draw2D() {
        if (dodgePreview) {
            Color dodgeLineColor = ColorAlpha(color, 0.3f);
            DrawLineEx(Origin, dodgeTarget, 1, dodgeLineColor);
        }
        DrawTextureV(texture, position, color);
        DrawTextureEx(weapon.texture, weapon.Origin, weapon.angle, 1, color);
    }

    //=======================================================================================

    public void draw() {

        float screenWidth = GetScreenWidth();
        float screenHeight = GetScreenHeight();

        Rectangle playerHealthBarBack = new Rectangle();
            playerHealthBarBack.width = screenWidth * 0.4f;
            playerHealthBarBack.height = screenHeight * 0.05f;
            playerHealthBarBack.x = 10;
            playerHealthBarBack.y = screenHeight - playerHealthBarBack.height - 10;
        DrawRectangleRec(playerHealthBarBack, new Color(40, 40, 40, 150));

        Rectangle playerHealthBarFront = new Rectangle();
            playerHealthBarFront.width = (playerHealthBarBack.width * 0.98f) * ((float)currentHealth / maxHealth);
            playerHealthBarFront.height = playerHealthBarBack.height * 0.8f;
            playerHealthBarFront.x = playerHealthBarBack.x + (playerHealthBarBack.width * 0.01f);
            playerHealthBarFront.y = playerHealthBarBack.y + (playerHealthBarBack.height - playerHealthBarFront.height) / 2;
        DrawRectangleRec(playerHealthBarFront, color);

    }

    //=======================================================================================

}