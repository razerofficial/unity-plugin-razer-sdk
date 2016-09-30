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

#define VERBOSE_LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID && !UNITY_EDITOR
using com.razerzone.store.sdk.content;
using org.json;
#endif
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class RazerGameObject : MonoBehaviour
    {
        public string m_secretApiKey = "";

        #region Private Variables
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static RazerGameObject s_instance = null;
        #endregion

        #region Singleton Accessor Class
        /// <summary>
        /// Singleton interface
        /// </summary>
        public static RazerGameObject Singleton
        {
            get
            {
                if (null == s_instance)
                {
                    GameObject razerGameObject = GameObject.Find("RazerGameObject");
                    if (razerGameObject)
                    {
                        s_instance = razerGameObject.GetComponent<RazerGameObject>();
                    }
                }
                return s_instance;
            }
        }
        #endregion

        #region Java To Unity Event Handlers

#if UNITY_ANDROID && !UNITY_EDITOR

        public void onPause(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("onPause listeners="+RazerSDK.getPauseListeners().Count);
#endif
            foreach (RazerSDK.IPauseListener listener in RazerSDK.getPauseListeners())
            {
                listener.OnPause();
            }
        }

        public void onResume(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("onResume listeners="+RazerSDK.getResumeListeners().Count);
#endif
            foreach (RazerSDK.IResumeListener listener in RazerSDK.getResumeListeners())
            {
                listener.OnResume();
            }
        }

#endif

#endregion

            #region Initialization Listeners

            IEnumerator InvokeInitPlugin(bool wait)
        {
            if (wait)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return null;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
#if VERBOSE_LOGGING
                Debug.Log("InvokeInitPlugin SecretAPIKey=" + m_secretApiKey);
#endif
                RazerSDK.initPlugin(m_secretApiKey);
            }
            catch (Exception)
            {
                OnFailureInitializePlugin("Failed to invoke initPlugin.");
            }
#endif
            }

        public void OnSuccessInitializePlugin(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("Razer Plugin Initialized.");
#endif
        }

        public void OnFailureInitializePlugin(string errorMessage)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("initPlugin failed: {0}", errorMessage));
#endif
        }

        #endregion

        #region JSON Data Listeners

#if UNITY_ANDROID && !UNITY_EDITOR

        public void ContentDeleteListenerOnDeleted(string ignore)
        {
            foreach (RazerSDK.IContentDeleteListener listener in RazerSDK.getContentDeleteListeners())
            {
                if (null != listener)
                {
                    listener.ContentDeleteOnDeleted(null);
                }
            }
        }
        public void ContentDeleteListenerOnDeleteFailed(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentDeleteListener listener in RazerSDK.getContentDeleteListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentDeleteOnDeleteFailed(null, code, reason);
                    }
                }
            }
        }

        public void ContentDownloadListenerOnComplete(string ignore)
        {
            foreach (RazerSDK.IContentDownloadListener listener in RazerSDK.getContentDownloadListeners())
            {
                if (null != listener)
                {
                    listener.ContentDownloadOnComplete(null);
                }
            }
        }
        public void ContentDownloadListenerOnProgress(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int progress = 0;
                if (jsonObject.has("progress"))
                {
                    progress = jsonObject.getInt("progress");
                }
                foreach (RazerSDK.IContentDownloadListener listener in RazerSDK.getContentDownloadListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentDownloadOnProgress(null, progress);
                    }
                }
            }
        }
        public void ContentDownloadListenerOnFailed(string ignore)
        {
            foreach (RazerSDK.IContentDownloadListener listener in RazerSDK.getContentDownloadListeners())
            {
                if (null != listener)
                {
                    listener.ContentDownloadOnFailed(null);
                }
            }
        }

        public void ContentInitListenerOnInitialized(string ignore)
        {
            foreach (RazerSDK.IContentInitializedListener listener in RazerSDK.getContentInitializedListeners())
            {
                if (null != listener)
                {
                    listener.ContentInitializedOnInitialized();
                }
            }
        }
        public void ContentInitListenerOnDestroyed(string ignore)
        {
            foreach (RazerSDK.IContentInitializedListener listener in RazerSDK.getContentInitializedListeners())
            {
                if (null != listener)
                {
                    listener.ContentInitializedOnDestroyed();
                }
            }
        }

        public void ContentInstalledSearchListenerOnResults(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int count = 0;
                if (jsonObject.has("count"))
                {
                    count = jsonObject.getInt("count");
                }
                List<GameMod> gameMods = Plugin.getGameModManagerInstalledResults();
                foreach (RazerSDK.IContentInstalledSearchListener listener in RazerSDK.getContentInstalledSearchListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentInstalledSearchOnResults(gameMods, count);
                    }
                }
            }
        }
        public void ContentInstalledSearchListenerOnError(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentInstalledSearchListener listener in RazerSDK.getContentInstalledSearchListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentInstalledSearchOnError(code, reason);
                    }
                }
            }
        }

        public void ContentPublishedSearchListenerOnResults(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int count = 0;
                if (jsonObject.has("count"))
                {
                    count = jsonObject.getInt("count");
                }
                List<GameMod> gameMods = Plugin.getGameModManagerPublishedResults();
                foreach (RazerSDK.IContentPublishedSearchListener listener in RazerSDK.getContentPublishedSearchListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentPublishedSearchOnResults(gameMods, count);
                    }
                }
            }
        }
        public void ContentPublishedSearchListenerOnError(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentPublishedSearchListener listener in RazerSDK.getContentPublishedSearchListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentPublishedSearchOnError(code, reason);
                    }
                }
            }
        }

        public void ContentSaveListenerOnSuccess(string ignore)
        {
            foreach (RazerSDK.IContentSaveListener listener in RazerSDK.getContentSaveListeners())
            {
                if (null != listener)
                {
                    listener.ContentSaveOnSuccess(null);
                }
            }
        }
        public void ContentSaveListenerOnError(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentSaveListener listener in RazerSDK.getContentSaveListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentSaveOnError(null, code, reason);
                    }
                }
            }
        }

        public void ContentPublishListenerOnSuccess(string ignore)
        {
            foreach (RazerSDK.IContentPublishListener listener in RazerSDK.getContentPublishListeners())
            {
                if (null != listener)
                {
                    listener.ContentPublishOnSuccess(null);
                }
            }
        }
        public void ContentPublishListenerOnError(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentPublishListener listener in RazerSDK.getContentPublishListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentPublishOnError(null, code, reason);
                    }
                }
            }
        }

        public void ContentUnpublishListenerOnSuccess(string ignore)
        {
            foreach (RazerSDK.IContentUnpublishListener listener in RazerSDK.getContentUnpublishListeners())
            {
                if (null != listener)
                {
                    listener.ContentUnpublishOnSuccess(null);
                }
            }
        }
        public void ContentUnpublishListenerOnError(string jsonData)
        {
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                int code = 0;
                string reason = string.Empty;
                if (jsonObject.has("code"))
                {
                    code = jsonObject.getInt("code");
                }
                if (jsonObject.has("reason"))
                {
                    reason = jsonObject.getString("reason");
                }
                foreach (RazerSDK.IContentUnpublishListener listener in RazerSDK.getContentUnpublishListeners())
                {
                    if (null != listener)
                    {
                        listener.ContentUnpublishOnError(null, code, reason);
                    }
                }
            }
        }

        public void RequestLoginSuccessListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestLoginSuccessListener:");
#endif
            foreach (RazerSDK.IRequestLoginListener listener in RazerSDK.getRequestLoginListeners())
            {
                if (null != listener)
                {
                    listener.RequestLoginOnSuccess();
                }
            }
        }
        public void RequestLoginFailureListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.LogError(string.Format("RequestLoginFailureListener: jsonData={0}", jsonData));
#endif

            foreach (RazerSDK.IRequestLoginListener listener in RazerSDK.getRequestLoginListeners())
            {
                if (null != listener)
                {
                    listener.RequestLoginOnFailure(0, jsonData);
                }
            }
        }
        public void RequestLoginCancelListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestLoginCancelListener:");
#endif

            foreach (RazerSDK.IRequestLoginListener listener in RazerSDK.getRequestLoginListeners())
            {
                if (null != listener)
                {
                    listener.RequestLoginOnCancel();
                }
            }
        }

        public void RequestGamerInfoSuccessListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("RequestGamerInfoSuccessListener: jsonData={0}", jsonData));
#endif
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                RazerSDK.GamerInfo gamerInfo = RazerSDK.GamerInfo.Parse(jsonObject);
                foreach (RazerSDK.IRequestGamerInfoListener listener in RazerSDK.getRequestGamerInfoListeners())
                {
                    if (null != listener)
                    {
                        listener.RequestGamerInfoOnSuccess(gamerInfo);
                    }
                }
            }
        }
        public void RequestGamerInfoFailureListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.LogError(string.Format("RequestGamerInfoFailureListener: jsonData={0}", jsonData));
#endif
            foreach (RazerSDK.IRequestGamerInfoListener listener in RazerSDK.getRequestGamerInfoListeners())
            {
                if (null != listener)
                {
                    listener.RequestGamerInfoOnFailure(0, jsonData);
                }
            }
        }
        public void RequestGamerInfoCancelListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestGamerInfoCancelListener:");
#endif

            foreach (RazerSDK.IRequestGamerInfoListener listener in RazerSDK.getRequestGamerInfoListeners())
            {
                if (null != listener)
                {
                    listener.RequestGamerInfoOnCancel();
                }
            }
        }

        public void RequestProductsSuccessListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("RazerSDK.RequestProductsSuccessListener: jsonData={0}", jsonData));
#endif

            using (JSONArray jsonArray = new JSONArray(jsonData))
            {
                List<RazerSDK.Product> products = new List<RazerSDK.Product>();
                for (int index = 0; index < jsonArray.length(); ++index)
                {
                    using (JSONObject jsonObject = jsonArray.getJSONObject(index))
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found Product: {0}", jsonObject.toString()));
#endif
                        RazerSDK.Product product = RazerSDK.Product.Parse(jsonObject);
                        products.Add(product);
                    }
                }
                foreach (RazerSDK.IRequestProductsListener listener in RazerSDK.getRequestProductsListeners())
                {
                    if (null != listener)
                    {
                        listener.RequestProductsOnSuccess(products);
                    }
                }
            }
        }
        public void RequestProductsFailureListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.LogError(string.Format("RequestProductsFailureListener: jsonData={0}", jsonData));
#endif
            foreach (RazerSDK.IRequestProductsListener listener in RazerSDK.getRequestProductsListeners())
            {
                if (null != listener)
                {
                    listener.RequestProductsOnFailure(0, jsonData);
                }
            }
        }
        public void RequestProductsCancelListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestProductsCancelListener:");
#endif
            foreach (RazerSDK.IRequestProductsListener listener in RazerSDK.getRequestProductsListeners())
            {
                if (null != listener)
                {
                    listener.RequestProductsOnCancel();
                }
            }
        }

        public void RequestPurchaseSuccessListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("RequestPurchaseSuccessListener: jsonData={0}", jsonData));
#endif
            using (JSONObject jsonObject = new JSONObject(jsonData))
            {
                RazerSDK.PurchaseResult purchaseResult = RazerSDK.PurchaseResult.Parse(jsonObject);
                foreach (RazerSDK.IRequestPurchaseListener listener in RazerSDK.getRequestPurchaseListeners())
                {
                    if (null != listener)
                    {
                        listener.RequestPurchaseOnSuccess(purchaseResult);
                    }
                }
            }
        }
        public void RequestPurchaseFailureListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.LogError(string.Format("RequestPurchaseFailureListener: jsonData={0}", jsonData));
#endif
            foreach (RazerSDK.IRequestPurchaseListener listener in RazerSDK.getRequestPurchaseListeners())
            {
                if (null != listener)
                {
                    listener.RequestPurchaseOnFailure(0, jsonData);
                }
            }
        }
        public void RequestPurchaseCancelListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestPurchaseCancelListener:");
#endif
            foreach (RazerSDK.IRequestPurchaseListener listener in RazerSDK.getRequestPurchaseListeners())
            {
                if (null != listener)
                {
                    listener.RequestPurchaseOnCancel();
                }
            }
        }

        public void RequestReceiptsSuccessListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("RazerSDK.RequestReceiptsSuccessListener: jsonData={0}", jsonData));
#endif

            using (JSONArray jsonArray = new JSONArray(jsonData))
            {
                List<RazerSDK.Receipt> receipts = new List<RazerSDK.Receipt>();
                for (int index = 0; index < jsonArray.length(); ++index)
                {
                    using (JSONObject jsonObject = jsonArray.getJSONObject(index))
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found Receipt: {0}", jsonObject.toString()));
#endif
                        RazerSDK.Receipt receipt = RazerSDK.Receipt.Parse(jsonObject);
                        receipts.Add(receipt);
                    }
                }
                foreach (RazerSDK.IRequestReceiptsListener listener in RazerSDK.getRequestReceiptsListeners())
                {
                    if (null != listener)
                    {
                        listener.RequestReceiptsOnSuccess(receipts);
                    }
                }
            }
        }
        public void RequestReceiptsFailureListener(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.LogError(string.Format("RequestReceiptsFailureListener: jsonData={0}", jsonData));
#endif
            foreach (RazerSDK.IRequestReceiptsListener listener in RazerSDK.getRequestReceiptsListeners())
            {
                if (null != listener)
                {
                    listener.RequestReceiptsOnFailure(0, jsonData);
                }
            }
        }
        public void RequestReceiptsCancelListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RequestReceiptsCancelListener:");
#endif
            foreach (RazerSDK.IRequestReceiptsListener listener in RazerSDK.getRequestReceiptsListeners())
            {
                if (null != listener)
                {
                    listener.RequestReceiptsOnCancel();
                }
            }
        }

        public void ShutdownOnSuccessListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RazerSDK.ShutdownOnSuccessListener");
#endif

            foreach (RazerSDK.IShutdownListener listener in RazerSDK.getShutdownListeners())
            {
                if (null != listener)
                {
                    listener.OnSuccessShutdown();
                }
            }
        }
        public void ShutdownOnFailureListener(string ignore)
        {
#if VERBOSE_LOGGING
            Debug.Log("RazerSDK.ShutdownOnFailureListener");
#endif

            foreach (RazerSDK.IShutdownListener listener in RazerSDK.getShutdownListeners())
            {
                if (null != listener)
                {
                    listener.OnFailureShutdown();
                }
            }
        }

#endif
        #endregion

        #region UNITY Awake, Start & Update
        void Awake()
        {
            s_instance = this;
            Debug.Log(string.Format("RazerPluginVersion: VERSION={0}", RazerSDK.PLUGIN_VERSION));
        }
        void Start()
        {
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(transform.gameObject);

            StartCoroutine("InvokeInitPlugin", false);
        }
#endregion

#region Controllers

#if UNITY_ANDROID && !UNITY_EDITOR
        public void Update()
        {
            if (RazerSDK.GetUseDefaultInput())
            {
                return;
            }
            RazerSDK.ControllerInput.UpdateInputFrame();
            RazerSDK.ControllerInput.ClearButtonStates();
        }

        private void FixedUpdate()
        {
            if (RazerSDK.GetUseDefaultInput())
            {
                return;
            }
            RazerSDK.UpdateJoysticks();
        }

#endif

#endregion

#region Debug Logs from Java
        public void DebugLog(string message)
        {
            Debug.Log(message);
        }

        public void DebugLogError(string message)
        {
            Debug.LogError(message);
        }
#endregion

    }
}
