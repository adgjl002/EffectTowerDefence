using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EInputType
{
    PointDown,
    PointUp,
    Drag,
}

public struct InputData
{
    public Vector3 screenPos;
    public Vector3 deltaPos;
    public Vector3 clickPos;
    public GameObject clickObj;
}

public enum ELayerMask
{
    IngameObject = 1 << 8,
    GridBlock = 1 << 9,
}

public interface IClickableObject
{
    void OnClick();
    void OnRelease();
}

public delegate void OnInput(InputData inputData, EInputType type);

public class InputManager
{
    public static InputManager Instance { get { return AppManager.Instance.inputManager; } }

    public enum EState
    {
        NONE,
        Build,
    }

    private OnInput inputEvents;
    private IClickableObject clickedObj;
    private Vector3 oldpos;

    public EState currentState;
    public void ChangeState(EState _changeTo)
    {
        Debug.LogFormat("Change State {0}", _changeTo);
        currentState = _changeTo;
    }
    public void InitializeState()
    {
        currentState = EState.NONE;
    }
    
    public int curMask
    {
        get
        {
            switch(currentState)
            {
                case EState.NONE: return (int)ELayerMask.IngameObject | (int)ELayerMask.GridBlock;
                case EState.Build: return (int)ELayerMask.GridBlock;
                default: return 0;
            }
        }
    }

    public void Initialize()
    {
        registedUI = new Queue<UIBase>();
    }

    private Queue<UIBase> registedUI;

    public static void RegistBackspaceEvent(UIBase ui)
    {
        Instance.registedUI.Enqueue(ui);
    }

    public static void UnregistBackspaceEvent(UIBase ui)
    {
        Instance.registedUI.Dequeue();
    }

    public static void RegistInputEvent(OnInput inputEvent)
    {
        Instance.inputEvents += inputEvent;
    }

    public static void UnregistInputEvent(OnInput inputEvent)
    {
        Instance.inputEvents -= inputEvent;
    }

    public void Update(float deltaTime)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                UpdatePointerDown();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                UpdateDrag();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                UpdatePointerUp();
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if(registedUI.Count > 0)
            {
                registedUI.Peek().Close();
            }
        }
        else if(Input.GetKey(KeyCode.Q))
        {
            // 화면 확대
            AppManager.Instance.cameraController.Zoom(-deltaTime * GameSettingsManager.CameraZoomSensitivity);
        }
        else if(Input.GetKey(KeyCode.W))
        {
            // 화면 축소
            AppManager.Instance.cameraController.Zoom(deltaTime * GameSettingsManager.CameraZoomSensitivity);
        }
#elif UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            if(Input.touchCount == 1)
            {
                switch (Input.GetTouch(0).phase)
                {
                    case TouchPhase.Began: UpdatePointerDown(); break;
                    case TouchPhase.Ended: UpdatePointerUp(); break;
                    case TouchPhase.Moved: UpdateDrag(); break;
                }
            }
            else if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId))
            {
                if(Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                    var firstTouch = Input.GetTouch(0);
                    var secondTouch = Input.GetTouch(1);

                    // 이전 위치
                    var firstTouchPrePos = firstTouch.position - firstTouch.deltaPosition;
                    var secondTouchPrePos = secondTouch.position - secondTouch.deltaPosition;

                    var preDis = Vector2.Distance(firstTouchPrePos, secondTouchPrePos);
                    var curDis = Vector2.Distance(firstTouch.position, secondTouch.position);
                    if(preDis > curDis)
                    {
                        // 화면 확대
                        AppManager.Instance.cameraController.Zoom(-deltaTime * (curDis - preDis) * GameSettingsManager.CameraZoomSensitivity);
                    }
                    else if(preDis < curDis)
                    {
                        // 화면 축소
                        AppManager.Instance.cameraController.Zoom(-deltaTime * (curDis - preDis) * GameSettingsManager.CameraZoomSensitivity);
                    }
                }
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            if(registedUI.Count > 0)
            {
                registedUI.Peek().Close();
            }
        }
#endif
    }

    public void Release()
    {
        if(registedUI != null) registedUI.Clear();
        registedUI = null;
    }
    
    public void UpdatePointerDown()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        InputData inputData = new InputData();
        inputData.screenPos = oldpos = Input.mousePosition;
        
        Ray ray = Camera.main.ScreenPointToRay(inputData.screenPos);

        Debug.DrawRay(ray.origin, ray.direction * 100);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 100, curMask);
        if(hit.collider != null)
        {
            inputData.clickObj = hit.collider.gameObject;
            inputData.clickPos = hit.point;
            clickedObj = hit.transform.GetComponent<IClickableObject>();
            if (clickedObj != null)
            {
                clickedObj.OnClick();
            }
        }

        if (inputEvents != null) inputEvents(inputData, EInputType.PointDown);

#elif UNITY_ANDROID || UNITY_IPHONE

        InputData inputData = new InputData();
        inputData.screenPos = oldpos = Input.GetTouch(0).position;

        Ray ray = Camera.main.ScreenPointToRay(inputData.screenPos);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 100, curMask);
        if (hit.collider != null)
        {
            inputData.clickObj = hit.transform.gameObject;
            inputData.clickPos = hit.point;
            clickedObj = hit.transform.GetComponent<IClickableObject>();
            if (clickedObj != null)
            {
                clickedObj.OnClick();
            }
        }

        if (inputEvents != null) inputEvents(inputData, EInputType.PointDown);
#endif
    }

    public void UpdateDrag()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        InputData inputData = new InputData();
        inputData.deltaPos = oldpos - Input.mousePosition;

        oldpos = inputData.screenPos = Input.mousePosition;
        if (inputEvents != null) inputEvents(inputData, EInputType.Drag);
#elif UNITY_ANDROID || UNITY_IPHONE

        InputData inputData = new InputData();
        inputData.deltaPos = oldpos - (Vector3)Input.GetTouch(0).position;

        oldpos = inputData.screenPos = Input.GetTouch(0).position;
        if (inputEvents != null) inputEvents(inputData, EInputType.Drag);
#endif
    }

    public void UpdatePointerUp()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (clickedObj != null)
        {
            clickedObj.OnRelease();
            clickedObj = null;
        }
        InputData inputData = new InputData();
        inputData.screenPos = Input.mousePosition;
        if (inputEvents != null) inputEvents(inputData, EInputType.PointUp);
#elif UNITY_ANDROID || UNITY_IPHONE

        InputData inputData = new InputData();
        if (clickedObj != null)
        {
            clickedObj.OnRelease();
            clickedObj = null;
        }
        inputData.screenPos = Input.GetTouch(0).position;
        if (inputEvents != null) inputEvents(inputData, EInputType.PointUp);
#endif
    }
}
