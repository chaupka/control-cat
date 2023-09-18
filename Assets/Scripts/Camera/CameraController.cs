using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CinemachineVirtualCamera vcam;
    CinemachineFramingTransposer cinemachineTransposer;
    Vector3 baseCameraOffset;
    public Vector3 lookCameraOffset;
    public float offsetSmoothing = 0.1f;
    Coroutine offsetCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        cinemachineTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        baseCameraOffset = cinemachineTransposer.m_TrackedObjectOffset;
        var player = GameObject.FindGameObjectWithTag(LayerTag.playerTag);
        vcam.Follow = player.transform;
    }

    public void OffsetCamera(bool isLookingUpOrDown, float lookingUp = 0)
    {
        if (offsetCoroutine != null)
        {
            StopCoroutine(offsetCoroutine);
        }
        offsetCoroutine = StartCoroutine(OffsetCoroutine(isLookingUpOrDown, lookingUp));
    }

    IEnumerator OffsetCoroutine(bool isLookingUpOrDown, float lookingUp)
    {
        var offset = isLookingUpOrDown ? baseCameraOffset + lookCameraOffset * lookingUp : baseCameraOffset;
        while (cinemachineTransposer.m_TrackedObjectOffset != offset)
        {
            cinemachineTransposer.m_TrackedObjectOffset = Vector3.Lerp(cinemachineTransposer.m_TrackedObjectOffset, offset, offsetSmoothing * Time.deltaTime);
            yield return null;
        }
    }


    public IEnumerator InitializeConfiner(BoundsInt dungeonBounds)
    {
        yield return new WaitUntil(() => vcam != null);
        var confiner = vcam.GetComponent<CinemachineConfiner2D>();
        confiner.InvalidateCache();
        var collider = (PolygonCollider2D)confiner.m_BoundingShape2D;
        var dungeonCorners = new Vector2[] {
            new Vector2Int(dungeonBounds.min.x, dungeonBounds.min.y),
            new Vector2Int(dungeonBounds.min.x, dungeonBounds.max.y),
            new Vector2Int(dungeonBounds.max.x, dungeonBounds.max.y),
            new Vector2Int(dungeonBounds.max.x, dungeonBounds.min.y)
         };
        collider.points = dungeonCorners;
    }
}
