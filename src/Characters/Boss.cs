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

    private bool transition = false;

    private bool immune = false;
    private bool untouchable = false;

    public bool Immune {get{return immune;}}
    public bool Untouchable {get {return untouchable;}}

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

    private Vector2 wanderLocation = Vector2.Zero;

    //------------------------------------------------------
    //Phase 3

    private int phase3MaxHealth = 150;

    //=======================================================================================

    public Boss(float x, float y) : base(x, y, Game.Instance.bossTexture, new Vector2(8, 8), 100) {
        color = new Color(239, 71, 0, 255);
        healthValueDisplay = maxHealth;
        healthChangeEvent += healthValueChanged;
    }

    //=======================================================================================

    public void update(float delta) {
        Game game = Game.Instance;

        if (!Game.DEBUG_MODE && !game.playerFallen)
            attackTimer += delta;

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
                    Vector2 playerReverseDirection = Vector2.Normalize(game.player.Origin - Origin) * -1;
                    float angle = (float)(Math.Atan2(playerReverseDirection.X, playerReverseDirection.Y) * 180 / Math.PI);
                    angle += rand.Next(-90, 90);
                    angle -= 90;

                    Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
                    newDir = Vector2.Normalize(newDir);

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

    private void startPhase2() {
        maxHealth = phase2MaxHealth;
        currentHealth = phase2MaxHealth;

        immune = true;
        transition = true;
    }

    private void startPhase3() {
        maxHealth = phase3MaxHealth;
        currentHealth = phase3MaxHealth;

        immune = true;
        transition = true;
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

        }
        else {
            Raylib.DrawTextureV(texture, position, color);
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

}