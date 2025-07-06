using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete("삭제 예정")]
public class IngameUI_BuildSelect : UIBase
{
    [SerializeField]
    private CustomButton m_Item1Btn;
    public CustomButton item1Btn { get { return m_Item1Btn; } }

    [SerializeField]
    private CustomButton m_Item2Btn;
    public CustomButton item2Btn { get { return m_Item2Btn; } }

    [SerializeField]
    private CustomButton m_Item3Btn;
    public CustomButton item3Btn { get { return m_Item3Btn; } }
    
    public TowerAdditionalEffect[] taes;

    public int curItemIdx;

    //private void Awake()
    //{
    //    item1Btn.OnClick += OnClickBtn1;
    //    item2Btn.OnClick += OnClickBtn2;
    //    item3Btn.OnClick += OnClickBtn3;
    //}

    //public void SetData(List<TowerAdditionalEffect> towerAdditionalEffects)
    //{
    //    taes = towerAdditionalEffects.ToArray();

    //    var tae = towerAdditionalEffects[0];
    //    if (tae == null) item1Btn.label.text = "New Tower";
    //    else item1Btn.label.text = tae.effectType.ToString();

    //    tae = towerAdditionalEffects[1];
    //    if (tae == null) item2Btn.label.text = "New Tower";
    //    else item2Btn.label.text = tae.effectType.ToString();

    //    tae = towerAdditionalEffects[2];
    //    if (tae == null) item3Btn.label.text = "New Tower";
    //    else item3Btn.label.text = tae.effectType.ToString();
    //}

    //public override void Open()
    //{
    //    if(InputManager.Instance.currentState == InputManager.EState.Build)
    //    {
    //        InputManager.UnregistInputEvent(OnInputEvent);
    //        InputManager.Instance.ChangeState(InputManager.EState.NONE);
    //    }

    //    if (IngameManager.Instance.curCost >= IngameManager.Instance.buildCost)
    //    {
    //        base.Open();
    //    }
    //    else
    //    {
    //        UIManager.Instance.ShowNoticeUI("자원이 부족합니다.", 1f);
    //    }
    //}

    //public void OnClickBtn1()
    //{
    //    curItemIdx = 0;
    //    InputManager.RegistInputEvent(OnInputEvent);
    //    InputManager.Instance.ChangeState(InputManager.EState.Build);
    //    Close();
    //}

    //public void OnClickBtn2()
    //{
    //    curItemIdx = 1;
    //    InputManager.RegistInputEvent(OnInputEvent);
    //    InputManager.Instance.ChangeState(InputManager.EState.Build);
    //    Close();
    //}

    //public void OnClickBtn3()
    //{
    //    curItemIdx = 2;
    //    InputManager.RegistInputEvent(OnInputEvent);
    //    InputManager.Instance.ChangeState(InputManager.EState.Build);
    //    Close();
    //}

    //public void OnInputEvent(InputData data, EInputType inputType)
    //{
    //    if (inputType == EInputType.PointDown)
    //    {
    //        Debug.LogFormat("On Input Event ({0} , {1})", data.clickObj, inputType);
    //        if (data.clickObj != null)
    //        {
    //            var gridBlock = data.clickObj.GetComponent<GridBlock>();
    //            if (gridBlock != null && gridBlock.gridBlockType == GridBlock.EType.Ground)
    //            {
    //                var curTAE = taes[curItemIdx];
    //                if (curTAE == null)
    //                {
    //                    gridBlock.BuildTower(new TowerData("TD-1", "PROJ-1", "TD-1", 100, 20, 1.2f, 0.6f));
    //                    IngameManager.Instance.AddCost(-100, true);
    //                    IngameManager.Instance.SetRandomTowerAdditionalEffects();
    //                }
    //                else if(gridBlock.tower.RegistAdditionalEffect(curTAE))
    //                {
    //                    IngameManager.Instance.AddCost(-100, true);
    //                    IngameManager.Instance.SetRandomTowerAdditionalEffects();
    //                }
    //                else
    //                {
    //                    UIManager.Instance.ShowNoticeUI("선택한 타워는 더이상 효과를 추가할 수 없습니다.", 1.5f);
    //                }
    //            }
    //        }

    //        InputManager.UnregistInputEvent(OnInputEvent);
    //        InputManager.Instance.ChangeState(InputManager.EState.NONE);
    //    }
    //}
}
