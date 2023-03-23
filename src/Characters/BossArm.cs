using System;
using System.Numerics;

using Raylib_cs;

public class BossArm {

    Boss parent;

    Vector2 position;
    Vector2 target;
    Vector2 velocity = Vector2.Zero;
    bool atTarget = false;
    float moveSpeed = 10;
    float maxSpeed = 3;

    int segments = 8;

    float maxMoveTimer = 0;
    float moveTimer = 0;

    public BossArm(Boss newParent) {
        parent = newParent;
        position = newParent.Origin;
        chooseNewTarget();
    }

    public void update(float delta) {

        if (!atTarget) {
            float distanceToTarget = Vector2.Distance(position, target);
            if (distanceToTarget < 0.4F) {
                position = target;
                atTarget = true;
            }
            else {
                Vector2 armDirection = Vector2.Normalize(target - position);
                velocity = Vector2.Lerp(velocity, armDirection * maxSpeed, moveSpeed * delta);
                position += velocity;
            }
        }
        else {

            if (parent.phase3PreparingPulseAttack) {

            }
            else if (parent.phase3DoingPulseAttack) {

                Random rand = new Random();
                float angle = rand.Next(0, 45);

                for (int n = 0; n < 8; n++) {
                    Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));
                    angle += 45;

                    Projectile.EnemyProjectiles.Add(new Projectile(
                        position,
                        newDir,
                        60,
                        10
                    ));
                }


            }
            else {
                moveTimer += delta;

                if (moveTimer > maxMoveTimer) {
                    chooseNewTarget();
                    moveTimer = 0;
                }
            }
        }
    }

    private void chooseNewTarget() {

        if (parent.velocity == Vector2.Zero) {
            getLocalTarget(4);
        }
        else {
            if (!getDistantTarget(70)) {
                getLocalTarget(4);
            }
        }
    }

    private bool getLocalTarget(int range) {
        Random rand = new Random();
        Vector2 currentPos = new Vector2();
        currentPos.X = (int)(parent.Origin.X / Tile.TILE_WIDTH);
        currentPos.Y = (int)(parent.Origin.Y / Tile.TILE_HEIGHT);

        Vector2[] targets = Game.Instance.map.getTilesInRange(currentPos, range);
        if (targets.Length == 0) {
            return false;
        }
        else {
            int index = rand.Next(0,  targets.Length);
            target = targets[index];
            target.X = (target.X * Tile.TILE_WIDTH) + (Tile.TILE_WIDTH / 2);
            target.Y = (target.Y * Tile.TILE_HEIGHT) + (Tile.TILE_HEIGHT / 2);

            atTarget = false;
            moveTimer = 0;
            maxMoveTimer = rand.Next(3, 8);
            return true;
        }
    }

    private bool getDistantTarget(int distance) {
        Random rand = new Random();

        Vector2 direction = Vector2.Normalize(parent.velocity);
        float angle = (float)(Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);
        angle = angle + rand.Next(-45, 45);
        Vector2 newDir = new Vector2((float)Math.Cos(angle * Math.PI / 180), (float)Math.Sin(angle * Math.PI / 180));

        Ray targetRay = new Ray(new Vector3(parent.Origin, 0), new Vector3(newDir * distance, 0));

        Vector2[] targets = Game.Instance.map.getTileRayCollisions(targetRay);
        if (targets.Length == 0) {
            return false;
        }
        else {
            int index = rand.Next(0,  targets.Length);
            target = targets[index];
            target.X += Tile.TILE_WIDTH / 2;
            target.Y += Tile.TILE_HEIGHT / 2;

            atTarget = false;
            moveTimer = 0;
            maxMoveTimer = rand.Next(5, 8);
            return true;
        }
    }


    public void draw2D(bool firstCall) {

        if (firstCall) {
            if (parent.currentPhase == Boss.BossPhase.phase2) {
                drawArm();
            }
            else {

            }
        }
        else {
            if (parent.currentPhase == Boss.BossPhase.phase2) {
                
                if (atTarget) {
                    drawHand();
                }

            }
            else {
                drawArm();
                if(atTarget) {
                    drawHand();
                }
            }
        }
    }

    private void drawArm() {

        Vector2 targetDir = Vector2.Normalize(position - parent.Origin);
        float distance = Vector2.Distance(position, parent.Origin);

        float segmentDistance = distance / segments;

        for (int x = 0; x < segments; x++) {
            Vector2 drawPosition = parent.Origin + (targetDir * (segmentDistance * x));
            Raylib.DrawCircleV(drawPosition, 2, GameTools.DarkenColor(parent.displayColor, 0.4f));
        }

    }

    private void drawHand() {
        
        int halfTileX = Tile.TILE_WIDTH / 2;
        int halfTileY = Tile.TILE_HEIGHT / 2;

        Vector2[] directions = {
            new Vector2( halfTileX,  halfTileY),
            new Vector2(-halfTileX, -halfTileY),
            new Vector2(-halfTileX,  halfTileY),
            new Vector2( halfTileX, -halfTileY),
        };

        foreach (Vector2 direction in directions) {
            Raylib.DrawCircleV(position + direction, 1, parent.displayColor);
        }
    }
}