using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPIDs
{
    public const string STARTER_PACKAGE_1 = "starter_package_1";

    public const string REMOVE_ADS = "remove_ads";
    public const string REMOVE_ADS_CONTINUE = "remove_ads_continue";
    public const string REMOVE_ADS_ALL = "remove_ads_all";

    public const string STAR_BUNDLE_1 = "star_bundle_1";
    public const string STAR_BUNDLE_2 = "star_bundle_2";
    public const string STAR_BUNDLE_3 = "star_bundle_3";
}


public class MyIAPManager : BaseMonoSingleton<MyIAPManager>, IStoreListener
{
    public enum EInitStatus
    {
        Initializing = 0,
        InitSuccess = 1,
        InitFailure = 2,
    }

    private IStoreController controller; // 구매 과정을 제어하는 함수를 제공
    private IExtensionProvider extensions; // 여러 플랫폼을 위한 확장 처리를 제공

    public EInitStatus initStatus { get; private set; }
    //public bool isInitialized => (controller != null && extensions != null);
    public bool isInitialized => initStatus == EInitStatus.InitSuccess;

    protected override void Awake()
    {
        base.Awake();

        if (!isInitialized)
        {
            Debug.LogFormat("IAPManager Initializeing...");

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(IAPIDs.STARTER_PACKAGE_1, ProductType.NonConsumable);
            builder.AddProduct(IAPIDs.REMOVE_ADS, ProductType.NonConsumable);
            builder.AddProduct(IAPIDs.REMOVE_ADS_CONTINUE, ProductType.NonConsumable);
            builder.AddProduct(IAPIDs.REMOVE_ADS_ALL, ProductType.NonConsumable);
            builder.AddProduct(IAPIDs.STAR_BUNDLE_1, ProductType.Consumable);
            builder.AddProduct(IAPIDs.STAR_BUNDLE_2, ProductType.Consumable);
            builder.AddProduct(IAPIDs.STAR_BUNDLE_3, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }
    }
    
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAPManager Initialized.");

        this.controller = controller;
        this.extensions = extensions;
        
        initStatus = (controller != null && extensions != null) ? EInitStatus.InitSuccess : EInitStatus.InitFailure;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        initStatus = EInitStatus.InitFailure;
        Debug.LogErrorFormat("IAPManager Initialization Failure {0}", error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        // Complete : 애플리케이션이 구매 처리를 완료했으며 다시 알리지 않아야 한다.
        // Pending : 애플리케이션이 여전히 구매를 처리하고 있으며, 
        //           IStoreController의 ConfirmPendingPurchase메소드가 호출되지 않는 한
        //           다음 애플리케이션을 시작할 때 ProcessPurchase가 다시 호출된다.

        OnPurchaseComplete(e.purchasedProduct);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseComplete(Product product)
    {
        Debug.LogFormat("OnPurchaseComplete product ({0})", product.transactionID);

        if (CheckValidation(product.receipt))
        {
            switch (product.definition.id)
            {
                case IAPIDs.STARTER_PACKAGE_1:
                    AdsManager.Instance.HideBannerAds();
                    UIManager_Lobby.Instance.shopUI.UpdateUI();
                    UserInfo.Instance.AddStarCount(200, true, UIManager.Instance.foreCanvas.transform.position);
                    UserInfo.Instance.Save();
                    HasPurchased(product.definition.id);
                    break;

                case IAPIDs.REMOVE_ADS:
                    AdsManager.Instance.HideBannerAds();
                    UIManager_Lobby.Instance.shopUI.UpdateUI();
                    ShowCompleteBuyProduct();
                    HasPurchased(product.definition.id);
                    break;

                case IAPIDs.REMOVE_ADS_CONTINUE:
                    UIManager_Lobby.Instance.shopUI.UpdateUI();
                    ShowCompleteBuyProduct();
                    HasPurchased(product.definition.id);
                    break;

                case IAPIDs.REMOVE_ADS_ALL:
                    AdsManager.Instance.HideBannerAds();
                    UIManager_Lobby.Instance.shopUI.UpdateUI();
                    ShowCompleteBuyProduct();
                    HasPurchased(product.definition.id);
                    break;

                case IAPIDs.STAR_BUNDLE_1:
                    UserInfo.Instance.AddStarCount(100, true, UIManager.Instance.foreCanvas.transform.position);
                    UserInfo.Instance.Save();
                    break;

                case IAPIDs.STAR_BUNDLE_2:
                    UserInfo.Instance.AddStarCount(250, true, UIManager.Instance.foreCanvas.transform.position);
                    UserInfo.Instance.Save();
                    break;

                case IAPIDs.STAR_BUNDLE_3:
                    UserInfo.Instance.AddStarCount(600, true, UIManager.Instance.foreCanvas.transform.position);
                    UserInfo.Instance.Save();
                    break;

                // 새로운 상품이 등록되었지만 구현되지 않았음.
                default:
                    Debug.LogErrorFormat("Product({0}) Purchase not implemented.", product.definition.id);
                    ShowFailureBuyProduct(3000);
                    break;
            }
        }
        else
        {
            // 결제 영수증이 유효하지 않은 경우
            ShowFailureBuyProduct(1000);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogFormat("OnPurchaseFailed product ({0}) reason ({1})", product.transactionID, reason);

        ShowFailureBuyProduct(2000 + (int)reason);
    }

    public void BuyProduct(string productId)
    {
        if (!isInitialized)
        {
            Debug.LogErrorFormat("IAPManager is not initialized.");
            return;
        }

        var product = controller.products.WithID(productId);

        if(product != null && product.availableToPurchase)
        {
            Debug.LogFormat("Try to {0}({1})", MethodBase.GetCurrentMethod().Name, productId);
            // 구매 시도
            controller.InitiatePurchase(productId);
        }
        else
        {
            Debug.LogFormat("{0}({1}) failure.", MethodBase.GetCurrentMethod().Name, productId);
        }
    }

    public void RestorePurchase()
    {
        if(!isInitialized)
        {
            Debug.LogErrorFormat("IAPManager is not initialized.");
            return;
        }

        if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.LogFormat("Try to {0}", MethodBase.GetCurrentMethod().Name);

            var appleExt = extensions.GetExtension<IAppleExtensions>();
            appleExt.RestoreTransactions(result => Debug.LogFormat("{0} Result : {1}", MethodBase.GetCurrentMethod().Name, result));
        }
    }

    public bool HasPurchased(string productId)
    {
        if(!isInitialized)
        {
            Debug.LogErrorFormat("IAPManager is not initialized.");
            return false;
        }
        
        var product = controller.products.WithID(productId);
        if(product != null)
        {
            Debug.Log(product.receipt);
            return product.hasReceipt;
        }

        return false;
    }

    public string GetLog()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("#### IAPManager Log #### ");
        foreach (var p in controller.products.all)
        {
            sb.AppendLine(string.Format("# {0} / {1} / {2}", p.definition.id, p.hasReceipt, p.receipt));
        }
        sb.AppendLine("############");
        return sb.ToString();
    }

    public void ShowCompleteBuyProduct()
    {
        UIManager.ShowMessageBoxUI
            (UITextManager.GetText("알림")
            , UITextManager.GetText("00037")
            , UITextManager.GetText("확인")
            , UIManager.Instance.messageBoxUI.Close);
    }

    public void ShowFailureBuyProduct(int errorCode)
    {
        Debug.LogFormat("ShowFailureBuyProduct ErrorCode ({0})", errorCode);

        if (errorCode == 2000 + (int)PurchaseFailureReason.DuplicateTransaction)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("00041")
                , UITextManager.GetText("확인")
                , UIManager.Instance.messageBoxUI.Close);
        }
        else if (errorCode == 2000 + (int)PurchaseFailureReason.UserCancelled)
        {
            // 유저가 취소했을 경우 아무것도 띄우지 않는다.
        }
        else
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , string.Format("{0}\n\r(ErrorCode : {1})", UITextManager.GetText("00038"), errorCode)
                , UITextManager.GetText("확인")
                , UIManager.Instance.messageBoxUI.Close);
        }
    }

    /// <summary>
    /// 인앱결제 영수증의 유효성을 확인한다.
    /// </summary>
    /// <param name="receipt">Product.receipt</param>
    /// <returns></returns>
    public static bool CheckValidation(string receipt)
    {
        bool validPurchase = true; // Presume valid for platforms with no R.V.

#if UNITY_EDITOR
        return validPurchase;

#elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(receipt);
            // For informational purposes, we list the receipt(s)
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.LogFormat("Receipt is valid. Contents : ProductID({0}) PurchaseDate({1}) TransactionID({2})"
                    , productReceipt.productID
                    , productReceipt.purchaseDate
                    , productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException e)
        {
            Debug.Log("Invalid receipt, not unlocking content");
            validPurchase = false;
        }
#endif

        return validPurchase;
    }
}