using System;
using System.Numerics;
using System.Collections.Generic;

using Raylib_cs;

public class Projectile {

    public static List<Projectile> PlayerProjectiles = new List<Projectile>();
    public static List<Projectile> EnemyProjectiles = new List<Projectile>();
    public static List<Projectile> EnemyHomingProjectiles = new List<Projectile>();
    public static List<Projectile> EnemySubmergedProjectiles = new List<Projectile>();

    public const int ProjectileRadius = 4;

    public Vector2 position;
    public Vector2 direction;
    public int speed;
    public int damage;
    public float expire;

    public Projectile(Vector2 newPos, Vector2 newDir, int newSpeed, int newDamage, float newExpire = 6) {
        position = newPos;
        direction = newDir;
        speed = newSpeed;
        damage = newDamage;
        expire = newExpire;
    }


    public static void update(float delta) {

        clearExpired();
        updatePlayerProjectiles(delta);
        updateEnemyProjectiles(delta);
        updateEnemyHomingProjectiles(delta);
        updateEnemySubmergedProjectiles(delta);
    }

    public static void clearExpired() {
        PlayerProjectiles.RemoveAll(p => p.expire <= 0);
        EnemyProjectiles.RemoveAll(p => p.expire <= 0);
        EnemyHomingProjectiles.RemoveAll(p => p.expire <= 0);
    }

    public static void clearProjectiles() {
        PlayerProjectiles.Clear();
        EnemyProjectiles.Clear();
        EnemyHomingProjectiles.Clear();
        EnemySubmergedProjectiles.Clear();
    }

    private static void updatePlayerProjectiles(float delta) {

        //Player projectiles check for enemy collision

        Boss boss = Game.Instance.boss;

        for (int n = 0; n < PlayerProjectiles.Count; n++) {
            Projectile projectile = updateProjectile(delta, PlayerProjectiles[n]);

            if (!boss.Untouchable) {
                Vector2 bossDirection = Vector2.Normalize(boss.Origin - projectile.position);
                Vector2 toCheck = projectile.position + (bossDirection * Projectile.ProjectileRadius);
                if (Raylib.CheckCollisionPointRec(toCheck, boss.Rect)) {
                    if (!boss.Immune)
                        boss.CurrentHealth -= projectile.damage;
                    projectile.expire = 0;
                }
            }

            PlayerProjectiles[n] = projectile;
        }


    }

    private static void updateEnemyProjectiles(float delta) {

        //Enemy Projectiles check for player collision

        Player player = Game.Instance.player;

        for (int n = 0; n < EnemyProjectiles.Count; n++) {
            Projectile projectile = updateProjectile(delta, EnemyProjectiles[n]);

            if (!player.IsDodging && !Game.Instance.playerFallen) {
                Vector2 playerDirection = Vector2.Normalize(player.Origin - projectile.position);
                Vector2 toCheck = projectile.position + (playerDirection * Projectile.ProjectileRadius);
                if (Raylib.CheckCollisionPointRec(toCheck, player.Rect)) {
                    player.knockback += playerDirection * (projectile.damage * player.KnockbackResistance);
                    player.CurrentHealth -= projectile.damage;
                    projectile.expire = 0;
                }
            }

            EnemyProjectiles[n] = projectile;
        }

    }

    private static void updateEnemyHomingProjectiles(float delta) {
        Player player = Game.Instance.player;

        List<Projectile> toRemove = new List<Projectile>();

        for (int n = 0; n < EnemyHomingProjectiles.Count; n++) {

            Projectile projectile = EnemyHomingProjectiles[n];
            Vector2 playerDirection = Vector2.Normalize(player.Origin - projectile.position);
            Vector2 lerpTarget = GameTools.Lerp(    projectile.direction * projectile.speed, 
                                                    playerDirection * projectile.speed,
                                                    0.02f);
                                                    //(Math.Abs(projectile.expire * -1)) / 6);

            projectile.direction = Vector2.Normalize(lerpTarget);
            projectile = updateProjectile(delta, projectile);

            if (!player.IsDodging && !Game.Instance.playerFallen) {
                Vector2 toCheck = projectile.position + (playerDirection * Projectile.ProjectileRadius);
                if (Raylib.CheckCollisionPointRec(toCheck, player.Rect)) {
                    player.knockback += playerDirection * (projectile.damage * player.KnockbackResistance);
                    player.CurrentHealth -= projectile.damage;
                    projectile.expire = 0;
                    toRemove.Add(projectile);
                }
            }
            EnemyHomingProjectiles[n] = projectile;

            if (projectile.expire <= 0 && !toRemove.Contains(projectile)) {
                projectile.expire = 6;
                EnemyProjectiles.Add(projectile);
                toRemove.Add(projectile);
            }
        }

        foreach (Projectile projectile in toRemove) {
            EnemyHomingProjectiles.Remove(projectile);
        }
    }

    private static void updateEnemySubmergedProjectiles(float delta) {

        List<Projectile> toRemove = new List<Projectile>();

        for (int n = 0; n < EnemySubmergedProjectiles.Count; n++) {
            Projectile projectile = updateProjectile(delta, EnemySubmergedProjectiles[n]);

            if (projectile.expire <= 0) {
                bool projectileCovered = false;
                foreach (Rectangle tile in Game.Instance.map.allTiles.Values) {
                    if (Vector2.Distance(new Vector2(tile.x, tile.y), projectile.position) < 15) {
                        projectileCovered = true;
                        break;
                    };
                }
                if (!projectileCovered) {
                    projectile.expire = 6;
                    EnemyHomingProjectiles.Add(projectile);
                    toRemove.Add(projectile);
                }
            }

            EnemySubmergedProjectiles[n] = projectile;
        }

        foreach(Projectile projectile in toRemove) {
            EnemySubmergedProjectiles.Remove(projectile);
        }
    }

    private static Projectile updateProjectile(float delta, Projectile toUpdate) {

        toUpdate.position = toUpdate.position + ((toUpdate.direction * toUpdate.speed) * delta);
        toUpdate.expire -= delta;
        return toUpdate;

    }

    public static void draw2D(bool firstCall, Texture2D projectileTexture) {

        if (firstCall)
            drawProjectiles(EnemySubmergedProjectiles, projectileTexture, GameTools.DarkenColor(Game.Instance.boss.GetColor(), 0.4f));
        else {
            drawProjectiles(PlayerProjectiles, projectileTexture, Game.Instance.player.GetColor());
            drawProjectiles(EnemyProjectiles, projectileTexture, Game.Instance.boss.GetColor());
            drawProjectiles(EnemyHomingProjectiles, projectileTexture, Game.Instance.boss.GetColor());
        }

    }

    private static void drawProjectiles(List<Projectile> toDraw, Texture2D projectileTexture, Color color) {
        foreach (Projectile projectile in toDraw) {
            Raylib.DrawTextureV(
                projectileTexture,
                projectile.position - new Vector2(Projectile.ProjectileRadius, Projectile.ProjectileRadius),
                color);
        }
    }

}