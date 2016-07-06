//#define VERBOSE_LOGGING
#if UNITY_ANDROID && !UNITY_EDITOR
using Android.Graphics;
using System;
using System.Reflection;
using UnityEngine;

namespace com.razerzone.store.sdk.content
{
    public class GameModScreenshot : IDisposable
    {
        private static IntPtr _jcGameModScreenshot = IntPtr.Zero;
        private static IntPtr _jmGetImage = IntPtr.Zero;
        private static IntPtr _jmGetThumbnail = IntPtr.Zero;
        private IntPtr _instance = IntPtr.Zero;

        static GameModScreenshot()
        {
            try
            {
                {
                    string strName = "com/razerzone/store/sdk/content/GameModScreenshot";
                    IntPtr localRef = AndroidJNI.FindClass(strName);
                    if (localRef != IntPtr.Zero)
                    {
#if VERBOSE_LOGGING
                        Debug.Log(string.Format("Found {0} class", strName));
#endif
                        _jcGameModScreenshot = AndroidJNI.NewGlobalRef(localRef);
                        AndroidJNI.DeleteLocalRef(localRef);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Failed to find {0} class", strName));
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
            {
                string strMethod = "getImage";
                _jmGetImage = AndroidJNI.GetMethodID(_jcGameModScreenshot, strMethod, "()Landroid/graphics/Bitmap;");
                if (_jmGetImage != IntPtr.Zero)
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
                string strMethod = "getThumbnail";
                _jmGetThumbnail = AndroidJNI.GetMethodID(_jcGameModScreenshot, strMethod, "()Landroid/graphics/Bitmap;");
                if (_jmGetThumbnail != IntPtr.Zero)
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

        public GameModScreenshot(IntPtr instance)
        {
            _instance = instance;
        }

        public IntPtr GetInstance()
        {
            return _instance;
        }

        public void Dispose()
        {
            if (_instance != IntPtr.Zero)
            {
                AndroidJNI.DeleteGlobalRef(_instance);
                _instance = IntPtr.Zero;
            }
        }

        public Bitmap getImage()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_instance == IntPtr.Zero)
            {
                Debug.LogError("_instance is not initialized");
                return null;
            }
            if (_jmGetImage == IntPtr.Zero)
            {
                Debug.LogError("_jmGetImage is not initialized");
                return null;
            }
            
            IntPtr result = AndroidJNI.CallObjectMethod(_instance, _jmGetImage, new jvalue[0] { });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetImage returned null");
                return null;
            }

            IntPtr globalRef = AndroidJNI.NewGlobalRef(result);
            AndroidJNI.DeleteLocalRef(result);
            return new Bitmap(globalRef);
        }

        public Bitmap getThumbnail()
        {
#if VERBOSE_LOGGING
            Debug.Log(string.Format("Invoking {0}...", MethodBase.GetCurrentMethod().Name));
#endif
            JNIFind();

            if (_instance == IntPtr.Zero)
            {
                Debug.LogError("_instance is not initialized");
                return null;
            }
            if (_jmGetThumbnail == IntPtr.Zero)
            {
                Debug.LogError("_jmGetThumbnail is not initialized");
                return null;
            }

            IntPtr result = AndroidJNI.CallObjectMethod(_instance, _jmGetThumbnail, new jvalue[0] { });
            if (result == IntPtr.Zero)
            {
                Debug.LogError("_jmGetThumbnail returned null");
                return null;
            }

            IntPtr globalRef = AndroidJNI.NewGlobalRef(result);
            AndroidJNI.DeleteLocalRef(result);
            return new Bitmap(globalRef);
        }
    }
}

#endif