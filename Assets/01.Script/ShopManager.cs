using UnityEngine;
using System.Collections;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class ShopManager : IStore
{
    private IStoreCallback callback;

    public void Initialize(IStoreCallback callback)
    {
        Debug.LogFormat("SHOP MANAGER INITIALIZE");
        this.callback = callback;
    }

    public void RetrieveProducts (System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Purchasing.ProductDefinition> products)
    {
        foreach(var p in products)
        {
            Debug.LogFormat("ID({0})/SpecID({1})/Type({2})/PayoutType({3})/PayoutData({4})", p.id, p.storeSpecificId, p.type, p.payout.type, p.payout.data);
        }
        //callback.OnProductsRetrieved(products);
    }

    public void Purchase(ProductDefinition product, string developerPayload)
    {

    }

    public void FinishTransaction (ProductDefinition product, string transactionId)
    {

    }
}
