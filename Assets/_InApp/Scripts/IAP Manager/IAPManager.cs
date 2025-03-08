using System;
using System.Collections;
using CI.WSANative.Common;
using CI.WSANative.Store;
using SingleApp;

public class IAPKey
{
    public const string PACK1 = "add1";
    public const string PACK2 = "add3";
    public const string PACK3 = "add5";
    public const string PACK4 = "add10";
}

public class IAPManager : PersistentSingleton<IAPManager>
{
    public static Action OnPurchaseSuccess;

    private bool _isBuyFromShop;
    void Awake()
    {
        WSANativeCore.Initialise();
    }
   
    //store id get from microsoft partner 
    public void BuyProductID(string storeid, int points = 0)
    {
        WSANativeStore.RequestPurchase(storeid, result =>
        {
            UnityEngine.Debug.Log(result.Status);
            if(result.Status == WSAStorePurchaseStatus.Succeeded)
            {
                // do something here to add point value
                OnPurchaseSuccess?.Invoke();
            }
        });
    }
}