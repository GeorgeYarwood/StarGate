using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds all the games input buttons, and also scene names
public class InputHolder : MonoBehaviour
{
    //Scene names
    public const string GAME_SCENE = "GameScene";
    public const string MENU_SCENE = "MainMenu";
    public const string PAUSE_MENU = "PauseMenu";

    //Inputs
    public const string MOVE_LEFT = "MoveLeft";
    public const string MOVE_RIGHT = "MoveRight";
    public const string MOVE_UP = "MoveUp";
    public const string MOVE_DOWN = "MoveDown";
    public const string FIRE = "Fire";
    public const string SKIP_DIALOGUE_BUTTON = "SkipDialogue";
}
