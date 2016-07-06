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

using System.Collections.Generic;
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class ExampleAudio : MonoBehaviour
#if UNITY_ANDROID && !UNITY_EDITOR
    ,
    RazerSDK.IPauseListener, RazerSDK.IResumeListener
#endif
    {
        public AudioClip m_soundMP3 = null;
        public AudioClip m_soundOGG = null;
        public AudioClip m_soundWAV = null;

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Handle focusing items
        /// </summary>
        private FocusManager m_focusManager = new FocusManager();

        /// <summary>
        /// Buttons
        /// </summary>
        private object m_btnPlayMP3 = new object();
        private object m_btnPlayOGG = new object();
        private object m_btnPlayWAV = new object();

        void Awake()
        {
            RazerSDK.registerPauseListener(this);
            RazerSDK.registerResumeListener(this);
        }
        void OnDestroy()
        {
            RazerSDK.unregisterPauseListener(this);
            RazerSDK.unregisterResumeListener(this);
        }

        public void OnPause()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        public void OnResume()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        private void OnGUI()
        {
            Color oldColor = GUI.backgroundColor;

            GUILayout.Label(string.Empty);
            GUILayout.Label(string.Empty);
            GUILayout.Label(string.Empty);
            GUILayout.Label(string.Empty);

            GUILayout.BeginHorizontal();
            GUILayout.Space(400);
            if (m_focusManager.SelectedButton == m_btnPlayMP3)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Play MP3", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnPlayMP3 &&
                RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
            {
#if UNITY_5
            GetComponent<AudioSource>().PlayOneShot(m_soundMP3, 100);
#else
                audio.PlayOneShot(m_soundMP3, 100);
#endif
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = oldColor;

            GUILayout.Label(string.Empty);
            GUILayout.Label(string.Empty);

            GUILayout.BeginHorizontal();
            GUILayout.Space(400);
            if (m_focusManager.SelectedButton == m_btnPlayOGG)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Play OGG", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnPlayOGG &&
                RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
            {
#if UNITY_5
            GetComponent<AudioSource>().PlayOneShot(m_soundOGG, 100);
#else
                audio.PlayOneShot(m_soundOGG, 100);
#endif
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = oldColor;

            GUILayout.Label(string.Empty);
            GUILayout.Label(string.Empty);

            GUILayout.BeginHorizontal();
            GUILayout.Space(400);
            if (m_focusManager.SelectedButton == m_btnPlayWAV)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Play WAV", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnPlayWAV &&
                RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O)))
            {
#if UNITY_5
            GetComponent<AudioSource>().PlayOneShot(m_soundWAV, 100);
#else
                audio.PlayOneShot(m_soundWAV, 100);
#endif
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = oldColor;
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

        public void Start()
        {
            m_focusManager.Mappings[m_btnPlayMP3] = new FocusManager.ButtonMapping()
            {
                Down = m_btnPlayOGG
            };
            m_focusManager.Mappings[m_btnPlayOGG] = new FocusManager.ButtonMapping()
            {
                Up = m_btnPlayMP3,
                Down = m_btnPlayWAV
            };
            m_focusManager.Mappings[m_btnPlayWAV] = new FocusManager.ButtonMapping()
            {
                Up = m_btnPlayOGG
            };

            // set default selection
            m_focusManager.SelectedButton = m_btnPlayMP3;
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
#endif
    }
}
