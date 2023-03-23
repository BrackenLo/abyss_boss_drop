using System;
using System.Numerics;
using Raylib_cs;

public class GameCamera {

    //=======================================================================================

    private float virtualScreenWidth = 300;
    private float virtualScreenHeight = 200;

    public float VirutalScreenWidth {get {return virtualScreenWidth;}}
    public float VirutalScreenHeight {get {return virtualScreenHeight;}}

    private Camera2D camera;
    public Camera2D Camera {get {return camera;}}

    public Vector2 Target {set {camera.target = value;}}

    private float shake = 0;
    public float Shake {get {return shake;} set{shake = Math.Clamp(value, 0, 1);}}

    private float maxAngle = 3;
    private float maxOffset = 5;

    private float angle;
    private float offsetX;
    private float offsetY;

    //=======================================================================================

    public GameCamera() {
        camera = new Camera2D();
        adjustCamera();
    }

    //=======================================================================================

    public void adjustCamera() {

        float xScale = Raylib.GetScreenWidth() / virtualScreenWidth;
        float yScale = Raylib.GetScreenHeight() / virtualScreenHeight;

        camera.zoom = Math.Max(xScale, yScale);
        camera.offset = new Vector2(
            Raylib.GetScreenWidth() / 2.0f,
            Raylib.GetScreenHeight() / 2.0f
        );
    }

    //=======================================================================================

    public void update(float delta, Vector2 moveTowards) {

        Random rand = new Random();

        if (shake != 0) {
            angle = maxAngle * shake * (rand.Next(-100, 100) / 100f);
            offsetX = maxOffset * shake * (rand.Next(-100, 100) / 100f);
            offsetY = maxOffset * shake * (rand.Next(-100, 100) / 100f);

            shake = Math.Max(0, shake - delta);
        }
        

        camera.target.X += ((moveTowards.X - camera.target.X) / 20) + offsetX;
        camera.target.Y += ((moveTowards.Y - camera.target.Y) / 20) + offsetY;
        camera.rotation = angle;

    }

    //=======================================================================================

    public void setVirtualScreen(Vector2 size) {
        setVirtualScreen(size.X, size.Y);
    }
    public void setVirtualScreen(float x, float y) {
        virtualScreenWidth = x;
        virtualScreenHeight = y;
        adjustCamera();
    }

    //=======================================================================================

    public Vector2 GetScreenToWorld(Vector2 screenPos) {
        return Raylib.GetScreenToWorld2D(screenPos, camera);
    }

    //=======================================================================================
}