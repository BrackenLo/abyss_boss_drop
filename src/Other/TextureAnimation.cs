using System;
using System.Numerics;

using Raylib_cs;

public struct TextureAnimation {

    private int totalFrames;
    private int currentFrame;
    private float framesPerSecond;

    private Vector2 frameSize;
    private Texture2D animationTexture;

    private float timer;
    private float timeBetween;

    public TextureAnimation(int newTotalFrames, float newFramesPerSecond, Vector2 newFrameSize, Texture2D newAnimationTexture) {
        totalFrames = newTotalFrames;
        framesPerSecond = newFramesPerSecond;

        frameSize = newFrameSize;
        animationTexture = newAnimationTexture;

        currentFrame = 0;
        timer = 0;
        timeBetween = 1 / framesPerSecond;
    }

    public void update(float delta) {

        timer += delta;

        if (timer >= timeBetween) {
            timer -= timeBetween;

            if (currentFrame >= totalFrames - 1)
                currentFrame = 0;
            else
                currentFrame += 1;
        }
    }

    public void drawTexture(Rectangle destination, float rotation, bool flipped, Color tint) {
        Rectangle framePos = new Rectangle(
            frameSize.X * currentFrame,
            0,
            frameSize.X,
            frameSize.Y
        );

        if (flipped) {
            framePos.x += framePos.width;
            framePos.width *= -1;
        }

        Raylib.DrawTexturePro(
            animationTexture,
            framePos,
            destination,
            new Vector2(),
            rotation,
            tint
        );
    }
}