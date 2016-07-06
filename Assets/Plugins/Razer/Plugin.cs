//#define VERBOSE_LOGGING
using System.Reflection;
#if UNITY_ANDROID && !UNITY_EDITOR
using Android.Graphics;
using com.razerzone.store.sdk.content;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class Plugin
    {
        private const string LOG_TAG = "Plugin";
        private static IntPtr _jcPlugin = IntPtr.Zero;
        private static IntPtr _jmConstructor = IntPtr.Zero;
        private static IntPtr _jmInitPlugin = IntPtr.Zero;
        private static IntPtr _jmIsInitialized = IntPtr.Zero;
		private static IntPtr _jmGetDeviceHardwareName = IntPtr.Zero;
        private static IntPtr _jmGetGameData = IntPtr.Zero;
        private static IntPtr _jmPutGameData = IntPtr.Zero;
        private static IntPtr _jmRequestGamerInfo = IntPtr.Zero;
        private static IntPtr _jmRequestProducts = IntPtr.Zero;
        private static IntPtr _jmRequestPurchase = IntPtr.Zero;
        private static IntPtr _jmRequestReceipts = IntPtr.Zero;
        private static IntPtr _jmIsRunningOnSupportedHardware = IntPtr.Zero;
        private static IntPtr _jmSetSafeArea = IntPtr.Zero;
        private static IntPtr _jmClearFocus = IntPtr.Zero;
        private static IntPtr _jmGetGameModManager = IntPtr.Zero;
        private static IntPtr _jmSaveGameMod = IntPtr.Zero;
        private static IntPtr _jmGetGameModManagerInstalled = IntPtr.Zero;
        private static IntPtr _jmGetGameModManagerInstalledResultsArray = IntPtr.Zero;
        private static IntPtr _jmGetGameModManagerPublished = IntPtr.Zero;
        private static IntPtr _jmGetGameModManagerPublishedResultsArray = IntPtr.Zero;
        private static IntPtr _jmContentDelete = IntPtr.Zero;
        private static IntPtr _jmContentDownload = IntPtr.Zero;
        private static IntPtr _jmContentPublish = IntPtr.Zero;
        private static IntPtr _jmContentUnpublish = IntPtr.Zero;
        private static IntPtr _jmGetFloat = IntPtr.Zero;
        private static IntPtr _jmGetBitmapArray = IntPtr.Zero;
        private static IntPtr _jmGetGameModScreenshotArray = IntPtr.Zero;
        private static IntPtr _jmGetStringArray = IntPtr.Zero;
        private static IntPtr _jmShutdown = IntPtr.Zero;
        private IntPtr _instance = IntPtr.Zero;

        /// <summary>
        /// Make one request at a time
        /// </summary>
        public static bool m_pendingRequestGamerInfo = false;

        /// <summary>
        /// Make one request at a time
        /// </summary>
        public static bool m_pendingRequestProducts = false;

        /// <summary>
        /// Make one request at a time
        /// </summary>
        public static bool m_pendingRequestPurchase = false;

        /// <summary>
        /// Make one request at a time
        /// </summary>
        public static bool m_pendingRequestReceipts = false;

        /// <summary>
        /// Make one request at a time
        /// </summary>
        public static bool m_pendingShutdown = false;

        static Plugin()
        {
            try
            {
                {
                    string strName = "com/razerzone/store/sdk/engine/unity/Plugin";
                    IntPtr localRef = AndroidJNI.FindClass(strName);
                    if (localRef != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} class", strName));
#endif
                        _jcPlugin = AndroidJNI.NewGlobalRef(localRef);
                        AndroidJNI.DeleteLocalRef(localRef);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} class", strName));
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("Exception loading JNI - {0}", ex));
            }
        }

        private static void JNIFind()
        {
            try
            {
                {
                    string strMethod = "<init>";
                    _jmConstructor = AndroidJNI.GetMethodID(_jcPlugin, strMethod, "(Landroid/app/Activity;)V");
                    if (_jmConstructor != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "initPlugin";
                    _jmInitPlugin = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)V");
                    if (_jmInitPlugin != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "isInitialized";
                    _jmIsInitialized = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()Z");
                    if (_jmIsInitialized != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

				{
					string strMethod = "getDeviceHardwareName";
					_jmGetDeviceHardwareName = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()Ljava/lang/String;");
					if (_jmGetDeviceHardwareName != IntPtr.Zero)
					{
#if VERBOSE_LOGGING
						Debug.Log(string.Format("Found {0} method", strMethod));
#endif
					}
					else
					{
						Debug.LogError(string.Format("Failed to find {0} method", strMethod));
						return;
					}
				}

                {
                    string strMethod = "getGameData";
                    _jmGetGameData = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)Ljava/lang/String;");
                    if (_jmGetGameData != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "putGameData";
                    _jmPutGameData = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;Ljava/lang/String;)V");
                    if (_jmPutGameData != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "requestGamerInfo";
                    _jmRequestGamerInfo = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
                    if (_jmRequestGamerInfo != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "requestProducts";
                    _jmRequestProducts = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)V");
                    if (_jmRequestProducts != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "requestPurchase";
                    _jmRequestPurchase = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)V");
                    if (_jmRequestPurchase != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "requestReceipts";
                    _jmRequestReceipts = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
                    if (_jmRequestReceipts != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "isRunningOnSupportedHardware";
                    _jmIsRunningOnSupportedHardware = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()Z");
                    if (_jmIsRunningOnSupportedHardware != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "setSafeArea";
                    _jmSetSafeArea = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(F)V");
                    if (_jmSetSafeArea != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "clearFocus";
                    _jmClearFocus = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
                    if (_jmClearFocus != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModManager";
                    _jmGetGameModManager = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()Lcom/razerzone/store/sdk/content/GameModManager;");
                    if (_jmGetGameModManager != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "saveGameMod";
                    _jmSaveGameMod = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Lcom/razerzone/store/sdk/content/GameMod;Lcom/razerzone/store/sdk/content/GameMod$Editor;)V");
                    if (_jmSaveGameMod != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModManagerInstalled";
                    _jmGetGameModManagerInstalled = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
                    if (_jmGetGameModManagerInstalled != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModManagerInstalledResultsArray";
                    _jmGetGameModManagerInstalledResultsArray = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()[Lcom/razerzone/store/sdk/content/GameMod;");
                    if (_jmGetGameModManagerInstalledResultsArray != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModManagerPublished";
                    _jmGetGameModManagerPublished = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)V");
                    if (_jmGetGameModManagerPublished != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModManagerPublishedResultsArray";
                    _jmGetGameModManagerPublishedResultsArray = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()[Lcom/razerzone/store/sdk/content/GameMod;");
                    if (_jmGetGameModManagerPublishedResultsArray != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "contentDelete";
                    _jmContentDelete = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Lcom/razerzone/store/sdk/content/GameMod;)V");
                    if (_jmContentDelete != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "contentDownload";
                    _jmContentDownload = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Lcom/razerzone/store/sdk/content/GameMod;)V");
                    if (_jmContentDownload != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "contentPublish";
                    _jmContentPublish = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Lcom/razerzone/store/sdk/content/GameMod;)V");
                    if (_jmContentPublish != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "contentUnpublish";
                    _jmContentUnpublish = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Lcom/razerzone/store/sdk/content/GameMod;)V");
                    if (_jmContentUnpublish != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getFloat";
                    _jmGetFloat = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/Float;)F");
                    if (_jmGetFloat != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getBitmapArray";
                    _jmGetBitmapArray = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/util/List;)[Landroid/graphics/Bitmap;");
                    if (_jmGetBitmapArray != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getGameModScreenshotArray";
                    _jmGetGameModScreenshotArray = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/util/List;)[Lcom/razerzone/store/sdk/content/GameModScreenshot;");
                    if (_jmGetGameModScreenshotArray != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "getStringArray";
                    _jmGetStringArray = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/util/List;)[Ljava/lang/String;");
                    if (_jmGetStringArray != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }

                {
                    string strMethod = "shutdown";
                    _jmShutdown = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
                    if (_jmShutdown != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} method", strMethod));
#endif
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("Exception loading JNI - {0}", ex));
            }
        }

        public Plugin(IntPtr currentActivity)
        {
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmConstructor == IntPtr.Zero)
            {
                Debug.LogError("_jmConstructor is not initialized");
                return;
            }
            _instance = AndroidJNI.NewObject(_jcPlugin, _jmConstructor, new jvalue[] { new jvalue() { l = currentActivity } });
        }

        public static void initPlugin(string secretAPIKey)
        {
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmInitPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jmInitPlugin is not initialized");
                return;
            }
            IntPtr arg1 = AndroidJNI.NewStringUTF(secretAPIKey);
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmInitPlugin, new jvalue[] { new jvalue() { l = arg1 } });
            AndroidJNI.DeleteLocalRef(arg1);
        }

        public static bool isInitialized()
        {
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return false;
            }
            if (_jmIsInitialized == IntPtr.Zero)
            {
                Debug.LogError("_jmIsInitialized is not initialized");
                return false;
            }
            return AndroidJNI.CallStaticBooleanMethod(_jcPlugin, _jmIsInitialized, new jvalue[] { });
        }

		public static string getDeviceHardwareName()
		{
#if VERBOSE_LOGGING
			Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
			JNIFind();
			
			if (_jcPlugin == IntPtr.Zero)
			{
				Debug.LogError("_jcPlugin is not initialized");
				return null;
			}
			if (_jmGetDeviceHardwareName == IntPtr.Zero)
			{
				Debug.LogError("_jmGetDeviceHardwareName is not initialized");
				return null;
			}
			IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetDeviceHardwareName, new jvalue[0]);

			if (result == IntPtr.Zero)
			{
				Debug.LogError("Failed to getDeviceHardwareName");
				return null;
			}
			
			String retVal = AndroidJNI.GetStringUTFChars(result);
			AndroidJNI.DeleteLocalRef(result);
			return retVal;
		}

        public static string getGameData(string key)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetGameData == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameData is not initialized");
                return null;
            }
            IntPtr arg1 = AndroidJNI.NewStringUTF(key);
            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetGameData, new jvalue[] { new jvalue() { l = arg1 } });
            AndroidJNI.DeleteLocalRef(arg1);

            if (result == IntPtr.Zero)
            {
                Debug.LogError("Failed to getGameData");
                return null;
            }

            String retVal = AndroidJNI.GetStringUTFChars(result);
            return retVal;
        }

        public static void putGameData(string key, string val)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmPutGameData == IntPtr.Zero)
            {
                Debug.LogError("_jmPutGameData is not initialized");
                return;
            }
            IntPtr arg1 = AndroidJNI.NewStringUTF(key);
            IntPtr arg2 = AndroidJNI.NewStringUTF(val);
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmPutGameData, new jvalue[] { new jvalue() { l = arg1 }, new jvalue() { l = arg2 } });
            AndroidJNI.DeleteLocalRef(arg1);
            AndroidJNI.DeleteLocalRef(arg2);
        }

        public static void requestGamerInfo()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmRequestGamerInfo == IntPtr.Zero)
            {
                Debug.LogError("_jmRequestGamerInfo is not initialized");
                return;
            }

            // Make one request at a time
            if (m_pendingRequestGamerInfo)
            {
                return;
            }
            m_pendingRequestGamerInfo = true;

            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmRequestGamerInfo, new jvalue[0]);
        }

        public static void requestProducts(string jsonData)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmRequestProducts == IntPtr.Zero)
            {
                Debug.LogError("_jmGetProductsAsync is not initialized");
                return;
            }

            // Make one request at a time
            if (m_pendingRequestProducts)
            {
                return;
            }
            m_pendingRequestProducts = true;

            IntPtr arg1 = AndroidJNI.NewStringUTF(jsonData);
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmRequestProducts, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static void requestPurchase(string identifier)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmRequestPurchase == IntPtr.Zero)
            {
                Debug.LogError("_jmRequestPurchase is not initialized");
                return;
            }

            // Make one request at a time
            if (m_pendingRequestPurchase)
            {
                return;
            }
            m_pendingRequestPurchase = true;

            IntPtr arg1 = AndroidJNI.NewStringUTF(identifier);
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmRequestPurchase, new jvalue[] { new jvalue() { l = arg1 } });
            AndroidJNI.DeleteLocalRef(arg1);
        }

        public static void requestReceipts()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmRequestReceipts == IntPtr.Zero)
            {
                Debug.LogError("_jmRequestReceipts is not initialized");
                return;
            }

            // Make one request at a time
            if (m_pendingRequestReceipts)
            {
                return;
            }
            m_pendingRequestReceipts = true;

            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmRequestReceipts, new jvalue[0]);
        }

        public static bool isRunningOnSupportedHardware()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return false;
            }
            if (_jmIsRunningOnSupportedHardware == IntPtr.Zero)
            {
                Debug.LogError("_jmIsRunningOnSupportedHardware is not initialized");
                return false;
            }
            return AndroidJNI.CallStaticBooleanMethod(_jcPlugin, _jmIsRunningOnSupportedHardware, new jvalue[0]);
        }

        public static void setSafeArea(float percentage)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmSetSafeArea == IntPtr.Zero)
            {
                Debug.LogError("_jmSetSafeArea is not initialized");
                return;
            }
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmSetSafeArea, new jvalue[1] { new jvalue() { f = percentage } });
        }

        public static void clearFocus()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmClearFocus == IntPtr.Zero)
            {
                Debug.LogError("_jmClearFocus is not initialized");
                return;
            }
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmClearFocus, new jvalue[0] { });
        }

        public static GameModManager getGameModManager()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetGameModManager == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManager is not initialized");
                return null;
            }
            IntPtr retVal = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetGameModManager, new jvalue[0] { });
            if (retVal == IntPtr.Zero)
            {
                Debug.LogError("GetGameModManager returned null");
                return null;
            }
            IntPtr globalPtr = AndroidJNI.NewGlobalRef(retVal);
            AndroidJNI.DeleteLocalRef(retVal);
            return new GameModManager(globalPtr);
        }

        public static void saveGameMod(GameMod gameMod, GameMod.Editor editor)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmSaveGameMod == IntPtr.Zero)
            {
                Debug.LogError("_jmSaveGameMod is not initialized");
                return;
            }

            IntPtr arg1 = gameMod.GetInstance();
            IntPtr arg2 = editor.GetInstance();
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmSaveGameMod, new jvalue[] { new jvalue() { l = arg1 }, new jvalue() { l = arg2 } });
        }

        public static void getGameModManagerInstalled()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmGetGameModManagerInstalled == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerInstalled is not initialized");
                return;
            }

            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmGetGameModManagerInstalled, new jvalue[0] { });
        }

        public static List<GameMod> getGameModManagerInstalledResults()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetGameModManagerInstalledResultsArray == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerInstalledResultsArray is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetGameModManagerInstalledResultsArray, new jvalue[0] { });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerInstalledResultsArray returned null");
                return null;
            }

            List<GameMod> gameMods = new List<GameMod>();
            IntPtr[] resultArray = AndroidJNI.FromObjectArray(result);
            foreach (IntPtr ptr in resultArray)
            {
                IntPtr globalRef = AndroidJNI.NewGlobalRef(ptr);
                AndroidJNI.DeleteLocalRef(ptr);
                GameMod gameMod = new GameMod(globalRef);
                gameMods.Add(gameMod);
            }
            AndroidJNI.DeleteLocalRef(result);
            return gameMods;
        }

        public static void getGameModManagerPublished(GameModManager.SortMethod sortMethod)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmGetGameModManagerPublished == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerPublished is not initialized");
                return;
            }

            String strSortMethod = sortMethod.ToString();
            //Debug.Log(string.Format("SortMethod={0}", strSortMethod));
            IntPtr arg1 = AndroidJNI.NewStringUTF(strSortMethod);
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmGetGameModManagerPublished, new jvalue[] { new jvalue() { l = arg1 } });
            AndroidJNI.DeleteLocalRef(arg1);
        }

        public static List<GameMod> getGameModManagerPublishedResults()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetGameModManagerPublishedResultsArray == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerPublishedResultsArray is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetGameModManagerPublishedResultsArray, new jvalue[0] { });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModManagerPublishedResultsArray returned null");
                return null;
            }

            List<GameMod> gameMods = new List<GameMod>();
            IntPtr[] resultArray = AndroidJNI.FromObjectArray(result);
            foreach (IntPtr ptr in resultArray)
            {
                IntPtr globalRef = AndroidJNI.NewGlobalRef(ptr);
                AndroidJNI.DeleteLocalRef(ptr);
                GameMod gameMod = new GameMod(globalRef);
                gameMods.Add(gameMod);
            }
            AndroidJNI.DeleteLocalRef(result);
            return gameMods;
        }

        public static void contentDelete(GameMod gameMod)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmContentDelete == IntPtr.Zero)
            {
                Debug.LogError("_jmContentDelete is not initialized");
                return;
            }

            IntPtr arg1 = gameMod.GetInstance();
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmContentDelete, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static void contentPublish(GameMod gameMod)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmContentPublish == IntPtr.Zero)
            {
                Debug.LogError("_jmContentPublish is not initialized");
                return;
            }

            IntPtr arg1 = gameMod.GetInstance();
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmContentPublish, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static void contentUnpublish(GameMod gameMod)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmContentUnpublish == IntPtr.Zero)
            {
                Debug.LogError("_jmContentUnpublish is not initialized");
                return;
            }

            IntPtr arg1 = gameMod.GetInstance();
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmContentUnpublish, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static void contentDownload(GameMod gameMod)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }
            if (_jmContentDownload == IntPtr.Zero)
            {
                Debug.LogError("_jmContentDownload is not initialized");
                return;
            }

            IntPtr arg1 = gameMod.GetInstance();
            AndroidJNI.CallStaticVoidMethod(_jcPlugin, _jmContentDownload, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static float getFloat(IntPtr f)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return 0;
            }
            if (_jmGetFloat == IntPtr.Zero)
            {
                Debug.LogError("_jmGetFloat is not initialized");
                return 0;
            }

            IntPtr arg1 = f;
            return AndroidJNI.CallStaticFloatMethod(_jcPlugin, _jmGetFloat, new jvalue[] { new jvalue() { l = arg1 } });
        }

        public static List<Bitmap> getBitmapList(IntPtr list)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetBitmapArray == IntPtr.Zero)
            {
                Debug.LogError("_jmGetBitmapArray is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetBitmapArray, new jvalue[] { new jvalue() { l = list } });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetBitmapArray returned null");
                return null;
            }

            List<Bitmap> items = new List<Bitmap>();

#if VERBOSE_LOGGING
            Debug.Log("Invoking AndroidJNI.FromObjectArray...");
#endif
            IntPtr[] resultArray = AndroidJNI.FromObjectArray(result);
#if VERBOSE_LOGGING
            Debug.Log("Invoked AndroidJNI.FromObjectArray.");
#endif

            foreach (IntPtr ptr in resultArray)
            {
#if VERBOSE_LOGGING
                Debug.Log("Found Bitmap making Global Ref...");
#endif
                IntPtr globalRef = AndroidJNI.NewGlobalRef(ptr);
                AndroidJNI.DeleteLocalRef(ptr);
#if VERBOSE_LOGGING
                Debug.Log("Made global ref for Bitmap.");
#endif
                Bitmap item = new Bitmap(globalRef);
#if VERBOSE_LOGGING
                Debug.Log("Deleting old bitmap reference...");
#endif
                items.Add(item);
            }
#if VERBOSE_LOGGING
            Debug.Log("Deleting bitmap list reference...");
#endif
            AndroidJNI.DeleteLocalRef(result);
            return items;
        }

        public static List<GameModScreenshot> getGameModScreenshotList(IntPtr list)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetGameModScreenshotArray == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModScreenshotArray is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetGameModScreenshotArray, new jvalue[] { new jvalue() { l = list } });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetGameModScreenshotArray returned null");
                return null;
            }

            List<GameModScreenshot> items = new List<GameModScreenshot>();

#if VERBOSE_LOGGING
            Debug.Log("Invoking AndroidJNI.FromObjectArray...");
#endif

            IntPtr[] resultArray = AndroidJNI.FromObjectArray(result);
            AndroidJNI.DeleteLocalRef(result);

#if VERBOSE_LOGGING
            Debug.Log("Invoked AndroidJNI.FromObjectArray.");
#endif

            foreach (IntPtr ptr in resultArray)
            {
#if VERBOSE_LOGGING
                Debug.Log("Found Bitmap making Global Ref...");
#endif
                IntPtr globalRef = AndroidJNI.NewGlobalRef(ptr);
                AndroidJNI.DeleteLocalRef(ptr);
#if VERBOSE_LOGGING
                Debug.Log("Made global ref for Bitmap.");
#endif
                GameModScreenshot item = new GameModScreenshot(globalRef);
#if VERBOSE_LOGGING
                Debug.Log("Deleting old local ref...");
#endif
                items.Add(item);
            }
#if VERBOSE_LOGGING
            Debug.Log("Deleting GameModScreenshot list reference...");
#endif
            return items;
        }

        public static List<string> getStringList(IntPtr list)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }
            if (_jmGetStringArray == IntPtr.Zero)
            {
                Debug.LogError("_jmGetStringArray is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallStaticObjectMethod(_jcPlugin, _jmGetStringArray, new jvalue[] { new jvalue() { l = list } });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetStringArray returned null");
                return null;
            }

            List<string> items = new List<string>();
            IntPtr[] resultArray = AndroidJNI.FromObjectArray(result);
            AndroidJNI.DeleteLocalRef(result);
            foreach (IntPtr ptr in resultArray)
            {
                string item = AndroidJNI.GetStringUTFChars(ptr);
                AndroidJNI.DeleteLocalRef(ptr);
                items.Add(item);
            }
            return items;
        }

        public static string getStringResource(string key)
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return null;
            }

            string strMethod = "getStringResource";
            IntPtr method = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "(Ljava/lang/String;)Ljava/lang/String;");
            if (method != IntPtr.Zero)
            {
#if VERBOSE_LOGGING
                Debug.Log(string.Format("Found {0} method", strMethod));
#endif
            }
            else
            {
                Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                return string.Empty;
            }

            IntPtr arg1 = AndroidJNI.NewStringUTF(key);
            string result = AndroidJNI.CallStaticStringMethod(_jcPlugin, method, new jvalue[] { new jvalue() { l = arg1 } });
            AndroidJNI.DeleteLocalRef(arg1);

#if VERBOSE_LOGGING
            if (null == result)
            {
                Debug.Log("getStringResource returned: null");
            }
            else
            {
                Debug.Log(string.Format("getStringResource returned: {0}", result));
            }
#endif

            return result;
        }

        public static void shutdown()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif

            if (_jcPlugin == IntPtr.Zero)
            {
                Debug.LogError("_jcPlugin is not initialized");
                return;
            }

            string strMethod = "shutdown";
            IntPtr method = AndroidJNI.GetStaticMethodID(_jcPlugin, strMethod, "()V");
            if (method != IntPtr.Zero)
            {
#if VERBOSE_LOGGING
                Debug.Log(string.Format("Found {0} method", strMethod));
#endif
            }
            else
            {
                Debug.LogError(string.Format("Failed to find {0} method", strMethod));
                return;
            }

            AndroidJNI.CallStaticVoidMethod(_jcPlugin, method, new jvalue[] { });
        }

    }
}

#endif
