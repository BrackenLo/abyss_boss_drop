using System;
using System.Numerics;

using Raylib_cs;

public class GameFinishMenu {

    private string[] textLines;

    Color textColor = new Color(163, 0, 145, 255);

    private float initialSpacing = 0;

    private float lineSpacing = 0;

    private float xStart = 0;

    private MainMenuButton returnButton = new MainMenuButton();

    public GameFinishMenu() {

        textLines = new string[]{
            "Thanks for playing my game!",
            "This is my first game jam and first finished game so I expect it's a",
            "little rough around the edges",
            "",
            "I wanted this to be a fairly difficult game where you were more likely",
            "to die to falling than die to lack of health.",
            "If you found the game difficult then brilliant. That's what I wanted.",
            "If not, you're clearly better than I am.",
            "",
            "Anyway, thanks again for playing. If you enjoyed than please leave",
            "a comment and any feedback is always appreciated!"
        };

        returnButton = new MainMenuButton("Return to Main Menu", gotoMainMenu, textColor, GameTools.DarkenColor(textColor, 0.6f));
    }

    public void update(float delta) {

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        initialSpacing = screenHeight * 0.2F;

        Font font = Raylib.GetFontDefault();
        lineSpacing = Raylib.MeasureTextEx(font, "Anything", 22, 1).Y * 1.1f;

        xStart = screenWidth * 0.2f;

        returnButton.rect.x = xStart;
        returnButton.rect.y = screenHeight * 0.7f;
        returnButton.rect.width = screenWidth * 0.4f;
        returnButton.rect.height = screenWidth * 0.1f;

        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), returnButton.rect)) {
            returnButton.isButtonHover = true;
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON)) {
                returnButton.buttonPressed();
            }
        }
        else returnButton.isButtonHover = false;


        //Game.Instance.player.gameFinishedUpdate(delta);
    }

    public void draw() {
        Raylib.DrawText(textLines[0], (int)(xStart * 0.85f), (int)initialSpacing,  40, textColor);
        for (int n = 1; n < textLines.Length; n++) {
            Raylib.DrawText(textLines[n], (int)(xStart * 0.85f), (int)(initialSpacing + (n + 1) * lineSpacing), 22, textColor);
        }
        Raylib.DrawText($"You beat the game in {Game.Instance.gameTime.ToString("0.00")} seconds!", (int)xStart,(int)(initialSpacing + (textLines.Length + 1) * lineSpacing), 36, textColor);

        Raylib.DrawRectangleRec(returnButton.rect, returnButton.ButtonColor);
        Vector2 buttonTextPos = new Vector2(
                returnButton.rect.x + returnButton.rect.width * 0.03f,
                returnButton.rect.y + returnButton.rect.height / 2
            );
        Raylib.DrawText(returnButton.text, (int)buttonTextPos.X, (int)(buttonTextPos.Y - returnButton.rect.height * 0.1f), 20, Color.BLACK);

        Vector2 mousePos = Raylib.GetMousePosition();
        Vector2 cursorSize = new Vector2(7 * 4, 7 * 4);

        Raylib.DrawTexturePro( 
            Game.Instance.cursorTexture,
            new Rectangle(0, 0, 7, 7),
            new Rectangle(mousePos.X, mousePos.Y, cursorSize.X, cursorSize.Y),
            cursorSize / 2,
            0,
            GameTools.DarkenColor(textColor, 1.4f)
        );

    }

    private void gotoMainMenu() {
        Game.Instance.mainMenuActive = true;
        Game.Instance.gameFinishedMenuActive = false;
    }

}