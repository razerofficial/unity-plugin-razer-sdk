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
import android.content.*;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.media.AudioManager;
import android.os.Bundle;
import android.util.Log;
import android.view.InputDevice;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.SurfaceView;
import android.view.View;
import android.view.WindowManager;
import android.widget.FrameLayout;

import com.razerzone.store.sdk.Controller;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MainActivity extends Activity {
	private static final String TAG = MainActivity.class.getSimpleName();

	private static final String PLUGIN_VERSION = "2.1.0.5";

	private static final boolean sEnableLogging = false;
	private static final boolean sEnableInputLogging = true;

    protected UnityPlayer mUnityPlayer;	// don't change the name of this variable; referenced from native code

	private InputView mInputView = null;

	@Override
	protected void onCreate(Bundle savedInstanceState) {

		Log.i(TAG, "Plugin: VERSION=" + PLUGIN_VERSION);

		//make activity accessible to Unity
		Plugin.setActivity(this);

		//make bundle accessible to Unity
		if (null != savedInstanceState) {
			Plugin.setSavedInstanceState(savedInstanceState);
		}

        if (sEnableLogging) {
            Log.d(TAG, "onCreate");
        }
		super.onCreate(savedInstanceState);

        getWindow().takeSurface(null);
        setTheme(android.R.style.Theme_NoTitleBar_Fullscreen);
        getWindow().setFormat(PixelFormat.RGB_565);

        mUnityPlayer = new UnityPlayer(this);
        if (mUnityPlayer.getSettings ().getBoolean ("hide_status_bar", true)) {
            getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                    WindowManager.LayoutParams.FLAG_FULLSCREEN);
        }

        setContentView(mUnityPlayer);

		mInputView = new InputView(this);

		if (sEnableLogging) {
			Log.d(TAG, "onCreate: Disable Screensaver");
		}
		mInputView.setKeepScreenOn(true);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);

		if (sEnableLogging) {
			Log.i(TAG, "onCreate complete.");
		}
	}

	// Pause Unity
	@Override
	protected void onPause() {
		super.onPause();
		if (sEnableLogging) {
			Log.d(TAG, "RazerGameObject->onPause");
		}
		Plugin.UnitySendMessage("RazerGameObject", "onPause", "");
        mUnityPlayer.pause();
		if (null != mInputView) {
			mInputView.requestFocus();
		}
	}

	// Resume Unity
	@Override
	protected void onResume() {
        super.onResume();
        mUnityPlayer.resume();
        if (sEnableLogging) {
            Log.d(TAG, "RazerGameObject->onResume");
        }
        Plugin.UnitySendMessage("RazerGameObject", "onResume", "");
        if (null != mInputView) {
            mInputView.requestFocus();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        mUnityPlayer.windowFocusChanged(hasFocus);
        if (null != mInputView) {
            mInputView.requestFocus();
        }
    }

	@Override
	public boolean dispatchGenericMotionEvent(MotionEvent motionEvent) {
		if (sEnableInputLogging) {
			Log.i(TAG, "dispatchGenericMotionEvent");
		}
		if (null != mInputView) {
			mInputView.dispatchGenericMotionEvent(motionEvent);
		}
		return false;
	}

	private void raiseVolume() {
		AudioManager audioMgr = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
		int stream = AudioManager.STREAM_SYSTEM;
		int maxVolume = audioMgr.getStreamMaxVolume(stream);
		int volume = audioMgr.getStreamVolume(stream);
		volume = Math.min(volume + 1, maxVolume);
		audioMgr.setStreamVolume(stream, volume, 0);
	}

	private void lowerVolume() {
		AudioManager audioMgr = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
		int stream = AudioManager.STREAM_SYSTEM;
		int maxVolume = audioMgr.getStreamMaxVolume(stream);
		int volume = audioMgr.getStreamVolume(stream);
		volume = Math.max(volume - 1, 0);
		audioMgr.setStreamVolume(stream, volume, 0);
	}

	@Override
	public boolean dispatchKeyEvent(KeyEvent keyEvent) {
		if (sEnableInputLogging) {
			Log.i(TAG, "dispatchKeyEvent keyCode=" + keyEvent.getKeyCode());
		}
		InputDevice device = keyEvent.getDevice();
		if (null != device) {
			String name = device.getName();
			if (null != name &&
					name.equals("aml_keypad")) {
				switch (keyEvent.getKeyCode()) {
					case 24:
						if (sEnableInputLogging) {
							Log.i(TAG, "Volume Up detected.");
						}
						//raiseVolume();
						//return true; //the volume was handled
						return false; //show the xiaomi volume overlay
					case 25:
						if (sEnableInputLogging) {
							Log.i(TAG, "Volume Down detected.");
						}
						//lowerVolume();
						//return true; //the volume was handled
						return false; //show the xiaomi volume overlay
					case 66:
						if (sEnableInputLogging) {
							Log.i(TAG, "Remote button detected.");
						}
						if (null != mInputView) {
							if (keyEvent.getAction() == KeyEvent.ACTION_DOWN) {
								mInputView.onKeyDown(Controller.BUTTON_O, keyEvent);
							} else if (keyEvent.getAction() == KeyEvent.ACTION_UP) {
								mInputView.onKeyUp(Controller.BUTTON_O, keyEvent);
							}
						}
						return false;
					case 4:
						if (sEnableInputLogging) {
							Log.i(TAG, "Remote back button detected.");
						}
						if (null != mInputView) {
							if (keyEvent.getAction() == KeyEvent.ACTION_DOWN) {
								mInputView.onKeyDown(Controller.BUTTON_A, keyEvent);
							} else if (keyEvent.getAction() == KeyEvent.ACTION_UP) {
								mInputView.onKeyUp(Controller.BUTTON_A, keyEvent);
							}
						}
						return true;
				}
			}
		}
		if (null != mInputView) {
			mInputView.dispatchKeyEvent(keyEvent);
		}
		return true;
	}

	@Override
	public boolean onGenericMotionEvent(MotionEvent motionEvent) {
		if (sEnableInputLogging) {
			Log.i(TAG, "onGenericMotionEvent");
		}
		if (null != mInputView) {
			mInputView.requestFocus();
		}
		return true;
	}

	@Override
	public boolean onKeyUp(int keyCode, KeyEvent keyEvent) {
		if (sEnableInputLogging) {
			Log.i(TAG, "onKeyUp");
		}
		if (null != mInputView) {
			mInputView.requestFocus();
		}
		return true;
	}

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent keyEvent) {
		if (sEnableInputLogging) {
			Log.i(TAG, "onKeyDown");
		}
		if (null != mInputView) {
			mInputView.requestFocus();
		}
		return true;
	}

	@Override
	public void onActivityResult(final int requestCode, final int resultCode, final Intent data) {
		if (sEnableLogging) {
			Log.i(TAG, "onActivityResult START");
		}
		if (null != mInputView) {
			mInputView.requestFocus();
		}
		StoreFacadeWrapper storeFacadeWrapper = Plugin.getStoreFacadeWrapper();
		if (null != storeFacadeWrapper) {
			if (sEnableLogging) {
				Log.i(TAG, "onActivityResult processActivityResult START");
			}
			// Forward this result to the facade, in case it is waiting for any activity results
			if (storeFacadeWrapper.processActivityResult(requestCode, resultCode, data)) {
				if (sEnableLogging) {
					Log.i(TAG, "onActivityResult processActivityResult END");
				}
				return;
			}
		} else {
			Log.e(TAG, "StoreFacadeWrapper is null");
		}
		super.onActivityResult(requestCode, resultCode, data);
		if (sEnableLogging) {
			Log.i(TAG, "* onActivityResult END");
		}
	}
}
