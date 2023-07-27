using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds all the games input buttons, and also scene names and now playerprefs because I'm lazy as fuck
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
    public const string SPEED_BOOST = "SpeedBoost";
    public const string SKIP_DIALOGUE_BUTTON = "SkipDialogue";
    public const string DEVELOPER_MENU = "DeveloperMenu";

    //Playerprefs
    public const string LAST_LEVEL = "LastLevel";
    public const string MUSIC_VOLUME = "MusicVolume";
    public const string SFX_VOLUME = "SfxVolume";
}
