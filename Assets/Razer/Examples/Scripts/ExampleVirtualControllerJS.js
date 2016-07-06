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

#pragma strict

#if UNITY_ANDROID && !UNITY_EDITOR
import com.razerzone.store.sdk.engine.unity;
import com.razerzone.store.sdk;
#endif
import System;

public class ExampleVirtualControllerJS extends MonoBehaviour
#if UNITY_ANDROID && !UNITY_EDITOR
	implements
	RazerSDK.IPauseListener,
	RazerSDK.IResumeListener
#endif
{
    public var button_menu : SpriteRenderer = null;
    public var button_a : SpriteRenderer = null;
    public var button_dpad_down : SpriteRenderer = null;
    public var button_dpad_left : SpriteRenderer = null;
    public var button_dpad_right : SpriteRenderer = null;
    public var button_dpad_up : SpriteRenderer = null;
    public var button_lb : SpriteRenderer = null;
    public var axis_lt : SpriteRenderer = null;
    public var axis_l_stick : SpriteRenderer = null;
    public var button_o : SpriteRenderer = null;
    public var button_rb : SpriteRenderer = null;
    public var axis_rt : SpriteRenderer = null;
    public var axis_r_stick : SpriteRenderer = null;
    public var axis_thumbl : SpriteRenderer = null;
    public var axis_thumbr : SpriteRenderer = null;
    public var button_u : SpriteRenderer = null;
    public var button_y : SpriteRenderer = null;

    public var _PlayerNumber : int = 0; // controller #1

    private var _timerMenu : System.DateTime = System.DateTime.MinValue;

    private var AXIS_SCALER : float = 0.05f;

    private var DEADZONE : float = 0.25f;

#if UNITY_ANDROID && !UNITY_EDITOR

	function Awake()
	{
		RazerSDK.registerPauseListener (this);
		RazerSDK.registerResumeListener (this);
	}

	function OnDestroy()
	{
		RazerSDK.unregisterPauseListener(this);
		RazerSDK.unregisterResumeListener(this);
	}

#endif

	function OnPause()
	{
		Debug.Log ("Example paused!");
	}
	
	function OnResume()
	{
		Debug.Log ("Example resumed!");
	}

    function OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Height(Screen.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
#if UNITY_ANDROID && !UNITY_EDITOR
		GUILayout.Label(String.Format ("Virtual Controller for Unity on {0}", RazerSDK.getDeviceHardwareName()));
#endif
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

#if UNITY_ANDROID && !UNITY_EDITOR

    function HelperUpdateSprite(sr : SpriteRenderer, button : int)
    {
        if (RazerSDK.ControllerInput.GetButtonDown(_PlayerNumber, button))
        {
            sr.gameObject.SetActive(true);
        }

        if (RazerSDK.ControllerInput.GetButtonUp(_PlayerNumber, button))
        {
            sr.gameObject.SetActive(false);
        }
    }

    function RotateInput(input : Vector2) : Vector2
    {
        //rotate input by N degrees to match image
        var degrees : float = 225;
        var radians : float = degrees/180f*Mathf.PI;
        var cos : float = Mathf.Cos(radians);
        var sin : float = Mathf.Sin(radians);

        var x : float = input.x*cos - input.y*sin;
        var y : float = input.x*sin + input.y*cos;

        input.x = x;
        input.y = y;
        
        return input;
    }

    function Update()
    {
        HelperUpdateSprite(button_o, Controller.BUTTON_O);
        HelperUpdateSprite(button_u, Controller.BUTTON_U);
        HelperUpdateSprite(button_y, Controller.BUTTON_Y);
        HelperUpdateSprite(button_a, Controller.BUTTON_A);
        HelperUpdateSprite(button_lb, Controller.BUTTON_L1);
        HelperUpdateSprite(button_dpad_down, Controller.BUTTON_DPAD_DOWN);
        HelperUpdateSprite(button_dpad_left, Controller.BUTTON_DPAD_LEFT);
        HelperUpdateSprite(button_dpad_right, Controller.BUTTON_DPAD_RIGHT);
        HelperUpdateSprite(button_dpad_up, Controller.BUTTON_DPAD_UP);

        HelperUpdateSprite(button_rb, Controller.BUTTON_R1);

        var inputLeft : Vector2;
        inputLeft.x = Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_LS_X), -1f, 1f);
        inputLeft.y = -Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_LS_Y), -1f, 1f);
        inputLeft = RotateInput(inputLeft);
        axis_l_stick.transform.localPosition = inputLeft * AXIS_SCALER;
        axis_thumbl.transform.localPosition = inputLeft * AXIS_SCALER;

        var inputRight : Vector2;
        inputRight.x = Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_RS_X), -1f, 1f);
        inputRight.y = -Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_RS_Y), -1f, 1f);
        inputRight = RotateInput(inputRight);
        axis_r_stick.transform.localPosition = inputRight * AXIS_SCALER;
        axis_thumbr.transform.localPosition = inputRight * AXIS_SCALER;

        HelperUpdateSprite(axis_thumbl, Controller.BUTTON_L3);
        HelperUpdateSprite(axis_thumbr, Controller.BUTTON_R3);

        var leftTrigger : float = Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_L2), 0f, 1f);
        if (Mathf.Abs(leftTrigger) > DEADZONE)
        {
            axis_lt.gameObject.SetActive(true);
        }
        else
        {
            axis_lt.gameObject.SetActive(false);
        }

        var leftRight : float = Mathf.Clamp(RazerSDK.ControllerInput.GetAxis(_PlayerNumber, Controller.AXIS_R2), 0f, 1f);
        if (Mathf.Abs(leftRight) > DEADZONE)
        {
            axis_rt.gameObject.SetActive(true);
        }
        else
        {
            axis_rt.gameObject.SetActive(false);
        }

        if (RazerSDK.ControllerInput.GetButtonDown(_PlayerNumber, Controller.BUTTON_MENU) ||
            RazerSDK.ControllerInput.GetButtonUp(_PlayerNumber, Controller.BUTTON_MENU))
        {
            _timerMenu = DateTime.Now + TimeSpan.FromSeconds(1);
        }
        if (_timerMenu < DateTime.Now)
        {
            button_menu.gameObject.SetActive(false);
        }
        else
        {
            button_menu.gameObject.SetActive(true);
        }
    }
#endif
}
