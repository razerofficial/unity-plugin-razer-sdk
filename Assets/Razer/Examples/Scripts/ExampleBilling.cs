/*
 * Copyright (C) 2012-2016 Razer, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class ExampleBilling : MonoBehaviour
#if UNITY_ANDROID && !UNITY_EDITOR
    ,
    RazerSDK.IPauseListener, RazerSDK.IResumeListener,
    RazerSDK.IRequestLoginListener,
    RazerSDK.IRequestGamerInfoListener,
    RazerSDK.IRequestProductsListener, RazerSDK.IRequestPurchaseListener, RazerSDK.IRequestReceiptsListener,
    RazerSDK.IShutdownListener
#endif
    {
        /// <summary>
        /// The products to display for purchase
        /// </summary>
        public string[] Purchasables =
        {
			"long_sword",
			"sharp_axe",
			"__DECLINED__THIS_PURCHASE",
		};

        private const int BUTTON_HEIGHT = 60;

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Show the current state
        /// </summary>
        private string m_state = string.Empty;

        /// <summary>
        /// Show the current status
        /// </summary>
        private string m_status = string.Empty;

        /// <summary>
        /// The gamer UUID
        /// </summary>
        private string m_gamerUUID = string.Empty;

        /// <summary>
        /// The gamer User Name
        /// </summary>
        private string m_gamerUsername = string.Empty;

        /// <summary>
        /// A key to store game data
        /// </summary>
        private const string KEY_PUT_GAME_DATA = "ExampleBilling";

        /// <summary>
        /// The game data to display what was stored
        /// </summary>
        private string m_gameData = string.Empty;

        /// <summary>
        /// Check for is running on Store Hardware
        /// </summary>
        private bool m_isRunningOnStoreHardware = false;

        void Awake()
        {
            RazerSDK.registerPauseListener(this);
            RazerSDK.registerResumeListener(this);
            RazerSDK.registerRequestLoginListener(this);
            RazerSDK.registerRequestGamerInfoListener(this);
            RazerSDK.registerRequestProductsListener(this);
            RazerSDK.registerRequestPurchaseListener(this);
            RazerSDK.registerRequestReceiptsListener(this);
            RazerSDK.registerShutdownListener(this);
        }
        void OnDestroy()
        {
            RazerSDK.unregisterPauseListener(this);
            RazerSDK.unregisterResumeListener(this);
            RazerSDK.unregisterRequestLoginListener(this);
            RazerSDK.unregisterRequestGamerInfoListener(this);
            RazerSDK.unregisterRequestProductsListener(this);
            RazerSDK.unregisterRequestPurchaseListener(this);
            RazerSDK.unregisterRequestReceiptsListener(this);
            RazerSDK.unregisterShutdownListener(this);
        }

        public void OnPause()
        {
            m_state = "Detected Pause";
        }

        public void OnResume()
        {
            m_state = "Detected Resume";
        }

        public void OnSuccessShutdown()
        {
            m_state = "Shutdown Success!";
            RazerSDK.quit();
        }

        public void OnFailureShutdown()
        {
            m_state = "Failed to shutdown!";
        }

        public void RequestLoginOnSuccess()
        {
            m_status = "RequestLoginOnSuccess";
        }

        public void RequestLoginOnFailure(int errorCode, string errorMessage)
        {
            m_status = string.Format("RequestLoginOnFailure: error={0} errorMessage={1}", errorCode, errorMessage);
        }

        public void RequestLoginOnCancel()
        {
            m_status = "RequestLoginOnCancel";
        }

        public void RequestGamerInfoOnSuccess(RazerSDK.GamerInfo gamerInfo)
        {
            m_status = "RequestGamerInfoOnSuccess";
            m_gamerUsername = gamerInfo.username;
            m_gamerUUID = gamerInfo.uuid;
        }

        public void RequestGamerInfoOnFailure(int errorCode, string errorMessage)
        {
            m_status = string.Format("RequestGamerInfoOnFailure: error={0} errorMessage={1}", errorCode, errorMessage);
        }

        public void RequestGamerInfoOnCancel()
        {
            m_status = "RequestGamerInfoOnCancel";
        }

        public void RequestProductsOnSuccess(List<RazerSDK.Product> products)
        {
            m_status = "RequestProductsOnSuccess";
            m_products.Clear();
            for (int index = 0; index < products.Count; ++index)
            {
                RazerSDK.Product product = products[index];
                m_products.Add(product);
            }
        }

        public void RequestProductsOnFailure(int errorCode, string errorMessage)
        {
            m_status = string.Format("RequestProductsOnFailure: error={0} errorMessage={1}", errorCode, errorMessage);
        }

        public void RequestProductsOnCancel()
        {
            m_status = "RequestProductsOnCancel";
        }

        public void RequestPurchaseOnSuccess(RazerSDK.PurchaseResult purchaseResult)
        {
            m_status = string.Format("RequestPurchaseOnSuccess: identifier={0} ownerId={1}", purchaseResult.identifier, purchaseResult.ownerId);

            // cache the receipt for offline use
            RazerSDK.putGameData("FULL_GAME_UNLOCK", "1");
        }

        public void RequestPurchaseOnFailure(int errorCode, string errorMessage)
        {
            m_status = string.Format("RequestPurchaseOnFailure: error={0} errorMessage={1}", errorCode, errorMessage);
        }

        public void RequestPurchaseOnCancel()
        {
            m_status = "RequestPurchaseOnCancel";
        }

        public void RequestReceiptsOnSuccess(List<RazerSDK.Receipt> receipts)
        {
            m_status = "RequestReceiptsOnSuccess";
            m_receipts.Clear();
            foreach (RazerSDK.Receipt receipt in receipts)
            {
                m_receipts.Add(receipt);
            }

            // if a receipt was found, cache the receipt
            if (receipts.Count > 0)
            {
                // cache receipt for offline use
                RazerSDK.putGameData("FULL_GAME_UNLOCK", "1");
            }
            // if receipt was removed, delete the cached receipt
            else
            {
                // delete cached receipt
                RazerSDK.putGameData("FULL_GAME_UNLOCK", "0");
            }
        }

        public void RequestReceiptsOnFailure(int errorCode, string errorMessage)
        {
            m_status = string.Format("RequestReceiptsOnFailure: error={0} errorMessage={1}", errorCode, errorMessage);

            // use cached receipt
            if (RazerSDK.getGameData("FULL_GAME_UNLOCK") == "1")
            {
                //unlock full game
            }
        }

        public void RequestReceiptsOnCancel()
        {
            m_status = "RequestReceiptsOnCancel";

            // use cached receipt
            if (RazerSDK.getGameData("FULL_GAME_UNLOCK") == "1")
            {
                //unlock full game
            }
        }

        #region Data containers

        private List<RazerSDK.Product> m_products = new List<RazerSDK.Product>();

        private List<RazerSDK.Receipt> m_receipts = new List<RazerSDK.Receipt>();

        #endregion

        #region Presentation

        private void OnGUI()
        {
            try
            {
                Color oldColor = GUI.backgroundColor;

                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Exit", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Exiting...";
                    RazerSDK.shutdown();
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("720p", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Setting 1280x720...";
                    Screen.SetResolution(1280, 720, true);
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("1080p", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Setting 1920x1080...";
                    Screen.SetResolution(1920, 1080, true);
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("IsRunningOnStoreHardware: {0}", m_isRunningOnStoreHardware));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("App Name: {0}", RazerSDK.getStringResource("app_name")));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(RazerSDK.isIAPInitComplete() ? "IAP is initialized" : "IAP initializing...");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("State: {0}", m_state));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("Status: {0}", m_status));
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Request Login", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Requesting login...";
                    RazerSDK.requestLogin();
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("Gamer UUID: {0}", m_gamerUUID));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label(string.Format("Gamer User Name: {0}", m_gamerUsername));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Request Gamer Info", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Requesting gamer info...";
                    RazerSDK.requestGamerInfo();
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Put Game Data", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    RazerSDK.putGameData(KEY_PUT_GAME_DATA, "This is a test!!!!");
                }

                if (GUILayout.Button("Get Game Data", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_gameData = RazerSDK.getGameData(KEY_PUT_GAME_DATA);
                }
                GUILayout.Label(string.Format("GameData: {0}", m_gameData));
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label("Products:");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Request Products", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    List<string> productIdentifierList =
                        new List<string>();

                    foreach (string identifier in Purchasables)
                    {
						productIdentifierList.Add(identifier);
                    }

                    m_status = "Requesting products...";
                    RazerSDK.requestProducts(productIdentifierList);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                foreach (RazerSDK.Product product in m_products)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(500);

                    GUILayout.Label(string.Format("Name={0}", product.name));
                    GUILayout.Label(string.Format("Price={0}", product.localPrice));
                    GUILayout.Label(string.Format("Identifier={0}", product.identifier));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Request Purchase", GUILayout.Height(BUTTON_HEIGHT)))
                    {
                        m_status = "Requesting purchase...";
                        //Debug.Log(string.Format("Purchase Identifier: {0}", product.identifier));
						RazerSDK.requestPurchase(product.identifier, RazerSDK.ProductType.ENTITLEMENT);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label("Receipts:");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (GUILayout.Button("Request Receipts", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    m_status = "Requesting receipts...";
                    RazerSDK.requestReceipts();
                }
                GUILayout.EndHorizontal();

                foreach (RazerSDK.Receipt receipt in m_receipts)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(400);

                    GUILayout.Label(string.Format("LocalPrice={0}", receipt.localPrice));
                    GUILayout.Label(string.Format("Identifier={0}", receipt.identifier));

                    GUILayout.EndHorizontal();
                }
            }
            catch (System.Exception)
            {
            }
        }

        #endregion

        #region Focus Handling

        public IEnumerator Start()
        {

            foreach (string identifier in Purchasables)
            {
                RazerSDK.Product product = new RazerSDK.Product();
                product.identifier = identifier;
                m_products.Add(product);
            }

            // wait for IAP to initialize
            while (!RazerSDK.isIAPInitComplete())
            {
                yield return null;
            }

            // request receipts
            RazerSDK.requestReceipts();

            m_isRunningOnStoreHardware = RazerSDK.isRunningOnSupportedHardware();
        }

        #endregion
#endif
    }
}
