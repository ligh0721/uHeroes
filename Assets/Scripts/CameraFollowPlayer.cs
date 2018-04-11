using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour {
    public Camera m_camera;
    public float elastic = 0.05f;
    public float CONST_MAX_ELASTIC = 0.3f;
    public const float CONST_SLOW = 0.6f;
    public const float CONST_FAST_DELTA = 0.2f;

    public GameObject followed;

    void Start() {
    }

    void FixedUpdate() {
        if (followed == null) {
            return;
        }

        Transform followedT = followed.transform;
        Vector3 pos = transform.position;
        Vector2 hsz = Utils.halfCameraSize;

        float slowX = hsz.x * CONST_SLOW;
        float fastDeltaX = hsz.x * CONST_FAST_DELTA;

        float deltaXLeft = followedT.position.x - pos.x + slowX;  // < 0
        if (deltaXLeft < 0) {
            float deltaXLeftMax = deltaXLeft + fastDeltaX;
            //pos.x += deltaXLeftMax < 0 ? deltaXLeftMax : deltaXLeft * elastic;
            pos.x += deltaXLeftMax < 0 ? deltaXLeft * CONST_MAX_ELASTIC : deltaXLeft * elastic;
        } else {
            float deltaXRight = followedT.position.x - pos.x - slowX;  // > 0
            if (deltaXRight > 0) {
                float deltaXRightMax = deltaXRight - fastDeltaX;
                //pos.x += deltaXRightMax > 0 ? deltaXRightMax : deltaXRight * elastic;
                pos.x += deltaXRightMax > 0 ? deltaXRight * CONST_MAX_ELASTIC : deltaXRight * elastic;
            }
        }

        float slowY = hsz.y * CONST_SLOW;
        float fastDeltaY = hsz.y * CONST_FAST_DELTA;

        float deltaYLeft = followedT.position.y - pos.y + slowY;  // < 0
        if (deltaYLeft < 0) {
            float deltaYLeftMax = deltaYLeft + fastDeltaY;
            //pos.y += deltaYLeftMax < 0 ? deltaYLeftMax : deltaYLeft * elastic;
            pos.y += deltaYLeftMax < 0 ? deltaYLeft * CONST_MAX_ELASTIC : deltaYLeft * elastic;
        } else {
            float deltaYRight = followedT.position.y - pos.y - slowY;  // > 0
            if (deltaYRight > 0) {
                float deltaYRightMax = deltaYRight - fastDeltaY;
                //pos.y += deltaYRightMax > 0 ? deltaYRightMax : deltaYRight * elastic;
                pos.y += deltaYRightMax > 0 ? deltaYRight * CONST_MAX_ELASTIC : deltaYRight * elastic;
            }
        }

        transform.position = pos;
    }
}
