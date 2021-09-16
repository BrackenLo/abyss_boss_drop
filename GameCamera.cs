using System;
using System.Numerics;
using Raylib_cs;

class GameCamera {

    private float virtualScreenWidth = 426;
    private float virtualScreenHeight = 240;

    private Camera2D camera;
    public Camera2D Camera {get {return camera;}}

    public Vector2 Target {set {camera.target = value;}}

    public GameCamera() {
        camera = new Camera2D();
        adjustCamera();
    }

    public void adjustCamera() {

        float xScale = Raylib.GetScreenWidth() / virtualScreenWidth;
        float yScale = Raylib.GetScreenHeight() / virtualScreenHeight;

        camera.zoom = Math.Min(xScale, yScale);
        camera.offset = new Vector2(
            Raylib.GetScreenWidth() / 2.0f,
            Raylib.GetScreenHeight() / 2.0f
        );
    }

    public Vector2 GetScreenToWorld(Vector2 screenPos) {
        return Raylib.GetScreenToWorld2D(screenPos, camera);
    }

    public void moveTowards(Vector2 towards) {
        camera.target.X += (towards.X - camera.target.X) / 20;
        camera.target.Y += (towards.Y - camera.target.Y) / 20;
    }

    public void setVirtualScreen(Vector2 size) {
        setVirtualScreen(size.X, size.Y);
    }
    public void setVirtualScreen(float x, float y) {
        virtualScreenWidth = x;
        virtualScreenHeight = y;
        adjustCamera();
    }
}