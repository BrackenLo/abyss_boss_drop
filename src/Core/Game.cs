using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

//Game name ideas:  Pit boss pushover?
//                  On the ledge of the abyss?


public class Game {

    //=============================================================================
    //Singleton code

    private static Game instance = new Game();
    static Game(){}
    public static Game Instance{
        get {return instance;}
    }
    private Game(){}

    //=======================================================================================

    public const bool DEBUG_MODE = false;

    //=======================================================================================

    const int DEFAULT_WINDOW_WIDTH = 1080;
    const int DEFAULT_WINDOW_HEIGHT = 720;

    Color clearColor = Color.BLACK;

    bool windowCreated = false;
    bool mainLoopRunning = false;

    public GameCamera camera;
    public AudioController audio;

    //=======================================================================================

    public Texture2D tileTexture;
    public Texture2D playerTexture;
    public Texture2D playerWeaponTexture;
    public Texture2D bossTexture;
    public Texture2D projectileTexture;
    public Texture2D cursorTexture;

    //=======================================================================================

    public bool gameRunning = true;
    public bool playerFallen = false;
    public int playerLives = 2;

    //=======================================================================================

    public Map map;
    public Player player;
    public Boss boss;

    //=======================================================================================

    int maxCameraPlayerDistance = 60;

    //=======================================================================================
    //Variables for drawing abyss under map - still not very good

    public float arenaWidth;
    public float arenaHeight;
 
    public Vector2 abyssStartPos;
    public Vector2 abyssSize;
    public int abyssTilePadding = 4;

    public Vector2 abyssScaleFactor;
    public Vector2 playerAbyssPos = Vector2.Zero;
    public Color abyssColor = new Color(80, 80, 80, 255);
    public float abyssDarkenScale = 0.6f;

    //=======================================================================================

    public void start() {
        createWindow();
        initGame();
        startLoop();
    }

    private void createWindow() {
        if (windowCreated) return;

        InitWindow(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT, "VimJam 2 Boss [8 Bits to Infinity]");
        InitAudioDevice();

        SetTargetFPS(60);
        SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        HideCursor();

        MaximizeWindow();

        windowCreated = true;
    }

    private void initGame() {
        audio = new AudioController();

        tileTexture = LoadTexture("assets/floorTile.png");
        playerTexture = LoadTexture("assets/player.png");
        playerWeaponTexture = LoadTexture("assets/playerWeapon.png");
        bossTexture = LoadTexture("assets/bossFace.png");
        projectileTexture = LoadTexture("assets/projectile.png");
        cursorTexture = LoadTexture("assets/cursor.png");

        audio.addTrack("phase1", LoadMusicStream("assets/xDeviruchi-MysteriousDungeon.wav"));

        Dictionary<AudioController.trackPart, Music> phase2Music = new Dictionary<AudioController.trackPart, Music>();
        phase2Music.Add(AudioController.trackPart.intro, LoadMusicStream("assets/minigameIntro.wav"));
        phase2Music.Add(AudioController.trackPart.loop, LoadMusicStream("assets/minigameLoop.wav"));
        audio.addTrack("phase2", phase2Music);

        Dictionary<AudioController.trackPart, Music> phase3Music = new Dictionary<AudioController.trackPart, Music>();
        phase3Music.Add(AudioController.trackPart.intro, LoadMusicStream("assets/prepareForBattleIntro.wav"));
        phase3Music.Add(AudioController.trackPart.loop, LoadMusicStream("assets/prepareForBattleLoop.wav"));
        audio.addTrack("phase3", phase3Music);

        resetGame();
    }

    private void resetGame() {
        camera = new GameCamera();

        Projectile.clearProjectiles();
        map = new Map();
        map.setMap(Map.Maps.map2);

        gameRunning = true;
        playerFallen = false;
        playerLives = 2;

        audio.stopAll(true);
        audio.playTrack("phase1", true);
    }

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
        audio.update(delta);

        //------------------------------------------------------
        //Update setupy thing

        if (IsKeyPressed(KeyboardKey.KEY_R)) {
            resetGame();
            return;
        }

        if (!gameRunning) return;
        if (IsWindowResized()) camera.adjustCamera();

        if (IsKeyPressed(KeyboardKey.KEY_M)) {
            audio.stopTrack("phase1", true);
            audio.playTrack("phase3", true);
        }

        //------------------------------------------------------
        //Boss stuff

        boss.update(delta);

        //------------------------------------------------------
        //Projectile stuff

        Projectile.update(delta);

        //------------------------------------------------------
        //Player stuff

        if (!playerFallen)
            player.update(delta);

        //------------------------------------------------------
        //Camera Stuff

        float cameraDistance = (player.Origin - boss.Origin).Length() / 2;
        if (cameraDistance > maxCameraPlayerDistance)  {
            cameraDistance = maxCameraPlayerDistance;
        }

        Vector2 cameraDirection = Vector2.Normalize(boss.Origin - player.Origin);
        Vector2 cameraTarget = player.Origin + (cameraDirection * cameraDistance);

        camera.moveTowards(cameraTarget);

        //------------------------------------------------------
        //Abyss stuff
        
        playerAbyssPos = player.Origin * abyssScaleFactor;
        playerAbyssPos.X = Math.Clamp(playerAbyssPos.X, 0, 2);
        playerAbyssPos.Y = Math.Clamp(playerAbyssPos.Y, 0, 2);

        //------------------------------------------------------

    }

    //=======================================================================================

    private void draw2D() {

        Rectangle firstAbyss = new Rectangle(abyssStartPos.X, abyssStartPos.Y, abyssSize.X, abyssSize.Y);
        DrawRectangleRec(firstAbyss, abyssColor);
        int abyssColorVal = (int)(abyssColor.r * abyssDarkenScale);
        drawAbyss(7, abyssColorVal, firstAbyss, playerAbyssPos.X, playerAbyssPos.Y);

        Projectile.draw2D(true, projectileTexture);
        boss.draw2D(true);

        map.draw2D(tileTexture);

        Projectile.draw2D(false, projectileTexture);

        if (!playerFallen)
            player.draw2D();
        boss.draw2D(false);

        Vector2 mousePos = camera.GetScreenToWorld(new Vector2(GetMouseX(), GetMouseY()));


        DrawTextureV(   cursorTexture, 
                        mousePos - (new Vector2(7, 7) / 2), 
                        player.GetColor());

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

    //=======================================================================================

    private void draw() {

        //DrawCircle(GetScreenWidth() / 2, GetScreenHeight() / 2, 3, Color.RED);

        DrawText($"FPS: {GetFPS()}", 10, 10, 20, Color.RED);
        //DrawText($"Width: {camera.VirutalScreenWidth}", 10, 40, 20, Color.RED);
        //DrawText($"Height: {camera.VirutalScreenHeight}", 10, 70, 20, Color.RED);

        player.draw();
        boss.draw();
    }

    //=======================================================================================

}