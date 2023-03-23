using System;
using System.Numerics;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class Player : Character {

    //=======================================================================================

    private enum playerAnimationTypes {
        idle,
        walkForward,
        walkBackward,
        walkUp,
        walkDown
    }

    private TextureAnimation[] playerAnimations = new TextureAnimation[Enum.GetNames(typeof(playerAnimationTypes)).Length];
    private playerAnimationTypes currentAnimation;

    private bool lookingLeft = false;

    //---------------------------------------------------------

    private Vector2 velocity = Vector2.Zero;
    private float maxSpeed = 0.6f;
    private float accel = 20;
    private float deaccel = 10;

    //---------------------------------------------------------

    public Vector2 knockback = Vector2.Zero;
    private float knockbackResistance = 0.5f;

    //---------------------------------------------------------

    private bool dodgePreview = false;
    private int dodgeDistance = 30;
    private int dodgeSpeed = 10;
    private bool isDodging = false;
    private Vector2 dodgeTarget = Vector2.Zero;

    private float maxDodgeCooldown = 0.3f;
    private float dodgeCooldown = 0;

    //---------------------------------------------------------

    private Weapon weapon;

    //---------------------------------------------------------

    private float reviveCooldown = 0;
    private float maxReviveCooldown = 1;

    public bool playerImmune = false;
    private float immuneCooldown = 0;
    private float maxImmuneCooldown = 2.4f;

    //=======================================================================================

    public Player(float x, float y) : base(x, y, new Vector2(8, 8), 120) {
        playerAnimations[(int)playerAnimationTypes.walkForward] = new TextureAnimation(4, 5, new Vector2(8, 8), Game.Instance.playerWalkForwardFrames);
        playerAnimations[(int)playerAnimationTypes.idle] = new TextureAnimation(2, 1, new Vector2(8, 8), Game.Instance.playerIdleFrames);
        currentAnimation = playerAnimationTypes.idle;

        color = new Color(239, 0, 183, 255);
        weapon = new Weapon(8, 60, 10, 1);
        healthChangeEvent += healthValueChanged;
    }

    //=======================================================================================


    public bool IsDodging {get {return isDodging;}}
    public float KnockbackResistance {get {return knockbackResistance;}}

    //=======================================================================================

    public void update(float delta) {

        if (playerImmune) {
            if (immuneCooldown > maxImmuneCooldown)
                playerImmune = false;
            else    immuneCooldown += delta;
        }

        if (!Game.Instance.playerFallen) {

            weapon.attackCooldown += delta;
            playerAnimations[(int)currentAnimation].update(delta);

            mouseUpdate(delta);

            if (isDodging) {
                Origin = Vector2.Lerp(Origin, dodgeTarget, dodgeSpeed * delta);
                if (Vector2.Distance(Origin, dodgeTarget) < 2) {
                    isDodging = false;
                    velocity = Vector2.Zero;
                    knockback = Vector2.Zero;
                }
            }
            else {
                dodgeCooldown += delta;
                keyboardUpdate(delta);
                fallCheck();
            }
        }
        else {
            if (reviveCooldown > maxReviveCooldown) {

                if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON)) {
                    
                    Vector2 mapPos = Origin;
                    mapPos.X = mapPos.X / Tile.TILE_WIDTH;
                    mapPos.Y = mapPos.Y / Tile.TILE_HEIGHT;

                    Vector2[] tiles = Game.Instance.map.getTilesInRange(mapPos, 3);
                    float smallestDistance = 9999;
                    Vector2 closestTile = Game.Instance.map.playerSpawn;

                    for (int n = 0; n < tiles.Length; n++) {
                        tiles[n].X *= Tile.TILE_WIDTH;
                        tiles[n].Y *= Tile.TILE_HEIGHT;
                        float distance = Vector2.Distance(tiles[n], position);
                        if (distance < smallestDistance) {
                            smallestDistance = distance;
                            closestTile = tiles[n];
                        }
                    }

                    position = closestTile;
                    reviveCooldown = 0;
                    Game.Instance.playerFallen = false;

                    playerImmune = true;
                    immuneCooldown = 0;
                }
            }
            else {
                reviveCooldown += delta;
            }
        }
    }

    public void gameFinishedUpdate(float delta) {

        weapon.attackCooldown += delta;
        mouseUpdate(delta);
        if (isDodging) {
            Origin = Vector2.Lerp(Origin, dodgeTarget, dodgeSpeed * delta);
            if (Vector2.Distance(Origin, dodgeTarget) < 2) {
                isDodging = false;
            }
        }
        else {
            dodgeCooldown += delta;
            keyboardUpdate(delta);
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
                Projectile.PlayerProjectiles.Add(new Projectile(weapon.position, normalizedDirToMouse, weapon.projectileSpeed, weapon.projectileDamage, 5));
                weapon.attackCooldown = 0;
            }
            
            if (IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON) && dodgeCooldown > maxDodgeCooldown) {
                dodgePreview = true;
                dodgeTarget = Origin + (normalizedDirToMouse * dodgeDistance);
            }
            if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON) && dodgeCooldown > maxDodgeCooldown) {
                dodgePreview = false;
                isDodging = true;
                dodgeCooldown = 0;
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

        if (moveDir != Vector2.Zero) {  //Move forwards
            velocity = Vector2.Lerp(velocity, moveDir * maxSpeed, accel * delta);
            currentAnimation = playerAnimationTypes.walkForward;
            if (moveDir.X > 0)      lookingLeft = false;
            else if (moveDir.X < 0) lookingLeft = true;
        }
        else {  //Slow down
            velocity = Vector2.Lerp(velocity, Vector2.Zero, deaccel * delta);
            currentAnimation = playerAnimationTypes.idle;
        }

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
            if (dodgePreview) {
                dodgePreview = false;
                isDodging = true;
                dodgeCooldown = 0;
            }
            else {
                Console.WriteLine("Player has fallen!");
                Game.Instance.playerFallen = true;

                if (Game.Instance.playerLives > 0) {
                    Game.Instance.playerLives--;
                    velocity = Vector2.Zero;
                    knockback = Vector2.Zero;
                }
                else {
                    Game.Instance.playerDead = true;
                    currentHealth = 0;
                }
            }
        }
    }

    //=======================================================================================

    private void healthValueChanged(int newHealth) {
        if (newHealth == 0) {
            Game.Instance.playerDead = true;
            Game.Instance.playerFallen = true;
            Game.Instance.playerLives = 0;
        }
    }

    //=======================================================================================

    public void draw2D() {
        if (playerImmune) {
            DrawCircleV(Origin, 6, GameTools.DarkenColor(color, 0.3f));
        }

        if (dodgePreview) {
            Color dodgeLineColor = ColorAlpha(color, 0.3f);
            DrawLineEx(Origin, dodgeTarget, 1, dodgeLineColor);
        }
        playerAnimations[(int)currentAnimation].drawTexture(Rect, 0, lookingLeft, color);
        //DrawTextureV(texture, position, color);
        DrawTextureEx(weapon.texture, weapon.Origin, weapon.angle, 1, color);
    }

    //=======================================================================================

    public void draw() {

        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();

        Rectangle playerHealthBarBack = new Rectangle();
            playerHealthBarBack.width = screenWidth * 0.4f;
            playerHealthBarBack.height = screenHeight * 0.05f;
            playerHealthBarBack.x = 35;
            playerHealthBarBack.y = screenHeight - playerHealthBarBack.height - 10;
        DrawRectangleRec(playerHealthBarBack, new Color(40, 40, 40, 150));

        Rectangle playerHealthBarFront = new Rectangle();
            playerHealthBarFront.width = (playerHealthBarBack.width * 0.98f) * ((float)currentHealth / maxHealth);
            playerHealthBarFront.height = playerHealthBarBack.height * 0.8f;
            playerHealthBarFront.x = playerHealthBarBack.x + (playerHealthBarBack.width * 0.01f);
            playerHealthBarFront.y = playerHealthBarBack.y + (playerHealthBarBack.height - playerHealthBarFront.height) / 2;
        DrawRectangleRec(playerHealthBarFront, color);

        DrawText($"{Game.Instance.playerLives}", 5, (int)playerHealthBarBack.y, 50, color);

        if (Game.Instance.playerFallen && !Game.Instance.playerDead && reviveCooldown > maxReviveCooldown) {
            int yPos = screenHeight / 2;

            string text = "Press Mouse 1 to get back up";
            int textWidth = MeasureText(text, 36);
            DrawText(text, screenWidth / 2 - textWidth / 2, yPos, 36, color);

            string text2 = $"Recoveries remaining = {Game.Instance.playerLives}";
            int textWidth2 = MeasureText(text2, 22);
            DrawText(text2, screenWidth / 2 - textWidth2 / 2, (int)(yPos * 1.1f), 22, color);
        }
        else if (Game.Instance.playerDead) {
            int yPos = GetScreenHeight() / 2;

            string text = "You are dead!";
            int textWidth = MeasureText(text, 36);
            DrawText(text, screenWidth / 2 - textWidth / 2, yPos, 36, color);

            string text2 = $"You lasted {Game.Instance.gameTime.ToString("0")} seconds";
            int textWidth2 = MeasureText(text2, 22);
            DrawText(text2, screenWidth / 2 - textWidth2 / 2, (int)(yPos * 1.1f), 22, color);

            string text3 = $"Press R to restart or Escape to return to main menu";
            int textWidth3 = MeasureText(text3, 36);
            DrawText(text3, screenWidth / 2 - textWidth3 / 2, (int)(yPos * 1.2f), 36, color);
        }

    }

    //=======================================================================================

}