using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    class ExampleInputTest : MonoBehaviour
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Dictionary<int, Dictionary<int, float>> _axis = new Dictionary<int, Dictionary<int, float>>();
        Dictionary<int, Dictionary<int, DateTime>> _buttonDown = new Dictionary<int, Dictionary<int, DateTime>>();
        Dictionary<int, Dictionary<int, DateTime>> _buttonUp = new Dictionary<int, Dictionary<int, DateTime>>();

        private int[] _axises =
        {
        Controller.AXIS_LS_X,
        Controller.AXIS_LS_Y,
        Controller.AXIS_RS_X,
        Controller.AXIS_RS_Y,
        Controller.AXIS_L2,
        Controller.AXIS_R2,
    };

        private int[] _buttons =
        {
        Controller.BUTTON_O,
        Controller.BUTTON_U,
        Controller.BUTTON_Y,
        Controller.BUTTON_A,
        Controller.BUTTON_L1,
        Controller.BUTTON_L3,
        Controller.BUTTON_R1,
        Controller.BUTTON_R3,
        Controller.BUTTON_DPAD_DOWN,
        Controller.BUTTON_DPAD_LEFT,
        Controller.BUTTON_DPAD_RIGHT,
        Controller.BUTTON_DPAD_UP,
        Controller.BUTTON_MENU,
    };

        private void Start()
        {
            DontDestroyOnLoad(transform.gameObject);
        }

        float GetAxis(int controller, int axis, Dictionary<int, Dictionary<int, float>> dict)
        {
            if (null == dict)
            {
                return 0f;
            }
            if (dict.ContainsKey(controller))
            {
                if (dict[controller].ContainsKey(axis))
                {
                    return dict[controller][axis];
                }
            }
            return 0f;
        }

        bool CheckButton(int controller, int button, Dictionary<int, Dictionary<int, DateTime>> dict)
        {
            if (null == dict)
            {
                return false;
            }
            if (dict.ContainsKey(controller))
            {
                if (dict[controller].ContainsKey(button))
                {
                    return dict[controller][button] > DateTime.Now;
                }
            }
            return false;
        }

        void Awake()
        {
            for (int controller = 0; controller < Controller.MAX_CONTROLLERS; ++controller)
            {
                _axis[controller] = new Dictionary<int, float>();
                _buttonDown[controller] = new Dictionary<int, DateTime>();
                _buttonUp[controller] = new Dictionary<int, DateTime>();
            }
        }

        void Update()
        {
            if (null == _axis ||
                null == _buttonDown ||
                null == _buttonUp)
            {
                return;
            }
            for (int controller = 0; controller < Controller.MAX_CONTROLLERS; ++controller)
            {
                if (_axis.ContainsKey(controller) &&
                    _axis[controller] != null)
                {
                    foreach (int axis in _axises)
                    {
                        _axis[controller][axis] = RazerSDK.ControllerInput.GetAxis(controller, axis);
                    }
                }

                if (_buttonDown.ContainsKey(controller) &&
                    _buttonDown[controller] != null)
                {
                    foreach (int button in _buttons)
                    {
                        if (RazerSDK.ControllerInput.GetButtonDown(controller, button))
                        {
                            _buttonDown[controller][button] = DateTime.Now + TimeSpan.FromSeconds(0.5f);
                        }
                        if (RazerSDK.ControllerInput.GetButtonUp(controller, button))
                        {
                            _buttonUp[controller][button] = DateTime.Now + TimeSpan.FromSeconds(0.5f);
                        }
                    }
                }
            }
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            {
                GUILayout.FlexibleSpace();
                for (int controller = 0; controller < Controller.MAX_CONTROLLERS; ++controller)
                {
                    GUILayout.BeginVertical(GUILayout.Height(Screen.height));
                    {
                        foreach (int axis in _axises)
                        {
                            GUILayout.Label(string.Format("({0}) {1} val={2}",
                                axis,
                                DebugInput.DebugGetAxisName(axis),
                                GetAxis(controller, axis, _axis)));
                        }
                        GUILayout.FlexibleSpace();
                        foreach (int button in _buttons)
                        {
                            GUILayout.Label(string.Format("({0}) {1} up={2} down={3}",
                                button,
                                DebugInput.DebugGetButtonName(button),
                                CheckButton(controller, button, _buttonUp),
                                CheckButton(controller, button, _buttonDown)));
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
#endif
    }
}
