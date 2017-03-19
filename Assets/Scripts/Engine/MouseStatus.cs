using UnityEngine;


public class MouseStatus
{
    public enum Status
    {
        kIdle,
        kDown,
        kStartMove,
        kMove,
        kUp
    }
    public Status status = Status.kIdle;
    public Vector3 begin;
    public Vector3 now;
    public Vector3 startMove;

    public Vector3 nowWorld
    {
        get
        {
            return Camera.main.ScreenToWorldPoint(now);
        }
    }

    public bool moved
    {
        get
        {
            return distance * Utils.perPixel >= minMoveDistance;
        }
    }

    public float minMoveDistance = 0.5f;
    float distance;

    public void update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            status = Status.kDown;
            now = Input.mousePosition;
            begin = now;
            distance = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 newNow = Input.mousePosition;
            distance += Vector3.Distance(newNow, now);
            now = newNow;
            switch (status)
            {
                case Status.kDown:
                    if (moved)
                    {
                        status = Status.kStartMove;
                        startMove = now;
                    }
                    break;
                case Status.kStartMove:
                    status = Status.kMove;
                    break;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            status = Status.kUp;
        }
        else if (status != Status.kIdle)
        {
            status = Status.kIdle;
            distance = 0;
        }
    }
}
