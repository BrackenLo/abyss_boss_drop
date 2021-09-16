using System;
using System.Numerics;

class GameTools {

    public static float Lerp(float val1, float val2, float speed) {
        //return val1 * (1 - speed) + val2 * speed;
        return val1 + (val2 - val1) * speed;
    }

    public static Vector2 Lerp(Vector2 val1, Vector2 val2, float speed) {
        float returnX = Lerp(val1.X, val2.X, speed);
        float returnY = Lerp(val1.Y, val2.Y, speed);
        return new Vector2(returnX, returnY);
    }

}