using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using TMPro;

public class T
{
    public static string S1 = "static string";
    public const string C1 = "const string";
}

public class RefInt : IEquatable<RefInt>
{
    public int value;

    public static RefInt operator +(RefInt a, RefInt b)
    {
        return new RefInt() { value = a.value + b.value };
    }

    public static bool operator ==(RefInt a, RefInt b)
    {
        return a.value == b.value;
    }

    public static bool operator !=(RefInt a, RefInt b)
    {
        return !(a.value == b.value);
    }

    public override bool Equals(object a)
    {
        return Equals((RefInt)a);
    }

    public bool Equals(RefInt a)
    {
        return value == a.value;
    }
}

public class Test : MonoBehaviour
{
    [SerializeField]
    private Transform obj1;

    [SerializeField]
    private Transform obj2;

    [SerializeField]
    private GameObject body;

    public Image slider;
    public int hp;
    public TextMeshProUGUI hpTxt;

    RefInt refInt;
    Func<int, int> OnAddInt2;

    private void Start()
    {
        Debug.Log(T.S1);
        Debug.Log(T.C1);

        refInt = new RefInt();

        Ani_Idle_1_1();

        hp = 10000;
        UpdateHp();
        //hpTxt.text = string.Format("{0} (x{1})", hp % 2000, hp / 2000);
    }

    public void UpdateHp()
    {
        int barCnt = hp / 2000;
        hpTxt.text = string.Format("{0} (x{1})", hp, barCnt);

        float fillAmount = Mathf.Clamp01((hp % 2000) / 2000);
        if (barCnt > 0 && fillAmount == 0)
        {
            fillAmount = 1;
        }
        slider.fillAmount = fillAmount;
    }

    private void Ani_Idle_1_1()
    {
        Debug.Log("Ani_Idle_1_1");
        body.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.5f); //.OnComplete(Ani_Idle_1_2);
        body.transform.DOScale(0.055f, 0.5f).SetLoops(2);
    }

    private void Ani_Idle_1_2()
    {
        Debug.Log("Ani_Idle_1_2");
        body.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.5f).OnComplete(Ani_Idle_1_1);
        body.transform.DOScale(0.055f, 0.5f).SetLoops(2);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            hp -= 200;
            UpdateHp();
            //transform.DOShakePosition(0.1f, new Vector3(0.2f, 0, 0), 4, 90, false, false);
            //OnAddInt2 = null;fdf
            //OnAddInt2 += OnAddInt_1;
            //OnAddInt2 += OnAddInt_2;
            //OnAddInt2 += OnAddInt_3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnAddInt2 = null;
            OnAddInt2 += OnAddInt_2;
            OnAddInt2 += OnAddInt_3;
            OnAddInt2 += OnAddInt_1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnAddInt2 = null;
            OnAddInt2 += OnAddInt_3;
            OnAddInt2 += OnAddInt_2;
            OnAddInt2 += OnAddInt_1;
        }
    }

    public int OnAddInt_1(int amount)
    {
        return amount + 1;
    }

    public int OnAddInt_2(int amount)
    {
        return amount + 2;
    }

    public int OnAddInt_3(int amount)
    {
        return amount + 3;
    }
}
