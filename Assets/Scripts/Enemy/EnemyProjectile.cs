using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : BaseProjectile
{
    [SerializeField] SpriteRenderer sRenderer;
    Vector2 startPos;
    Vector2 target;
    public Vector2 Target
    {
        set { target = value; }
    }

    void Start()
    {
        //Colour = inverted level starts colour

        //Color NewColour = GameController.AllLevels[GameController.CurrentLevel].StarsColour
        //    - Color.white;

        //if(NewColour.r < 0.0f) 
        //{
        //    NewColour.r= 0.0f;  
        //}

        //if (NewColour.g < 0.0f)
        //{
        //    NewColour.g = 0.0f;
        //}

        //if (NewColour.b < 0.0f)
        //{
        //    NewColour.b = 0.0f;
        //}

        //if (NewColour != Color.black)
        //{
        //    sRenderer.color = NewColour;
        //}
        startPos = transform.position;
    }

    public override void Tick()
    {
        Vector2 Dir = (target - (Vector2)startPos);
        Dir.Normalize();
        transform.Translate(Dir * MoveSpeed * Time.deltaTime);
    }
}
