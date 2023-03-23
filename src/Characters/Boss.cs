using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class Boss : Character {

    //=======================================================================================

    Texture2D currentTexture;

    //=======================================================================================

    public enum BossPhase {
        intro,
        phase1,
        phase2,
        phase3,
        end
    }

    private Random rand = new Random();

    //=======================================================================================

    private bool healthChanging = false;
    private float startHealthValue;
    private float endHealthValue;

    private float healthValueDisplay = 0;

    private float healthBarChangeTime = 0.3F;
    private float healthBarTime = 0;

    //------------------------------------------------------

    public BossPhase currentPhase = BossPhase.intro;

    private float attackTimer = 0;

    private bool immune = false;
    private bool untouchable = false;

    public bool Immune {get{return immune;}}
    public bool Untouchable {get {return untouchable;}}

    private List<BossArm> arms = new List<BossArm>();

    //------------------------------------------------------

    private bool transitioning = false;
    private float transitionChangeTime1 = 8;
    private float transitionChangeTime2 = 2;
    private float transitionChangeTime3 = 4;
    private float transitionChangeTime4 = 7;
    private float transitionChangeTime5 = 4;
    private float transitionTime = 0;

    public Color displayColor;
    private int transitionValue = 0;

    //------------------------------------------------------
    //Phase 1

    private int phase1MaxHealth = 105;

    private float phase1AttackTimer = 0.5f;
    private int projectileSpeed = 60;
    private int homingProjectileSpeed = 40;
    private int projectileDamage = 10;

    private int specialAttackCounter = 0;

    //------------------------------------------------------
    //Phase 2

    private int phase2MaxHealth = 110;

    public Vector2 velocity = Vector2.Zero;
    private Vector2 wanderTarget = Vector2.Zero;
    private float maxSpeed = 0.3f;
    private int moveSpeed = 10;

    private bool changingSubmergedState = false;
    private bool submerged = false;
    private bool hasEmergeLocation = false;
    private float maxSubmergeTimer = 5;
    private float maxEmergeTimer = 15;
    private float submergeTimer = 0;

    private float phase2SubmergeAttackTimer = 0.8f;
    private float phase2EmergeAttackTimer = 2;

    private bool isCircleSprayAttack = false;
    private float phase2SprayAngle = 0;
    private int phase2Attacks = 0;

    //------------------------------------------------------
    //Phase 3

    private int phase3MaxHealth = 205;

    private float bossSize = 1;
    private float bossMaxSize = 2;
    private float bossSpin = 0;
    private float bossMaxSpin = 360 * 10;

    private Vector2 originalSize = new Vector2(8, 8);

    private float phase3AttackTimer = 1f;

    private float phase3SpecialAttackCouter = 0;
    private float phase3SpecialAttackMaxCounter = 12;
    public bool phase3PreparingPulseAttack = false;
    private float phase3PulseAttackTimer = 1.5f;
    public bool phase3DoingPulseAttack = false;

    //------------------------------------------------------
    //Phase End

    int fadeAlpha = 0;

    //=======================================================================================

    public Boss(float x, float y) : base(x, y, new Vector2(8, 8), 10) {
        color = new Color(239, 71, 0, 255);
        displayColor = color;
        healthValueDisplay = maxHealth;
        healthChangeEvent += healthValueChanged;
        wanderTarget = Origin;

        currentTexture = Game.Instance.bossSleepTexture;
    }

    //=======================================================================================

    public void update(float delta) {

        if (!Game.DEBUG_MODE && !Game.Instance.playerFallen) {
            attackTimer += delta;
        }

        if (transitioning)  updateTransition(delta);
        else                updateAttack(delta);

        foreach(BossArm arm in arms) arm.update(delta);

        if (healthChanging) {

            if (healthBarTime < healthBarChangeTime) {
                healthValueDisplay = GameTools.Lerp(startHealthValue, endHealthValue, healthBarTime / healthBarChangeTime);
                healthBarTime += delta;
            }
            else {
                healthValueDisplay = endHealthValue;
                healthChanging = false;
            }

        }
    }

    private void updateTransition(float delta) {

        transitionTime += delta;

        switch(currentPhase) {
            case BossPhase.intro: {
                break;
            }
            case BossPhase.phase1: {
                break;
            }
            case BossPhase.phase2: {
                phase2Transition(delta);
                break;
            }
            case BossPhase.phase3: {
                phase3Transition(delta);
                break;
            }
            case BossPhase.end: {
                phaseEndTransition(delta);
                break;
            }
        }
    }

    private void updateAttack(float delta) {
        switch(currentPhase) {
            case BossPhase.intro: {
                break;
            }
            case BossPhase.phase1: {
                phase1Attack(delta);
                break;
            }
            case BossPhase.phase2: {
                phase2Attack(delta);
                break;
            }
            case BossPhase.phase3: {
                phase3Attack(delta);
                break;
            }
            case BossPhase.end: {
                break;
            }
        }
    }

    //=======================================================================================

    private void phase1Start() {
        currentPhase = BossPhase.phase1;

        maxHealth = phase1MaxHealth;
        CurrentHealth = phase1MaxHealth;

        currentTexture = Game.Instance.bossNormalTexture;

        Game.Instance.audio.playTrack("phase1", true);
    }

    private void phase1Transition(float delta) {

    }

    private void phase1Attack(float delta) {

        /** Phase 1:
        Boss shoots constant stream of projectiles at player.
        Boss moves around a little from tile to tile.
        Boss occasionally does a shotgun attack
        BOSS FIRES PROJECTILES IN RANDOM DIRECTIONS THAT HOME IN ON PLAYER FOR SHORT TIME
        */

        Game game = Game.Instance;

        if (attackTimer > phase1AttackTimer) {
            specialAttackCounter += 1;
            attackTimer = 0;
            
            Vector2 playerDirection = Vector2.Normalize(game.player.Origin - Origin);
            Projectile.EnemyProjectiles.Add(
                new Projectile( Origin, 
                                playerDirection, 
                                projectileSpeed, 
                                projectileDamage));
        }
        if (specialAttackCounter >= 3) {
            specialAttackCounter = 0;
            
            Vector2 newDir = getHomingAngleDirection();

            Projectile.EnemyHomingProjectiles.Add(
                new Projectile( Origin,
                                newDir,
                                homingProjectileSpeed,
                                projectileDamage,
                                6));

        }
    }

    //=======================================================================================

    private void phase2Start() {
        currentPhase = BossPhase.phase2;
        maxHealth = phase2MaxHealth;

        immune = true;
        transitioning = true;

        transitionTime = 0;
        transitionValue = 0;
        Game.Instance.audio.stopAll(true);
    }

    private void phase2Transition(float delta) {

        switch (transitionValue) {

            case 0: {
                if (transitionTime < transitionChangeTime1) {
                    Vector3 hsvColor = ColorToHSV(color);
                    float originalColor = hsvColor.Z;
                    float newColor = GameTools.Lerp(originalColor, 0, transitionTime / transitionChangeTime1);

                    hsvColor.Z = newColor;
                    displayColor = ColorFromHSV(hsvColor.X, hsvColor.Y, hsvColor.Z);
                }
                else {
                    transitionValue = 1;
                    transitionTime = 0;
                    Console.WriteLine("Phase 2 Transition 0 done");
                    Game.Instance.cameraOverride = true;
                }
                break;
            }

            
            case 1: {
                float angle = rand.Next(0, 360);
                Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
                Projectile.EnemyProjectiles.Add(new Projectile(
                    Origin, 
                    newDir, 
                    70, 
                    projectileDamage,
                    0.2F));
                
                //Game.Instance.camera.moveTowards(Origin);
                Game.Instance.camera.Shake = 0.4f;
                Game.Instance.camera.update(delta, Origin);
                
                if (transitionTime > 5) {
                    transitionValue = 2;
                    transitionTime = 0;
                    CurrentHealth = maxHealth;
                    immune = false;
                    Console.WriteLine("Phase 2 Transition 1 done");
                    for (int n = 0; n < 3; n++)
                        arms.Add(new BossArm(this));
                    Game.Instance.audio.stopAll(false); //In case any music is playing for whatever reason
                    Game.Instance.audio.playTrack("phase2", true);
                    Game.Instance.cameraOverride = false;
                }
                break;
            }


            case 2: {
                if (transitionTime < transitionChangeTime2) {
                    
                    float transitionSpeed = transitionTime / transitionChangeTime2;

                    float newR = GameTools.Lerp(0, color.r, transitionSpeed);
                    float newG = GameTools.Lerp(0, color.g, transitionSpeed);
                    float newB = GameTools.Lerp(0, color.b, transitionSpeed);

                    displayColor = new Color((int)newR, (int)newG, (int)newB, 255);
                }
                else {
                    displayColor = color;
                    Console.WriteLine("Phase 2 TransitionDone");
                    transitionValue = 0;
                    transitionTime = 0;
                    transitioning = false;
                    wanderTarget = Origin;
                }
                break;
            }   


            default: {
                Console.WriteLine("Error in phase 2 transition phase");
                break;
            }
        }
    }

    private void phase2Attack(float delta) {

        /** Phase 2:
        Boss goes under the stage and starts swinging around under the platforms.
        Hands go to different parts of the map and turn them impassable.
        Each hand will also shoot at the player.
        Boss' head will periodically pop up for short amounts of time.
        */

        Game game = Game.Instance;
        submergeTimer += delta;

        //---------------------------------------------------------------------
        //1 - Moving to somewhere where it can emerge
        if (changingSubmergedState && submerged) {

            if (hasEmergeLocation) {
                float wanderDistance = Vector2.Distance(Origin, wanderTarget);
                if (wanderDistance > 0.3f) {
                    Vector2 wanderDirection = Vector2.Normalize(wanderTarget - Origin);
                    velocity = Vector2.Lerp(velocity, wanderDirection * maxSpeed, moveSpeed * delta);
                    position += velocity;
                }
                else { 
                    submerged = false;
                    displayColor = color;
                    changingSubmergedState = false;
                    submergeTimer = 0;
                    untouchable = false;
                    hasEmergeLocation = false;
                    velocity = Vector2.Zero;
                    attackTimer = 0;
                }
            }
            else {
                Map map = Game.Instance.map;

                Vector2 currentPos = new Vector2();
                currentPos.X = (int)(Origin.X / Tile.TILE_WIDTH);
                currentPos.Y = (int)(Origin.Y / Tile.TILE_HEIGHT);


                Vector2[] emergePoints = map.getMissingTilesInRange(currentPos, 3);
                if (emergePoints.Length == 0) changingSubmergedState = false;
                else {
                    int index = rand.Next(0, emergePoints.Length);
                    Vector2 emergePos = emergePoints[index];
                    wanderTarget = new Vector2(emergePos.X * Tile.TILE_WIDTH + (Tile.TILE_WIDTH / 2), emergePos.Y * Tile.TILE_HEIGHT + (Tile.TILE_HEIGHT / 2));
                    hasEmergeLocation = true;
                }
                
            }
        }

        //---------------------------------------------------------------------
        //2 - Resubmerge
        else if (changingSubmergedState && !submerged) {
            submerged = true;
            displayColor = GameTools.DarkenColor(color, 0.4f);
            changingSubmergedState = false;
            submergeTimer = 0;
            untouchable = true;
        }

        //---------------------------------------------------------------------
        //3 - Moving and attacking from underneath
        else if (submerged) {
            if (submergeTimer > maxEmergeTimer) {
                //resubmerge
                changingSubmergedState = true;
            }
            wander(delta);

            if (attackTimer > phase2SubmergeAttackTimer) {
                attackTimer = 0;

                Vector2 newDir = getHomingAngleDirection();

                Projectile.EnemySubmergedProjectiles.Add(
                    new Projectile( Origin,
                                    newDir,
                                    homingProjectileSpeed,
                                    projectileDamage,
                                    0.5f));

            }
        }

        //---------------------------------------------------------------------
        //4 - Holding still and attacking
        else if (!submerged) {
            if (submergeTimer > maxSubmergeTimer) {
                //Start submerging
                changingSubmergedState = true;
                isCircleSprayAttack = false;
            }

            if (isCircleSprayAttack) {
                
                if (attackTimer > 0.2f) {
                    float radians = (float)(Math.PI / 180) * phase2SprayAngle;
                    Projectile.EnemyProjectiles.Add(
                        new Projectile( Origin,
                                        new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians)),
                                        projectileSpeed,
                                        projectileDamage));
                    phase2SprayAngle += 10;

                    phase2Attacks += 1;
                }
                if (phase2Attacks > 10) {
                    isCircleSprayAttack = false;
                }

            }
            else if (attackTimer > phase2EmergeAttackTimer) {
                attackTimer = 0;
                isCircleSprayAttack = true;
                Vector2 playerDirection = Vector2.Normalize(game.player.Origin - Origin);
                phase2SprayAngle = (float)(Math.Atan2(playerDirection.Y, playerDirection.X) * (180 / Math.PI)) - rand.Next(20, 70);

                phase2Attacks = 0;
            }
        }
    }

    //=======================================================================================

    private void phase3Start() {
        submerged = false;

        currentPhase = BossPhase.phase3;
        maxHealth = phase3MaxHealth;

        immune = true;
        transitioning = true;

        transitionTime = 0;
        transitionValue = 0;
        Game.Instance.audio.stopAll(true);
    }

    private void phase3Transition(float delta) {

        switch (transitionValue) {
            case 0: {

                if (transitionTime < transitionChangeTime3) {
                    Vector3 hsvColor = ColorToHSV(color);
                    float originalColor = hsvColor.Z;
                    float newColor = GameTools.Lerp(originalColor, 0, transitionTime / transitionChangeTime3);

                    hsvColor.Z = newColor;
                    displayColor = ColorFromHSV(hsvColor.X, hsvColor.Y, hsvColor.Z);
                }
                else {
                    transitionValue = 1;
                    transitionTime = 0;
                    Console.WriteLine("Phase 3 Transition 0 done");
                    attackTimer = 0;
                    Game.Instance.audio.stopAll(false); //in case any music is playing for whatever reason
                    Game.Instance.audio.playTrack("phase3", true);
                    currentTexture = Game.Instance.bossShoutTexture;
                }
                break;
            }

            case 1: {

                if (transitionTime < transitionChangeTime4) {
                    float transitionSpeed = transitionTime / transitionChangeTime4;

                    float newR = GameTools.Lerp(0, color.r, transitionSpeed);
                    float newG = GameTools.Lerp(0, color.g, transitionSpeed);
                    float newB = GameTools.Lerp(0, color.b, transitionSpeed);

                    displayColor = new Color((int)newR, (int)newG, (int)newB, 255);

                    bossSize = GameTools.Lerp(1, bossMaxSize, transitionTime / transitionChangeTime4);
                    bossSpin = GameTools.Lerp(0, bossMaxSpin, transitionTime / transitionChangeTime4);

                    size = originalSize * bossSize;
                    Game.Instance.camera.Shake = 0.2f;

                    if (attackTimer > 0.07f) {
                        attackTimer = 0;

                        float bossSpin2 = bossSpin - 180;
                        Vector2 newDir = new Vector2((float)Math.Cos(bossSpin * Math.PI / 180), (float)Math.Sin(bossSpin * Math.PI / 180));
                        Vector2 newDir2 = new Vector2((float)Math.Cos(bossSpin2 * Math.PI / 180), (float)Math.Sin(bossSpin2 * Math.PI / 180));

                        Projectile.EnemyProjectiles.Add(new Projectile(
                            Origin, 
                            newDir, 
                            projectileSpeed, 
                            projectileDamage));
                        Projectile.EnemyProjectiles.Add(new Projectile(
                            Origin, 
                            newDir2, 
                            projectileSpeed, 
                            projectileDamage));
                    }
                }
                else {
                    bossSpin = 0;
                    transitionValue = 2;
                    transitionTime = 0;
                    CurrentHealth = maxHealth;
                    immune = false;

                    displayColor = color;
                    Console.WriteLine("Phase 3 Transformation 1 done");
                    for (int n = 0; n < 3; n++)
                        arms.Add(new BossArm(this));
                }
                break;
            }
            

            case 2: {
                transitioning = false;
                transitionValue = 0;
                transitionTime = 0;
                attackTimer = 0;
                Console.WriteLine("Phase 3 Transformation Done!");
                break;
            }

        }

    }

    private void phase3Attack(float delta) {
        /** Phase 3:
        Similar to phase 2 but boss is above the stage and shoots at the player.
        Boss moves towards player and has a orbit of projectiles that damage and push the player if too close
        */

        if (phase3PreparingPulseAttack) {

            if (attackTimer < phase3PulseAttackTimer) {

                float transitionSpeed = attackTimer / phase3PulseAttackTimer;

                float newR = GameTools.Lerp(color.r, 255, transitionSpeed);
                float newG = GameTools.Lerp(color.g, 255, transitionSpeed);
                float newB = GameTools.Lerp(color.b, 255, transitionSpeed);

                displayColor = new Color((int)newR, (int)newG, (int)newB, 255);
            }
            else {
                displayColor = color;
                phase3PreparingPulseAttack = false;
                phase3DoingPulseAttack = true;
            }

        }
        else if (phase3DoingPulseAttack) {
            Random rand = new Random();
            float angle = rand.Next(0, 45);

            for (int n = 0; n < 8; n++) {
                Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
                angle += 45;
                Projectile.EnemyProjectiles.Add(new Projectile(
                    Origin,
                    newDir,
                    projectileSpeed,
                    projectileDamage
                ));
            }

            phase3DoingPulseAttack = false;
            attackTimer = 0;
        }
        else if (phase3SpecialAttackCouter > phase3SpecialAttackMaxCounter) {
            attackTimer = 0;
            phase3PreparingPulseAttack = true;
            phase3SpecialAttackCouter = 0;
        }
        else {
            wander(delta);
            if (attackTimer > phase3AttackTimer) {
                attackTimer = 0;
                phase3SpecialAttackCouter += 1;

                Vector2 newDir = getHomingAngleDirection();

                Projectile.EnemyHomingProjectiles.Add(
                    new Projectile( Origin,
                                    newDir,
                                    homingProjectileSpeed,
                                    projectileDamage,
                                    6));
            }
        }

        {
        float angle = rand.Next(0, 360);
        Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
        Projectile.EnemyProjectiles.Add(new Projectile(
            Origin, 
            newDir, 
            45, 
            projectileDamage,
            0.4F));
        }


    }

    //=======================================================================================

    private void phaseEndStart() {
        currentPhase = BossPhase.end;

        transitioning = true;
        transitionTime = 0;
        transitionValue = 0;
        displayColor = color;

        Game.Instance.audio.stopAll(true);
    }

    private void phaseEndTransition(float delta) {

        switch (transitionValue) {
            
            case 0: {
                Map map = Game.Instance.map;

                Vector2 currentPos = new Vector2();
                currentPos.X = (int)(Origin.X / Tile.TILE_WIDTH);
                currentPos.Y = (int)(Origin.Y / Tile.TILE_HEIGHT);


                Vector2[] emergePoints = map.getMissingTilesInRange(currentPos, 3);

                float smallestDistance = 9999;
                Vector2 closestTile = Origin;

                for (int n = 0; n < emergePoints.Length; n++) {
                    emergePoints[n].X *= Tile.TILE_WIDTH;
                    emergePoints[n].Y *= Tile.TILE_HEIGHT;
                    float distance = Vector2.Distance(emergePoints[n], position);
                    if (distance < smallestDistance) {
                        smallestDistance = distance;
                        closestTile = emergePoints[n];
                    }
                }
                wanderTarget = new Vector2(closestTile.X + (Tile.TILE_WIDTH / 2), closestTile.Y + (Tile.TILE_HEIGHT / 2));

                Console.WriteLine(wanderTarget);
                transitionValue = 1;
                Console.WriteLine("Phase End Transition 0 done");
                break;
            }

            case 1 : {
                float wanderDistance = Vector2.Distance(Origin, wanderTarget);
                if (wanderDistance > 1f) {
                    Vector2 wanderDirection = Vector2.Normalize(wanderTarget - Origin);
                    velocity = Vector2.Lerp(velocity, wanderDirection * maxSpeed, moveSpeed * delta);
                    position += velocity;
                }
                else {
                    transitionValue = 2;
                    transitionTime = 0;
                    arms.Clear();
                    Console.WriteLine("Phase End Transition 1 done");
                }
                break;
            }

            case 2: {
                if (transitionTime < transitionChangeTime4) {
                    float transitionSpeed = transitionTime / transitionChangeTime4;

                    float newA = GameTools.Lerp(255, 0, transitionSpeed);
                    
                    displayColor = Fade(color, newA / 255);

                    bossSize = GameTools.Lerp(2, 0, transitionTime / transitionChangeTime4);
                    bossSpin = GameTools.Lerp(0, bossMaxSpin, transitionTime / transitionChangeTime4);

                    size = originalSize * bossSize;
                    Game.Instance.camera.Shake = 0.4f;
                }
                else {
                    transitionValue = 3;
                    transitionTime = 0;
                    Console.WriteLine("Phase End Transition 2 done");
                }
                break;
            }

            case 3: {
                
                if (transitionTime < transitionChangeTime5) {
                    fadeAlpha = (int)GameTools.Lerp(0, 255, transitionTime / transitionChangeTime5);
                }
                else {
                    fadeAlpha = 255;
                    transitionValue = 4;
                    transitionTime = 0;
                    Console.WriteLine("Phase End Transition 3 done");
                }
                break;
            }
            case 4: {
                
                if (transitionTime > 2)
                    Game.Instance.gameFinishedMenuActive = true;
                break;
            }
        }

    }

    //=======================================================================================

    private void wander(float delta) {
        Game game = Game.Instance;

        float wanderDistance = Vector2.Distance(Origin, wanderTarget);
        if (wanderDistance > 10) {
            Vector2 wanderDirection = Vector2.Normalize(wanderTarget - Origin);
            velocity = Vector2.Lerp(velocity, wanderDirection * maxSpeed, moveSpeed * delta);
        }
        else {
            if (rand.Next(0, 4) == 3) { // 1/4 chance for boss to come to player
                wanderTarget = game.player.Origin;
            }
            else {
                float newTargetX = rand.Next(0, (int)game.arenaWidth);
                float newTargetY = rand.Next(0, (int)game.arenaHeight);
                wanderTarget = new Vector2(newTargetX, newTargetY);
            }
        }
        position += velocity;
    }

    //=======================================================================================

    public void healthValueChanged(int newHealth) {

        if (currentPhase == BossPhase.intro) {
            phase1Start();
            return;
        }

        healthChanging = true;
        startHealthValue = healthValueDisplay;
        endHealthValue = newHealth;
        healthBarTime = 0;

        if (newHealth == 0) {
            switch(currentPhase) {
                case BossPhase.phase1:
                    phase2Start();
                    break;
                case BossPhase.phase2:
                    phase3Start();
                    break;
                case BossPhase.phase3:
                    phaseEndStart();
                    break;
            }
        }
    }

    //=======================================================================================

    public void draw2D(bool firstCall) {
        foreach(BossArm arm in arms)
            arm.draw2D(firstCall);

        if (firstCall){
            if (submerged) {
                Raylib.DrawTextureV(currentTexture, position, GameTools.DarkenColor(displayColor, 0.4f));
            }
        }
        else {
            if (currentPhase == BossPhase.phase3 || currentPhase == BossPhase.end) {

                Rectangle drawRect = Rect;
                drawRect.x += 8;
                drawRect.y += 8;

                Raylib.DrawTexturePro(
                    currentTexture, 
                    new Rectangle(0, 0, 8, 8),
                    drawRect,
                    (size / 2),
                    bossSpin,
                    displayColor);
            }
            else {
                if (!submerged) {
                    Raylib.DrawTextureV(currentTexture, position, displayColor);
                }
            }
        }
    }

    //=======================================================================================

    public void draw() {

        if (currentPhase == BossPhase.intro) return;

        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();


        Rectangle bossHealthBarBack = new Rectangle();
            bossHealthBarBack.width = screenWidth * 0.8f;
            bossHealthBarBack.height = screenHeight * 0.05f;
            bossHealthBarBack.x = (screenWidth / 2) - (bossHealthBarBack.width / 2);
            bossHealthBarBack.y = 10;
        DrawRectangleRec(bossHealthBarBack, new Color(40, 40, 40, 150));

        Rectangle bossHealthBarFront = new Rectangle();
            bossHealthBarFront.width = (bossHealthBarBack.width * 0.98f) * (healthValueDisplay / maxHealth);
            bossHealthBarFront.height = bossHealthBarBack.height * 0.8f;
            bossHealthBarFront.x = bossHealthBarBack.x + (bossHealthBarBack.width - bossHealthBarFront.width) / 2;
            bossHealthBarFront.y = bossHealthBarBack.y + (bossHealthBarBack.height - bossHealthBarFront.height) / 2;

        DrawRectangleRec(bossHealthBarFront, color);

        if (currentPhase == BossPhase.end) {
            DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, fadeAlpha));
        }

    }

    //=======================================================================================

    private Vector2 getHomingAngleDirection() {
        Vector2 playerReverseDirection = Vector2.Normalize(Game.Instance.player.Origin - Origin) * -1;
        float angle = (float)(Math.Atan2(playerReverseDirection.Y, playerReverseDirection.X) * 180 / Math.PI);
        angle += rand.Next(-90, 90);

        Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
        newDir = Vector2.Normalize(newDir);
        return newDir;
    }

    //=======================================================================================

}