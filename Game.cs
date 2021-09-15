using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

class Game {

    //=======================================================================================

    private const int WINDOW_WIDTH = 1280;
    private const int WINDOW_HEIGHT = 720;

    private Color clearColor = Color.GRAY;

    private bool windowCreated = false;
    private bool mainLoopRunning = false;

    private GameCamera camera;

    //------------------------------------------------------

    private Texture2D tileTexture;
    private Vector2 tileSize = new Vector2(8, 8);

    private List<Rectangle> tiles = new List<Rectangle>();
    private List<Rectangle> breakableTiles = new List<Rectangle>();
    private List<Rectangle> bossTiles = new List<Rectangle>();

    //------------------------------------------------------
    
    private float arenaWidth;
    private float arenaHeight;

    private Vector2 abyssStartPos;
    private Vector2 abyssSize;

    private Vector2 abyssScaleFactor;
    private Vector2 playerAbyssPos = Vector2.Zero;
    private Color abyssColor = new Color(30, 30, 30, 255);

    //------------------------------------------------------

    private const float PLAYER_MAX_SPEED = 0.6f;
    private const float PLAYER_MAX_SPRINT_SPEED = 1.2f;
    private const float PLAYER_ACCEL = 20;
    private const float PLAYER_DEACCEL = 10;
    private Vector2 playerSize = new Vector2(8, 8);

    private Player player;
    private Texture2D playerTexture;
    private Color playerColor = new Color(239, 0, 183, 255);

    //------------------------------------------------------

    private Vector2 weaponPos = Vector2.Zero;
    private float weaponAngle = 0;
    private int weaponDistance = 8;
    private Vector2 weaponSize = new Vector2(5, 5);
    private Texture2D playerWeaponTexture;

    //------------------------------------------------------

    private Rectangle boss = new Rectangle();  
    private Texture2D bossTexture;

    //------------------------------------------------------

    private Texture2D projectileTexture;
    private List<Projectile> playerProjectiles = new List<Projectile>();
    private List<Projectile> enemyProjectiles = new List<Projectile>();
    private int projectileRadius = 4;
    private int projectileSpeed = 60;

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

        windowCreated = true;
    }

    //------------------------------------------------------

    private void initGame() {

        tileTexture = LoadTexture("assets/floorTile.png");
        playerTexture = LoadTexture("assets/player.png");
        playerWeaponTexture = LoadTexture("assets/playerWeapon.png");
        bossTexture = LoadTexture("assets/bossFace.png");
        projectileTexture = LoadTexture("assets/projectile.png");

        createMap();
    }

    private void createMap() {
        tiles = new List<Rectangle>();
        breakableTiles = new List<Rectangle>();
        bossTiles = new List<Rectangle>();

        int[,] map = Maps.map1;
        
        for (int y = 0; y < map.GetLength(0); y++)
            for (int x = 0; x < map.GetLength(1); x++) {
                switch(map[y, x]) {
                    case 0:
                        break;
                    case 1:
                        tiles.Add(createTile(x, y));
                        break;
                    case 2:
                        breakableTiles.Add(createTile(x, y));
                        break;
                    case 3:
                        bossTiles.Add(createTile(x, y));
                        break;
                    case 10:
                        setPlayer(x * tileSize.X, y * tileSize.Y);
                        breakableTiles.Add(createTile(x, y));
                        break;  //create Player
                    case 11:
                        boss = createTile(x, y);
                        bossTiles.Add(createTile(x, y));
                        break; //create boss
                }
            }
        

        arenaWidth = map.GetLength(1) * tileSize.X;
        arenaHeight = map.GetLength(0) * tileSize.Y;

        abyssStartPos = new Vector2(tileSize.X * 3 , tileSize.Y * 3);
        abyssSize = new Vector2(arenaWidth + tileSize.X * 3, arenaHeight + tileSize.Y * 3);
        abyssScaleFactor = new Vector2(2 / abyssSize.X, 2 / abyssSize.Y);
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
            new Vector2(x, y),
            playerTexture,
            playerSize,
            PLAYER_MAX_SPEED,
            PLAYER_MAX_SPRINT_SPEED,
            PLAYER_ACCEL,
            PLAYER_DEACCEL
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
                ClearBackground(abyssColor);

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

        //------------------------------------------------------
        //Projectiles and stuff

        for (int n = 0; n < playerProjectiles.Count; n++) {
            Projectile projectile = playerProjectiles[n];
            projectile.position = projectile.position + ((projectile.direction * projectile.speed) * delta);
            playerProjectiles[n] = projectile;
        }

        //------------------------------------------------------
        //Mouse and Weapon stuff

        Vector2 mousePos = camera.GetScreenToWorld(GetMousePosition());
        float angleRad = (float)Math.Atan2(mousePos.Y - player.Origin.Y, mousePos.X - player.Origin.X);
        weaponAngle = (float)(angleRad * (180 / Math.PI));

        Vector2 normalizedDirToMouse = Vector2.Normalize(mousePos - player.Origin);

        weaponPos = player.Origin + (normalizedDirToMouse * weaponDistance);

        if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON)) {
            playerProjectiles.Add(new Projectile(weaponPos, normalizedDirToMouse, projectileSpeed));
        }

        //------------------------------------------------------
        //Keyboard and movement stuff

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

        camera.Target = player.position;

        //------------------------------------------------------

        playerAbyssPos = player.Origin * abyssScaleFactor;


    }

    //=======================================================================================

    private void draw2D() {

        //------------------------------------------------------
        //DRAW THE ABYSS!!!!

        Rectangle firstAbyss = new Rectangle(abyssStartPos.X, abyssStartPos.Y, abyssSize.X, abyssSize.Y);

        int abyssColorVal = (int)(abyssColor.r * 0.8F);

        DrawRectangleRec(firstAbyss, new Color(abyssColorVal, abyssColorVal, abyssColorVal, 255));

        drawAbyss(7, abyssColor.r, firstAbyss, playerAbyssPos.X, playerAbyssPos.Y);

        //------------------------------------------------------
        //Draw tiles (different arrays for different colours + features)
        drawTiles(tiles, tileTexture, Color.WHITE);
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

        //------------------------------------------------------
        //Draw player, weapon and boss

        DrawTextureV(player.texture, player.position, playerColor);
        DrawTextureEx(playerWeaponTexture, weaponPos - weaponSize / 2, weaponAngle, 1, Color.WHITE);
        DrawTexture(bossTexture, (int)boss.x, (int)boss.y, Color.WHITE);

        //------------------------------------------------------
    }

    private void drawTiles(List<Rectangle> tiles, Texture2D tileTexture, Color tint) {

        foreach (Rectangle tile in tiles) {
            DrawTexture(tileTexture, (int)tile.x, (int)tile.y, tint);
        }
    }

    private void drawAbyss(int remaining, int previousColor, Rectangle previousAbyss, float xMod, float yMod) {

        float newWidth = previousAbyss.width * 0.7F;
        float newHeight = previousAbyss.height * 0.7F;

        float spaceLeftX = previousAbyss.width - newWidth;
        float spaceLeftY = previousAbyss.height - newHeight;

        Rectangle newAbyss = new Rectangle(
            previousAbyss.x + (spaceLeftX / 2) * xMod,
            previousAbyss.y + (spaceLeftY / 2) * yMod,
            newWidth,
            newHeight
        );

        int newColor = (int)Math.Max(previousColor * 0.7F, 0.0f);

        DrawRectangleRec(newAbyss, new Color(newColor, newColor, newColor, 255));

        if (remaining > 0)  drawAbyss(remaining - 1, newColor, newAbyss, xMod, yMod);
    }

    private void draw() {
        DrawFPS(10, 10);
        DrawText($"PlayerScale??? = {player.Origin * abyssScaleFactor}", 10, 40, 20, Color.RED);
    }

    //=======================================================================================

}