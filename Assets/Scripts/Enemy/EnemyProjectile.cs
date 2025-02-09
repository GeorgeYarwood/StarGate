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
    public override void Init()
    {
        base.Init();
        startPos = transform.position;
    }

    public override void Tick()
    {
        Vector2 Dir = (target - (Vector2)startPos);
        Dir.Normalize();
        transform.Translate(Dir * MoveSpeed * Time.deltaTime);
    }
}
