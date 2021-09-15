using System;
using System.Numerics;
using Raylib_cs;

class GameCamera {

    private Camera2D camera;
    public Camera2D Camera {get {return camera;}}

    public Vector2 Target {set {camera.target = value;}}

    public GameCamera() {
        camera = new Camera2D();
        adjustCamera();
    }

    public void adjustCamera() {
        camera.zoom = 4;
        camera.offset = new Vector2(
            Raylib.GetScreenWidth() / 2.0f,
            Raylib.GetScreenHeight() / 2.0f
        );
    }

    public Vector2 GetScreenToWorld(Vector2 screenPos) {
        return Raylib.GetScreenToWorld2D(screenPos, camera);
    }
}