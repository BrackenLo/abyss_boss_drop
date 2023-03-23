using System;
using System.Numerics;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class MainMenu {

    private MainMenuButton[] buttons = new MainMenuButton[2];

    Vector2 titleTextPos;
    float subTextSpacing = 0;

    Color textColor = new Color(163, 0, 145, 255);

    Rectangle raylibIcon = new Rectangle(0, 0, 128, 128);
    int sideTextFontSize = 24;
    float sideTextHeight = 0;

    

    public MainMenu() {

        Color buttonNormalColor = textColor;
        Color buttonPressColor = GameTools.DarkenColor(textColor, 0.6f);

        buttons[0] = new MainMenuButton("Start Game", startGame, buttonNormalColor, buttonPressColor);
        buttons[1] = new MainMenuButton("Exit Game", exitGame, buttonNormalColor, buttonPressColor);

    }

    public void update(float delta) {

        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();

        float buttonWidth = screenWidth * 0.4f;
        float buttonheight = screenHeight * 0.1f;

        titleTextPos.X = (screenWidth / 2) - (buttonWidth / 2);
        titleTextPos.Y = screenHeight * 0.2F;

        Font font = GetFontDefault();
        subTextSpacing = (int)MeasureTextEx(font, "titleText", 60, 1).Y * 1.1f;

        Vector2 mousePos = GetMousePosition();
        bool mouse1Released = IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON);

        float initialSpacing = titleTextPos.Y + subTextSpacing * 2;
        float buttonSpacing = screenHeight * 0.12f;
        float buttonXPos = (screenWidth / 2) - (buttonWidth / 2);

        for (int n = 0; n < buttons.Length; n++) {

            buttons[n].rect.width = buttonWidth;
            buttons[n].rect.height = buttonheight;

            buttons[n].rect.x = buttonXPos;
            buttons[n].rect.y = initialSpacing + buttonSpacing * n;

            if (CheckCollisionPointRec(mousePos, buttons[n].rect)) {

                buttons[n].isButtonHover = true;

                if (mouse1Released) {
                    buttons[n].buttonPressed();
                }

            }
            else buttons[n].isButtonHover = false;
        }

        sideTextHeight = MeasureTextEx(font, "Test!", sideTextFontSize, 1).Y;
        raylibIcon.x = 10;
        raylibIcon.y = screenHeight - raylibIcon.height - sideTextHeight * 3;
    }

    public void draw() {

        DrawText(Game.Instance.gameName, (int)titleTextPos.X, (int)titleTextPos.Y, 60, textColor);
        DrawText("Made for the VimJam 2 on itch.io", (int)titleTextPos.X, (int)(titleTextPos.Y + subTextSpacing), 10, textColor);

        for (int n = 0; n < buttons.Length; n++) {
            DrawRectangleRec(buttons[n].rect, buttons[n].ButtonColor);
            Vector2 buttonTextPos = new Vector2(
                buttons[n].rect.x + buttons[n].rect.width * 0.03f,
                buttons[n].rect.y + (buttons[n].rect.height / 2) + buttons[n].rect.height * 0.04f
            );
            DrawText(buttons[n].text, (int)buttonTextPos.X, (int)buttonTextPos.Y, 20, Color.BLACK);
        }

        //-------------------------------------------------------------------
        //Draw Raylib and other thanks
        
        DrawTexturePro(
            Game.Instance.raylibTexture,
            new Rectangle(0, 0, 32, 32),
            raylibIcon,
            new Vector2(0, 0),
            0,
            textColor
            );
        DrawText("Made with Raylib!", (int)raylibIcon.x, (int)(raylibIcon.y + raylibIcon.height), sideTextFontSize, textColor);
        DrawText("Sound effects by JDWassabi @jdwasabi.itch.io/", (int)raylibIcon.x, (int)(raylibIcon.y + raylibIcon.height + sideTextHeight), sideTextFontSize, textColor);
        DrawText("Music by Marllon Silva (xDeviruchi) @xdeviruchi.itch.io/ ", (int)raylibIcon.x, (int)(raylibIcon.y + raylibIcon.height + sideTextHeight * 2), sideTextFontSize, textColor);

        //-------------------------------------------------------------------
        //Draw Mouse Cursor

        Vector2 mousePos = GetMousePosition();
        Vector2 cursorSize = new Vector2(7 * 4, 7 * 4);

        DrawTexturePro( Game.Instance.cursorTexture,
                        new Rectangle(0, 0, 7, 7),
                        new Rectangle(mousePos.X, mousePos.Y, cursorSize.X, cursorSize.Y),
                        cursorSize / 2,
                        0,
                        GameTools.DarkenColor(textColor, 1.4f)
        );
    }

    public void startGame() {

        Game.Instance.mainMenuActive = false;
        Game.Instance.resetGame();
    }

    public void exitGame() {

        Game.Instance.applicationRunning = false;

    }

}