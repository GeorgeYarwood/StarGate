using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BackgroundDirection
{
    LEFT,
    RIGHT,
    NONE
}

public class WorldScroller : MonoBehaviour
{
    [SerializeField] SpriteRenderer leftWorldSection;
    [SerializeField] SpriteRenderer middleWorldSection;
    [SerializeField] SpriteRenderer rightWorldSection;

    [SerializeField] Transform leftWorldRayOrigin;
    [SerializeField] SpriteRenderer rightWorldRayOrigin;

    bool canRunBackgroundCheck = true;
    const float BG_CHECK_DELAY = 1.0f;
    const float OUT_OF_RANGE_RESET = 1000.0f; //We reset the world position to 0 so we don't go on forever

    Vector2 leftWorldInitialPos;
    Vector2 middleWorldInitialPos;
    Vector2 rightWorldInitialPos;


    static WorldScroller instance;
    public static WorldScroller Instance
    {
        get { return instance; }
    }

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        leftWorldInitialPos = leftWorldSection.transform.position;
        middleWorldInitialPos = middleWorldSection.transform.position;
        rightWorldInitialPos = rightWorldSection.transform.position;
    }

    bool CheckBackgroundInFrame(out BackgroundDirection Direction)
    {
        RaycastHit2D LeftCentre = Physics2D.Raycast(leftWorldRayOrigin.transform.position, leftWorldRayOrigin.transform.forward);
        RaycastHit2D RightCentre = Physics2D.Raycast(rightWorldRayOrigin.transform.position, rightWorldRayOrigin.transform.forward);

        if (!LeftCentre.collider)
        {
            Direction = BackgroundDirection.LEFT;
            return false;
        }

        else if (!RightCentre.collider)
        {
            Direction = BackgroundDirection.RIGHT;
            return false;
        }

        Direction = BackgroundDirection.NONE;
        return true;
    }

    void SwapSectionDirections(BackgroundDirection Direction)
    {
        //Incase future me forgets again why these are here, it's because were assiging things to themselves so we need to cache them beforehand
        SpriteRenderer TempLeft = leftWorldSection;
        SpriteRenderer TempMiddle = middleWorldSection;
        SpriteRenderer TempRight = rightWorldSection;
        if(Direction == BackgroundDirection.LEFT)
        {
            leftWorldSection = TempRight;
            middleWorldSection = TempLeft;
            rightWorldSection = TempMiddle;
        }
        else if(Direction == BackgroundDirection.RIGHT)
        {
            leftWorldSection = TempMiddle;
            middleWorldSection = TempRight;
            rightWorldSection = TempLeft;
        }
    }

    public void ResetToZero(bool NewLevel = false)
    {
        if (NewLevel)
        {
            ResetPos();
            return;
        }
        for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
        {
            GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].ParentToBackground();
        }

        PlayerShip.Instance.ParentToBackground();

        LaserProjectile[] Projectiles = GameController.Instance.GetAllProjectiles();

        for (int p = 0; p < Projectiles.Length; p++)
        {
            Projectiles[p].ParentToBackground();
        }

        ResetPos();

        for (int p = 0; p < Projectiles.Length; p++)
        {
            Projectiles[p].DetachFromParent();
        }

        PlayerShip.Instance.DetachFromParent();
    }

    void ResetPos()
    {
        leftWorldSection.transform.position = leftWorldInitialPos;
        middleWorldSection.transform.position = middleWorldInitialPos;
        rightWorldSection.transform.position = rightWorldInitialPos;
    }

    void Update()
    {
        BackgroundDirection DirectionToUpdate;
        if (!CheckBackgroundInFrame(out DirectionToUpdate) && canRunBackgroundCheck)
        {
            for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
            {
                GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].ParentToBackground();
            }

            switch (DirectionToUpdate)
            {
                case BackgroundDirection.LEFT:
                    rightWorldSection.transform.position = new(
                        (leftWorldSection.transform.position.x - leftWorldSection.bounds.size.x),
                        rightWorldSection.transform.position.y, rightWorldSection.transform.position.z);
                    break;
                case BackgroundDirection.RIGHT:
                    leftWorldSection.transform.position = new(
                       (rightWorldSection.transform.position.x + rightWorldSection.bounds.size.x),
                       leftWorldSection.transform.position.y, leftWorldSection.transform.position.z);
                    break;
            }

            SwapSectionDirections(DirectionToUpdate);
            StartCoroutine(WaitForBackgroundUpate());
        }

        if (leftWorldSection.transform.position.x > OUT_OF_RANGE_RESET || leftWorldSection.transform.position.x < -OUT_OF_RANGE_RESET
           || rightWorldSection.transform.position.x > OUT_OF_RANGE_RESET || rightWorldSection.transform.position.x < -OUT_OF_RANGE_RESET)
        {
            //ResetToZero();
        }
    }

    public float GetMinXBounds()
    {
        return leftWorldSection.bounds.center.x - Mathf.Abs((leftWorldSection.bounds.size.x / 2.0f));
    }

    public float GetMaxXBounds()
    {
        return rightWorldSection.bounds.center.x + (rightWorldSection.bounds.size.x / 2.0f);
    }

    public float GetCurrentCentre()
    {
        return leftWorldSection.bounds.center.x + (leftWorldSection.bounds.size.x / 2);
    }

    IEnumerator WaitForBackgroundUpate()
    {
        canRunBackgroundCheck = false;
        yield return new WaitForSeconds(BG_CHECK_DELAY);
        canRunBackgroundCheck = true;
    }
}
