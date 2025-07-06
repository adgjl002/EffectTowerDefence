//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Purchasing;
//using UnityEngine.Purchasing.Security;

//// -------------------------------------
////
//// 판매 아이템의 ID값을 가지고 있는 클래스
//// (해당 클래스는 사용해도되고 안해도 됨)
////
//// -------------------------------------
//public class IAPIDs
//{
//    public const string REMOVE_ADS = "remove_ads";
//    public const string REMOVE_ADS_CONTINUE = "remove_ads_continue";
//    public const string REMOVE_ADS_ALL = "remove_ads_all";

//    public const string STAR_BUNDLE_1 = "star_bundle_1";
//    public const string STAR_BUNDLE_2 = "star_bundle_2";
//    public const string STAR_BUNDLE_3 = "star_bundle_3";
//}

//public class IAPManager : MonoBehaviour, IStoreListener
//{
//    public enum EInitStatus
//    {
//        Initializing = 0,
//        InitSuccess = 1,
//        InitFailure = 2,
//    }

//    private IStoreController controller; // 구매 과정을 제어하는 함수를 제공
//    private IExtensionProvider extensions; // 여러 플랫폼을 위한 확장 처리를 제공

//    public static IAPManager Instance { get; private set; }

//    public EInitStatus initStatus { get; private set; }
//    public bool isInitialized => initStatus == EInitStatus.InitSuccess;

//    private void Awake()
//    {
//        if(Instance != null)
//        {
//            Destroy(gameObject);
//        }
//        Instance = this;

//        if (!isInitialized)
//        {
//            Debug.LogFormat("IAPManager Initializeing...");

//            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); ;

//            // -------------------------------------
//            //
//            // 판매해야할 제품들을 이곳에서 등록한다.
//            //
//            // -------------------------------------

//            builder.AddProduct(IAPIDs.REMOVE_ADS, ProductType.NonConsumable);
//            builder.AddProduct(IAPIDs.REMOVE_ADS_CONTINUE, ProductType.NonConsumable);
//            builder.AddProduct(IAPIDs.REMOVE_ADS_ALL, ProductType.NonConsumable);
//            builder.AddProduct(IAPIDs.STAR_BUNDLE_1, ProductType.Consumable);
//            builder.AddProduct(IAPIDs.STAR_BUNDLE_2, ProductType.Consumable);
//            builder.AddProduct(IAPIDs.STAR_BUNDLE_3, ProductType.Consumable);

//            UnityPurchasing.Initialize(this, builder);
//        }
//    }

//    /// <summary>
//    /// 초기화 성공 콜백
//    /// </summary>
//    /// <param name="controller"></param>
//    /// <param name="extensions"></param>
//    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//    {
//        Debug.Log("IAPManager Initialized.");

//        this.controller = controller;
//        this.extensions = extensions;

//        initStatus = (controller != null && extensions != null) ? EInitStatus.InitSuccess : EInitStatus.InitFailure;
//    }

//    /// <summary>
//    /// 초기화 실패 콜백
//    /// </summary>
//    /// <param name="error"></param>
//    public void OnInitializeFailed(InitializationFailureReason error)
//    {
//        Debug.LogErrorFormat("IAPManager Initialization Failure {0}", error);
//        initStatus = EInitStatus.InitFailure;
//    }

//    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
//    {
//        // Complete : 애플리케이션이 구매 처리를 완료했으며 다시 알리지 않아야 한다.
//        // Pending : 애플리케이션이 여전히 구매를 처리하고 있으며, 
//        //           IStoreController의 ConfirmPendingPurchase메소드가 호출되지 않는 한
//        //           다음 애플리케이션을 시작할 때 ProcessPurchase가 다시 호출된다.

//        OnPurchaseComplete(e.purchasedProduct);

//        return PurchaseProcessingResult.Complete;
//    }

//    /// <summary>
//    /// 구매 시도 시 결재 성공 콜백
//    /// </summary>
//    /// <param name="product"></param>
//    public void OnPurchaseComplete(Product product)
//    {
//        Debug.LogFormat("OnPurchaseComplete product ({0})", product.transactionID);

//        // -------------------------------------
//        //
//        // 구매 성공 시 제품별 처리를 이곳에 작성
//        //
//        // -------------------------------------

//        if (CheckValidation(product.receipt))
//        {
//            switch (product.definition.id)
//            {
//                case IAPIDs.REMOVE_ADS:
//                    // 광고 제거
//                    break;

//                case IAPIDs.REMOVE_ADS_CONTINUE:
//                    // 무료 이어하기
//                    break;

//                case IAPIDs.REMOVE_ADS_ALL:
//                    // 모든 광고 제거
//                    break;

//                case IAPIDs.STAR_BUNDLE_1:
//                    // 게임 재화 구매
//                    break;

//                case IAPIDs.STAR_BUNDLE_2:
//                    // 게임 재화 구매
//                    break;

//                case IAPIDs.STAR_BUNDLE_3:
//                    // 게임 재화 구매
//                    break;

//                // 새로운 상품이 등록되었지만 구현되지 않았음.
//                default:
//                    break;
//            }
//        }
//        else
//        {
//            // 결제 영수증이 유효하지 않은 경우
//        }
//    }

//    /// <summary>
//    /// 구매 시도 시 결재 실패 콜백
//    /// </summary>
//    /// <param name="product"></param>
//    /// <param name="reason"></param>
//    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
//    {
//        Debug.LogFormat("OnPurchaseFailed product ({0}) reason ({1})", product.transactionID, reason);

//        // -------------------------------------
//        //
//        // 결재 실패 또는 취소 했을 때 처리 작성
//        //
//        // -------------------------------------
//    }

//    /// <summary>
//    /// 구매 시도 (해당 함수를 통해서 제품 구매한다.)
//    /// </summary>
//    /// <param name="productId"></param>
//    public void BuyProduct(string productId)
//    {
//        if (!isInitialized)
//        {
//            Debug.LogErrorFormat("IAPManager is not initialized.");
//            return;
//        }

//        var product = controller.products.WithID(productId);

//        if (product != null && product.availableToPurchase)
//        {
//            Debug.LogFormat("Try to {0}({1})", MethodBase.GetCurrentMethod().Name, productId);
//            controller.InitiatePurchase(productId);
//        }
//        else
//        {
//            Debug.LogFormat("{0}({1}) failure.", MethodBase.GetCurrentMethod().Name, productId);
//        }
//    }

//    /// <summary>
//    /// 구매 복원
//    /// * Android에서는 필요 없음
//    /// * IOS에서는 해당 기능을 사용할 수 있도록 해야함
//    /// </summary>
//    public void RestorePurchase()
//    {
//        if (!isInitialized)
//        {
//            Debug.LogErrorFormat("IAPManager is not initialized.");
//            return;
//        }

//        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
//        {
//            Debug.LogFormat("Try to {0}", MethodBase.GetCurrentMethod().Name);

//            var appleExt = extensions.GetExtension<IAppleExtensions>();
//            appleExt.RestoreTransactions(result => Debug.LogFormat("{0} Result : {1}", MethodBase.GetCurrentMethod().Name, result));
//        }
//    }

//    /// <summary>
//    /// 제품을 구매한 이력이 있는가??
//    /// (NonConsumable 타입의 아이템의 구매여부를 확인하기 위한 함수)
//    /// </summary>
//    /// <param name="productId"></param>
//    /// <returns></returns>
//    public bool HasPurchased(string productId)
//    {
//        if (!isInitialized)
//        {
//            Debug.LogErrorFormat("IAPManager is not initialized.");
//            return false;
//        }

//        var product = controller.products.WithID(productId);
//        if (product != null)
//        {
//            return product.hasReceipt;
//        }

//        return false;
//    }

//    /// <summary>
//    /// 인앱결제 영수증의 유효성을 확인한다.
//    /// </summary>
//    /// <param name="receipt">Product.receipt</param>
//    /// <returns></returns>
//    public static bool CheckValidation(string receipt)
//    {
//        bool validPurchase = true; // Presume valid for platforms with no R.V.

//#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
//        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

//        try
//        {
//            // On Google Play, result has a single product ID.
//            // On Apple stores, receipts contain multiple products.
//            var result = validator.Validate(receipt);
//            // For informational purposes, we list the receipt(s)
//            foreach (IPurchaseReceipt productReceipt in result)
//            {
//                Debug.LogFormat("Receipt is valid. Contents : ProductID({0}) PurchaseDate({1}) TransactionID({2})"
//                    , productReceipt.productID
//                    , productReceipt.purchaseDate
//                    , productReceipt.transactionID);
//            }
//        }
//        catch (IAPSecurityException e)
//        {
//            Debug.Log("Invalid receipt, not unlocking content");
//            validPurchase = false;
//        }
//#endif

//        return validPurchase;
//    }
//}