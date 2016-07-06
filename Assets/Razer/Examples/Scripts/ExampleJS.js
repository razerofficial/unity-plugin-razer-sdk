#pragma strict

#if UNITY_ANDROID && !UNITY_EDITOR
import com.razerzone.store.sdk.engine.unity;
#endif
import System.Collections.Generic;

public class ExampleJS extends MonoBehaviour
#if UNITY_ANDROID && !UNITY_EDITOR
    implements
    RazerSDK.IPauseListener,
    RazerSDK.IResumeListener,
	RazerSDK.IRequestGamerInfoListener,
	RazerSDK.IRequestProductsListener,
	RazerSDK.IRequestPurchaseListener,
	RazerSDK.IRequestReceiptsListener
#endif
{

#if UNITY_ANDROID && !UNITY_EDITOR
    function Awake()
    {
        RazerSDK.registerPauseListener(this);
        RazerSDK.registerResumeListener(this);
        RazerSDK.registerRequestGamerInfoListener(this);
        RazerSDK.registerRequestProductsListener(this);
        RazerSDK.registerRequestPurchaseListener(this);
        RazerSDK.registerRequestReceiptsListener(this);
    }

    function OnDestroy()
    {
        RazerSDK.unregisterPauseListener(this);
        RazerSDK.unregisterResumeListener(this);
        RazerSDK.unregisterRequestGamerInfoListener(this);
        RazerSDK.unregisterRequestProductsListener(this);
        RazerSDK.unregisterRequestPurchaseListener(this);
        RazerSDK.unregisterRequestReceiptsListener(this);
    }

    public function OuyaOnPause()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function OuyaOnResume()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function RequestGamerInfoOnSuccess(uuid : String, username : String)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestGamerInfoOnFailure(errorCode : int, errorMessage : String)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestGamerInfoOnCancel()
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function RequestProductsOnSuccess(products : List.<RazerSDK.Product>)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
		for (var index : int = 0; index < products.Count; ++index)
        {
			//product : RazerSDK.Product in products
        }
    }
    public function RequestProductsOnFailure(errorCode : int, errorMessage : String)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestProductsOnCancel()
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function RequestPurchaseOnSuccess(product : RazerSDK.Product)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestPurchaseOnFailure(errorCode : int, errorMessage : String)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestPurchaseOnCancel()
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function RequestReceiptsOnSuccess(receipts : List.<RazerSDK.Receipt>)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        for (var receipt : RazerSDK.Receipt in receipts)
        {
                if (receipt.identifier == "__MY_ID__")
                {
                    //detected purchase
                }
        }
    }
    public function RequestReceiptsOnFailure(errorCode : int, errorMessage : String)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }
    public function RequestReceiptsOnCancel()
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
    }

    public function OnGUI() {
        GUILayout.BeginVertical(GUILayout.Height(Screen.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
        GUILayout.Label("This is JavaScript! Check out the code for syntax examples.");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }
#endif
}