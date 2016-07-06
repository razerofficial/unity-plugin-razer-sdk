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
import android.os.Bundle;
import android.util.Log;

import com.razerzone.store.sdk.CancelIgnoringResponseListener;
import com.razerzone.store.sdk.GamerInfo;
import com.razerzone.store.sdk.PurchaseResult;
import com.razerzone.store.sdk.ResponseListener;
import com.razerzone.store.sdk.StoreFacade;
import com.razerzone.store.sdk.content.GameMod;
import com.razerzone.store.sdk.content.GameModException;
import com.razerzone.store.sdk.content.GameModManager;
import com.razerzone.store.sdk.purchases.Product;
import com.razerzone.store.sdk.purchases.Purchasable;
import com.razerzone.store.sdk.purchases.Receipt;
import com.unity3d.player.UnityPlayer;

import java.lang.reflect.Method;
import java.util.Collection;
import java.util.List;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class StoreFacadeWrapper {

	private static final String TAG = StoreFacadeWrapper.class.getSimpleName();

	private static final boolean sEnableLogging = false;

	private StoreFacade mStoreFacade = null;

	// InitCompleteListener has initialized
	private static boolean sInitialized = false;

	// listener for init complete
	private ResponseListener<Bundle> mInitCompleteListener = null;

	// listener for fetching gamer info
	private ResponseListener<GamerInfo> mRequestGamerInfoListener = null;

	// listener for getting products
	private ResponseListener<List<Product>> mRequestProductsListener = null;

	// listener for requesting purchase
	private ResponseListener<PurchaseResult> mRequestPurchaseListener = null;

	// listener for getting receipts
	private ResponseListener<Collection<Receipt>> mRequestReceiptsListener = null;

    // listener for shutdown
    private CancelIgnoringResponseListener mShutdownListener = null;

	// Content interface for community content
	private GameModManager mGameModManager = null;

	// listener for initializing community content
	private GameModManager.InitializedListener mGameModManagerInitListener = null;

	// listener for looking for installed content
	private GameModManager.SearchListener mInstalledSearchListener = null;

	// listener for looking for published content
	private GameModManager.SearchListener mPublishedSearchListener = null;

	// listener for saving content
	private GameModManager.SaveListener mSaveListener = null;

	// listener for publishing content
	private GameModManager.PublishListener mPublishListener = null;

	// listener for unpublishing content
	private GameModManager.UnpublishListener mUnpublishListener = null;

	// listener for downloading content
	private GameModManager.DownloadListener mDownloadListener = null;

	// listener for deleting content
	private GameModManager.DeleteListener mDeleteListener = null;

	public StoreFacadeWrapper(Context context, Bundle savedInstanceState, Bundle developerInfo) {
		try {
            if (sEnableLogging) {
                Log.i(TAG, "StoreFacadeWrapper");
            }
			if (null == mStoreFacade) {
				mStoreFacade = StoreFacade.getInstance();
			}

			Init(developerInfo);

		} catch (Exception ex) {
			Log.e(TAG, "StoreFacadeWrapper constructor exception", ex);
		}
	}

	private void Init(Bundle developerInfo) {
        if (sEnableLogging) {
            Log.i(TAG, "Init(Bundle developerInfo)");
        }

		Activity activity = Plugin.getActivity();
		if (null == activity) {
			Log.e(TAG, "Activity is null!");
			return;
		}

		Method registerInitCompletedListener = null;
		try {
			registerInitCompletedListener = StoreFacade.class.getMethod("registerInitCompletedListener");
		} catch (Exception e) {
			// skip
		}

		try {
			if (null == registerInitCompletedListener) {
				sInitialized = true;
			} else {
				mInitCompleteListener = new ResponseListener<Bundle>() {
					@Override
					public void onSuccess(Bundle bundle) {
						if (sEnableLogging) {
							Log.i(TAG, "InitCompleteListener onSuccess");
						}
						sInitialized = true;
						Log.i(TAG, "initPlugin: RazerGameObject send OnSuccessInitializePlugin");
						Plugin.UnitySendMessage("RazerGameObject", "OnSuccessInitializePlugin", "");
					}

					@Override
					public void onFailure(int i, String s, Bundle bundle) {
						if (sEnableLogging) {
							Log.i(TAG, "InitCompleteListener onFailure");
						}
                        Plugin.UnitySendMessage("RazerGameObject", "OnFailureInitializePlugin", "InitCompleteListener onFailure");
					}

					@Override
					public void onCancel() {
					}
				};
				registerInitCompletedListener.invoke(mStoreFacade, mInitCompleteListener);
			}
		} catch (Exception e) {
			// skip
		}

		mRequestGamerInfoListener = new ResponseListener<GamerInfo>() {
			@Override
			public void onSuccess(GamerInfo info) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestGamerInfoListener onSuccess");
				}

				JSONObject json = new JSONObject();
				try {
					json.put("uuid", info.getUuid());
					json.put("username", info.getUsername());
				} catch (JSONException e1) {
				}
				String jsonData = json.toString();

                Plugin.UnitySendMessage("RazerGameObject", "RequestGamerInfoSuccessListener", jsonData);
			}

			@Override
			public void onFailure(int errorCode, String errorMessage, Bundle optionalData) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestGamerInfoListener onFailure");
				}

				JSONObject json = new JSONObject();
				try {
					json.put("errorCode", errorCode);
					json.put("errorMessage", errorMessage);
				} catch (JSONException e1) {
				}
				String jsonData = json.toString();

                Plugin.UnitySendMessage("RazerGameObject", "RequestGamerInfoFailureListener", jsonData);
			}

			@Override
			public void onCancel() {
			}
		};

		mRequestProductsListener = new ResponseListener<List<Product>>() {
			@Override
			public void onSuccess(final List<Product> products) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestProductsListener onSuccess");
				}

				if (products != null) {
					JSONArray jarray = new JSONArray();
					int index = 0;
					for (Product product : products) {
						JSONObject json = new JSONObject();
						try {
							json.put("currencyCode", product.getCurrencyCode());
							json.put("description", product.getDescription());
							json.put("identifier", product.getIdentifier());
							json.put("localPrice", product.getLocalPrice());
							json.put("name", product.getName());
							json.put("originalPrice", product.getOriginalPrice());
							json.put("percentOff", product.getPercentOff());
							json.put("developerName", product.getDeveloperName());
							jarray.put(index, json);
							++index;
						} catch (JSONException e1) {
						}
						String jsonData = jarray.toString();

                        Plugin.UnitySendMessage("RazerGameObject", "RequestProductsSuccessListener", jsonData);
					}
				}
			}

			@Override
			public void onFailure(int errorCode, String errorMessage, Bundle optionalData) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestProductsListener onFailure");
				}

				JSONObject json = new JSONObject();
				try {
					json.put("errorCode", errorCode);
					json.put("errorMessage", errorMessage);
				} catch (JSONException e1) {
				}
				String jsonData = json.toString();

                Plugin.UnitySendMessage("RazerGameObject", "RequestProductsFailureListener", jsonData);
			}

			@Override
			public void onCancel() {
			}
		};

		mRequestPurchaseListener = new ResponseListener<PurchaseResult>() {

			@Override
			public void onSuccess(PurchaseResult result) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestPurchaseListener onSuccess");
				}
				if (null != result) {
					JSONObject json = new JSONObject();
					try {
						json.put("identifier", result.getProductIdentifier());
					} catch (JSONException e) {
					}
					String jsonData = json.toString();

                    Plugin.UnitySendMessage("RazerGameObject", "RequestPurchaseSuccessListener", jsonData);
				}
			}

			@Override
			public void onFailure(int errorCode, String errorMessage, Bundle optionalData) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestPurchaseListener onFailure");
				}

				JSONObject json = new JSONObject();
				try {
					json.put("errorCode", errorCode);
					json.put("errorMessage", errorMessage);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();

                Plugin.UnitySendMessage("RazerGameObject", "RequestPurchaseFailureListener", jsonData);
			}

			@Override
			public void onCancel() {
				if (sEnableLogging) {
					Log.i(TAG, "RequestPurchaseListener onCancel");
				}

                Plugin.UnitySendMessage("RazerGameObject", "RequestPurchaseCancelListener", "");
			}
		};

		mRequestReceiptsListener = new ResponseListener<Collection<Receipt>>() {

			@Override
			public void onSuccess(Collection<Receipt> receipts) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestReceiptsListener onSuccess");
				}

				if (receipts != null) {

					JSONArray jarray = new JSONArray();
					int index = 0;
					for (Receipt receipt : receipts) {
						JSONObject json = new JSONObject();
						try {
							json.put("identifier", receipt.getIdentifier());
							json.put("purchaseDate", receipt.getPurchaseDate());
							json.put("gamer", receipt.getGamer());
							json.put("uuid", receipt.getUuid());
							json.put("localPrice", receipt.getLocalPrice());
							json.put("currency", receipt.getCurrency());
							json.put("generatedDate", receipt.getGeneratedDate());
							jarray.put(index, json);
							++index;
						} catch (JSONException e1) {
						}
					}
					String jsonData = jarray.toString();

					//Log.i(TAG, "ReceiptListener ReceiptListListener jsonData=" + jsonData);
                    Plugin.UnitySendMessage("RazerGameObject", "RequestReceiptsSuccessListener", jsonData);
				}
			}

			@Override
			public void onFailure(int errorCode, String errorMessage, Bundle optionalData) {
				if (sEnableLogging) {
					Log.i(TAG, "RequestReceiptsListener onFailure");
				}

				JSONObject json = new JSONObject();
				try {
					json.put("errorCode", errorCode);
					json.put("errorMessage", errorMessage);
				} catch (JSONException e1) {
				}
				String jsonData = json.toString();

                Plugin.UnitySendMessage("RazerGameObject", "RequestReceiptsFailureListener", jsonData);
			}

			@Override
			public void onCancel() {
				if (sEnableLogging) {
					Log.i(TAG, "RequestReceiptsListener onCancel");
				}

				//Log.i(TAG, "PurchaseListener Invoke ReceiptListCancelListener");
                Plugin.UnitySendMessage("RazerGameObject", "RequestReceiptsCancelListener", "");
			}
		};

        mShutdownListener = new CancelIgnoringResponseListener() {
            @Override
            public void onSuccess(Object o) {
                if (sEnableLogging) {
                    Log.i(TAG, "ShutdownListener onSuccess");
                }
                Plugin.UnitySendMessage("RazerGameObject", "ShutdownOnSuccessListener", "");
            }

            @Override
            public void onFailure(int errorCode, String message, Bundle bundle) {
                Log.e(TAG, "ShutdownListener onFailure failed to shutdown! errorCode="+errorCode+" message="+message);
                Plugin.UnitySendMessage("RazerGameObject", "ShutdownOnFailureListener", "");
            }
        };

		try {
			mStoreFacade.init(activity, developerInfo);
		} catch (Exception e) {
			e.printStackTrace();
		}

		mGameModManager = GameModManager.getInstance();
		Plugin.setGameModManager(mGameModManager);

		mGameModManagerInitListener = new GameModManager.InitializedListener() {

			@Override
			public void onDestroyed() {
				Log.i(TAG, "ContentInitListener: onDestroyed");
                Plugin.UnitySendMessage("RazerGameObject", "ContentInitListenerOnDestroyed", "");
			}

			@Override
			public void onInitialized() {
				Log.i(TAG, "ContentInitListener: onInitialized");
                Plugin.UnitySendMessage("RazerGameObject", "ContentInitListenerOnInitialized", "");
			}

		};

		mGameModManager.registerInitializedListener(mGameModManagerInitListener);

		mInstalledSearchListener = new GameModManager.SearchListener() {
			@Override
			public void onError(int code, String reason) {
				Log.e(TAG, "InstalledSearchListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentInstalledSearchListenerOnError", jsonData);
			}

			@Override
			public void onResults(List<GameMod> gameMods, int count) {
				//Log.i(TAG, "InstalledSearchListener: onResults count="+count+" list count="+gameMods.size());
				for (GameMod gameMod : gameMods) {
				}
				JSONObject json = new JSONObject();
				try {
					json.put("count", count);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentInstalledSearchListenerOnResults", jsonData);
				Plugin.setGameModManagerInstalledResults(gameMods);
			}
		};

		mPublishedSearchListener = new GameModManager.SearchListener() {
			@Override
			public void onError(int code, String reason) {
				Log.e(TAG, "PublishedSearchListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentPublishedSearchListenerOnError", jsonData);
			}

			@Override
			public void onResults(List<GameMod> gameMods, int count) {
				//Log.i(TAG, "PublishedSearchListener: onResults count="+count+" list count="+gameMods.size());
				for (GameMod gameMod : gameMods) {
				}
				JSONObject json = new JSONObject();
				try {
					json.put("count", count);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentPublishedSearchListenerOnResults", jsonData);
				Plugin.setGameModManagerPublishedResults(gameMods);
			}
		};

		mSaveListener = new GameModManager.SaveListener() {

			@Override
			public void onError(GameMod gameMod, int code, String reason) {
				Log.e(TAG, "SaveListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentSaveListenerOnError", jsonData);
			}

			@Override
			public void onSuccess(GameMod gameMod) {
				Log.i(TAG, "SaveListener: onSuccess");
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentSaveListenerOnSuccess", jsonData);
			}
		};

		mPublishListener = new GameModManager.PublishListener() {

			@Override
			public void onError(GameMod gameMod, int code, String reason, Bundle bundle) {
				Log.e(TAG, "PublishListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentPublishListenerOnError", jsonData);
			}

			@Override
			public void onSuccess(GameMod gameMod) {
				Log.i(TAG, "PublishListener: onSuccess");
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentPublishListenerOnSuccess", jsonData);
			}

		};

		mUnpublishListener = new GameModManager.UnpublishListener() {

			@Override
			public void onError(GameMod gameMod, int code, String reason) {
				Log.e(TAG, "UnpublishListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentUnpublishListenerOnError", jsonData);
			}

			@Override
			public void onSuccess(GameMod gameMod) {
				Log.i(TAG, "UnpublishListener: onSuccess");
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentUnpublishListenerOnSuccess", jsonData);
			}

		};

		mDeleteListener = new GameModManager.DeleteListener() {

			@Override
			public void onDeleteFailed(GameMod gameMod, int code, String reason) {
				Log.e(TAG, "DeleteListener: onError code=" + code + " reason=" + reason);
				JSONObject json = new JSONObject();
				try {
					json.put("code", code);
					json.put("reason", reason);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentDeleteListenerOnDeleteFailed", jsonData);
			}

			@Override
			public void onDeleted(GameMod gameMod) {
				Log.i(TAG, "DeleteListener: onDeleted");
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentDeleteListenerOnDeleted", jsonData);
			}

		};

		mDownloadListener = new GameModManager.DownloadListener() {

			@Override
			public void onProgress(GameMod gameMod, int progress) {
				JSONObject json = new JSONObject();
				try {
					json.put("progress", progress);
				} catch (JSONException e) {
				}
				String jsonData = json.toString();
                Plugin.UnitySendMessage("RazerGameObject", "ContentDownloadListenerOnProgress", jsonData);
			}

			@Override
			public void onFailed(GameMod gameMod) {
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentDownloadListenerOnFailed", jsonData);
			}

			@Override
			public void onComplete(GameMod gameMod) {
				String jsonData = "";
                Plugin.UnitySendMessage("RazerGameObject", "ContentDownloadListenerOnComplete", jsonData);
			}

		};
	}

	public void shutdown() {
        if (sEnableLogging) {
            Log.i(TAG, "shutdown");
        }
		if (null == mStoreFacade) {
			Log.e(TAG, "shutdown: StoreFacade is null!");
		} else {
			mStoreFacade.shutdown(mShutdownListener);
		}
	}

	public boolean processActivityResult(final int requestCode, final int resultCode, final Intent data) {
		if (null == mStoreFacade) {
			Log.e(TAG, "processActivityResult: StoreFacade is null!");
			return false;
		}
		return (mStoreFacade.processActivityResult(requestCode, resultCode, data));
	}

	public boolean isInitialized() {
        /*
		if (null == mStoreFacade) {
			return false;
		} else {
			return mStoreFacade.isInitialized();
		}
		*/

		return sInitialized;
	}

	public void requestProducts(String[] products) {
		if (sEnableLogging) {
			Log.i(TAG, "requestProducts");
		}
        if (null == mStoreFacade) {
            Log.e(TAG, "requestProducts: StoreFacade is null!");
            return;
        }
		if (null != mRequestProductsListener) {
			mStoreFacade.requestProductList(Plugin.getActivity(), products, mRequestProductsListener);
		} else {
			Log.e(TAG, "mRequestProductsListener is null");
		}
	}

	public void requestGamerInfo() {
        if (sEnableLogging) {
            Log.i(TAG, "requestGamerInfo");
        }
        if (null == mStoreFacade) {
            Log.e(TAG, "requestGamerInfo: StoreFacade is null!");
            return;
        }
		if (null != mRequestGamerInfoListener) {
			mStoreFacade.requestGamerInfo(Plugin.getActivity(), mRequestGamerInfoListener);
		} else {
			Log.e(TAG, "StoreFacadeWrapper.requestGamerInfo mRequestGamerInfoListener is null");
		}
	}

	public void putGameData(String key, String val) {
		mStoreFacade.putGameData(key, val);
	}

	public String getGameData(String key) {
		return mStoreFacade.getGameData(key);
	}

	public void requestReceipts() {
		if (sEnableLogging) {
			Log.i(TAG, "requestReceipts");
		}
        if (null == mStoreFacade) {
            Log.e(TAG, "requestReceipts: StoreFacade is null!");
            return;
        }
		if (null != mRequestReceiptsListener) {
			mStoreFacade.requestReceipts(Plugin.getActivity(), mRequestReceiptsListener);
		} else {
			Log.e(TAG, "mRequestReceiptsListener is null");
		}
	}

	public Boolean isRunningOnSupportedHardware() {
		return mStoreFacade.isRunningOnSupportedHardware();
	}

	public void requestPurchase(final Product product) {
        if (sEnableLogging) {
            Log.i(TAG, "requestPurchase(" + product.getIdentifier() + ")");
        }
        if (null == mStoreFacade) {
            Log.e(TAG, "requestPurchase: StoreFacade is null!");
            return;
        }
		if (null != mRequestPurchaseListener) {
			Purchasable purchasable = product.createPurchasable();
			mStoreFacade.requestPurchase(Plugin.getActivity(), purchasable, mRequestPurchaseListener);
		} else {
			Log.e(TAG, "mRequestPurchaseListener is null");
		}
	}

	public void saveGameMod(GameMod gameMod, GameMod.Editor editor) {
        if (sEnableLogging) {
            Log.i(TAG, "saveGameMod");
        }
		try {
			editor.save(mSaveListener);
		} catch (GameModException e) {
			switch (e.getCode()) {
				case GameModException.CONTENT_NO_TITLE:
					mSaveListener.onError(gameMod, e.getCode(), "Title required!");
					break;
				case GameModException.CONTENT_NO_CATEGORY:
					mSaveListener.onError(gameMod, e.getCode(), "Category required!");
					break;
				case GameModException.CONTENT_NO_SCREENSHOTS:
					mSaveListener.onError(gameMod, e.getCode(), "At least one screenshot required!");
					break;
				case GameModException.CONTENT_NO_FILES:
					mSaveListener.onError(gameMod, e.getCode(), "At least one file required!");
					break;
				case GameModException.CONTENT_NOT_EDITABLE:
					mSaveListener.onError(gameMod, e.getCode(), "GameMod is not editable!");
					break;
				default:
					mSaveListener.onError(gameMod, e.getCode(), "Save Exception!");
					break;
			}
		}
	}

	public void getGameModManagerInstalled() {
        if (sEnableLogging) {
            Log.i(TAG, "getGameModManagerInstalled");
        }
		if (null == mGameModManager) {
			Log.e(TAG, "mGameModManager is null");
		} else if (null == mInstalledSearchListener) {
			Log.e(TAG, "mInstalledSearchListener is null");
		} else {
			mGameModManager.getInstalled(mInstalledSearchListener);
		}
	}

	public void getGameModManagerPublished(final GameModManager.SortMethod sortMethod) {
        if (sEnableLogging) {
            Log.i(TAG, "getGameModManagerPublished");
        }
		if (null == mGameModManager) {
			Log.e(TAG, "mGameModManager is null");
		} else if (null == mPublishedSearchListener) {
			Log.e(TAG, "mPublishedSearchListener is null");
		} else {
			mGameModManager.search(sortMethod, mPublishedSearchListener);
		}
	}

	public void contentDelete(final GameMod gameMod) {
        if (sEnableLogging) {
            Log.i(TAG, "contentDelete");
        }
		if (null == gameMod) {
			Log.e(TAG, "GameMod is null!");
		} else if (null == mDeleteListener) {
			Log.e(TAG, "mDeleteListener is null");
		} else {
			gameMod.delete(mDeleteListener);
		}

	}

	public void contentPublish(final GameMod gameMod) {
        if (sEnableLogging) {
            Log.i(TAG, "contentPublish");
        }
		if (null == gameMod) {
			Log.e(TAG, "GameMod is null!");
		} else if (null == mPublishListener) {
			Log.e(TAG, "mPublishListener is null");
		} else {
			gameMod.publish(mPublishListener);
		}
	}

	public void contentUnpublish(final GameMod gameMod) {
        if (sEnableLogging) {
            Log.i(TAG, "contentUnpublish");
        }
		if (null == gameMod) {
			Log.e(TAG, "GameMod is null!");
		} else if (null == mUnpublishListener) {
			Log.e(TAG, "mUnpublishListener is null");
		} else {
			gameMod.unpublish(mUnpublishListener);
		}
	}

	public void contentDownload(final GameMod gameMod) {
        if (sEnableLogging) {
            Log.i(TAG, "contentDownload");
        }
		if (null == gameMod) {
			Log.e(TAG, "GameMod is null!");
		} else if (null == mDownloadListener) {
			Log.e(TAG, "mDownloadListener is null");
		} else {
			gameMod.download(mDownloadListener);
		}
	}

	public String getDeviceHardwareName() {
        if (sEnableLogging) {
            Log.i(TAG, "getDeviceHardwareName");
        }
		if (null == mStoreFacade) {
			Log.e(TAG, "StoreFacade is null!");
			return "";
		}
		if (!mStoreFacade.isInitialized()) {
			Log.e(TAG, "StoreFacade is not initialized!");
			return "";
		}
		StoreFacade.DeviceHardware deviceHardware = mStoreFacade.getDeviceHardware();
		if (null == deviceHardware) {
			return "";
		}
		return deviceHardware.deviceName();
	}
}
