using System;
using System.Numerics;

using Raylib_cs;

public struct MainMenuButton {

    public delegate void buttonPressHandler();
    public event buttonPressHandler buttonPressEvent;

    public Rectangle rect;
    public string text;

    public Color buttonNormalColor;
    public Color buttonHoverColor;
    public bool isButtonHover;

    public MainMenuButton(string newText, buttonPressHandler buttonPress, Color newButtonColor, Color newButtonHoverColor) {
        text = newText;
        buttonPressEvent = buttonPress;
        rect = new Rectangle();

        buttonNormalColor = newButtonColor;
        buttonHoverColor = newButtonHoverColor;
        isButtonHover = false;
    }

    public void buttonPressed() {
        if (buttonPressEvent != null)   buttonPressEvent.Invoke();
    }

    public Color ButtonColor {
        get {
            if (isButtonHover)return buttonHoverColor;
            else return buttonNormalColor;
        }
    }
}