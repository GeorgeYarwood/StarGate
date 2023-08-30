using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateBase : MonoBehaviour
{
    internal bool allowCursorVisible = true;
    public bool AllowCursorVisible
    {
        get { return allowCursorVisible; }
    }
    public virtual void OnStateEnter()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual void OnStateExit() 
    {

    }
}
