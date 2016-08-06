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

// Unity JNI reference: http://docs.unity3d.com/Documentation/ScriptReference/AndroidJNI.html
// JNI Spec: http://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/jniTOC.html
// Android Plugins: http://docs.unity3d.com/Documentation/Manual/Plugins.html#AndroidPlugins 
// Unity Android Plugin Guide: http://docs.unity3d.com/Documentation/Manual/PluginsForAndroid.html

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_ANDROID && !UNITY_EDITOR
using com.unity3d.player;
using org.json;
using com.razerzone.store.sdk.content;
#endif
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public static class RazerSDK
    {
        public const string PLUGIN_VERSION = "2.1.0.6";

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Dictionary for quick localization string lookup
        /// </summary>
        private static Dictionary<string, string> s_stringResources = new Dictionary<string, string>();

        // When true, disables plugin input handling
        private static bool s_useDefaultInput = false;

        static RazerSDK()
        {
            // attach our thread to the java vm; obviously the main thread is already attached but this is good practice..
            AndroidJNI.AttachCurrentThread();

            new Plugin(UnityPlayer.currentActivity);
        }

        public static bool GetUseDefaultInput()
        {
            return s_useDefaultInput;
        }

        public class NdkWrapper
        {
            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API float getAxis(int deviceId, int axis)
            public static extern float getAxis(int deviceId, int axis);

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API bool isPressed(int deviceId, int keyCode)
            public static extern bool isPressed(int deviceId, int keyCode);

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API bool isPressedDown(int deviceId, int keyCode)
            public static extern bool isPressedDown(int deviceId, int keyCode);

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API bool isPressedUp(int deviceId, int keyCode)
            public static extern bool isPressedUp(int deviceId, int keyCode);

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API void clearButtonStates()
            public static extern void clearButtonStates();

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API void clearAxes()
            public static extern void clearAxes();

            [DllImport("lib-ndk-unity-store-sdk")]
            // EXPORT_API void clearButtons()
            public static extern void clearButtons();
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR

        public class ControllerInput
        {
            #region Private API

            private static object m_lockObject = new object();
            private static List<Dictionary<int, float>> m_axisStates = new List<Dictionary<int, float>>();
            private static List<Dictionary<int, bool>> m_buttonStates = new List<Dictionary<int, bool>>();
            private static List<Dictionary<int, bool>> m_buttonDownStates = new List<Dictionary<int, bool>>();
            private static List<Dictionary<int, bool>> m_buttonUpStates = new List<Dictionary<int, bool>>();

            static ControllerInput()
            {
                for (int deviceId = 0; deviceId < Controller.MAX_CONTROLLERS; ++deviceId)
                {
                    m_axisStates.Add(new Dictionary<int, float>());
                    m_buttonStates.Add(new Dictionary<int, bool>());
                    m_buttonDownStates.Add(new Dictionary<int, bool>());
                    m_buttonUpStates.Add(new Dictionary<int, bool>());
                }
            }

            private static float GetState(int axis, Dictionary<int, float> dictionary)
            {
                float result;
                lock (m_lockObject)
                {
                    if (dictionary.ContainsKey(axis))
                    {
                        result = dictionary[axis];
                    }
                    else
                    {
                        result = 0f;
                    }
                }
                return result;
            }

            private static bool GetState(int button, Dictionary<int, bool> dictionary)
            {
                bool result;
                lock (m_lockObject)
                {
                    if (dictionary.ContainsKey(button))
                    {
                        result = dictionary[button];
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }

            private static bool GetState(bool isPressed, int button, Dictionary<int, bool> dictionary)
            {
                bool result;
                lock (m_lockObject)
                {
                    if (dictionary.ContainsKey(button))
                    {
                        result = isPressed == dictionary[button];
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }

            public static void UpdateInputFrame()
            {
                if (GetUseDefaultInput())
                {
                    return;
                }

                lock (m_lockObject)
                {
                    for (int deviceId = 0; deviceId < Controller.MAX_CONTROLLERS; ++deviceId)
                    {
                        #region Track Axis States

                        Dictionary<int, float> axisState = m_axisStates[deviceId];
                        axisState[Controller.AXIS_LS_X] = NdkWrapper.getAxis(deviceId, Controller.AXIS_LS_X);
                        axisState[Controller.AXIS_LS_Y] = NdkWrapper.getAxis(deviceId, Controller.AXIS_LS_Y);
                        axisState[Controller.AXIS_RS_X] = NdkWrapper.getAxis(deviceId, Controller.AXIS_RS_X);
                        axisState[Controller.AXIS_RS_Y] = NdkWrapper.getAxis(deviceId, Controller.AXIS_RS_Y);
                        axisState[Controller.AXIS_L2] = NdkWrapper.getAxis(deviceId, Controller.AXIS_L2);
                        axisState[Controller.AXIS_R2] = NdkWrapper.getAxis(deviceId, Controller.AXIS_R2);

                        #endregion

                        #region Track Button Up / Down States

                        Dictionary<int, bool> buttonState = m_buttonStates[deviceId];
                        Dictionary<int, bool> buttonDownState = m_buttonDownStates[deviceId];
                        Dictionary<int, bool> buttonUpState = m_buttonUpStates[deviceId];

                        buttonState[Controller.BUTTON_O] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_O);
                        buttonState[Controller.BUTTON_U] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_U);
                        buttonState[Controller.BUTTON_Y] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_Y);
                        buttonState[Controller.BUTTON_A] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_A);
                        buttonState[Controller.BUTTON_L1] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_L1);
                        buttonState[Controller.BUTTON_R1] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_R1);
                        buttonState[Controller.BUTTON_L3] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_L3);
                        buttonState[Controller.BUTTON_R3] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_R3);
                        buttonState[Controller.BUTTON_DPAD_UP] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_DPAD_UP);
                        buttonState[Controller.BUTTON_DPAD_DOWN] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_DPAD_DOWN);
                        buttonState[Controller.BUTTON_DPAD_RIGHT] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_DPAD_RIGHT);
                        buttonState[Controller.BUTTON_DPAD_LEFT] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_DPAD_LEFT);
                        buttonState[Controller.BUTTON_MENU] = NdkWrapper.isPressed(deviceId, Controller.BUTTON_MENU);

                        buttonDownState[Controller.BUTTON_O] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_O);
                        buttonDownState[Controller.BUTTON_U] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_U);
                        buttonDownState[Controller.BUTTON_Y] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_Y);
                        buttonDownState[Controller.BUTTON_A] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_A);
                        buttonDownState[Controller.BUTTON_L1] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_L1);
                        buttonDownState[Controller.BUTTON_R1] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_R1);
                        buttonDownState[Controller.BUTTON_L3] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_L3);
                        buttonDownState[Controller.BUTTON_R3] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_R3);
                        buttonDownState[Controller.BUTTON_DPAD_UP] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_DPAD_UP);
                        buttonDownState[Controller.BUTTON_DPAD_DOWN] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_DPAD_DOWN);
                        buttonDownState[Controller.BUTTON_DPAD_RIGHT] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_DPAD_RIGHT);
                        buttonDownState[Controller.BUTTON_DPAD_LEFT] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_DPAD_LEFT);
                        buttonDownState[Controller.BUTTON_MENU] = NdkWrapper.isPressedDown(deviceId, Controller.BUTTON_MENU);

                        buttonUpState[Controller.BUTTON_O] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_O);
                        buttonUpState[Controller.BUTTON_U] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_U);
                        buttonUpState[Controller.BUTTON_Y] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_Y);
                        buttonUpState[Controller.BUTTON_A] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_A);
                        buttonUpState[Controller.BUTTON_L1] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_L1);
                        buttonUpState[Controller.BUTTON_R1] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_R1);
                        buttonUpState[Controller.BUTTON_L3] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_L3);
                        buttonUpState[Controller.BUTTON_R3] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_R3);
                        buttonUpState[Controller.BUTTON_DPAD_UP] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_DPAD_UP);
                        buttonUpState[Controller.BUTTON_DPAD_DOWN] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_DPAD_DOWN);
                        buttonUpState[Controller.BUTTON_DPAD_RIGHT] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_DPAD_RIGHT);
                        buttonUpState[Controller.BUTTON_DPAD_LEFT] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_DPAD_LEFT);
                        buttonUpState[Controller.BUTTON_MENU] = NdkWrapper.isPressedUp(deviceId, Controller.BUTTON_MENU);

                        #endregion

                        //debugController(deviceId);
                    }
                }
            }

            public static void ClearButtonStates()
            {
                NdkWrapper.clearButtonStates();
            }

            public static void ClearAxes()
            {
                NdkWrapper.clearAxes();
            }

            public static void ClearButtons()
            {
                NdkWrapper.clearButtons();
            }

            private static void debugController(int deviceId, int button)
            {
                if (GetButtonDown(deviceId, button))
                {
                    Debug.Log("Device=" + deviceId + " GetButtonDown: " + button);
                }

                if (GetButtonUp(deviceId, button))
                {
                    Debug.Log("Device=" + deviceId + " GetButtonUp: " + button);
                }
            }

            private static void debugController(int deviceId)
            {
                debugController(deviceId, Controller.BUTTON_O);
                debugController(deviceId, Controller.BUTTON_U);
                debugController(deviceId, Controller.BUTTON_Y);
                debugController(deviceId, Controller.BUTTON_A);
                debugController(deviceId, Controller.BUTTON_L1);
                debugController(deviceId, Controller.BUTTON_R1);
                debugController(deviceId, Controller.BUTTON_L3);
                debugController(deviceId, Controller.BUTTON_R3);
                debugController(deviceId, Controller.BUTTON_DPAD_UP);
                debugController(deviceId, Controller.BUTTON_DPAD_DOWN);
                debugController(deviceId, Controller.BUTTON_DPAD_RIGHT);
                debugController(deviceId, Controller.BUTTON_DPAD_LEFT);
                debugController(deviceId, Controller.BUTTON_MENU);
            }

            #endregion

            #region Public API

            public static bool IsControllerConnected(int playerNum)
            {
                if (playerNum >= 0 &&
                    null != RazerSDK.Joysticks &&
                    playerNum < RazerSDK.Joysticks.Length)
                {
                    return (null != RazerSDK.Joysticks[playerNum]);
                }
                else
                {
                    return false;
                }
            }

            public static float GetAxis(int playerNum, int axis)
            {
                if (playerNum >= 0 &&
                    null != m_axisStates &&
                    playerNum < m_axisStates.Count)
                {
                    return GetState(axis, m_axisStates[playerNum]);
                }
                else
                {
                    return 0f;
                }
            }

            public static float GetAxisRaw(int playerNum, int axis)
            {
                if (playerNum >= 0 &&
                    null != m_axisStates &&
                    playerNum < m_axisStates.Count)
                {
                    return GetState(axis, m_axisStates[playerNum]);
                }
                else
                {
                    return 0f;
                }
            }

            public static bool GetButton(int playerNum, int button)
            {
                if (playerNum >= 0 &&
                    null != m_buttonStates &&
                    playerNum < m_buttonStates.Count)
                {
                    return GetState(button, m_buttonStates[playerNum]);
                }
                else
                {
                    return false;
                }
            }

            public static bool GetButton(int button)
            {
                for (int playerNum = 0; playerNum < Controller.MAX_CONTROLLERS; ++playerNum)
                {
                    if (GetButton(playerNum, button))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool GetButtonDown(int playerNum, int button)
            {
                if (playerNum >= 0 &&
                    null != m_buttonDownStates &&
                    playerNum < m_buttonDownStates.Count)
                {
                    return GetState(button, m_buttonDownStates[playerNum]);
                }
                else
                {
                    return false;
                }
            }

            public static bool GetButtonDown(int button)
            {
                for (int playerNum = 0; playerNum < Controller.MAX_CONTROLLERS; ++playerNum)
                {
                    if (GetButtonDown(playerNum, button))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool GetButtonUp(int playerNum, int button)
            {
                if (playerNum >= 0 &&
                    null != m_buttonUpStates &&
                    playerNum < m_buttonUpStates.Count)
                {
                    return GetState(button, m_buttonUpStates[playerNum]);
                }
                else
                {
                    return false;
                }
            }

            public static bool GetButtonUp(int button)
            {
                for (int playerNum = 0; playerNum < Controller.MAX_CONTROLLERS; ++playerNum)
                {
                    if (GetButtonUp(playerNum, button))
                    {
                        return true;
                    }
                }
                return false;
            }

            #endregion
        }

#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Cache joysticks
        /// </summary>
        public static string[] Joysticks = null;

        /// <summary>
        /// Query joysticks every N seconds
        /// </summary>
        private static DateTime m_timerJoysticks = DateTime.MinValue;

        private static string getDeviceName(int deviceId)
        {
            Controller Controller = Controller.getControllerByPlayer(deviceId);
            if (null != Controller)
            {
                return Controller.getDeviceName();
            }
            return null;
        }
#endif

        /// <summary>
        /// Update joysticks with a timer
        /// </summary>
        public static void UpdateJoysticks()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            return;
#else
            if (m_timerJoysticks < DateTime.Now)
            {
                //check for new joysticks every N seconds
                m_timerJoysticks = DateTime.Now + TimeSpan.FromSeconds(3);

                string[] joysticks = null;
                List<string> devices = new List<string>();
                for (int deviceId = 0; deviceId < Controller.MAX_CONTROLLERS; ++deviceId)
                {
                    string deviceName = getDeviceName(deviceId);
                    //Debug.Log(string.Format("Device={0} name={1}", deviceId, deviceName));
                    devices.Add(deviceName);
                }
                joysticks = devices.ToArray();

                // look for changes
                bool detectedChange = false;

                if (null == Joysticks)
                {
                    detectedChange = true;
                }
                else if (joysticks.Length != Joysticks.Length)
                {
                    detectedChange = true;
                }
                else
                {
                    for (int index = 0; index < joysticks.Length; ++index)
                    {
                        if (joysticks[index] != Joysticks[index])
                        {
                            detectedChange = true;
                            break;
                        }
                    }
                }

                Joysticks = joysticks;

                if (detectedChange)
                {
                    foreach (RazerSDK.IJoystickCalibrationListener listener in RazerSDK.getJoystickCalibrationListeners())
                    {
                        //Debug.Log("RazerSDK: Invoke OnJoystickCalibration");
                        listener.OnJoystickCalibration();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Initialized by RazerGameObject
        /// </summary>
        public static void initPlugin(string secretAPIKey)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.initPlugin(secretAPIKey);
#endif
        }

        public static bool isIAPInitComplete()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Plugin.isInitialized();
#else
            return false;
#endif
        }

        public static string getDeviceHardwareName()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Plugin.isInitialized())
            {
                return Plugin.getDeviceHardwareName();
            }
            else
            {
                return string.Empty;
            }
#else
            return string.Empty;
#endif
        }

        #region Mirror Java API

        public static void requestGamerInfo()
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.requestGamerInfo();
#endif
        }

        public static void putGameData(string key, string val)
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.putGameData(key, val);
#endif
        }

        public static string getGameData(string key)
        {
            if (!isIAPInitComplete())
            {
                return string.Empty;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            return Plugin.getGameData(key);
#else
            return String.Empty;
#endif
        }

        public static void requestProducts(List<String> purchasables)
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            JSONArray jsonArray = new JSONArray();
            int index = 0;
            foreach (String identifier in purchasables)
            {
                jsonArray.put(index, identifier);
                ++index;
            }
            Plugin.requestProducts(jsonArray.toString());
            jsonArray.Dispose();
#endif
        }

        public enum ProductType
        {
            ENTITLEMENT,
            CONSUMABLE,
            SUBSCRIPTION,
            UNKNOWN
        }

        public static void requestPurchase(String identifier, ProductType productType)
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.requestPurchase(identifier, productType.ToString());
#endif
        }

        public static void requestReceipts()
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.requestReceipts();
#endif
        }

        public static bool isRunningOnSupportedHardware()
        {
            if (!isIAPInitComplete())
            {
                return false;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            return Plugin.isRunningOnSupportedHardware();
#else
            return false;
#endif
        }

        /// <summary>
        /// 1f - Safe Area all the way to the edge of the screen
        /// 0f - Safe Area will use the maximum overscan border
        /// </summary>
        /// <param name="percentage"></param>
        public static void setSafeArea(float percentage)
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.setSafeArea(percentage);
#endif
        }

        /// <summary>
        /// Clear input focus
        /// </summary>
        public static void clearFocus()
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.clearFocus();
#endif
        }

        /// <summary>
        /// Get localized string resource
        /// </summary>
        public static string getStringResource(string key)
        {
            if (!isIAPInitComplete())
            {
                return string.Empty;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (s_stringResources.ContainsKey(key))
            {
                return s_stringResources[key];
            }
            string val = Plugin.getStringResource(key);
            s_stringResources.Add(key, val);
            return val;
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Shutdown the SDK
        /// </summary>
        public static void shutdown()
        {
            if (!isIAPInitComplete())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.shutdown();
#endif
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public static void quit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Plugin.quit();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Use default input, bypass plugin input
        /// </summary>
        public static void useDefaultInput()
        {
            if (!isIAPInitComplete())
            {
                return;
            }
            Plugin.useDefaultInput();
            ControllerInput.ClearAxes();
            ControllerInput.ClearButtons();
            ControllerInput.ClearButtonStates();
            ControllerInput.ClearButtonStates();
            ControllerInput.UpdateInputFrame();
            s_useDefaultInput = true;
        }

#endif

        #endregion

        #region Data containers

        [Serializable]
        public class GamerInfo
        {
            public string uuid = string.Empty;
            public string username = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            public static GamerInfo Parse(JSONObject jsonObject)
            {
                GamerInfo result = new GamerInfo();

                //Debug.Log(jsonData);
                if (jsonObject.has("uuid"))
                {
                    result.uuid = jsonObject.getString("uuid");
                }
                if (jsonObject.has("username"))
                {
                    result.username = jsonObject.getString("username");
                }
                return result;
            }
#endif
        }

        [Serializable]
        public class Product
        {
            public string currencyCode = string.Empty;
            public string description = string.Empty;
            public string identifier = string.Empty;
            public float localPrice = 0f;
            public string name = string.Empty;
            public float originalPrice = 0f;
            public float percentOff = 0f;
            public string developerName = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            public static Product Parse(JSONObject jsonData)
            {
                Product result = new Product();
                if (jsonData.has("currencyCode"))
                {
                    result.currencyCode = jsonData.getString("currencyCode");
                }
                if (jsonData.has("description"))
                {
                    result.description = jsonData.getString("description");
                }
                if (jsonData.has("identifier"))
                {
                    result.identifier = jsonData.getString("identifier");
                }
                if (jsonData.has("localPrice"))
                {
                    result.localPrice = (float)jsonData.getDouble("localPrice");
                }
                if (jsonData.has("name"))
                {
                    result.name = jsonData.getString("name");
                }
                if (jsonData.has("originalPrice"))
                {
                    result.originalPrice = (float)jsonData.getDouble("originalPrice");
                }
                if (jsonData.has("percentOff"))
                {
                    result.percentOff = (float)jsonData.getDouble("percentOff");
                }
                if (jsonData.has("developerName"))
                {
                    result.developerName = jsonData.getString("developerName");
                }
                return result;
            }
#endif
        }

        [Serializable]
        public class Receipt
        {
            public string currency = string.Empty;
            public string gamer = string.Empty;
            public DateTime generatedDate = DateTime.MinValue;
            public string identifier = string.Empty;
            public float localPrice = 0f;
            public DateTime purchaseDate = DateTime.MinValue;
            public string uuid = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            public static Receipt Parse(JSONObject jsonObject)
            {
                Receipt result = new Receipt();

                if (jsonObject.has("identifier"))
                {
                    result.identifier = jsonObject.getString("identifier");
                }
                if (jsonObject.has("purchaseDate"))
                {
                    DateTime date;
                    DateTime.TryParse(jsonObject.getString("purchaseDate"), out date);
                    result.purchaseDate = date;
                }
                if (jsonObject.has("gamer"))
                {
                    result.gamer = jsonObject.getString("gamer");
                }
                if (jsonObject.has("localPrice"))
                {
                    result.localPrice = (float)jsonObject.getDouble("localPrice");
                }
                if (jsonObject.has("uuid"))
                {
                    result.uuid = jsonObject.getString("uuid");
                }
                if (jsonObject.has("currency"))
                {
                    result.currency = jsonObject.getString("currency");
                }
                if (jsonObject.has("generatedDate"))
                {
                    DateTime date;
                    DateTime.TryParse(jsonObject.getString("generatedDate"), out date);
                    result.generatedDate = date;
                }

                return result;
            }
#endif
        }

        #endregion

#if UNITY_ANDROID && !UNITY_EDITOR

        #region Joystick Callibration Listeners

        public interface IJoystickCalibrationListener
        {
            void OnJoystickCalibration();
        }
        private static List<IJoystickCalibrationListener> m_joystickCalibrationListeners = new List<IJoystickCalibrationListener>();
        public static List<IJoystickCalibrationListener> getJoystickCalibrationListeners()
        {
            return m_joystickCalibrationListeners;
        }
        public static void registerJoystickCalibrationListener(IJoystickCalibrationListener listener)
        {
            if (!m_joystickCalibrationListeners.Contains(listener))
            {
                m_joystickCalibrationListeners.Add(listener);
            }
        }
        public static void unregisterJoystickCalibrationListener(IJoystickCalibrationListener listener)
        {
            if (m_joystickCalibrationListeners.Contains(listener))
            {
                m_joystickCalibrationListeners.Remove(listener);
            }
        }

        #endregion

        #region Pause Listeners

        public interface IPauseListener
        {
            void OnPause();
        }
        private static List<IPauseListener> m_pauseListeners = new List<IPauseListener>();
        public static List<IPauseListener> getPauseListeners()
        {
            return m_pauseListeners;
        }
        public static void registerPauseListener(IPauseListener listener)
        {
            if (!m_pauseListeners.Contains(listener))
            {
                m_pauseListeners.Add(listener);
            }
        }
        public static void unregisterPauseListener(IPauseListener listener)
        {
            if (m_pauseListeners.Contains(listener))
            {
                m_pauseListeners.Remove(listener);
            }
        }

        #endregion

        #region Resume Listeners

        public interface IResumeListener
        {
            void OnResume();
        }
        private static List<IResumeListener> m_resumeListeners = new List<IResumeListener>();
        public static List<IResumeListener> getResumeListeners()
        {
            return m_resumeListeners;
        }
        public static void registerResumeListener(IResumeListener listener)
        {
            if (!m_resumeListeners.Contains(listener))
            {
                m_resumeListeners.Add(listener);
            }
        }
        public static void unregisterResumeListener(IResumeListener listener)
        {
            if (m_resumeListeners.Contains(listener))
            {
                m_resumeListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Initialized Listener

        public interface IContentInitializedListener
        {
            void ContentInitializedOnInitialized();
            void ContentInitializedOnDestroyed();
        }
        private static List<IContentInitializedListener> m_contentInitializedListeners = new List<IContentInitializedListener>();
        public static List<IContentInitializedListener> getContentInitializedListeners()
        {
            return m_contentInitializedListeners;
        }
        public static void registerContentInitializedListener(IContentInitializedListener listener)
        {
            if (!m_contentInitializedListeners.Contains(listener))
            {
                m_contentInitializedListeners.Add(listener);
            }
        }
        public static void unregisterContentInitializedListener(IContentInitializedListener listener)
        {
            if (m_contentInitializedListeners.Contains(listener))
            {
                m_contentInitializedListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Delete Listener

        public interface IContentDeleteListener
        {
            void ContentDeleteOnDeleted(GameMod gameMod);
            void ContentDeleteOnDeleteFailed(GameMod gameMod, int code, string reason);
        }
        private static List<IContentDeleteListener> m_contentDeleteListeners = new List<IContentDeleteListener>();
        public static List<IContentDeleteListener> getContentDeleteListeners()
        {
            return m_contentDeleteListeners;
        }
        public static void registerContentDeleteListener(IContentDeleteListener listener)
        {
            if (!m_contentDeleteListeners.Contains(listener))
            {
                m_contentDeleteListeners.Add(listener);
            }
        }
        public static void unregisterContentDeleteListener(IContentDeleteListener listener)
        {
            if (m_contentDeleteListeners.Contains(listener))
            {
                m_contentDeleteListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Download Listener

        public interface IContentDownloadListener
        {
            void ContentDownloadOnComplete(GameMod gameMod);
            void ContentDownloadOnFailed(GameMod gameMod);
            void ContentDownloadOnProgress(GameMod gameMod, int progress);
        }
        private static List<IContentDownloadListener> m_contentDownloadListeners = new List<IContentDownloadListener>();
        public static List<IContentDownloadListener> getContentDownloadListeners()
        {
            return m_contentDownloadListeners;
        }
        public static void registerContentDownloadListener(IContentDownloadListener listener)
        {
            if (!m_contentDownloadListeners.Contains(listener))
            {
                m_contentDownloadListeners.Add(listener);
            }
        }
        public static void unregisterContentDownloadListener(IContentDownloadListener listener)
        {
            if (m_contentDownloadListeners.Contains(listener))
            {
                m_contentDownloadListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Installed Search Listener

        public interface IContentInstalledSearchListener
        {
            void ContentInstalledSearchOnResults(List<GameMod> gameMods, int count);
            void ContentInstalledSearchOnError(int code, string reason);
        }
        private static List<IContentInstalledSearchListener> m_contentInstalledSearchListeners = new List<IContentInstalledSearchListener>();
        public static List<IContentInstalledSearchListener> getContentInstalledSearchListeners()
        {
            return m_contentInstalledSearchListeners;
        }
        public static void registerContentInstalledSearchListener(IContentInstalledSearchListener listener)
        {
            if (!m_contentInstalledSearchListeners.Contains(listener))
            {
                m_contentInstalledSearchListeners.Add(listener);
            }
        }
        public static void unregisterContentInstalledSearchListener(IContentInstalledSearchListener listener)
        {
            if (m_contentInstalledSearchListeners.Contains(listener))
            {
                m_contentInstalledSearchListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Published Search Listener

        public interface IContentPublishedSearchListener
        {
            void ContentPublishedSearchOnResults(List<GameMod> gameMods, int count);
            void ContentPublishedSearchOnError(int code, string reason);
        }
        private static List<IContentPublishedSearchListener> m_contentPublishedSearchListeners = new List<IContentPublishedSearchListener>();
        public static List<IContentPublishedSearchListener> getContentPublishedSearchListeners()
        {
            return m_contentPublishedSearchListeners;
        }
        public static void registerContentPublishedSearchListener(IContentPublishedSearchListener listener)
        {
            if (!m_contentPublishedSearchListeners.Contains(listener))
            {
                m_contentPublishedSearchListeners.Add(listener);
            }
        }
        public static void unregisterContentPublishedSearchListener(IContentPublishedSearchListener listener)
        {
            if (m_contentPublishedSearchListeners.Contains(listener))
            {
                m_contentPublishedSearchListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Publish Listener

        public interface IContentPublishListener
        {
            void ContentPublishOnSuccess(GameMod gameMod);
            void ContentPublishOnError(GameMod gameMod, int code, string reason);
        }
        private static List<IContentPublishListener> m_contentPublishListeners = new List<IContentPublishListener>();
        public static List<IContentPublishListener> getContentPublishListeners()
        {
            return m_contentPublishListeners;
        }
        public static void registerContentPublishListener(IContentPublishListener listener)
        {
            if (!m_contentPublishListeners.Contains(listener))
            {
                m_contentPublishListeners.Add(listener);
            }
        }
        public static void unregisterContentPublishListener(IContentPublishListener listener)
        {
            if (m_contentPublishListeners.Contains(listener))
            {
                m_contentPublishListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Save Listener

        public interface IContentSaveListener
        {
            void ContentSaveOnSuccess(GameMod gameMod);
            void ContentSaveOnError(GameMod gameMod, int code, string reason);
        }
        private static List<IContentSaveListener> m_contentSaveListeners = new List<IContentSaveListener>();
        public static List<IContentSaveListener> getContentSaveListeners()
        {
            return m_contentSaveListeners;
        }
        public static void registerContentSaveListener(IContentSaveListener listener)
        {
            if (!m_contentSaveListeners.Contains(listener))
            {
                m_contentSaveListeners.Add(listener);
            }
        }
        public static void unregisterContentSaveListener(IContentSaveListener listener)
        {
            if (m_contentSaveListeners.Contains(listener))
            {
                m_contentSaveListeners.Remove(listener);
            }
        }

        #endregion

        #region Content Unpublish Listener

        public interface IContentUnpublishListener
        {
            void ContentUnpublishOnSuccess(GameMod gameMod);
            void ContentUnpublishOnError(GameMod gameMod, int code, string reason);
        }
        private static List<IContentUnpublishListener> m_contentUnpublishListeners = new List<IContentUnpublishListener>();
        public static List<IContentUnpublishListener> getContentUnpublishListeners()
        {
            return m_contentUnpublishListeners;
        }
        public static void registerContentUnpublishListener(IContentUnpublishListener listener)
        {
            if (!m_contentUnpublishListeners.Contains(listener))
            {
                m_contentUnpublishListeners.Add(listener);
            }
        }
        public static void unregisterContentUnpublishListener(IContentUnpublishListener listener)
        {
            if (m_contentUnpublishListeners.Contains(listener))
            {
                m_contentUnpublishListeners.Remove(listener);
            }
        }

        #endregion

        #region Request Gamer Info Listener

        public interface IRequestGamerInfoListener
        {
            void RequestGamerInfoOnSuccess(GamerInfo gamerInfo);
            void RequestGamerInfoOnFailure(int errorCode, string errorMessage);
            void RequestGamerInfoOnCancel();
        }
        private static List<IRequestGamerInfoListener> m_requestGamerInfoListeners = new List<IRequestGamerInfoListener>();
        public static List<IRequestGamerInfoListener> getRequestGamerInfoListeners()
        {
            return m_requestGamerInfoListeners;
        }
        public static void registerRequestGamerInfoListener(IRequestGamerInfoListener listener)
        {
            if (!m_requestGamerInfoListeners.Contains(listener))
            {
                m_requestGamerInfoListeners.Add(listener);
            }
        }
        public static void unregisterRequestGamerInfoListener(IRequestGamerInfoListener listener)
        {
            if (m_requestGamerInfoListeners.Contains(listener))
            {
                m_requestGamerInfoListeners.Remove(listener);
            }
        }

        #endregion

        #region Request Products Listeners

        public interface IRequestProductsListener
        {
            void RequestProductsOnSuccess(List<RazerSDK.Product> products);
            void RequestProductsOnFailure(int errorCode, string errorMessage);
            void RequestProductsOnCancel();
        }
        private static List<IRequestProductsListener> m_requestProductsListeners = new List<IRequestProductsListener>();
        public static List<IRequestProductsListener> getRequestProductsListeners()
        {
            return m_requestProductsListeners;
        }
        public static void registerRequestProductsListener(IRequestProductsListener listener)
        {
            if (!m_requestProductsListeners.Contains(listener))
            {
                m_requestProductsListeners.Add(listener);
            }
        }
        public static void unregisterRequestProductsListener(IRequestProductsListener listener)
        {
            if (m_requestProductsListeners.Contains(listener))
            {
                m_requestProductsListeners.Remove(listener);
            }
        }

        #endregion

        #region Request Purchase Listener

        public interface IRequestPurchaseListener
        {
            void RequestPurchaseOnSuccess(RazerSDK.Product product);
            void RequestPurchaseOnFailure(int errorCode, string errorMessage);
            void RequestPurchaseOnCancel();
        }
        private static List<IRequestPurchaseListener> m_requestPurchaseListeners = new List<IRequestPurchaseListener>();
        public static List<IRequestPurchaseListener> getRequestPurchaseListeners()
        {
            return m_requestPurchaseListeners;
        }
        public static void registerRequestPurchaseListener(IRequestPurchaseListener listener)
        {
            if (!m_requestPurchaseListeners.Contains(listener))
            {
                m_requestPurchaseListeners.Add(listener);
            }
        }
        public static void unregisterRequestPurchaseListener(IRequestPurchaseListener listener)
        {
            if (m_requestPurchaseListeners.Contains(listener))
            {
                m_requestPurchaseListeners.Remove(listener);
            }
        }

        #endregion

        #region Request Receipts Listeners

        public interface IRequestReceiptsListener
        {
            void RequestReceiptsOnSuccess(List<Receipt> receipts);
            void RequestReceiptsOnFailure(int errorCode, string errorMessage);
            void RequestReceiptsOnCancel();
        }
        private static List<IRequestReceiptsListener> m_requestReceiptsListeners = new List<IRequestReceiptsListener>();
        public static List<IRequestReceiptsListener> getRequestReceiptsListeners()
        {
            return m_requestReceiptsListeners;
        }
        public static void registerRequestReceiptsListener(IRequestReceiptsListener listener)
        {
            if (!m_requestReceiptsListeners.Contains(listener))
            {
                m_requestReceiptsListeners.Add(listener);
            }
        }
        public static void unregisterRequestReceiptsListener(IRequestReceiptsListener listener)
        {
            if (m_requestReceiptsListeners.Contains(listener))
            {
                m_requestReceiptsListeners.Remove(listener);
            }
        }

        #endregion

        #region Shutdown Listeners

        public interface IShutdownListener
        {
            void OnSuccessShutdown();
            void OnFailureShutdown();
        }
        private static List<IShutdownListener> m_shutdownListeners = new List<IShutdownListener>();
        public static List<IShutdownListener> getShutdownListeners()
        {
            return m_shutdownListeners;
        }
        public static void registerShutdownListener(IShutdownListener listener)
        {
            if (!m_shutdownListeners.Contains(listener))
            {
                m_shutdownListeners.Add(listener);
            }
        }
        public static void unregisterShutdownListener(IShutdownListener listener)
        {
            if (m_shutdownListeners.Contains(listener))
            {
                m_shutdownListeners.Remove(listener);
            }
        }

        #endregion

#endif
    }
}
