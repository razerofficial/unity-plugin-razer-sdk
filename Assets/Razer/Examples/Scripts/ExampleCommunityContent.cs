#if UNITY_ANDROID && !UNITY_EDITOR
using Android.Graphics;
using com.razerzone.store.sdk.content;
using java.io;
using Java.IO.InputStream;
using Java.IO.OutputStream;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
#endif
using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class ExampleCommunityContent : MonoBehaviour
#if UNITY_ANDROID && !UNITY_EDITOR
    ,
    RazerSDK.IContentInitializedListener,
    RazerSDK.IContentDeleteListener,
    RazerSDK.IContentDownloadListener,
    RazerSDK.IContentInstalledSearchListener,
    RazerSDK.IContentPublishedSearchListener,
    RazerSDK.IContentPublishListener,
    RazerSDK.IContentSaveListener,
    RazerSDK.IContentUnpublishListener,
    RazerSDK.IResumeListener
#endif
    {
        public Texture2D[] m_textureScreenshots = null;

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Handle focusing items
        /// </summary>
        private FocusManager m_focusManager = new FocusManager();

        /// <summary>
        /// Buttons
        /// </summary>
        private object m_btnSearchByRating = new object();
        private object m_btnSearchByCreatedAt = new object();
        private object m_btnSearchByUpdatedAt = new object();
        private object m_btnCreate = new object();
        private bool m_buttonClicked = false;

        /// <summary>
        /// Visual status indicator
        /// </summary>
        private string m_status = "Status:";

        /// <summary>
        /// Indicates community content is available
        /// </summary>
        private bool m_isAvailable = false;

        private string m_lblInstalled = "Installed Results: count=0 list count=0";
        private string m_lblPublished = "Published Results: count=0 list count=0";

        /// <summary>
        /// Reference to screenshot to save with community content
        /// </summary>
        private Bitmap[] m_bitmapScreenshots = null;

        /// <summary>
        /// The list of installed mods
        /// </summary>
        private List<WidgetGameMod> m_widgets = new List<WidgetGameMod>();

        /// <summary>
        /// The list of pending results for the update event
        /// </summary>
        private List<GameMod> m_pendingInstalledSearchResults = null;

        /// <summary>
        /// The list of pending results for the update event
        /// </summary>
        private List<GameMod> m_pendingPublishedSearchResults = null;

        /// <summary>
        /// The selected sort method
        /// </summary>
        private GameModManager.SortMethod m_sortMethod = GameModManager.SortMethod.Rating;

        private class WidgetGameMod
        {
            public GameMod m_instance = null;
            public bool m_searchByInstalled = false;
            public bool m_searchByPublished = false;
            public object m_buttonEdit = new object();
            public object m_buttonPublish = new object();
            public object m_buttonDelete = new object();
            public object m_buttonDownload = new object();
            public object m_buttonFlag = new object();
            public object m_buttonRate = new object();
            public string m_category = string.Empty;
            public string m_description = string.Empty;
            public string m_filenames = string.Empty;
            public string[] m_files = null;
            public string m_metaData = string.Empty;
            public Texture2D[] m_screenshots = null;
            public string m_tags = string.Empty;
            public Texture2D[] m_thumbnails = null;
            public string m_title = string.Empty;
            public bool m_isPublished = false;
            public bool m_isDownloading = false;
            public bool m_isInstalled = false;
            public bool m_isFlagged = false;
            public int m_ratingCount = 0;
            public float m_ratingAverage = 0;
            public float? m_userRating = null;
        }

        private void Awake()
        {
            RazerSDK.registerContentInitializedListener(this);
            RazerSDK.registerContentDeleteListener(this);
            RazerSDK.registerContentDownloadListener(this);
            RazerSDK.registerContentInstalledSearchListener(this);
            RazerSDK.registerContentPublishedSearchListener(this);
            RazerSDK.registerContentPublishListener(this);
            RazerSDK.registerContentSaveListener(this);
            RazerSDK.registerContentUnpublishListener(this);
            RazerSDK.registerResumeListener(this);

            if (null != m_textureScreenshots)
            {
                m_bitmapScreenshots = new Bitmap[m_textureScreenshots.Length];
                for (int index = 0; index < m_textureScreenshots.Length; ++index)
                {
                    Texture2D texture = m_textureScreenshots[index];

                    Debug.Log("Encoding PNG from texture bytes");

                    // make texture readable
                    byte[] buffer = texture.EncodeToPNG();
                    if (null == buffer ||
                        buffer.Length == 0)
                    {
                        Debug.LogError("Failed to encode png");
                    }
                    else
                    {
                        Debug.Log("Converting bytes to bitmap");
                        m_bitmapScreenshots[index] = BitmapFactory.decodeByteArray(buffer, 0, buffer.Length);
                    }
                }
            }

            ResetFocus();
        }
        private void OnDestroy()
        {
            RazerSDK.unregisterContentInitializedListener(this);
            RazerSDK.unregisterContentDeleteListener(this);
            RazerSDK.unregisterContentDownloadListener(this);
            RazerSDK.unregisterContentInstalledSearchListener(this);
            RazerSDK.unregisterContentPublishedSearchListener(this);
            RazerSDK.unregisterContentPublishListener(this);
            RazerSDK.unregisterContentSaveListener(this);
            RazerSDK.unregisterContentUnpublishListener(this);
            RazerSDK.unregisterResumeListener(this);
        }

        /// <summary>
        /// IContentInitializedListener
        /// </summary>
        public void ContentInitializedOnInitialized()
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            using (GameModManager gameModManager = Plugin.getGameModManager())
            {
                if (null == gameModManager)
                {
                    Debug.LogError("gameModManager is null!");
                    return;
                }
                m_isAvailable = gameModManager.isAvailable();
            }

            RunSearch();
        }
        public void ContentInitializedOnDestroyed()
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentDeleteListener
        /// </summary>
        public void ContentDeleteOnDeleted(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            RunSearch();
        }
        public void ContentDeleteOnDeleteFailed(GameMod gameMod, int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentDownloadListener
        /// </summary>
        public void ContentDownloadOnComplete(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            RunSearch();
        }
        public void ContentDownloadOnFailed(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);
        }
        public void ContentDownloadOnProgress(GameMod gameMod, int progress)
        {
            m_status = string.Format("Status: {0} progress={1}", MethodBase.GetCurrentMethod().Name, progress);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentInstalledSearchListener
        /// </summary>
        public void ContentInstalledSearchOnResults(List<GameMod> gameMods, int count)
        {
            m_status = string.Format("Status: {0} count={1}", MethodBase.GetCurrentMethod().Name, count);
            Debug.Log(m_status);

            if (null == gameMods)
            {
                m_lblInstalled = string.Format("Installed Results: null list count={0}", count);
            }
            else
            {
                m_lblInstalled = string.Format("Installed Results: count={0} list count={1}", gameMods.Count, count);
                m_pendingInstalledSearchResults = gameMods;
            }
        }
        public void ContentInstalledSearchOnError(int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentPublishedSearchListener
        /// </summary>
        public void ContentPublishedSearchOnResults(List<GameMod> gameMods, int count)
        {
            m_status = string.Format("Status: {0} count={1}", MethodBase.GetCurrentMethod().Name, count);
            Debug.Log(m_status);

            if (null == gameMods)
            {
                m_lblPublished = string.Format("Published Results: null list count={0}", count);
            }
            else
            {
                m_lblPublished = string.Format("Published Results: count={0} list count={1}", gameMods.Count, count);
                m_pendingPublishedSearchResults = gameMods;
            }
        }
        public void ContentPublishedSearchOnError(int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentPublishListener
        /// </summary>
        public void ContentPublishOnSuccess(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            RunSearch();
        }
        public void ContentPublishOnError(GameMod gameMod, int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentSaveListener
        /// </summary>
        public void ContentSaveOnSuccess(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            RunSearch();
        }
        public void ContentSaveOnError(GameMod gameMod, int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IContentUnpublishListener
        /// </summary>
        public void ContentUnpublishOnSuccess(GameMod gameMod)
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            RunSearch();
        }
        public void ContentUnpublishOnError(GameMod gameMod, int code, string reason)
        {
            m_status = string.Format("Status: {0} code={1} reason={2}", MethodBase.GetCurrentMethod().Name, code, reason);
            Debug.Log(m_status);
        }

        /// <summary>
        /// IResumeListener
        /// </summary>
        public void OnResume()
        {
            m_status = string.Format("Status: {0}", MethodBase.GetCurrentMethod().Name);
            Debug.Log(m_status);

            if (m_isAvailable)
            {
                RunSearch();
            }
        }

        void OnGUI()
        {
            Color oldColor = GUI.backgroundColor;

            GUILayout.BeginVertical(GUILayout.Height(Screen.height));
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Unity Community Content Example");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_isAvailable ? "Community Content is available" : "Community Content is not available");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_status);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            if (m_focusManager.SelectedButton == m_btnSearchByRating)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Search by rating", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnSearchByRating))
            {
                if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                {
                    m_buttonClicked = true;
                }
                else if (m_buttonClicked)
                {
                    m_buttonClicked = false;
                    m_sortMethod = GameModManager.SortMethod.Rating;
                    RunSearch();
                }
            }
            GUI.backgroundColor = oldColor;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            if (m_focusManager.SelectedButton == m_btnSearchByCreatedAt)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Search by created at", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnSearchByCreatedAt))
            {
                if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                {
                    m_buttonClicked = true;
                }
                else if (m_buttonClicked)
                {
                    m_buttonClicked = false;
                    m_sortMethod = GameModManager.SortMethod.CreatedAt;
                    RunSearch();
                }
            }
            GUI.backgroundColor = oldColor;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            if (m_focusManager.SelectedButton == m_btnSearchByUpdatedAt)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Search by updated at", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnSearchByUpdatedAt))
            {
                if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                {
                    m_buttonClicked = true;
                }
                else if (m_buttonClicked)
                {
                    m_buttonClicked = false;
                    m_sortMethod = GameModManager.SortMethod.UpdatedAt;
                    RunSearch();
                }
            }
            GUI.backgroundColor = oldColor;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUI.enabled = m_widgets.Count < 7;
            if (m_focusManager.SelectedButton == m_btnCreate)
            {
                GUI.backgroundColor = Color.red;
            }
            if ((GUILayout.Button("Create CC Item", GUILayout.Height(40)) ||
                (m_focusManager.SelectedButton == m_btnCreate)) &&
                GUI.enabled)
            {
                if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                {
                    m_buttonClicked = true;
                }
                else if (m_buttonClicked)
                {
                    m_buttonClicked = false;
                    Create();
                }
            }
            GUI.backgroundColor = oldColor;
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_lblInstalled);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            StringBuilder sb = new StringBuilder();

            foreach (WidgetGameMod gameMod in m_widgets)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));

                GUILayout.Space(150);

                if (m_focusManager.SelectedButton == gameMod.m_buttonPublish)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button(gameMod.m_isPublished ? "Unpublish" : "Publish", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonPublish))
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        if (gameMod.m_isPublished)
                        {
                            Unpublish(gameMod.m_instance);
                        }
                        else
                        {
                            Publish(gameMod.m_instance);
                        }
                    }
                }
                GUI.backgroundColor = oldColor;

                GUI.enabled = gameMod.m_searchByInstalled;
                if (m_focusManager.SelectedButton == gameMod.m_buttonDelete)
                {
                    GUI.backgroundColor = Color.red;
                }
                if ((GUILayout.Button("Delete", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonDelete)) &&
                    GUI.enabled)
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        Delete(gameMod.m_instance);
                    }
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = true;

                GUI.enabled = gameMod.m_searchByPublished && !gameMod.m_isDownloading;
                if (m_focusManager.SelectedButton == gameMod.m_buttonDownload)
                {
                    GUI.backgroundColor = Color.red;
                }
                if ((GUILayout.Button("Download", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonDownload)) &&
                    GUI.enabled)
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        Download(gameMod.m_instance);
                    }
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = true;

                GUI.enabled = gameMod.m_isPublished;
                if (m_focusManager.SelectedButton == gameMod.m_buttonRate)
                {
                    GUI.backgroundColor = Color.red;
                }
                if ((GUILayout.Button("Rate", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonRate)) &&
                    GUI.enabled)
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        Rate(gameMod.m_instance);
                    }
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = true;

                GUI.enabled = gameMod.m_searchByInstalled;
                if (m_focusManager.SelectedButton == gameMod.m_buttonEdit)
                {
                    GUI.backgroundColor = Color.red;
                }
                if ((GUILayout.Button("Edit", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonEdit)) &&
                    GUI.enabled)
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        Edit(gameMod.m_instance);
                    }
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = true;

                GUI.enabled = gameMod.m_isPublished;
                if (m_focusManager.SelectedButton == gameMod.m_buttonFlag)
                {
                    GUI.backgroundColor = Color.red;
                }
                if ((GUILayout.Button("Flag", GUILayout.Height(40)) ||
                    (m_focusManager.SelectedButton == gameMod.m_buttonFlag)) &&
                    GUI.enabled)
                {
                    if (RazerSDK.ControllerInput.GetButtonUp(Controller.BUTTON_O))
                    {
                        m_buttonClicked = true;
                    }
                    else if (m_buttonClicked)
                    {
                        m_buttonClicked = false;
                        Flag(gameMod.m_instance);
                    }
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = true;

                if (sb.Length > 0)
                {
                    sb.Remove(0, sb.Length);
                }

                if (gameMod.m_searchByInstalled)
                {
                    sb.Append("[Installed]");
                }
                if (gameMod.m_searchByPublished)
                {
                    sb.Append("[Published]");
                }

                sb.Append(" [Category]=");
                sb.Append(gameMod.m_category);
                sb.Append(" [Description]=");
                sb.Append(gameMod.m_description);
                sb.Append(" [Filenames]=");
                sb.Append(gameMod.m_filenames);
                if (null != gameMod.m_files)
                {
                    foreach (String file in gameMod.m_files)
                    {
                        sb.Append("***");
                        sb.Append(file);
                    }
                }
                sb.Append(" [Title]=");
                sb.Append(gameMod.m_title);
                sb.Append(" [Tags]=");
                sb.Append(gameMod.m_tags);
                sb.Append(" [Metadata]=");
                sb.Append(null == gameMod.m_metaData ? string.Empty : gameMod.m_metaData);
                sb.Append(" [IsInstalled]=");
                sb.Append(gameMod.m_isInstalled.ToString());
                sb.Append(" [RatingCount]=");
                sb.Append(gameMod.m_ratingCount.ToString());
                sb.Append(" [RatingAverage]=");
                sb.Append(gameMod.m_ratingAverage.ToString());
                sb.Append(" [UserRating]=");
                sb.Append(null == gameMod.m_userRating ? "none" : gameMod.m_userRating.ToString());
                sb.Append(" [IsFlagged]=");
                sb.Append(gameMod.m_isFlagged.ToString());
                GUILayout.Label(sb.ToString());

                GUILayout.Label(" [Screenshots]=");
                if (null != gameMod.m_screenshots)
                {
                    foreach (Texture2D texture in gameMod.m_screenshots)
                    {
                        if (null != texture)
                        {
                            GUILayout.Label(texture, GUILayout.Width(128), GUILayout.Height(128));
                        }
                    }
                }

                GUILayout.Label(" [Thumbnail]=");
                if (null != gameMod.m_thumbnails)
                {
                    foreach (Texture2D texture in gameMod.m_thumbnails)
                    {
                        if (null != texture)
                        {
                            GUILayout.Label(texture, GUILayout.Width(128), GUILayout.Height(128));
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(150);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_lblPublished);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public void Create()
        {
            using (GameModManager gameModManager = Plugin.getGameModManager())
            {
                if (null == gameModManager)
                {
                    Debug.LogError("gameModManager is null");
                    return;
                }
                if (gameModManager.isInitialized())
                {
                    //Debug.Log("GameModManager is initialized");
                    using (GameMod gameMod = gameModManager.create())
                    {
                        //Debug.Log("Created GameMod");
                        using (GameMod.Editor editor = gameMod.edit())
                        {
                            //Debug.Log("Access to Editor");

                            // Required fields:
                            editor.setTitle("Custom Level");
                            editor.setCategory("level");
                            editor.setDescription("This is my custom level");

                            using (OutputStream os = editor.newFile("level.dat"))
                            {
                                os.write(System.Text.UTF8Encoding.UTF8.GetBytes("Hello World"));
                                os.close();
                            }

                            if (m_bitmapScreenshots.Length > 0)
                            {
                                editor.addScreenshot(m_bitmapScreenshots[0]);
                            }

                            // Add optional tags:
                            editor.addTag("space");
                            editor.addTag("king of the hill");

                            // Add optional metadata for your own display:
                            editor.setMetadata("difficulty=4;theme=space;mode=koth");

                            Plugin.saveGameMod(gameMod, editor);
                        }
                    }
                }
                else
                {
                    Debug.LogError("GameModManager is not initialized");
                }
            }
        }

        public void Edit(GameMod gameMod)
        {
            using (GameMod.Editor editor = gameMod.edit())
            {
                //Debug.Log("Access to Editor");

                // Required fields:
                editor.setTitle("edit title");
                editor.setCategory("edit category");
                editor.setDescription("edit description");

                // replace file
                editor.deleteFile("level.dat");

                using (OutputStream os = editor.newFile("level.dat"))
                {
                    os.write(System.Text.UTF8Encoding.UTF8.GetBytes("edit file"));
                    os.close();
                }

                // remove old screenshot
                List<GameModScreenshot> screenshots = gameMod.getScreenshots();
                for (int index = 0; index < screenshots.Count; ++index)
                {
                    using (GameModScreenshot gameModScreenshot = screenshots[index])
                    {
                        editor.removeScreenshot(gameModScreenshot);
                    }
                }

                // add new screenshot
                if (m_bitmapScreenshots.Length > 1)
                {
                    Debug.Log("Adding screenshot");
                    editor.addScreenshot(m_bitmapScreenshots[1]);
                }
                else
                {
                    Debug.LogError("Missing screenshot");
                }

                // remove old tags
                foreach (string tag in gameMod.getTags())
                {
                    editor.removeTag(tag);
                }

                // Add optional tags:
                editor.addTag("edit tag");

                // Add optional metadata for your own display:
                editor.setMetadata("edit meta data");

                Plugin.saveGameMod(gameMod, editor);
            }
        }

        void Publish(GameMod gameMod)
        {
            Plugin.contentPublish(gameMod);
        }

        void Unpublish(GameMod gameMod)
        {
            Plugin.contentUnpublish(gameMod);
        }

        void Delete(GameMod gameMod)
        {
            Plugin.contentDelete(gameMod);
        }

        void Download(GameMod gameMod)
        {
            Plugin.contentDownload(gameMod);
        }

        void Flag(GameMod gameMod)
        {
            gameMod.flag();
        }

        void Rate(GameMod gameMod)
        {
            gameMod.rate();
        }

        void RunSearch()
        {
            m_status = "Refreshing list...";

            CleanWidgets();
            ResetFocus();

            using (GameModManager gameModManager = Plugin.getGameModManager())
            {
                if (null == gameModManager)
                {
                    Debug.LogError("gameModManager is null");
                    return;
                }
                if (gameModManager.isInitialized())
                {
                    Plugin.getGameModManagerInstalled();
                    Plugin.getGameModManagerPublished(m_sortMethod);
                }
            }
        }

        void CleanWidgets()
        {
            foreach (WidgetGameMod widget in m_widgets)
            {
                widget.m_instance.Dispose();
                if (null != widget.m_screenshots)
                {
                    foreach (Texture2D texture in widget.m_screenshots)
                    {
                        if (null != texture)
                        {
                            Destroy(texture);
                        }
                    }
                }
                if (null != widget.m_thumbnails)
                {
                    foreach (Texture2D texture in widget.m_thumbnails)
                    {
                        if (null != texture)
                        {
                            Destroy(texture);
                        }
                    }
                }
            }
            m_widgets.Clear();
        }

        void AddWidgets(List<GameMod> gameMods, bool searchByInstalled, bool searchByPublished)
        {
            StringBuilder sb = new StringBuilder();
            foreach (GameMod gameMod in gameMods)
            {
                WidgetGameMod widget = new WidgetGameMod()
                {
                    m_instance = gameMod,
                    m_category = gameMod.getCategory(),
                    m_description = gameMod.getDescription(),
                    m_isDownloading = gameMod.isDownloading(),
                    m_isFlagged = gameMod.isFlagged(),
                    m_isInstalled = gameMod.isInstalled(),
                    m_isPublished = gameMod.isPublished(),
                    m_metaData = gameMod.getMetaData(),
                    m_ratingCount = gameMod.getRatingCount(),
                    m_ratingAverage = gameMod.getRatingAverage(),
                    m_title = gameMod.getTitle(),
                    m_userRating = gameMod.getUserRating(),
                    m_searchByInstalled = searchByInstalled,
                    m_searchByPublished = searchByPublished,
                };
                if (sb.Length > 0)
                {
                    sb.Remove(0, sb.Length);
                }
                foreach (string filename in gameMod.getFilenames())
                {
                    sb.Append(filename);
                    sb.Append(",");

                    using (InputStream inputStream = gameMod.openFile(filename))
                    {
                        byte[] buffer = new byte[100000];
                        int readAmount = inputStream.read(ref buffer);
                        inputStream.close();

                        byte[] copy = new byte[readAmount];
                        Array.Copy(buffer, copy, readAmount);

                        sb.Append("***");
                        string content = System.Text.UTF8Encoding.UTF8.GetString(copy);
                        sb.Append(content);
                    }
                }
                widget.m_filenames = sb.ToString();
                List<GameModScreenshot> screenshots = gameMod.getScreenshots();
                widget.m_screenshots = new Texture2D[screenshots.Count];
                widget.m_thumbnails = new Texture2D[screenshots.Count];
                for (int index = 0; index < screenshots.Count; ++index)
                {
                    using (GameModScreenshot gameModScreenshot = screenshots[index])
                    {
                        if (null != gameModScreenshot)
                        {
                            using (Bitmap bitmap = gameModScreenshot.getImage())
                            {
                                if (null != bitmap)
                                {
                                    using (ByteArrayOutputStream stream = new ByteArrayOutputStream())
                                    {
                                        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
                                        if (stream.size() >= 0)
                                        {
                                            Texture2D texture = new Texture2D(0, 0);
                                            texture.LoadImage(stream.toByteArray());
                                            widget.m_screenshots[index] = texture;
                                        }
                                        stream.close();
                                    }
                                }
                            }

                            using (Bitmap bitmap = gameModScreenshot.getThumbnail())
                            {
                                if (null != bitmap)
                                {
                                    using (ByteArrayOutputStream stream = new ByteArrayOutputStream())
                                    {
                                        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
                                        if (stream.size() >= 0)
                                        {
                                            Texture2D texture = new Texture2D(0, 0);
                                            texture.LoadImage(stream.toByteArray());
                                            widget.m_thumbnails[index] = texture;
                                        }
                                        stream.close();
                                    }
                                }
                            }
                        }
                    }
                }
                if (sb.Length > 0)
                {
                    sb.Remove(0, sb.Length);
                }
                foreach (string tag in gameMod.getTags())
                {
                    sb.Append(tag);
                    sb.Append(",");
                }
                widget.m_tags = sb.ToString();

                m_widgets.Add(widget);

                if (m_widgets.Count == 1)
                {
                    m_focusManager.Mappings[m_btnCreate].Down = widget.m_buttonPublish;
                    m_focusManager.Mappings[widget.m_buttonPublish] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Right = widget.m_buttonDelete,
                    };
                    m_focusManager.Mappings[widget.m_buttonDelete] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Left = widget.m_buttonPublish,
                        Right = widget.m_buttonDownload,
                    };
                    m_focusManager.Mappings[widget.m_buttonDownload] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Left = widget.m_buttonDelete,
                        Right = widget.m_buttonRate,
                    };
                    m_focusManager.Mappings[widget.m_buttonRate] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Left = widget.m_buttonDownload,
                        Right = widget.m_buttonEdit,
                    };
                    m_focusManager.Mappings[widget.m_buttonEdit] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Left = widget.m_buttonRate,
                        Right = widget.m_buttonFlag,
                    };
                    m_focusManager.Mappings[widget.m_buttonFlag] = new FocusManager.ButtonMapping()
                    {
                        Up = m_btnCreate,
                        Left = widget.m_buttonEdit,
                    };
                }
                else
                {
                    WidgetGameMod lastWidget = m_widgets[m_widgets.Count - 2];
                    m_focusManager.Mappings[lastWidget.m_buttonPublish].Down = widget.m_buttonPublish;
                    m_focusManager.Mappings[lastWidget.m_buttonDelete].Down = widget.m_buttonDelete;
                    m_focusManager.Mappings[lastWidget.m_buttonDownload].Down = widget.m_buttonDownload;
                    m_focusManager.Mappings[lastWidget.m_buttonRate].Down = widget.m_buttonRate;
                    m_focusManager.Mappings[lastWidget.m_buttonEdit].Down = widget.m_buttonEdit;
                    m_focusManager.Mappings[lastWidget.m_buttonFlag].Down = widget.m_buttonFlag;
                    m_focusManager.Mappings[widget.m_buttonPublish] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonPublish,
                        Right = widget.m_buttonDelete,
                    };
                    m_focusManager.Mappings[widget.m_buttonDelete] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonDelete,
                        Left = widget.m_buttonPublish,
                        Right = widget.m_buttonDownload,
                    };
                    m_focusManager.Mappings[widget.m_buttonDownload] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonDownload,
                        Left = widget.m_buttonDelete,
                        Right = widget.m_buttonRate,
                    };
                    m_focusManager.Mappings[widget.m_buttonRate] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonRate,
                        Left = widget.m_buttonDownload,
                        Right = widget.m_buttonEdit,
                    };
                    m_focusManager.Mappings[widget.m_buttonEdit] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonEdit,
                        Left = widget.m_buttonRate,
                        Right = widget.m_buttonFlag,
                    };
                    m_focusManager.Mappings[widget.m_buttonFlag] = new FocusManager.ButtonMapping()
                    {
                        Up = lastWidget.m_buttonFlag,
                        Left = widget.m_buttonEdit,
                    };
                }
            }
        }

        private void Update()
        {
            if (null != m_pendingInstalledSearchResults)
            {
                AddWidgets(m_pendingInstalledSearchResults, true, false);
                m_pendingInstalledSearchResults = null;
            }
            if (null != m_pendingPublishedSearchResults)
            {
                AddWidgets(m_pendingPublishedSearchResults, false, true);
                m_pendingPublishedSearchResults = null;
            }
            UpdateFocus();
        }

        #region Focus Handling

        void ResetFocus()
        {
            m_focusManager.Mappings.Clear();
            m_focusManager.Mappings[m_btnSearchByRating] = new FocusManager.ButtonMapping()
            {
                Down = m_btnSearchByCreatedAt,
            };
            m_focusManager.Mappings[m_btnSearchByCreatedAt] = new FocusManager.ButtonMapping()
            {
                Up = m_btnSearchByRating,
                Down = m_btnSearchByUpdatedAt,
            };
            m_focusManager.Mappings[m_btnSearchByUpdatedAt] = new FocusManager.ButtonMapping()
            {
                Up = m_btnSearchByCreatedAt,
                Down = m_btnCreate,
            };
            m_focusManager.Mappings[m_btnCreate] = new FocusManager.ButtonMapping()
            {
                Up = m_btnSearchByUpdatedAt,
            };

            // set default selection
            m_focusManager.SelectedButton = m_btnCreate;
        }

        private void UpdateFocus()
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

        #endregion
#endif
    }
}
