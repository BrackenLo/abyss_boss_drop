using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

//Pit boss pushover?

class Game {

    //=======================================================================================

    private bool DEBUG_MODE = false;

    //=======================================================================================

    private const int WINDOW_WIDTH = 1280;
    private const int WINDOW_HEIGHT = 720;

    private Color clearColor = Color.BLACK;

    private bool windowCreated = false;
    private bool mainLoopRunning = false;

    private GameCamera camera;

    //------------------------------------------------------

    private bool gameRunning = true;
    private bool playerFallen = false;

    //------------------------------------------------------

    private Texture2D tileTexture;
    private Vector2 tileSize = new Vector2(8, 8);

    private List<Rectangle> tiles = new List<Rectangle>();
    private List<Rectangle> normalTiles = new List<Rectangle>();
    private List<Rectangle> breakableTiles = new List<Rectangle>();
    private List<Rectangle> bossTiles = new List<Rectangle>();

    //------------------------------------------------------
    
    private float arenaWidth;
    private float arenaHeight;

    private Vector2 abyssStartPos;
    private Vector2 abyssSize;
    private int abyssTilePadding = 4;

    private Vector2 abyssScaleFactor;
    private Vector2 playerAbyssPos = Vector2.Zero;
    private Color abyssColor = new Color(80, 80, 80, 255);
    private float abyssDarkenScale = 0.6f;

    //------------------------------------------------------

    private const float PLAYER_MAX_SPEED = 0.6f;
    private const float PLAYER_MAX_SPRINT_SPEED = 1.2f;
    private const float PLAYER_ACCEL = 20;
    private const float PLAYER_DEACCEL = 10;
    private Vector2 playerSize = new Vector2(8, 8);

    private Player player;
    private Texture2D playerTexture;
    private Color playerColor = new Color(239, 0, 183, 255);
    private const int PLAYER_MAX_HEALTH = 100;
    private float playerProjectilePadding = 0.5f;

    private const int PLAYER_DODGE_DISTANCE = 30;
    private const int PLAYER_DODGE_SPEED = 10;
    private const int PLAYER_ATTACK_SPEED = 1;

    //------------------------------------------------------

    private Vector2 weaponPos = Vector2.Zero;
    private float weaponAngle = 0;
    private int weaponDistance = 8;
    private Vector2 weaponSize = new Vector2(5, 5);
    private Texture2D playerWeaponTexture;

    //------------------------------------------------------

    private Boss boss;
    private Texture2D bossTexture;
    private Color bossColor = new Color(239, 71, 0, 255);
    private Vector2 bossSize = new Vector2(8, 8);

    private const float BOSS_ATTACK_INTERVAL = 0.5f;
    private const int BOSS_PROJECTILE_SPEED = 60;
    private const int BOSS_PHASE_1_HEALTH = 60;
    private const int BOSS_PHASE_2_HEALTH = 40;
    private const int BOSS_PHASE_3_HEALTH = 20;

    //------------------------------------------------------

    private Texture2D projectileTexture;
    private List<Projectile> playerProjectiles = new List<Projectile>();
    private List<Projectile> enemyProjectiles = new List<Projectile>();
    private int projectileRadius = 4;
    private int projectileSpeed = 60;
    private int projectileDamage = 10;
    private float projectileExpireTime = 6;

    //------------------------------------------------------

    //=======================================================================================

    public Game() {
        createWindow();
        initGame();
        startLoop();
    }

    //------------------------------------------------------

    private void createWindow() {
        if (windowCreated)  return;

        InitWindow(WINDOW_WIDTH, WINDOW_HEIGHT, "VimJam 2 Boss [8 Bits to Infinity]");
        SetTargetFPS(60);
        SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        windowCreated = true;
    }

    //------------------------------------------------------

    private void initGame() {

        tileTexture = LoadTexture("assets/floorTile.png");
        playerTexture = LoadTexture("assets/player.png");
        playerWeaponTexture = LoadTexture("assets/playerWeapon.png");
        bossTexture = LoadTexture("assets/bossFace.png");
        projectileTexture = LoadTexture("assets/projectile.png");

        resetGame();
    }

    private void resetGame() {
        camera = new GameCamera();

        playerProjectiles = new List<Projectile>();
        enemyProjectiles = new List<Projectile>();
        createMap();
        gameRunning = true;
        playerFallen = false;
    }

    private void createMap() {
        normalTiles = new List<Rectangle>();
        breakableTiles = new List<Rectangle>();
        bossTiles = new List<Rectangle>();

        int[,] map = Maps.map2;
        
        for (int y = 0; y < map.GetLength(0); y++)
            for (int x = 0; x < map.GetLength(1); x++) {
                switch(map[y, x]) {
                    case 0:
                        break;
                    case 1: {
                        Rectangle newTile = createTile(x, y);
                        normalTiles.Add(newTile);
                        tiles.Add(newTile);
                        break;
                    }
                    case 2: {
                        Rectangle newTile = createTile(x, y);
                        breakableTiles.Add(newTile);
                        tiles.Add(newTile);
                        break;
                    }
                    case 3: {
                        Rectangle newTile = createTile(x, y);
                        bossTiles.Add(newTile);
                        tiles.Add(newTile);
                        break;
                    }
                    case 10:
                        setPlayer(x * tileSize.X, y * tileSize.Y);
                        breakableTiles.Add(createTile(x, y));
                        break;  //create Player
                    case 11:
                        setBoss(x * tileSize.X, y * tileSize.Y);
                        bossTiles.Add(createTile(x, y));
                        break; //create boss
                }
            }
        

        arenaWidth = map.GetLength(1) * tileSize.X;
        arenaHeight = map.GetLength(0) * tileSize.Y;

        abyssStartPos = new Vector2(0, 0);
        abyssSize = new Vector2(arenaWidth, arenaHeight);
        abyssScaleFactor = new Vector2(2 / abyssSize.X, 2 / abyssSize.Y);

        camera.setVirtualScreen(arenaWidth, arenaHeight);
        
        /**Vector2 extraTiles = tileSize * abyssTilePadding;
        abyssPadding = new Rectangle(
            -extraTiles.X, 
            -extraTiles.Y, 
            abyssSize.X + extraTiles.X * 2,
            abyssSize.Y + extraTiles.Y * 2 );**/
    }

    private Rectangle createTile(int x, int y) {
        return new Rectangle(
            x * tileSize.X,
            y * tileSize.Y,
            tileSize.X,
            tileSize.Y);
    }

    private void setPlayer(float x, float y) {
        player = new Player(
            new Vector2(x + 1, y + 1),
            playerTexture,
            playerSize,
            PLAYER_MAX_HEALTH,
            PLAYER_MAX_SPEED,
            PLAYER_MAX_SPRINT_SPEED,
            PLAYER_ACCEL,
            PLAYER_DEACCEL,
            PLAYER_ATTACK_SPEED,
            playerProjectilePadding,
            PLAYER_DODGE_DISTANCE,
            PLAYER_DODGE_SPEED
            );
        camera.Target = new Vector2(x, y);
    }

    private void setBoss(float x, float y) {
        boss = new Boss(
            new Vector2(x + 1, y + 1),
            bossSize,
            BOSS_ATTACK_INTERVAL,
            BOSS_PROJECTILE_SPEED,
            BOSS_PHASE_1_HEALTH + BOSS_PHASE_2_HEALTH + BOSS_PHASE_3_HEALTH
        );
    }

    //------------------------------------------------------

    private void startLoop() {
        if (mainLoopRunning) return;
        mainLoopRunning = true;
        if (camera == null) camera = new GameCamera();

        while (!WindowShouldClose()) {
            float deltaTime = GetFrameTime();

            update(deltaTime);
            BeginDrawing(); {
                ClearBackground(clearColor);

                BeginMode2D(camera.Camera); {
                    draw2D();
                }
                EndMode2D();

                draw();
            }
            EndDrawing();
        }
    }

    //=======================================================================================

    private void update(float delta) {

        if (IsKeyPressed(KeyboardKey.KEY_R)) {
            resetGame();
            return;
        }

        if (!gameRunning) {
            return;
        }

        if (IsWindowResized()) camera.adjustCamera();
        if (IsKeyPressed(KeyboardKey.KEY_L)) {
            ToggleFullscreen();
        }

        //-----------------------------------------------------
        //Stuff for later

        Rectangle playerRect = player.Rect;
        Rectangle bossRect = boss.Rect;

        //------------------------------------------------------
        //Boss Stuff?

        if (!DEBUG_MODE && !playerFallen) 
            boss.attackTimer += delta;
        if (boss.attackTimer > boss.attackInterval) {
            boss.attackTimer = 0;
            Vector2 playerDirection = Vector2.Normalize(player.Origin - boss.Origin);
            enemyProjectiles.Add(new Projectile(boss.Origin, playerDirection, boss.projectileSpeed, projectileDamage, projectileExpireTime));
        }

        //------------------------------------------------------
        //Projectiles and stuff

        //Player projectiles check for enemy collision
        for (int n = 0; n < playerProjectiles.Count; n++) {
            Projectile projectile = playerProjectiles[n];
            projectile.position = projectile.position + ((projectile.direction * projectile.speed) * delta);
            projectile.expire -= delta;

            Vector2 bossDirection = Vector2.Normalize(boss.Origin - projectile.position);
            Vector2 toCheck = projectile.position + (bossDirection * projectileRadius);
            if (CheckCollisionPointRec(toCheck, bossRect)) {
                boss.currentHealth -= projectile.damage;
                projectile.expire = 0;
            }

            playerProjectiles[n] = projectile;
        }

        //Enemy Projectiles check for player collision
        for (int n = 0; n < enemyProjectiles.Count; n++) {
            Projectile projectile = enemyProjectiles[n];
            projectile.position = projectile.position + ((projectile.direction * projectile.speed) * delta);
            projectile.expire -= delta;

            if (!player.isDodging && !playerFallen) {
                Vector2 playerDirection = Vector2.Normalize(player.Origin - projectile.position);
                Vector2 toCheck = projectile.position + (playerDirection * projectileRadius);
                if (CheckCollisionPointRec(toCheck, playerRect)) {
                    player.knockback += playerDirection * (projectileDamage * player.projectilePadding);
                    player.currentHealth -= projectile.damage;
                    projectile.expire = 0;
                }
            }
            
            enemyProjectiles[n] = projectile;
        }

        playerProjectiles.RemoveAll(expire => expire.expire <= 0);
        enemyProjectiles.RemoveAll(expire => expire.expire <= 0);

    

        //------------------------------------------------------
        //Player stuff below so exit now if player is dead

        if (playerFallen) return;

        //------------------------------------------------------
        //Mouse and Weapon stuff

        Vector2 mousePos = camera.GetScreenToWorld(GetMousePosition());
        float angleRad = (float)Math.Atan2(mousePos.Y - player.Origin.Y, mousePos.X - player.Origin.X);
        weaponAngle = (float)(angleRad * (180 / Math.PI));

        Vector2 normalizedDirToMouse = Vector2.Normalize(mousePos - player.Origin);

        weaponPos = player.Origin + (normalizedDirToMouse * weaponDistance);

        if (!player.isDodging) {

            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) && player.attackCooldown >= player.attackMaxCooldown) {
                playerProjectiles.Add(new Projectile(weaponPos, normalizedDirToMouse, projectileSpeed, projectileDamage, projectileExpireTime));
                player.attackCooldown = 0;
            }

            
            if (IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON)) {
                player.dodgePreview= true;
                player.dodgeTarget = player.Origin + (normalizedDirToMouse * player.dodgeDistance);
            }
            if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON)) {
                player.dodgePreview = false;
                player.isDodging = true;
            }
        }

        player.attackCooldown += delta;

        //------------------------------------------------------
        //Keyboard and movement stuff

        if (player.isDodging) {
            player.Origin = Vector2.Lerp(player.Origin, player.dodgeTarget, player.dodgeSpeed * delta);
            if (Vector2.Distance(player.Origin, player.dodgeTarget) < 2) {
                player.isDodging = false;
            }
        }
        else
            playerMovement(delta);

        //camera.Target = player.position;
        camera.moveTowards(player.Origin);

        //------------------------------------------------------

        playerAbyssPos = player.Origin * abyssScaleFactor;
        playerAbyssPos.X = Math.Clamp(playerAbyssPos.X, 0, 2);
        playerAbyssPos.Y = Math.Clamp(playerAbyssPos.Y, 0, 2);

        //------------------------------------------------------

        if (!player.isDodging) {
            bool playerStanding = false;
            foreach (Rectangle tile in tiles) {
                if (CheckCollisionRecs(tile, playerRect)) {
                    playerStanding = true;
                }
            }
            if (!playerStanding && !DEBUG_MODE) {
                Console.WriteLine("Player has fallen!");
                playerFallen = true;
            }
        }
    }

    //------------------------------------------------------

    private void playerMovement(float delta) {

        Vector2 moveDir = Vector2.Zero;

        bool isSprinting = false;

        if (IsKeyDown(KeyboardKey.KEY_W))   moveDir.Y -= 1;
        if (IsKeyDown(KeyboardKey.KEY_S))   moveDir.Y += 1;
        if (IsKeyDown(KeyboardKey.KEY_A))   moveDir.X -= 1;
        if (IsKeyDown(KeyboardKey.KEY_D))   moveDir.X += 1;
        if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))  isSprinting = true;

        float maxSpeed;

        if (isSprinting) {
            maxSpeed = player.maxSprintSpeed;
        }
        else {
            maxSpeed = player.maxSpeed;
        }

        if (player.knockback != Vector2.Zero) {
            player.velocity += player.knockback;
            player.knockback = Vector2.Zero;
        }

        if (moveDir != Vector2.Zero)
            player.velocity = Vector2.Lerp(player.velocity, moveDir * maxSpeed, player.accel * delta);
        else
            player.velocity = Vector2.Lerp(player.velocity, Vector2.Zero, player.deaccel * delta);

        player.position += player.velocity;
    }

    //=======================================================================================

    private void draw2D() {

        //------------------------------------------------------
        //DRAW THE ABYSS!!!!

        //DrawRectangleRec(abyssPadding, abyssColor);

        Rectangle firstAbyss = new Rectangle(abyssStartPos.X, abyssStartPos.Y, abyssSize.X, abyssSize.Y);
        DrawRectangleRec(firstAbyss, abyssColor);

        int abyssColorVal = (int)(abyssColor.r * abyssDarkenScale);
        drawAbyss(7, abyssColorVal, firstAbyss, playerAbyssPos.X, playerAbyssPos.Y);

        //------------------------------------------------------
        //Draw tiles (different arrays for different colours + features)
        drawTiles(normalTiles, tileTexture, Color.WHITE);
        drawTiles(breakableTiles, tileTexture, Color.RED);
        drawTiles(bossTiles, tileTexture, Color.ORANGE);

        //------------------------------------------------------
        //Draw projectiles (player and boss (different colours))
        foreach (Projectile projectile in playerProjectiles) {
            DrawTextureV(
                projectileTexture, 
                projectile.position - new Vector2(projectileRadius, projectileRadius),  //center texture
                playerColor);
        }
        foreach (Projectile projectile in enemyProjectiles) {
            DrawTextureV(
                projectileTexture,
                projectile.position - new Vector2(projectileRadius, projectileRadius),
                bossColor);
        }

        //------------------------------------------------------
        //Draw player, weapon and boss

        if (!playerFallen) {
            if (player.dodgePreview) {
                Color dodgeLineColor = playerColor;
                dodgeLineColor = ColorAlpha(dodgeLineColor, 0.3F);
                DrawLineEx(player.Origin, player.dodgeTarget, 1, dodgeLineColor);
            }
            DrawTextureV(player.texture, player.position, playerColor);
            DrawTextureEx(playerWeaponTexture, weaponPos - weaponSize / 2, weaponAngle, 1, Color.WHITE);
        }
        DrawTextureV(bossTexture, boss.position, bossColor);

        //------------------------------------------------------
    }

    private void drawTiles(List<Rectangle> tiles, Texture2D tileTexture, Color tint) {

        foreach (Rectangle tile in tiles) {
            DrawTexture(tileTexture, (int)tile.x, (int)tile.y, tint);
        }
    }

    private void drawAbyss(int remaining, int previousColor, Rectangle previousAbyss, float xMod, float yMod) {

        float newWidth = previousAbyss.width * 0.9F;
        float newHeight = previousAbyss.height * 0.9F;

        float spaceLeftX = previousAbyss.width - newWidth;
        float spaceLeftY = previousAbyss.height - newHeight;

        Rectangle newAbyss = new Rectangle(
            previousAbyss.x + (spaceLeftX / 2) * xMod,
            previousAbyss.y + (spaceLeftY / 2) * yMod,
            newWidth,
            newHeight
        );

        int newColor = (int)Math.Max(previousColor * abyssDarkenScale, 0.0f);

        DrawRectangleRec(newAbyss, new Color(newColor, newColor, newColor, 255));

        if (remaining > 0)  drawAbyss(remaining - 1, newColor, newAbyss, xMod, yMod);
    }

    private void draw() {
        DrawFPS(10, 10);

        float screenWidth = GetScreenWidth();
        float screenHeight = GetScreenHeight();

        Rectangle playerHealthBarBack = new Rectangle();
            playerHealthBarBack.width = screenWidth * 0.4f;
            playerHealthBarBack.height = screenHeight * 0.05f;
            playerHealthBarBack.x = 10;
            playerHealthBarBack.y = screenHeight - playerHealthBarBack.height - 10;
        DrawRectangleRec(playerHealthBarBack, new Color(40, 40, 40, 150));

        Rectangle playerHealthBarFront = new Rectangle();
            playerHealthBarFront.width = (playerHealthBarBack.width * 0.98f) * ((float)player.currentHealth / player.maxHealth);
            playerHealthBarFront.height = playerHealthBarBack.height * 0.8f;
            playerHealthBarFront.x = playerHealthBarBack.x + (playerHealthBarBack.width * 0.01f);
            playerHealthBarFront.y = playerHealthBarBack.y + (playerHealthBarBack.height - playerHealthBarFront.height) / 2;
        DrawRectangleRec(playerHealthBarFront, playerColor);

        //------------------------------------------------------

        Rectangle bossHealthBarBack = new Rectangle();
            bossHealthBarBack.width = screenWidth * 0.8f;
            bossHealthBarBack.height = screenHeight * 0.05f;
            bossHealthBarBack.x = (screenWidth / 2) - (bossHealthBarBack.width / 2);
            bossHealthBarBack.y = 10;
        DrawRectangleRec(bossHealthBarBack, new Color(40, 40, 40, 150));

        Rectangle bossHealthBarFront = new Rectangle();
            bossHealthBarFront.width = (bossHealthBarBack.width * 0.98f) * ((float)boss.currentHealth / boss.maxHealth);
            bossHealthBarFront.height = bossHealthBarBack.height * 0.8f;
            bossHealthBarFront.x = bossHealthBarBack.x + (bossHealthBarBack.width - bossHealthBarFront.width) / 2;
            bossHealthBarFront.y = bossHealthBarBack.y + (bossHealthBarBack.height - bossHealthBarFront.height) / 2;

        DrawRectangleRec(bossHealthBarFront, bossColor);
    }

    //=======================================================================================

}