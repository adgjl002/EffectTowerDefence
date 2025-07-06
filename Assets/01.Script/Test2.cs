using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    [SerializeField]
    private RectTransform ARtf;

    [SerializeField]
    private RectTransform BRtf;

    [SerializeField]
    private LineRenderer lineRender;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLine();
    }

    public void UpdateLine()
    {
        lineRender.SetPositions(new Vector3[] { ARtf.position, BRtf.position });
    }
}
