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

package com.razerzone.store.sdk.engine.unity;

import android.app.Activity;
import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.util.SparseArray;
import android.view.InputDevice;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.widget.FrameLayout;

import com.razerzone.store.sdk.Controller;
import com.razerzone.store.sdk.InputMapper;

import java.util.HashMap;

public class InputView extends View {

	private static final String TAG = InputView.class.getSimpleName();

	private static final boolean sEnableLogging = false;

	public static boolean sNativeInitialized = false;

	private static SparseArray<HashMap<Integer, Float>> sAxisValues = new SparseArray<HashMap<Integer, Float>>();
	private static SparseArray<HashMap<Integer, Boolean>> sButtonValues = new SparseArray<HashMap<Integer, Boolean>>();

	private static final float DEAD_ZONE = 0.25f;

	static {
		for (int index = 0; index < Controller.MAX_CONTROLLERS; ++index) {
			HashMap<Integer, Float> axisMap = new HashMap<Integer, Float>();
			axisMap.put(MotionEvent.AXIS_HAT_X, 0f);
			axisMap.put(MotionEvent.AXIS_HAT_Y, 0f);
			sAxisValues.put(index, axisMap);
			HashMap<Integer, Boolean> buttonMap = new HashMap<Integer, Boolean>();
			sButtonValues.put(index, buttonMap);
		}
		Log.i(TAG, "Loading lib-ndk-unity-store-sdk...");
		System.loadLibrary("-ndk-unity-store-sdk");
	}

	public InputView(Context context, AttributeSet attrs) {
		super(context, attrs);
		init();
	}

	public InputView(Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
		init();
	}

	public InputView(Context context) {
		super(context);
		init();
	}

	private void init() {
		if (sEnableLogging) {
			Log.i(TAG, "Construct InputView");
		}
		Activity activity = ((Activity) getContext());
		if (null != activity) {

			FrameLayout content = (FrameLayout) activity.findViewById(android.R.id.content);
			if (null != content) {
				content.addView(this);
			} else {
				Log.e(TAG, "Content view is missing");
			}

			InputMapper.init(activity);
			activity.takeKeyEvents(true);
			setFocusable(true);
			requestFocus();
		} else {
			Log.e(TAG, "Activity is null");
		}
	}

	public void shutdown() {
		try {
			Activity activity = ((Activity) getContext());
			if (null != activity) {
				InputMapper.shutdown(activity);
			} else {
				Log.e(TAG, "Activity was not found.");
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public boolean dispatchGenericMotionEvent(MotionEvent motionEvent) {
		//Log.i(TAG, "dispatchGenericMotionEvent");
		//DebugInput.debugMotionEvent(motionEvent);
		Activity activity = ((Activity) getContext());
		if (null != activity) {
			if (InputMapper.shouldHandleInputEvent(motionEvent)) {
				return InputMapper.dispatchGenericMotionEvent(activity, motionEvent);
			}
		} else {
			Log.e(TAG, "Activity was not found.");
		}
		return super.dispatchGenericMotionEvent(motionEvent);
	}

	@Override
	public boolean dispatchKeyEvent(KeyEvent keyEvent) {
		//Log.i(TAG, "dispatchKeyEvent");
		Activity activity = ((Activity) getContext());
		if (null != activity) {
			if (InputMapper.shouldHandleInputEvent(keyEvent)) {
				return InputMapper.dispatchKeyEvent(activity, keyEvent);
			}
		} else {
			Log.e(TAG, "Activity was not found.");
		}
		return super.dispatchKeyEvent(keyEvent);
	}

	public native void dispatchGenericMotionEventNative(int deviceId, int axis, float value);

	public native void dispatchKeyEventNative(int deviceId, int keyCode, int action);

	@Override
	public boolean onGenericMotionEvent(MotionEvent motionEvent) {
		if (sEnableLogging) {
			DebugInput.debugEngineMotionEvent(motionEvent);
			//DebugInput.debugMotionEvent(motionEvent);
		}

		if (!sNativeInitialized) {
			Log.e(TAG, "Native Plugin has not yet initialized...");
			return false;
		}

		int playerNum = Controller.getPlayerNumByDeviceId(motionEvent.getDeviceId());
		if (playerNum < 0 || playerNum >= Controller.MAX_CONTROLLERS) {
			playerNum = 0;
		}

		float dpadX = motionEvent.getAxisValue(MotionEvent.AXIS_HAT_X);
		float dpadY = motionEvent.getAxisValue(MotionEvent.AXIS_HAT_Y);
		sAxisValues.get(playerNum).put(MotionEvent.AXIS_HAT_X, dpadX);
		sAxisValues.get(playerNum).put(MotionEvent.AXIS_HAT_Y, dpadY);

		if (null == sButtonValues.get(playerNum).get(Controller.BUTTON_DPAD_LEFT) &&
				null == sButtonValues.get(playerNum).get(Controller.BUTTON_DPAD_RIGHT)) {
			if (dpadX < -DEAD_ZONE) {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_LEFT, KeyEvent.ACTION_DOWN);
			} else {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_LEFT, KeyEvent.ACTION_UP);
			}

			if (dpadX > DEAD_ZONE) {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_RIGHT, KeyEvent.ACTION_DOWN);
			} else {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_RIGHT, KeyEvent.ACTION_UP);
			}
		}

		if (null == sButtonValues.get(playerNum).get(Controller.BUTTON_DPAD_DOWN) &&
				null == sButtonValues.get(playerNum).get(Controller.BUTTON_DPAD_UP)) {
			if (dpadY > DEAD_ZONE) {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_DOWN, KeyEvent.ACTION_DOWN);
			} else {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_DOWN, KeyEvent.ACTION_UP);
			}
			if (dpadY < -DEAD_ZONE) {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_UP, KeyEvent.ACTION_DOWN);
			} else {
				dispatchKeyEventNative(playerNum, Controller.BUTTON_DPAD_UP, KeyEvent.ACTION_UP);
			}
		}

		dispatchGenericMotionEventNative(playerNum, MotionEvent.AXIS_HAT_X, dpadX);
		dispatchGenericMotionEventNative(playerNum, MotionEvent.AXIS_HAT_Y, dpadY);
		if (motionEvent.getSource() != 8194) {
			dispatchGenericMotionEventNative(playerNum, Controller.AXIS_LS_X, motionEvent.getAxisValue(Controller.AXIS_LS_X));
			dispatchGenericMotionEventNative(playerNum, Controller.AXIS_LS_Y, motionEvent.getAxisValue(Controller.AXIS_LS_Y));
		}
		dispatchGenericMotionEventNative(playerNum, Controller.AXIS_RS_X, motionEvent.getAxisValue(Controller.AXIS_RS_X));
		dispatchGenericMotionEventNative(playerNum, Controller.AXIS_RS_Y, motionEvent.getAxisValue(Controller.AXIS_RS_Y));
		dispatchGenericMotionEventNative(playerNum, Controller.AXIS_L2, motionEvent.getAxisValue(Controller.AXIS_L2));
		dispatchGenericMotionEventNative(playerNum, Controller.AXIS_R2, motionEvent.getAxisValue(Controller.AXIS_R2));
		return false;
	}

	@Override
	public boolean onKeyUp(int keyCode, KeyEvent keyEvent) {
		if (sEnableLogging) {
			Log.i(TAG, "onKeyUp keyCode=" + DebugInput.debugGetButtonName(keyCode));
		}

		if (!sNativeInitialized) {
			Log.e(TAG, "Native Plugin has not yet initialized...");
			return false;
		}

		if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
			return false;
		}

		int playerNum = Controller.getPlayerNumByDeviceId(keyEvent.getDeviceId());
		if (playerNum < 0 || playerNum >= Controller.MAX_CONTROLLERS) {
			playerNum = 0;
		}

		int action = keyEvent.getAction();
		switch (keyCode) {
			case Controller.BUTTON_DPAD_DOWN:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_Y) > DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, true);
				}
				break;
			case Controller.BUTTON_DPAD_LEFT:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_X) < -DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, true);
				}
				break;
			case Controller.BUTTON_DPAD_RIGHT:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_X) > DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, true);
				}
				break;
			case Controller.BUTTON_DPAD_UP:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_Y) < -DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, true);
				}
				break;
			case KeyEvent.KEYCODE_BUTTON_L2:
				sAxisValues.get(playerNum).put(Controller.AXIS_L2, 0f);
				break;
			case KeyEvent.KEYCODE_BUTTON_R2:
				sAxisValues.get(playerNum).put(Controller.AXIS_R2, 0f);
				break;
		}
		dispatchKeyEventNative(playerNum, keyCode, action);
		return true;
	}

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent keyEvent) {
		if (sEnableLogging) {
			Log.i(TAG, "onKeyDown keyCode=" + DebugInput.debugGetButtonName(keyCode));
		}

		if (!sNativeInitialized) {
			Log.e(TAG, "Native Plugin has not yet initialized...");
			return false;
		}

		if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
			return false;
		}

		int playerNum = Controller.getPlayerNumByDeviceId(keyEvent.getDeviceId());
		if (playerNum < 0 || playerNum >= Controller.MAX_CONTROLLERS) {
			playerNum = 0;
		}

		int action = keyEvent.getAction();
		switch (keyCode) {
			case Controller.BUTTON_DPAD_DOWN:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_Y) > DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, false);
				}
				break;
			case Controller.BUTTON_DPAD_LEFT:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_X) < -DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, false);
				}
				break;
			case Controller.BUTTON_DPAD_RIGHT:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_X) > DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, false);
				}
				break;
			case Controller.BUTTON_DPAD_UP:
				if (keyEvent.getSource() == InputDevice.SOURCE_JOYSTICK) {
					if (sAxisValues.get(playerNum).get(MotionEvent.AXIS_HAT_Y) < -DEAD_ZONE) {
						dispatchKeyEventNative(playerNum, keyCode, action);
					}
					return true;
				} else {
					sButtonValues.get(playerNum).put(keyCode, false);
				}
				break;
			case KeyEvent.KEYCODE_BUTTON_L2:
				sAxisValues.get(playerNum).put(Controller.AXIS_L2, 1f);
				break;
			case KeyEvent.KEYCODE_BUTTON_R2:
				sAxisValues.get(playerNum).put(Controller.AXIS_R2, 1f);
				break;
		}
		dispatchKeyEventNative(playerNum, keyCode, action);
		return true;
	}
}
