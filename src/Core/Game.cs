using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

/**
    Game name ideas:    Pit boss pushover?
                        On the ledge of the abyss?
                        
*/                  


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
    public bool applicationRunning = true;

    public GameCamera camera;
    public AudioController audio;

    public bool cameraOverride = false;

    public string gameName = "Abyss Boss Drop";

    //=======================================================================================

    public Texture2D raylibTexture;

    public Texture2D playerWalkForwardFrames;
    public Texture2D playerIdleFrames;

    public Texture2D bossSleepTexture;
    public Texture2D bossNormalTexture;
    public Texture2D bossShoutTexture;

    public Texture2D tileTexture;
    public Texture2D playerWeaponTexture;
    public Texture2D projectileTexture;
    public Texture2D cursorTexture;

    public Sound bossHitSound;
    public Sound playerHitSound;

    //=======================================================================================

    public MainMenu mainMenu = new MainMenu();
    public bool mainMenuActive = true;

    public GameFinishMenu finishMenu = new GameFinishMenu();
    public bool gameFinishedMenuActive = false;

    //=======================================================================================

    public bool gameRunning = true;
    public bool playerFallen = false;
    public int playerLives = 2;
    public bool playerDead = false;
    public float gameTime = 0;

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
    public int abyssTilePadding = 15;

    public Vector2 abyssScaleFactor;
    public Vector2 playerAbyssPos = Vector2.Zero;
    //public Color abyssColor = new Color(80, 80, 80, 255);
    //public float abyssDarkenScale = 0.6f;

    //=======================================================================================
    //Abyss replacement particles

    List<Vector2> backParticles = new List<Vector2>();
    int backParticleCount = 250;
    float backParticleSpeed = 6;
    float backParticleSize = 0.2f;
    Color backParticleColor = new Color(12, 7, 7, 255);

    List<Vector2> midParticles = new List<Vector2>();
    int midParticleCount = 150;
    float midParticleSpeed = 24;
    float midParticleSize = 0.9f;
    Color midParticleColor = new Color(79, 14, 14, 255);

    List<Vector2> frontParticles = new List<Vector2>();
    int frontParticleCount = 140;
    float frontParticleSpeed = 45;
    float frontParticleSize = 1.5f;
    Color frontParticleColor = new Color(171, 16, 16, 255);

    //=======================================================================================

    public void start() {
        createWindow();
        initGame();
        startLoop();
    }

    private void createWindow() {
        if (windowCreated) return;

        InitWindow(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT, gameName);
        InitAudioDevice();

        SetTargetFPS(60);
        SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        SetExitKey(0);

        Image icon = LoadImage("assets/icon.gif");
        SetWindowIcon(icon);
        UnloadImage(icon);

        HideCursor();

        windowCreated = true;
    }

    private void initGame() {
        audio = new AudioController();

        raylibTexture = LoadTexture("assets/raylib_32x32.png");

        playerWalkForwardFrames = LoadTexture("assets/images/playerWalkFrames.png");
        playerIdleFrames = LoadTexture("assets/images/playerIdleFrames.png");

        bossSleepTexture = LoadTexture("assets/images/bossSleep.png");
        bossNormalTexture = LoadTexture("assets/images/bossFace.png");
        bossShoutTexture = LoadTexture("assets/images/bossShout.png");

        tileTexture = LoadTexture("assets/images/floorTile.png");
        playerWeaponTexture = LoadTexture("assets/images/playerWeapon.png");
        projectileTexture = LoadTexture("assets/images/projectile.png");
        cursorTexture = LoadTexture("assets/images/cursor.png");

        audio.addTrack("phase1", LoadMusicStream("assets/audio/xDeviruchi-MysteriousDungeon.wav"));

        Dictionary<AudioController.trackPart, Music> phase2Music = new Dictionary<AudioController.trackPart, Music>();
        phase2Music.Add(AudioController.trackPart.intro, LoadMusicStream("assets/audio/minigameIntro.wav"));
        phase2Music.Add(AudioController.trackPart.loop, LoadMusicStream("assets/audio/minigameLoop.wav"));
        audio.addTrack("phase2", phase2Music);

        Dictionary<AudioController.trackPart, Music> phase3Music = new Dictionary<AudioController.trackPart, Music>();
        phase3Music.Add(AudioController.trackPart.intro, LoadMusicStream("assets/audio/prepareForBattleIntro.wav"));
        phase3Music.Add(AudioController.trackPart.loop, LoadMusicStream("assets/audio/prepareForBattleLoop.wav"));
        audio.addTrack("phase3", phase3Music);

        bossHitSound = LoadSound("assets/audio/BossHit1.wav");
        playerHitSound = LoadSound("assets/audio/HitDamage1.wav");
    }

    public void resetGame() {
        camera = new GameCamera();

        Projectile.clearProjectiles();
        map = new Map();
        map.setMap(Map.Maps.map3);

        gameRunning = true;
        playerFallen = false;
        playerLives = 2;
        playerDead = false;
        gameTime = 0;

        spawnParticles();

        audio.stopAll(true);
    }

    private void spawnParticles() {
        Random rand = new Random();
        backParticles.Clear();
        midParticles.Clear();
        frontParticles.Clear();

        for (int n = 0; n < backParticleCount; n++) {
            int particleX = rand.Next((int)abyssStartPos.X, (int)abyssSize.X);
            int particleY = rand.Next((int)abyssStartPos.Y, (int)abyssSize.Y);
            backParticles.Add(new Vector2(particleX, particleY));
        }
        for (int n = 0; n < midParticleCount; n++) {
            int particleX = rand.Next((int)abyssStartPos.X, (int)abyssSize.X);
            int particleY = rand.Next((int)abyssStartPos.Y, (int)abyssSize.Y);
            midParticles.Add(new Vector2(particleX, particleY));
        }
        for (int n = 0; n < frontParticleCount; n++) {
            int particleX = rand.Next((int)abyssStartPos.X, (int)abyssSize.X);
            int particleY = rand.Next((int)abyssStartPos.Y, (int)abyssSize.Y);
            frontParticles.Add(new Vector2(particleX, particleY));
        }

    }

    private void startLoop() {
        if (mainLoopRunning) return;
        mainLoopRunning = true;
        if (camera == null) camera = new GameCamera();

        while (!WindowShouldClose() && applicationRunning) {
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
        
        if (mainMenuActive) { 
            mainMenu.update(delta);
            return;
        }

        if (gameFinishedMenuActive) {
            finishMenu.update(delta);
            return;
        }

        if (delta > 0.7f) return;

        //------------------------------------------------------
        //Update setupy thing

        if (IsKeyPressed(KeyboardKey.KEY_R)) {
            resetGame();
            return;
        }
        if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) {
            mainMenuActive = true;
            audio.stopAll(true);
            return;
        }

        if (!gameRunning) return;
        if (IsWindowResized()) camera.adjustCamera();

        //------------------------------------------------------
        //Boss stuff

        boss.update(delta);

        //------------------------------------------------------
        //Projectile stuff

        Projectile.update(delta);

        //------------------------------------------------------
        //Player stuff

        if (!playerDead){
            player.update(delta);
            if (!playerFallen) gameTime += delta;
        }
        

        //------------------------------------------------------
        //Camera Stuff

        float cameraDistance = (player.Origin - boss.Origin).Length() / 2;
        if (cameraDistance > maxCameraPlayerDistance)  {
            cameraDistance = maxCameraPlayerDistance;
        }

        Vector2 cameraDirection = Vector2.Normalize(boss.Origin - player.Origin);
        Vector2 cameraTarget = player.Origin + (cameraDirection * cameraDistance);


        if (!cameraOverride)
            camera.update(delta, cameraTarget);
            //camera.moveTowards(cameraTarget);

        //------------------------------------------------------
        //Abyss stuff
        
        playerAbyssPos = player.Origin * abyssScaleFactor;
        playerAbyssPos.X = Math.Clamp(playerAbyssPos.X, 0, 2);
        playerAbyssPos.Y = Math.Clamp(playerAbyssPos.Y, 0, 2);

        //------------------------------------------------------

    }

    //=======================================================================================

    private void draw2D() {

        if (mainMenuActive) { 
            return;
        }
        if (gameFinishedMenuActive) {
            return;
        }

        /**Rectangle firstAbyss = new Rectangle(abyssStartPos.X, abyssStartPos.Y, abyssSize.X, abyssSize.Y);
        DrawRectangleRec(firstAbyss, abyssColor);
        int abyssColorVal = (int)(abyssColor.r * abyssDarkenScale);
        drawAbyss(7, abyssColorVal, firstAbyss, playerAbyssPos.X, playerAbyssPos.Y);**/

        foreach (Vector2 backParticle in backParticles) {
            DrawCircleV(backParticle + (playerAbyssPos * backParticleSpeed), backParticleSize, backParticleColor);
        }
        foreach (Vector2 midParticle in midParticles) {
            DrawCircleV(midParticle + (playerAbyssPos * midParticleSpeed), midParticleSize, midParticleColor);
        }
        foreach (Vector2 frontParticle in frontParticles) {
            DrawCircleV(frontParticle + (playerAbyssPos * frontParticleSpeed), frontParticleSize, frontParticleColor);
        }

        Projectile.draw2D(true, projectileTexture);
        boss.draw2D(true);

        map.draw2D(tileTexture);

        Projectile.draw2D(false, projectileTexture);

        if (!playerFallen && !playerDead)
            player.draw2D();
        boss.draw2D(false);

        Vector2 mousePos = camera.GetScreenToWorld(new Vector2(GetMouseX(), GetMouseY()));


        DrawTextureV(   cursorTexture, 
                        mousePos - (new Vector2(7, 7) / 2), 
                        player.GetColor());

    }

    //=======================================================================================

    private void draw() {

        if (mainMenuActive) { 
            mainMenu.draw();
            return;
        }
        if (gameFinishedMenuActive) {
            finishMenu.draw();
            return;
        }

        //DrawCircle(GetScreenWidth() / 2, GetScreenHeight() / 2, 3, Color.RED);

        //DrawText($"FPS: {GetFPS()}", 10, 10, 20, Color.RED);
        //DrawText($"Width: {camera.VirutalScreenWidth}", 10, 40, 20, Color.RED);
        //DrawText($"Height: {camera.VirutalScreenHeight}", 10, 70, 20, Color.RED);

        player.draw();
        boss.draw();
    }

    //=======================================================================================

}