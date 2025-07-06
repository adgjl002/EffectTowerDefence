using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour {
    
    public static LobbyManager Instance { get { return AppManager.Instance.lobbyManager; } }

    public void Initialize()
    {

    }

    private void OnEnable()
    {
        StartTutorial();
    }

    private void OnDisable()
    {
        StopTutorial();
    }

    public void Release()
    {

    }

    #region < Tutorial >

    public void StartTutorial()
    {
        if(!UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Mask_Lobby))
        {
            StartCoroutine(TutorialProc());
        }
    }

    private IEnumerator TutorialProc()
    {
        yield return Tutorial_0_Proc();
        yield return Tutorial_1_Proc();
    }

    private IEnumerator Tutorial_0_Proc()
    {
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_1))
        {
            yield break;
        }
        else if (UserInfo.Instance.GetStageStarCount("SN_1") == 0)
        {
            UIManager.Instance.handUI.Open();
            UIManager.Instance.handUI.SetPosition(UIManager_Lobby.Instance.gameStartBtn.transform);
            yield break;
        }
        yield return null;
    }

    private IEnumerator Tutorial_1_Proc()
    {
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Lobby_1))
        {
            yield break;
        }
        // 이미 꽃게 타워 스킬을 구매한 경우
        else if(UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_102_1_0"))
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Lobby_1);
            UserInfo.Instance.Save();
            yield break;
        }
        // 아직 꽃게 타워 스킬을 구매하지 않은 경우
        else if(UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_1))
        {
            // 1. 스킬 맵 버튼 터치 유도
            // 2. 스킬 맵 UI에서 꽃게 타워 스킬 터치 유도
            // 3. 스킬 구매 버튼 터치 유도
            // 4. 튜토리얼 완료 팝업

            yield return new WaitForSeconds(2f);

            var lobbyUIManager = UIManager_Lobby.Instance;
            var msgPopUp = UIManager.GetMessageBoxUI();

            UIManager.ShowMessageBoxUI
                ( UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Lobby_1")
                , UITextManager.GetText("확인")
                , msgPopUp.Close);

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

            UIManager.Instance.handUI.Open();

            while(!UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_102_1_0"))
            {
                yield return null;

                if (lobbyUIManager.skillMapUI.isOpened)
                {
                    if (lobbyUIManager.skillMapUI.skillInfoUI.isOpened)
                    {
                        UIManager.Instance.handUI.SetPosition(UIManager_Lobby.Instance.skillMapUI.skillInfoUI.buyBtn.transform.position);
                    }
                    else
                    {
                        UIManager.Instance.handUI.SetPosition(UIManager_Lobby.Instance.skillMapUI.tutorialNode.transform.position);
                    }
                }
                else
                {
                    UIManager.Instance.handUI.SetPosition(UIManager_Lobby.Instance.skillMapBtn.transform);
                }
            }
            
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Lobby_2")
                , UITextManager.GetText("확인")
                , msgPopUp.Close);

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Lobby_1);
            UserInfo.Instance.Save();
            yield break;
        }
    }

    public void StopTutorial()
    {
        if(UIManager.Instance.handUI != null && UIManager.Instance.handUI.isOpened)
        {
            UIManager.Instance.handUI.Close();
        }
        StopAllCoroutines();
    }

    #endregion  
}
