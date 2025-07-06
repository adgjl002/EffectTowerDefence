using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class LobbyUI_Shop : UIBase, IPointerClickHandler
{
    [Header("Package")]
    [SerializeField]
    private RectTransform m_PackageGruopRtf;
    public RectTransform packageGroupRtf => m_PackageGruopRtf;

    [SerializeField]
    private CustomButton m_StarterPackage;
    public CustomButton starterPackage => m_StarterPackage;

    [Header("ADS")]
    [SerializeField]
    private RectTransform m_ADSGroupRtf;
    public RectTransform adsGroupRtf => m_ADSGroupRtf;

    [SerializeField]
    private CustomButton m_RemoveAdsBtn;
    public CustomButton removeAdsBtn => m_RemoveAdsBtn;
    
    [SerializeField]
    private GameObject m_RemoveAdsBtnPurchasedPannel;
    public GameObject removeAdsBtnPurchasedPannel => m_RemoveAdsBtnPurchasedPannel;
    
    [SerializeField]
    private CustomButton m_RemoveAdsContinueBtn;
    public CustomButton removeAdsContinueBtn => m_RemoveAdsContinueBtn;
    
    [SerializeField]
    private GameObject m_RemoveAdsContinueBtnPurchasedPannel;
    public GameObject removeAdsContinueBtnPurchasedPannel => m_RemoveAdsContinueBtnPurchasedPannel;
    
    [SerializeField]
    private CustomButton m_RemoveAdsAllBtn;
    public CustomButton removeAdsAllBtn => m_RemoveAdsAllBtn;
    
    [SerializeField]
    private GameObject m_RemoveAdsAllBtnPurchasedPannel;
    public GameObject removeAdsAllBtnPurchasedPannel => m_RemoveAdsAllBtnPurchasedPannel;

    [Header("Star Bundle")]
    [SerializeField]
    private RectTransform m_StarBundleGroupRtf;
    public RectTransform starBundleGroupRtf => m_StarBundleGroupRtf;

    [SerializeField]
    private CustomButton m_StarBundle1Btm;
    public CustomButton starBundle1Btn => m_StarBundle1Btm;
    
    [SerializeField]
    private CustomButton m_StarBundle2Btm;
    public CustomButton starBundle2Btn => m_StarBundle2Btm;
    
    [SerializeField]
    private CustomButton m_StarBundle3Btm;
    public CustomButton starBundle3Btn => m_StarBundle3Btm;

    private void Awake()
    {
        starterPackage.OnClick += () =>
        {
            if (!MyIAPManager.Instance.HasPurchased(IAPIDs.STARTER_PACKAGE_1))
            {
                MyIAPManager.Instance.BuyProduct(IAPIDs.STARTER_PACKAGE_1);
            }
        };

        removeAdsBtn.OnClick += () =>
        {
            if (!UserInfo.Instance.IsPurchasedRemoveAds)
            {
                MyIAPManager.Instance.BuyProduct(IAPIDs.REMOVE_ADS);
            }
        };

        removeAdsContinueBtn.OnClick += () =>
        {
            if (!UserInfo.Instance.IsPurchasedRemoveAdsContinue)
            {
                MyIAPManager.Instance.BuyProduct(IAPIDs.REMOVE_ADS_CONTINUE);
            }
        };

        removeAdsAllBtn.OnClick += () =>
        {
            if (!UserInfo.Instance.IsPurchasedRemoveAdsAll && (!UserInfo.Instance.IsPurchasedRemoveAdsContinue || !UserInfo.Instance.IsPurchasedRemoveAds))
            {
                MyIAPManager.Instance.BuyProduct(IAPIDs.REMOVE_ADS_ALL);
            }
        };

        starBundle1Btn.OnClick += () =>
        {
            MyIAPManager.Instance.BuyProduct(IAPIDs.STAR_BUNDLE_1);
        };

        starBundle2Btn.OnClick += () =>
        {
            MyIAPManager.Instance.BuyProduct(IAPIDs.STAR_BUNDLE_2);
        };

        starBundle3Btn.OnClick += () =>
        {
            MyIAPManager.Instance.BuyProduct(IAPIDs.STAR_BUNDLE_3);
        };

        removeAdsBtn.label.text = UITextManager.GetText(IAPIDs.REMOVE_ADS);
        removeAdsContinueBtn.label.text = UITextManager.GetText(IAPIDs.REMOVE_ADS_CONTINUE);
        removeAdsAllBtn.label.text = UITextManager.GetText(IAPIDs.REMOVE_ADS_ALL);

        starBundle1Btn.label.text = UITextManager.GetText(IAPIDs.STAR_BUNDLE_1);
        starBundle2Btn.label.text = UITextManager.GetText(IAPIDs.STAR_BUNDLE_2);
        starBundle3Btn.label.text = UITextManager.GetText(IAPIDs.STAR_BUNDLE_3);
    }

    public override void Open()
    {
        base.Open();
        
        Debug.LogFormat("HAS REMOVE ADS ({0})", MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS));
        Debug.LogFormat("HAS REMOVE ADS CONTINUE ({0})", MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS_CONTINUE));
        Debug.LogFormat("HAS REMOVE ADS ALL ({0})", MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS_ALL));

        UpdateUI();
    }

    public void UpdateUI()
    {
        // 상품을 구매하지 않았고
        // 모든 광고 제거도 구매하지 않았고
        // 광고제거 두개다 구매하지 않았을 경우
        packageGroupRtf.gameObject.SetActive(!MyIAPManager.Instance.HasPurchased(IAPIDs.STARTER_PACKAGE_1) && !UserInfo.Instance.IsPurchasedRemoveAdsAll && !(UserInfo.Instance.IsPurchasedRemoveAds && UserInfo.Instance.IsPurchasedRemoveAdsContinue));

        removeAdsBtnPurchasedPannel.gameObject.SetActive(UserInfo.Instance.IsPurchasedRemoveAds);
        removeAdsContinueBtnPurchasedPannel.gameObject.SetActive(UserInfo.Instance.IsPurchasedRemoveAdsContinue);
        removeAdsAllBtnPurchasedPannel.gameObject.SetActive(UserInfo.Instance.IsPurchasedRemoveAdsAll || UserInfo.Instance.IsPurchasedRemoveAds || UserInfo.Instance.IsPurchasedRemoveAdsContinue);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Close();
    }
}
