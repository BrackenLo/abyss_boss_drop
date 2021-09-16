using System;

using Raylib_cs;

public class Tile {

    public const int TILE_WIDTH = 8;
    public const int TILE_HEIGHT = 8;

    public static Rectangle createTile(int x, int y) {
        return new Rectangle(
            x * TILE_WIDTH,
            y * TILE_HEIGHT,
            TILE_WIDTH,
            TILE_HEIGHT
        );
    }

}