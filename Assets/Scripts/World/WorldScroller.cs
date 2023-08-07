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
    [SerializeField] SpriteRenderer rightWorldSection;

    [SerializeField] Transform leftWorldRayOrigin;
    [SerializeField] SpriteRenderer rightWorldRayOrigin;

    bool canRunBackgroundCheck = true;
    const float BG_CHECK_DELAY = 0.5f;
    const float OUT_OF_RANGE_RESET = 1000.0f; //We reset the world position to 0 so we don't go on forever

    Vector2 leftWorldInitialPos;
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
        rightWorldInitialPos = rightWorldSection.transform.position;
    }

    //public void ForceUpdateWorldScoller(BackgroundDirection Direction)
    //{
    //    canRunBackgroundCheck = false;
    //    //yield return new WaitUntil(() => !CheckBackgroundInFrame(out _));
    //    //SwapSectionDirections();
    //    switch (Direction)
    //    {
    //        case BackgroundDirection.LEFT:
    //            leftWorldSection.transform.position = new(leftWorldSection.transform.position.y - GameController.GetMapBoundsXVal,
    //                leftWorldSection.transform.position.y, leftWorldSection.transform.position.z);
    //            rightWorldSection.transform.position = new(rightWorldSection.transform.position.y - GameController.GetMapBoundsXVal,
    //                rightWorldSection.transform.position.y, rightWorldSection.transform.position.z);
    //            break;
    //        case BackgroundDirection.RIGHT:
    //            leftWorldSection.transform.position = new(leftWorldSection.transform.position.y + GameController.GetMapBoundsXVal,
    //               leftWorldSection.transform.position.y, leftWorldSection.transform.position.z);
    //            rightWorldSection.transform.position = new(rightWorldSection.transform.position.y + GameController.GetMapBoundsXVal,
    //                rightWorldSection.transform.position.y, rightWorldSection.transform.position.z);
    //            break;
    //    }
    //    canRunBackgroundCheck = true;
    //}

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

    void SwapSectionDirections()
    {
        SpriteRenderer TempLeft, TempRight;
        TempLeft = leftWorldSection;
        TempRight = rightWorldSection;
        leftWorldSection = TempRight;
        rightWorldSection = TempLeft;
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
            SpriteRenderer HitBackground;
            GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].ParentToBackground(out HitBackground);
            if (leftWorldSection.transform.position.x < -OUT_OF_RANGE_RESET)
            {
                if(HitBackground == leftWorldSection)
                {
                    float Delta = leftWorldInitialPos.x - leftWorldSection.transform.position.x;
                    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position
                        = new(GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.x + Delta,
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.y);
                }
                else if (HitBackground == rightWorldSection)
                {
                    float Delta = rightWorldInitialPos.x - rightWorldSection.transform.position.x;
                    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position
                        = new(GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.x + Delta,
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.y);
                }
            }
            else if(rightWorldSection.transform.position.x > OUT_OF_RANGE_RESET)
            {
                if (HitBackground == leftWorldSection)
                {
                    float Delta = leftWorldInitialPos.x - leftWorldSection.transform.position.x;
                    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position
                        = new(GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.x - Delta,
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.y);
                }
                else if (HitBackground == rightWorldSection)
                {
                    float Delta = rightWorldInitialPos.x - rightWorldSection.transform.position.x;
                    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position
                        = new(GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.x - Delta,
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position.y);
                }
            }
        }

        PlayerShip.Instance.ParentToBackground();

        LaserProjectile[] Projectiles = GameController.Instance.GetAllProjectiles();

        for (int p = 0; p < Projectiles.Length; p++)
        {
            Projectiles[p].ParentToBackground();
        }

        ResetPos();

        //for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
        //{
        //    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].DetachFromParent();
        //}

        for (int p = 0; p < Projectiles.Length; p++)
        {
            Projectiles[p].DetachFromParent();
        }

        PlayerShip.Instance.DetachFromParent();
    }

    void ResetPos()
    {
        leftWorldSection.transform.position = leftWorldInitialPos;
        rightWorldSection.transform.position = rightWorldInitialPos;
    }

    void Update()
    {
        BackgroundDirection DirectionToUpdate;
        if (!CheckBackgroundInFrame(out DirectionToUpdate) && canRunBackgroundCheck)
        {
            for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
            {
                SpriteRenderer HitBackground;
                GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].ParentToBackground(out HitBackground);

                if (HitBackground == rightWorldSection)
                {
                    if (DirectionToUpdate == BackgroundDirection.LEFT)
                    {
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].CorrectPosition(rightWorldSection.bounds.size.x,
                            rightWorldSection.bounds.center.x - rightWorldSection.bounds.extents.x,
                            rightWorldSection.bounds.center.x + rightWorldSection.bounds.extents.x, true);
                    }
                }
                else if (HitBackground == leftWorldSection)
                {
                    if (DirectionToUpdate == BackgroundDirection.RIGHT)
                    {
                        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].CorrectPosition(leftWorldSection.bounds.size.x,
                            leftWorldSection.bounds.center.x + leftWorldSection.bounds.extents.x,
                            leftWorldSection.bounds.center.x - leftWorldSection.bounds.extents.x, false);
                    }
                }
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

            SwapSectionDirections();
            StartCoroutine(WaitForBackgroundUpate());

            //for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
            //{
            //    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].DetachFromParent();
            //}
        }

        if (leftWorldSection.transform.position.x > OUT_OF_RANGE_RESET || leftWorldSection.transform.position.x < -OUT_OF_RANGE_RESET
           || rightWorldSection.transform.position.x > OUT_OF_RANGE_RESET || rightWorldSection.transform.position.x < -OUT_OF_RANGE_RESET)
        {
            ResetToZero();
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
