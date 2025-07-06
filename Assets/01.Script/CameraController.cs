using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;
    public new Camera camera { get { return m_Camera; } }

    [SerializeField]
    private float m_DefaultSize = 3;
    public float defaultSize { get { return m_DefaultSize; } }

    public float curSize { get; private set; }
    public Vector3 curDes { get; private set; }

    [SerializeField]
    private float m_MaxSize = 5;
    public float maxSize { get { return m_MaxSize; } }

    [SerializeField]
    private float m_MinSize = 2;
    public float minSize { get { return m_MinSize; } }

    [SerializeField]
    private float m_ZoomSpeed = 20f;
    public float zoomSpeed { get { return m_ZoomSpeed; } }

    [SerializeField]
    private Vector2 m_LimitedPosLeftTop;
    public Vector2 limitedPosLeftTop { get { return m_LimitedPosLeftTop; } }

    [SerializeField]
    private Vector2 m_LimitedPosRightBottom;
    public Vector2 limitedPosRightBottom { get { return m_LimitedPosRightBottom; } }

    [SerializeField]
    private float m_MoveSpeed = 20f;
    public float moveSpeed { get { return m_MoveSpeed; } }

    [SerializeField]
    private float m_MoveSensitivity = 0.5f;
    public float moveSensitivity { get { return m_MoveSensitivity; } }

    [SerializeField]
    private GameObject m_Background;
    public GameObject background => m_Background;

    [SerializeField]
    private Vector3 m_OriginBackgroundScale;
    public Vector3 originBackgroundScale => m_OriginBackgroundScale;

    public void Initialize()
    {
        SetSize(defaultSize, true);
    }

    public void SetZoomSize(float zoomSize)
    {
        SetSize(zoomSize, false);
    }

    public void Zoom(float addSize)
    {
        SetSize(curSize + addSize, false);
    }

    public void Move(Vector2 addPos)
    {
        SetDes(transform.position + ((new Vector3(addPos.x, addPos.y, 0) * moveSensitivity)) / curSize);
    }

    public void SetSize(float size, bool isDirect = false)
    {
        curSize = Mathf.Clamp(size, minSize, maxSize);

        float ratio = (minSize + ((curSize - minSize) * 0.5f)) / minSize;
        background.transform.localScale = new Vector3(originBackgroundScale.x * ratio, originBackgroundScale.y * ratio, originBackgroundScale.z);

        if (isDirect) camera.orthographicSize = defaultSize;
    }

    public void SetDes(Vector3 worldPos, bool isDirect = false)
    {
        curDes = new Vector3(Mathf.Clamp(worldPos.x, limitedPosLeftTop.x, limitedPosRightBottom.x) 
                            , Mathf.Clamp(worldPos.y, limitedPosRightBottom.y, limitedPosLeftTop.y)
                            , transform.position.z);

        if (isDirect) transform.position = curDes;
    }

    private void Update()
    {
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, curSize, Time.deltaTime * zoomSpeed);
        transform.position = Vector3.Lerp(transform.position, curDes, Time.deltaTime * moveSpeed);
    }

    public void Release()
    {

    }
}