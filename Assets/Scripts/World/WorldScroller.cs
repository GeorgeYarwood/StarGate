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
    }

    public void ForceUpdateWorldScoller(BackgroundDirection Direction)
    {
        canRunBackgroundCheck = false;
        //yield return new WaitUntil(() => !CheckBackgroundInFrame(out _));
        //SwapSectionDirections();
        switch (Direction)
        {
            case BackgroundDirection.LEFT:
                leftWorldSection.transform.position = new(leftWorldSection.transform.position.y - GameController.GetMapBoundsXVal,
                    leftWorldSection.transform.position.y, leftWorldSection.transform.position.z);
                rightWorldSection.transform.position = new(rightWorldSection.transform.position.y - GameController.GetMapBoundsXVal,
                    rightWorldSection.transform.position.y, rightWorldSection.transform.position.z);
                break;
            case BackgroundDirection.RIGHT:
                leftWorldSection.transform.position = new(leftWorldSection.transform.position.y + GameController.GetMapBoundsXVal,
                   leftWorldSection.transform.position.y, leftWorldSection.transform.position.z);
                rightWorldSection.transform.position = new(rightWorldSection.transform.position.y + GameController.GetMapBoundsXVal,
                    rightWorldSection.transform.position.y, rightWorldSection.transform.position.z);
                break;
        }
        canRunBackgroundCheck = true;
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

    void SwapSectionDirections()
    {
        SpriteRenderer TempLeft, TempRight;
        TempLeft = leftWorldSection;
        TempRight = rightWorldSection;
        leftWorldSection = TempRight;
        rightWorldSection = TempLeft;
    }

    void Update()
    {
        BackgroundDirection DirectionToUpdate;
        if (!CheckBackgroundInFrame(out DirectionToUpdate) && canRunBackgroundCheck)
        {
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
        }
    }

    IEnumerator WaitForBackgroundUpate()
    {
        canRunBackgroundCheck = false;
        yield return new WaitForSeconds(BG_CHECK_DELAY);
        canRunBackgroundCheck = true;
    }
}
