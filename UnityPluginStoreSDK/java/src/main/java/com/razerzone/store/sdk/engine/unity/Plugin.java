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
import android.content.res.Resources;
import android.os.Bundle;
import android.graphics.Bitmap;
import android.graphics.Point;
import android.util.Log;
import android.view.Display;
import android.view.ViewGroup.LayoutParams;
import android.view.View;
import android.view.WindowManager;
import android.widget.FrameLayout;
import com.razerzone.store.sdk.content.GameMod;
import com.razerzone.store.sdk.content.GameModManager;
import com.razerzone.store.sdk.content.GameModScreenshot;
import com.razerzone.store.sdk.purchases.Product;
import com.razerzone.store.sdk.StoreFacade;
import com.unity3d.player.UnityPlayer;

import java.lang.reflect.Method;
import java.security.InvalidParameterException;
import java.util.*;
import org.json.JSONArray;

public class Plugin {
    private static final String TAG = Plugin.class.getSimpleName();

    private static final boolean sEnableLogging = false;

    private static Activity sActivity = null;
    private static Bundle sSavedInstanceState = null;
    private static StoreFacadeWrapper sStoreFacadeWrapper = null;
    private static UnityPlayer sUnityPlayer = null;
    private static GameModManager sGameModManager = null;
    private static List<GameMod> sGameModManagerInstalledResults = null;
    private static List<GameMod> sGameModManagerPublishedResults = null;

    public static Activity getActivity() {
        return sActivity;
    }

    public static void setActivity(Activity activity) {
        sActivity = activity;
    }

    public static Bundle getSavedInstanceState() {
        return sSavedInstanceState;
    }

    public static void setSavedInstanceState(Bundle savedInstanceState) {
        sSavedInstanceState = savedInstanceState;
    }

    public static StoreFacadeWrapper getStoreFacadeWrapper() {
        return sStoreFacadeWrapper;
    }

    public static void setStoreFacadeWrapper(StoreFacadeWrapper storeFacadeWrapper) {
        sStoreFacadeWrapper = storeFacadeWrapper;
    }

    public static UnityPlayer getUnityPlayer() {
        return sUnityPlayer;
    }

    public static void setUnityPlayer(UnityPlayer unityPlayer) {
        sUnityPlayer = unityPlayer;
    }

    public static GameModManager getGameModManager() {
        return sGameModManager;
    }

    public static void setGameModManager(GameModManager gameModManager) {
        sGameModManager = gameModManager;
    }

    public static List<GameMod> getGameModManagerInstalledResults() {
        return sGameModManagerInstalledResults;
    }

    public static void setGameModManagerInstalledResults(List<GameMod> results) {
        sGameModManagerInstalledResults = results;
    }

    public static List<GameMod> getGameModManagerPublishedResults() {
        return sGameModManagerPublishedResults;
    }

    public static void setGameModManagerPublishedResults(List<GameMod> results) {
        sGameModManagerPublishedResults = results;
    }

    public static void UnitySendMessage(String gameObject, String method, String message) {
        try {
            UnityPlayer.UnitySendMessage(gameObject, method, message);
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "UnitySendMessage failed gameObject=" + gameObject + " method=" + method + " message=" + message);
            e.printStackTrace();
        }
    }

    // This initializes the Unity plugin - our Plugin,
    // and it gets a generic reference to the activity
    public Plugin(Activity currentActivity) {
    }

    private static void abort() {
        Log.e(TAG, "Plugin failed to load and stopped application!");
        android.os.Process.killProcess(android.os.Process.myPid());
    }

    public static void initPlugin(final String secretApiKey) {
        if (null != getStoreFacadeWrapper()) {
            return;
        }

        Activity activity = getActivity();
        if (null != activity) {
            Runnable runnable = new Runnable() {
                public void run() {
                    try {
                        if (null == getActivity()) {
                            Log.e(TAG, "initPlugin: activity is null!");
                            abort();
                            return;
                        }

                        Bundle developerInfo = null;
                        try {
                            developerInfo = StoreFacade.createInitBundle(secretApiKey);
                        } catch (InvalidParameterException e) {
                            Log.e(TAG, e.getMessage());
                            abort();
                            return;
                        }

                        if (sEnableLogging) {
                            Log.d(TAG, "developer_id=" + developerInfo.getString(StoreFacade.DEVELOPER_ID));
                        }

                        if (sEnableLogging) {
                            Log.d(TAG, "developer_public_key length=" + developerInfo.getByteArray(StoreFacade.DEVELOPER_PUBLIC_KEY).length);
                        }

                        StoreFacadeWrapper storeFacadeWrapper =
                                new StoreFacadeWrapper(getActivity(),
                                        getSavedInstanceState(),
                                        developerInfo);

                        setStoreFacadeWrapper(storeFacadeWrapper);

                        Method registerInitCompletedListener = null;
                        try {
                            registerInitCompletedListener = StoreFacade.class.getMethod("registerInitCompletedListener");
                        } catch (Exception e) {
                            // skip
                        }

                        if (null == registerInitCompletedListener) {
                            Log.i(TAG, "initPlugin: RazerGameObject send OnSuccessInitializePlugin");
                            UnitySendMessage("RazerGameObject", "OnSuccessInitializePlugin", "");
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                        UnitySendMessage("RazerGameObject", "OnFailureInitializePlugin", "InitializePlugin exception");
                        abort();
                        return;
                    }
                }
            };
            activity.runOnUiThread(runnable);
        }
    }

    public static boolean isInitialized() {
        boolean result = false;
        try {
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "isInitialized: storeFacadeWrapper is null!");
            } else {
                result = storeFacadeWrapper.isInitialized();
            }
        } catch (Exception e) {
            Log.e(TAG, "isInitialized: exception=" + e.toString());
        }
        return result;
    }

    public static void putGameData(String key, String val) {
        try {
            //Log.i(TAG, "putGameData: key=" + key + " val=" + val);
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "putGameData: storeFacadeWrapper is null!");
            } else {
                storeFacadeWrapper.putGameData(key, val);
            }
        } catch (Exception e) {
            Log.e(TAG, "putGameData: exception=" + e.toString());
        }
    }

    public static String getGameData(String key) {
        String result = null;
        try {
            //Log.i(TAG, "getGameData");
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "getGameData: storeFacadeWrapper is null!");
            } else {
                result = storeFacadeWrapper.getGameData(key);
            }
        } catch (Exception e) {
            Log.e(TAG, "getGameData: exception=" + e.toString());
        }
        return result;
    }

    public static void requestGamerInfo() {
        Activity activity = getActivity();
        if (null == activity) {
            Log.e(TAG, "requestGamerInfo: Activity is null!");
            return;
        }
        Runnable runnable = new Runnable() {
            @Override
            public void run() {
                try {
                    if (sEnableLogging) {
                        Log.i(TAG, "requestGamerInfo");
                    }
                    StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                    if (null == storeFacadeWrapper) {
                        Log.e(TAG, "requestGamerInfo: storeFacadeWrapper is null!");
                    } else {
                        storeFacadeWrapper.requestGamerInfo();
                    }
                } catch (Exception e) {
                    Log.e(TAG, "requestGamerInfo: exception=" + e.toString());
                }
            }
        };
        activity.runOnUiThread(runnable);
    }

    public static void requestProducts(String jsonData) {
        try {
            //Log.i(TAG, "requestProducts");
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "requestProducts: storeFacadeWrapper is null!");
                return;
            }

            JSONArray jsonArray = new JSONArray(jsonData);

            List<String> products = new ArrayList<String>();

            for (int i = 0; i < jsonArray.length(); ++i) {
                String productId = jsonArray.getString(i);
                if (null != productId) {
                    products.add(productId);
                }
            }

            String[] purchasables = new String[products.size()];
            purchasables = products.toArray(purchasables);
            storeFacadeWrapper.requestProducts(purchasables);
        } catch (Exception e) {
            Log.e(TAG, "requestProducts: exception=" + e.toString());
        }
    }

    public static void requestPurchase(String productId, String productType) {
        try {
            //Log.i(TAG, "requestPurchase: productId=" + productId+" productType="+productType);
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "requestPurchase: storeFacadeWrapper is null!");
            } else {
                Product product = new Product(productId, "", 0, 0, "", 0, 0, "", "", Product.Type.valueOf(productType));
                storeFacadeWrapper.requestPurchase(product);
            }
        } catch (Exception e) {
            Log.e(TAG, "requestPurchase: exception=" + e.toString());
        }
    }

    public static void requestReceipts() {
        try {
            //Log.i(TAG, "requestReceipts");
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "requestReceipts: storeFacadeWrapper is null");
            } else {
                storeFacadeWrapper.requestReceipts();
            }
        } catch (Exception e) {
            Log.e(TAG, "requestReceipts: exception=" + e.toString());
        }
    }

    public static boolean isRunningOnSupportedHardware() {
        boolean result = false;
        try {
            //Log.i(TAG, "isRunningOnSupportedHardware");
            StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
            if (null == storeFacadeWrapper) {
                Log.e(TAG, "isRunningOnSupportedHardware: storeFacadeWrapper is null!");
            } else {
                result = storeFacadeWrapper.isRunningOnSupportedHardware();
            }
        } catch (Exception e) {
            Log.e(TAG, "isRunningOnSupportedHardware exception: " + e.toString());
        }
        return result;
    }

    private static int getDisplayWidth() {
        Activity activity = getActivity();
        WindowManager windowManager = activity.getWindowManager();
        Display display = windowManager.getDefaultDisplay();
        Point size = new Point();
        display.getSize(size);
        return size.x;
    }

    private static int getDisplayHeight() {
        Activity activity = getActivity();
        WindowManager windowManager = activity.getWindowManager();
        Display display = windowManager.getDefaultDisplay();
        Point size = new Point();
        display.getSize(size);
        return size.y;
    }

    private static void updateSafeArea(float progress) {
        // bring in by %
        float percent = 0.1f;
        float ratio = 1 - (1 - progress) * percent;
        float halfRatio = 1 - (1 - progress) * percent * 0.5f;
        float maxWidth = getDisplayWidth();
        float maxHeight = getDisplayHeight();
        Activity activity = getActivity();
        FrameLayout content = (FrameLayout) activity.findViewById(android.R.id.content);
        LayoutParams layout = content.getLayoutParams();
        layout.width = (int) (maxWidth * ratio);
        layout.height = (int) (maxHeight * ratio);
        content.setLayoutParams(layout);
        content.setX(maxWidth - maxWidth * halfRatio);
        content.setY(maxHeight - maxHeight * halfRatio);
    }

    public static void setSafeArea(final float percentage) {
        try {
            //Log.i(TAG, "setSafeArea: "+percentage);
            Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        updateSafeArea(percentage);
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "setSafeArea: exception=" + e.toString());
        }
    }

    public static void clearFocus() {
        try {
            //Log.i(TAG, "clearFocus");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        View view = activity.getCurrentFocus();
                        if (null != view) {
                            view.setFocusable(false);
                            view.clearFocus();
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "clearFocus: exception=" + e.toString());
        }
    }

    public static void saveGameMod(final GameMod gameMod, final GameMod.Editor editor) {
        try {
            //Log.i(TAG, "saveGameMod");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "saveGameMod: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.saveGameMod(gameMod, editor);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "saveGameMod: exception=" + e.toString());
        }
    }

    public static void getGameModManagerInstalled() {
        try {
            //Log.i(TAG, "getGameModManagerInstalled");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "getGameModManagerInstalled: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.getGameModManagerInstalled();
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "getGameModManagerInstalled: exception=" + e.toString());
        }
    }

    public static GameMod[] getGameModManagerInstalledResultsArray() {
        List<GameMod> result = getGameModManagerInstalledResults();
        if (null == result) {
            Log.e(TAG, "getGameModManagerInstalledResults result is null!");
            return null;
        }
        GameMod[] retVal = new GameMod[result.size()];
        result.toArray(retVal);
        return retVal;
    }

    public static void getGameModManagerPublished(final String sortMethod) {
        try {
            //Log.i(TAG, "getGameModManagerPublished");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "getGameModManagerPublished: storeFacadeWrapper is null!");
                        } else {
                            GameModManager.SortMethod sort = GameModManager.SortMethod.valueOf(sortMethod);
                            //Log.i(TAG, "sortMethod="+sortMethod);
                            storeFacadeWrapper.getGameModManagerPublished(sort);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "getGameModManagerPublished: exception=" + e.toString());
        }
    }

    public static GameMod[] getGameModManagerPublishedResultsArray() {
        List<GameMod> result = getGameModManagerPublishedResults();
        if (null == result) {
            Log.e(TAG, "getGameModManagerPublishedResults result is null!");
            return null;
        }
        //Log.i(TAG, "getGameModPublishedResults returning size="+result.size());
        GameMod[] retVal = new GameMod[result.size()];
        result.toArray(retVal);
        return retVal;
    }

    public static void contentDelete(final GameMod gameMod) {
        try {
            //Log.i(TAG, "contentDelete");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "contentDelete: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.contentDelete(gameMod);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "contentDelete: exception=" + e.toString());
        }
    }

    public static void contentPublish(final GameMod gameMod) {
        try {
            //Log.i(TAG, "contentPublish");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "contentPublish: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.contentPublish(gameMod);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "contentPublish: exception=" + e.toString());
        }
    }

    public static void contentUnpublish(final GameMod gameMod) {
        try {
            //Log.i(TAG, "contentUnpublish");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "contentUnpublish: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.contentUnpublish(gameMod);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "contentUnpublish: exception=" + e.toString());
        }
    }

    public static void contentDownload(final GameMod gameMod) {
        try {
            //Log.i(TAG, "contentDownload");
            final Activity activity = getActivity();
            if (null != activity) {
                Runnable runnable = new Runnable() {
                    public void run() {
                        StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
                        if (null == storeFacadeWrapper) {
                            Log.e(TAG, "contentDownload: storeFacadeWrapper is null!");
                        } else {
                            storeFacadeWrapper.contentDownload(gameMod);
                        }
                    }
                };
                activity.runOnUiThread(runnable);
            }
        } catch (Exception e) {
            Log.e(TAG, "contentDownload: exception=" + e.toString());
        }
    }

    public static float getFloat(Float f) {
        if (null == f) {
            return 0;
        }
        return f.floatValue();
    }

    public static Bitmap[] getBitmapArray(List<Bitmap> list) {
        if (null == list) {
            return new Bitmap[0];
        }
        Bitmap[] retVal = new Bitmap[list.size()];
        list.toArray(retVal);
        return retVal;
    }

    public static GameModScreenshot[] getGameModScreenshotArray(List<GameModScreenshot> list) {
        if (null == list) {
            return new GameModScreenshot[0];
        }
        GameModScreenshot[] retVal = new GameModScreenshot[list.size()];
        list.toArray(retVal);
        return retVal;
    }

    public static String[] getStringArray(List<String> list) {
        if (null == list) {
            return new String[0];
        }
        String[] retVal = new String[list.size()];
        list.toArray(retVal);
        return retVal;
    }

    public static String getStringResource(String name) {
        final Activity activity = getActivity();
        if (null == activity) {
            Log.e(TAG, "getStringResource: activity is null!");
            return "";
        }
        Resources resources = activity.getResources();
        if (null == resources) {
            return "";
        }
        int id = resources.getIdentifier(name, "string", activity.getPackageName());
        if (id <= 0) {
            return "";
        }
        return resources.getString(id);
    }

    public static void shutdown() {
        final StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
        if (null == storeFacadeWrapper) {
            Log.e(TAG, "shutdown: storeFacadeWrapper is null!");
            return;
        }
        storeFacadeWrapper.shutdown();
    }

    public static String getDeviceHardwareName() {
        final StoreFacadeWrapper storeFacadeWrapper = getStoreFacadeWrapper();
        if (null == storeFacadeWrapper) {
            Log.e(TAG, "getDeviceHardwareName: storeFacadeWrapper is null!");
            return "";
        }
        return storeFacadeWrapper.getDeviceHardwareName();
    }

    public static void quit() {
        if (sEnableLogging) {
            Log.d(TAG, "quit: Application quiting...");
        }

        final Activity activity = getActivity();
        if (null == activity) {
            Log.e(TAG, "quit: activity is null!");
            return;
        }

        Runnable runnable = new Runnable() {
            @Override
            public void run() {
                UnityPlayer unityPlayer = getUnityPlayer();
                if (null == unityPlayer) {
                    Log.e(TAG, "quit: UnityPlayer is null!");
                }
                unityPlayer.quit();
                activity.finish();
            }
        };
        activity.runOnUiThread(runnable);
    }

    public static void useDefaultInput() {
        final Activity activity = getActivity();
        if (null == activity) {
            Log.e(TAG, "useDefaultInput: Activity is null!");
            return;
        }
        ((MainActivity) activity).useDefaultInput();
    }
}
