using System;
using System.Numerics;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class Boss : Character {

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

    private BossPhase currentPhase = BossPhase.phase1;

    private float attackTimer = 0;

    private bool immune = false;
    private bool untouchable = false;

    public bool Immune {get{return immune;}}
    public bool Untouchable {get {return untouchable;}}

    //------------------------------------------------------

    private bool transitioning = false;
    private bool transitionColorChanging = false;
    private float transitionColorChangeTime1 = 8;
    private float transitionColorChangeTime2 = 2;
    private float transitionTime = 0;

    private Color displayColor;
    private int transitionValue = 0;

    //------------------------------------------------------
    //Phase 1

    private float phase1AttackTimer = 0.5f;
    private int projectileSpeed = 60;
    private int homingProjectileSpeed = 40;
    private int projectileDamage = 10;

    private int specialAttackCounter = 0;

    //------------------------------------------------------
    //Phase 2

    private int phase2MaxHealth = 120;

    private Vector2 velocity = Vector2.Zero;
    private Vector2 wanderTarget = Vector2.Zero;
    private float maxSpeed = 0.3f;
    private int moveSpeed = 10;

    private bool changingSubmergedState = false;
    private bool submerged = false;
    private bool hasEmergeLocation = false;
    private float maxSubmergeTimer = 5;
    private float maxEmergeTimer = 15;
    private float submergeTimer = 0;

    private float phase2SubmergeAttackTimer = 1f;
    private float phase2EmergeAttackTimer = 2;

    private bool isCircleSprayAttack = false;
    private float phase2SprayAngle = 0;
    private int phase2Attacks = 0;

    //------------------------------------------------------
    //Phase 3

    private int phase3MaxHealth = 150;

    //=======================================================================================

    public Boss(float x, float y) : base(x, y, Game.Instance.bossTexture, new Vector2(8, 8), 85) {
        color = new Color(239, 71, 0, 255);
        displayColor = color;
        healthValueDisplay = maxHealth;
        healthChangeEvent += healthValueChanged;
        wanderTarget = Origin;
    }

    //=======================================================================================

    public void update(float delta) {

        if (!Game.DEBUG_MODE && !Game.Instance.playerFallen) {
            attackTimer += delta;
        }

        if (transitioning)  updateTransition(delta);
        else                updateAttack(delta);

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

       // Do me next!!!
        transitionTime += delta;

        switch(currentPhase) {
            case BossPhase.intro: {
                break;
            }
            case BossPhase.phase1: {
                break;
            }
            case BossPhase.phase2: {

                if (transitionValue == 0) {
                    if (transitionTime < transitionColorChangeTime1) {
                        Vector3 hsvColor = ColorToHSV(color);
                        float originalColor = hsvColor.Z;
                        float newColor = GameTools.Lerp(originalColor, 0, transitionTime / transitionColorChangeTime1);

                        hsvColor.Z = newColor;
                        displayColor = ColorFromHSV(hsvColor.X, hsvColor.Y, hsvColor.Z);
                    }
                    else {
                        transitionValue = 1;
                        transitionTime = 0;
                        Console.WriteLine("Phase 2 Transition 0 done");
                    }
                }
                else if (transitionValue == 1){
                    float angle = rand.Next(0, 360);
                    Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
                    Projectile.EnemyProjectiles.Add(new Projectile(
                        Origin, 
                        newDir, 
                        70, 
                        projectileDamage,
                        0.2F));
                    
                    if (transitionTime > 5) {
                        transitionValue = 2;
                        transitionTime = 0;
                        CurrentHealth = maxHealth;
                        immune = false;
                        Console.WriteLine("Phase 2 Transition 1 done");
                        Game.Instance.audio.playTrack("phase2", true);
                    }
                }
                else if (transitionValue == 2) {
                    if (transitionTime < transitionColorChangeTime2) {
                        
                        float transitionSpeed = transitionTime / transitionColorChangeTime2;

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
                        wanderTarget = Game.Instance.player.Origin;
                    }
                }
                else {
                    Console.WriteLine("Error in phase 2 transition phase");
                }
                break;
            }
            case BossPhase.phase3: {
                break;
            }
            case BossPhase.end: {
                break;
            }
        }

    }

    private void updateAttack(float delta) {
        Game game = Game.Instance;

        switch(currentPhase) {
            case BossPhase.intro: {
                break;
            }
            case BossPhase.phase1: {
                /** Phase 1:
                Boss shoots constant stream of projectiles at player.
                Boss moves around a little from tile to tile.
                Boss occasionally does a shotgun attack
                BOSS FIRES PROJECTILES IN RANDOM DIRECTIONS THAT HOME IN ON PLAYER FOR SHORT TIME
                */
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
                break;
            }
            case BossPhase.phase2: {
                /** Phase 2:
                Boss goes under the stage and starts swinging around under the platforms.
                Hands go to different parts of the map and turn them impassable.
                Each hand will also shoot at the player.
                Boss' head will periodically pop up for short amounts of time.
                */
                submergeTimer += delta;

                if (changingSubmergedState && submerged) {  //Moving to somewhere where it can emerge

                    if (hasEmergeLocation) {
                        float wanderDistance = Vector2.Distance(Origin, wanderTarget);
                        if (wanderDistance > 0.3f) {
                            Vector2 wanderDirection = Vector2.Normalize(wanderTarget - Origin);
                            velocity = Vector2.Lerp(velocity, wanderDirection * maxSpeed, moveSpeed * delta);
                            position += velocity;
                        }
                        else { 
                            submerged = false;
                            changingSubmergedState = false;
                            submergeTimer = 0;
                            untouchable = false;
                            hasEmergeLocation = false;
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
                else if (changingSubmergedState && !submerged) {    //Moving to somewhere where it can submerge
                    submerged = true;
                    changingSubmergedState = false;
                    submergeTimer = 0;
                    untouchable = true;
                }
                else if (submerged) {   //Moving and attacking from underneith
                    if (submergeTimer > maxEmergeTimer) {
                        //resubmerge
                        changingSubmergedState = true;
                    }
                    float wanderDistance = Vector2.Distance(Origin, wanderTarget);
                    if (wanderDistance > 10) {
                        Vector2 wanderDirection = Vector2.Normalize(wanderTarget - Origin);
                        velocity = Vector2.Lerp(velocity, wanderDirection * maxSpeed, moveSpeed * delta);
                    }
                    else {
                        if (rand.Next(0, 4) == 3) { //1 / 4 chance for boss to come to player
                            wanderTarget = game.player.Origin;
                        }
                        else {
                            float newTargetX = rand.Next(0, (int)game.arenaWidth);
                            float newTargetY = rand.Next(0, (int)game.arenaHeight);
                            wanderTarget = new Vector2(newTargetX, newTargetY);
                        }
                    }
                    position += velocity;


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
                else if (!submerged) { //Holding still and attacking
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

                break;
            }
            case BossPhase.phase3: {
                /** Phase 3:
                Similar to phase 2 but boss is above the stage and shoots at the player.
                Boss moves towards player and has a orbit of projectiles that damage and push the player if too close
                */
                break;
            }
            case BossPhase.end: {
                break;
            }
        }

    }

    private void startPhase2() {
        currentPhase = BossPhase.phase2;
        maxHealth = phase2MaxHealth;

        immune = true;
        transitioning = true;

        transitionColorChanging = true;
        transitionTime = 0;
        Game.Instance.audio.stopAll(true);
    }

    private void startPhase3() {
        currentPhase = BossPhase.phase3;
        maxHealth = phase3MaxHealth;
        currentHealth = phase3MaxHealth;

        immune = true;
        transitioning = true;
    }

    public void healthValueChanged(int newHealth) {

        healthChanging = true;
        startHealthValue = healthValueDisplay;
        endHealthValue = newHealth;
        healthBarTime = 0;

        if (newHealth == 0) {
            switch(currentPhase) {
                case BossPhase.phase1:
                    startPhase2();
                    break;
                case BossPhase.phase2:
                    startPhase3();
                    break;
                case BossPhase.phase3:
                    break;
            }
        }

    }

    //=======================================================================================

    public void draw2D(bool firstCall) {
        if (firstCall){
            if (submerged) {
                Raylib.DrawTextureV(texture, position, GameTools.DarkenColor(displayColor, 0.4f));
            }
        }
        else {
            if (!submerged) {
                Raylib.DrawTextureV(texture, position, displayColor);
            }
        }
    }

    //=======================================================================================

    public void draw() {

        float screenWidth = GetScreenWidth();
        float screenHeight = GetScreenHeight();


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

    }

    //=======================================================================================

    private Vector2 getHomingAngleDirection() {
        Vector2 playerReverseDirection = Vector2.Normalize(Game.Instance.player.Origin - Origin) * -1;
                float angle = (float)(Math.Atan2(playerReverseDirection.X, playerReverseDirection.Y) * 180 / Math.PI);
                angle += rand.Next(-90, 90);
                angle -= 90;

        Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
        newDir = Vector2.Normalize(newDir);
        return newDir;
    }

    //=======================================================================================

}