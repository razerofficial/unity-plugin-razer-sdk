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

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Handle focusing items
        /// </summary>
        private FocusManager m_focusManager = new FocusManager();

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
        private const string KEY_PUT_GAME_DATA = "ShowProductsExample";

        /// <summary>
        /// The game data to display what was stored
        /// </summary>
        private string m_gameData = string.Empty;

        /// <summary>
        /// Check for is running on Store Hardware
        /// </summary>
        private bool m_isRunningOnStoreHardware = false;

        /// <summary>
        /// Buttons
        /// </summary>
        private object m_btnPutGameData = new object();
        private object m_btnGetGameData = new object();
        private object m_btnRequestGamerInfo = new object();
        private object m_btnRequestProducts = new object();
        private object m_btnRequestReceipts = new object();
        private object m_btn720 = new object();
        private object m_btn1080 = new object();
        private object m_btnExit = new object();

        void Awake()
        {
            RazerSDK.registerPauseListener(this);
            RazerSDK.registerResumeListener(this);
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
            Application.Quit();
        }

        public void OnFailureShutdown()
        {
            m_state = "Failed to shutdown!";
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
                // Get Products Right goes to the first element
                if (index == 0)
                {
                    m_focusManager.Mappings[m_btnRequestProducts].Right = product;
                }
                // Products left goes back to the GetProducts button
                m_focusManager.Mappings[product] = new FocusManager.ButtonMapping()
                {
                    Left = m_btnRequestProducts
                };
                // Product down goes to the next element
                if ((index + 1) < products.Count)
                {
                    m_focusManager.Mappings[product].Down = products[index + 1];
                }
                // Product up goes to the previous element
                if (index > 0)
                {
                    m_focusManager.Mappings[product].Up = products[index - 1];
                }
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

        public void RequestPurchaseOnSuccess(RazerSDK.Product product)
        {
            m_status = string.Format("RequestPurchaseOnSuccess: {0}", product.identifier);

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
                if (m_focusManager.SelectedButton == m_btnExit)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Exit", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnExit &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_status = "Exiting...";
                    RazerSDK.shutdown();
                }
                GUI.backgroundColor = oldColor;
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (m_focusManager.SelectedButton == m_btn720)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("720p", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btn720 &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_status = "Setting 1280x720...";
                    Screen.SetResolution(1280, 720, true);
                }
                GUI.backgroundColor = oldColor;
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (m_focusManager.SelectedButton == m_btn1080)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("1080p", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btn1080 &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_status = "Setting 1920x1080...";
                    Screen.SetResolution(1920, 1080, true);
                }
                GUI.backgroundColor = oldColor;
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
                if (m_focusManager.SelectedButton == m_btnRequestGamerInfo)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Request Gamer Info", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnRequestGamerInfo &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_status = "Requesting gamer info...";
                    RazerSDK.requestGamerInfo();
                }
                GUI.backgroundColor = oldColor;
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Empty);
                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (m_focusManager.SelectedButton == m_btnPutGameData)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Put Game Data", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnPutGameData &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    RazerSDK.putGameData(KEY_PUT_GAME_DATA, "This is a test!!!!");
                }
                GUI.backgroundColor = oldColor;

                if (m_focusManager.SelectedButton == m_btnGetGameData)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Get Game Data", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnGetGameData &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_gameData = RazerSDK.getGameData(KEY_PUT_GAME_DATA);
                }
                GUI.backgroundColor = oldColor;
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
                if (m_focusManager.SelectedButton == m_btnRequestProducts)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Request Products", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnRequestProducts &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    List<RazerSDK.Purchasable> productIdentifierList =
                        new List<RazerSDK.Purchasable>();

                    foreach (string productId in Purchasables)
                    {
                        RazerSDK.Purchasable purchasable = new RazerSDK.Purchasable();
                        purchasable.productId = productId;
                        productIdentifierList.Add(purchasable);
                    }

                    m_status = "Requesting products...";
                    RazerSDK.requestProducts(productIdentifierList);
                }
                GUI.backgroundColor = oldColor;
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

                    if (m_focusManager.SelectedButton == product)
                    {
                        GUI.backgroundColor = Color.red;
                    }
                    if (GUILayout.Button("Request Purchase") ||
                        (m_focusManager.SelectedButton == product &&
                        RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                    {
                        m_status = "Requesting purchase...";
                        //Debug.Log(string.Format("Purchase Identifier: {0}", product.identifier));
                        RazerSDK.Purchasable purchasable = new RazerSDK.Purchasable();
                        purchasable.productId = product.identifier;
                        RazerSDK.requestPurchase(purchasable);
                    }
                    GUI.backgroundColor = oldColor;

                    GUILayout.EndHorizontal();
                }

                GUILayout.Label(string.Empty);

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                GUILayout.Label("Receipts:");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(400);
                if (m_focusManager.SelectedButton == m_btnRequestReceipts)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Request Receipts", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == m_btnRequestReceipts &&
                    RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
                {
                    m_status = "Requesting receipts...";
                    RazerSDK.requestReceipts();
                }
                GUI.backgroundColor = oldColor;
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
            m_focusManager.Mappings[m_btnExit] = new FocusManager.ButtonMapping()
            {
                Down = m_btn720
            };
            m_focusManager.Mappings[m_btn720] = new FocusManager.ButtonMapping()
            {
                Up = m_btnExit,
                Down = m_btn1080
            };
            m_focusManager.Mappings[m_btn1080] = new FocusManager.ButtonMapping()
            {
                Up = m_btn720,
                Down = m_btnRequestGamerInfo
            };
            m_focusManager.Mappings[m_btnRequestGamerInfo] = new FocusManager.ButtonMapping()
            {
                Up = m_btn1080,
                Down = m_btnPutGameData
            };
            m_focusManager.Mappings[m_btnPutGameData] = new FocusManager.ButtonMapping()
            {
                Up = m_btnRequestGamerInfo,
                Right = m_btnGetGameData,
                Down = m_btnRequestProducts
            };
            m_focusManager.Mappings[m_btnGetGameData] = new FocusManager.ButtonMapping()
            {
                Up = m_btnRequestGamerInfo,
                Left = m_btnPutGameData,
                Down = m_btnRequestProducts
            };
            m_focusManager.Mappings[m_btnRequestProducts] = new FocusManager.ButtonMapping()
            {
                Up = m_btnPutGameData,
                Down = m_btnRequestReceipts
            };
            m_focusManager.Mappings[m_btnRequestReceipts] = new FocusManager.ButtonMapping()
            {
                Up = m_btnRequestProducts,
            };

            // set default selection
            m_focusManager.SelectedButton = m_btnRequestGamerInfo;

            // wait for IAP to initialize
            while (!RazerSDK.isIAPInitComplete())
            {
                yield return null;
            }

            // request receipts
            RazerSDK.requestReceipts();

            m_isRunningOnStoreHardware = RazerSDK.isRunningOnSupportedHardware();
        }

        private void Update()
        {
            if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_DPAD_DOWN))
            {
                m_focusManager.FocusDown();
            }
            if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_DPAD_LEFT))
            {
                m_focusManager.FocusLeft();
            }
            if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_DPAD_RIGHT))
            {
                m_focusManager.FocusRight();
            }
            if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_DPAD_UP))
            {
                m_focusManager.FocusUp();
            }
        }

        public class FocusManager
        {
            private const int DELAY_MS = 150;

            public object SelectedButton = null;

            private void SetSelection(object selection)
            {
                if (null != selection)
                {
                    SelectedButton = selection;
                }
            }

            public class ButtonMapping
            {
                public object Up = null;
                public object Left = null;
                public object Right = null;
                public object Down = null;
            }

            public Dictionary<object, ButtonMapping> Mappings = new Dictionary<object, ButtonMapping>();

            public void FocusDown()
            {
                if (null != SelectedButton &&
                    Mappings.ContainsKey(SelectedButton))
                {
                    SetSelection(Mappings[SelectedButton].Down);
                }
            }
            public void FocusLeft()
            {
                if (null != SelectedButton &&
                    Mappings.ContainsKey(SelectedButton))
                {
                    SetSelection(Mappings[SelectedButton].Left);
                }
            }
            public void FocusRight()
            {
                if (null != SelectedButton &&
                    Mappings.ContainsKey(SelectedButton))
                {
                    SetSelection(Mappings[SelectedButton].Right);
                }
            }
            public void FocusUp()
            {
                if (null != SelectedButton &&
                    Mappings.ContainsKey(SelectedButton))
                {
                    SetSelection(Mappings[SelectedButton].Up);
                }
            }
        }

        #endregion
#endif
    }
}
