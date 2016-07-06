//#define VERBOSE_LOGGING
#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace com.razerzone.store.sdk.engine.unity
{
    public class DebugInput
    {
        public static string DebugGetAxisName(int axis)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            names[Controller.AXIS_LS_X] = "AXIS_LS_X";
            names[Controller.AXIS_LS_Y] = "AXIS_LS_Y";
            names[Controller.AXIS_RS_X] = "AXIS_RS_X";
            names[Controller.AXIS_RS_Y] = "AXIS_RS_Y";
            names[Controller.AXIS_L2] = "AXIS_L2";
            names[Controller.AXIS_R2] = "AXIS_R2";

            if (names.ContainsKey(axis))
            {
                return names[axis];
            }
            else
            {
                return string.Empty;
            }
        }

        public static String DebugGetButtonName(int button)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            names[Controller.BUTTON_O] = "BUTTON_O";
            names[Controller.BUTTON_U] = "BUTTON_U";
            names[Controller.BUTTON_Y] = "BUTTON_Y";
            names[Controller.BUTTON_A] = "BUTTON_A";
            names[Controller.BUTTON_L1] = "BUTTON_L1";
            names[Controller.BUTTON_R1] = "BUTTON_R1";
            names[Controller.BUTTON_L3] = "BUTTON_L3";
            names[Controller.BUTTON_R3] = "BUTTON_R3";
            names[Controller.BUTTON_DPAD_UP] = "BUTTON_DPAD_UP";
            names[Controller.BUTTON_DPAD_DOWN] = "BUTTON_DPAD_DOWN";
            names[Controller.BUTTON_DPAD_RIGHT] = "BUTTON_DPAD_RIGHT";
            names[Controller.BUTTON_DPAD_LEFT] = "BUTTON_DPAD_LEFT";
            names[Controller.BUTTON_MENU] = "BUTTON_MENU";

            if (names.ContainsKey(button))
            {
                return names[button];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

#endif