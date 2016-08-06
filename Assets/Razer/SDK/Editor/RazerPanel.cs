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

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.razerzone.store.sdk.engine.unity
{
    public class RazerPanel : EditorWindow
    {
        private const string KEY_ADB_IP = "RazerPanelAdbIpAddress";

        public static string AdbIpAddress
        {
            get
            {
                if (EditorPrefs.HasKey(KEY_ADB_IP))
                {
                    return EditorPrefs.GetString(KEY_ADB_IP);
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                EditorPrefs.SetString(KEY_ADB_IP, value);
            }
        }

        private static string[] m_toolSets =
            {
            "Razer",
            "Unity",
            "Java JDK",
            "Android SDK",
        };

        private int m_selectedToolSet = 0;

        #region Operations

        private bool m_toggleBuildAndRunApplication = false;

        private bool m_toggleOpenAndroidSDK = false;

        #endregion

        #region SDK

        public const string KEY_PATH_RAZER_SDK = @"Razer SDK";
        public const string KEY_PATH_AAR_RAZER_UNITY_PLUGIN = @"Razer Plugin";

        private static string pathRazerSDKAar = string.Empty;
        private static string pathPluginAar = string.Empty;

        private static string pathClassesJar = string.Empty;

        private static string pathRazerSDKJar = string.Empty;
        private static string pathPluginJar = string.Empty;

        private static string pathArmeabiv7a = string.Empty;
        private static string pathArmeabiv7aMeta = string.Empty;
        private static string pathArmeabiv7aNative = string.Empty;
        private static string pathArmeabiv7aNativeMeta = string.Empty;

        private static string pathJni = string.Empty;
        private static string pathJniMeta = string.Empty;
        private static string pathJniArmeabiv7a = string.Empty;
        private static string pathJniArmeabiv7aMeta = string.Empty;
        private static string pathJniArmeabiv7aNative = string.Empty;
        private static string pathJniArmeabiv7aNativeMeta = string.Empty;

        private static string pathManifestPath = string.Empty;
        private static string pathRes = string.Empty;

        private static string apkName = "Game.apk";

        void UpdatePluginPaths()
        {
            pathRazerSDKAar = string.Format("{0}/Assets/Plugins/Android/libs/store-sdk-standard-release.aar", pathUnityProject);
            pathPluginAar = string.Format("{0}/Assets/Plugins/Android/libs/UnityPluginStoreSDK.aar", pathUnityProject);

            pathClassesJar = string.Format("{0}/Assets/Plugins/Android/libs/classes.jar", pathUnityProject);

            pathRazerSDKJar = string.Format("{0}/Assets/Plugins/Android/libs/store-sdk-standard-release.jar", pathUnityProject);
            pathPluginJar = string.Format("{0}/Assets/Plugins/Android/libs/UnityPluginStoreSDK.jar", pathUnityProject);

            pathArmeabiv7a = string.Format("{0}/Assets/Plugins/Android/libs/armeabi-v7a", pathUnityProject);
            pathArmeabiv7aMeta = string.Format("{0}/Assets/Plugins/Android/libs/armeabi-v7a.meta", pathUnityProject);
            pathArmeabiv7aNative = string.Format("{0}/Assets/Plugins/Android/libs/armeabi-v7a/lib-ndk-unity-store-sdk.so", pathUnityProject);
            pathArmeabiv7aNativeMeta = string.Format("{0}/Assets/Plugins/Android/libs/armeabi-v7a/lib-ndk-unity-store-sdk.so.meta", pathUnityProject);

            pathJni = string.Format("{0}/Assets/Plugins/Android/libs/jni", pathUnityProject);
            pathJniMeta = string.Format("{0}/Assets/Plugins/Android/libs/jni.meta", pathUnityProject);
            pathJniArmeabiv7a = string.Format("{0}/Assets/Plugins/Android/libs/jni/armeabi-v7a", pathUnityProject);
            pathJniArmeabiv7aMeta = string.Format("{0}/Assets/Plugins/Android/libs/jni/armeabi-v7a.meta", pathUnityProject);
            pathJniArmeabiv7aNative = string.Format("{0}/Assets/Plugins/Android/libs/jni/armeabi-v7a/lib-ndk-unity-store-sdk.so", pathUnityProject);
            pathJniArmeabiv7aNativeMeta = string.Format("{0}/Assets/Plugins/Android/libs/jni/armeabi-v7a/lib-ndk-unity-store-sdk.so.meta", pathUnityProject);

            pathManifestPath = string.Format("{0}/Assets/Plugins/Android/AndroidManifest.xml", pathUnityProject);
            pathRes = string.Format("{0}/Assets/Plugins/Android/res", pathUnityProject);

            EditorPrefs.SetString(KEY_PATH_RAZER_SDK, pathRazerSDKAar);
        }

        public static string GetBundleId()
        {
            return PlayerSettings.bundleIdentifier;
        }

        #endregion

        #region Android SDK

        public const string KEY_PATH_ANDROID_JAR = @"Android Jar";
        public const string KEY_PATH_ANDROID_ADB = @"ADB Path";
        public const string KEY_PATH_ANDROID_AAPT = @"APT Path";
        public const string KEY_PATH_ANDROID_SDK = @"SDK Path";

        public const string REL_ANDROID_PLATFORM_TOOLS = "platform-tools";
        public const string FILE_AAPT_WIN = "aapt.exe";
        public const string FILE_AAPT_MAC = "aapt";
        public const string FILE_ADB_WIN = "adb.exe";
        public const string FILE_ADB_MAC = "adb";

        public static string pathADB = string.Empty;
        public static string pathAAPT = string.Empty;
        public static string pathSDK = string.Empty;

        static string GetPathAndroidJar()
        {
            return string.Format("{0}/platforms/android-{1}/android.jar", pathSDK, (int)PlayerSettings.Android.minSdkVersion);
        }

        public static void FindFile(DirectoryInfo searchFolder, string searchFile, ref string path)
        {
            if (null == searchFolder)
            {
                return;
            }
            foreach (FileInfo file in searchFolder.GetFiles(searchFile))
            {
                if (string.IsNullOrEmpty(file.FullName))
                {
                    continue;
                }
                path = file.FullName;
                return;
            }
            foreach (DirectoryInfo subDir in searchFolder.GetDirectories())
            {
                if (null == subDir)
                {
                    continue;
                }
                if (subDir.Name.ToUpper().Equals(".SVN"))
                {
                    continue;
                }
                if (subDir.Name.ToUpper().Equals(".GIT"))
                {
                    continue;
                }
                //Debug.Log(string.Format("Directory: {0}", subDir));
                FindFile(subDir, searchFile, ref path);
            }
        }

        void UpdateAndroidSDKPaths()
        {
            if (string.IsNullOrEmpty(pathADB))
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                        FindFile(new DirectoryInfo(string.Format("{0}", pathSDK)), FILE_ADB_MAC, ref pathADB);
                        pathADB = pathADB.Replace(@"\", "/");
                        break;
                    case RuntimePlatform.WindowsEditor:
                        FindFile(new DirectoryInfo(string.Format("{0}", pathSDK)), FILE_ADB_WIN, ref pathADB);
                        break;
                }
            }

            if (string.IsNullOrEmpty(pathAAPT))
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                        FindFile(new DirectoryInfo(string.Format("{0}", pathSDK)), FILE_AAPT_MAC, ref pathAAPT);
                        pathAAPT = pathAAPT.Replace(@"\", "/");
                        break;
                    case RuntimePlatform.WindowsEditor:
                        FindFile(new DirectoryInfo(string.Format("{0}", pathSDK)), FILE_AAPT_WIN, ref pathAAPT);
                        break;
                }
            }

            EditorPrefs.SetString(KEY_PATH_ANDROID_SDK, pathSDK);
            EditorPrefs.SetString(KEY_PATH_ANDROID_ADB, pathADB);
            EditorPrefs.SetString(KEY_PATH_ANDROID_AAPT, pathAAPT);
        }

        void ResetAndroidSDKPaths()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    pathSDK = @"android-sdk-mac_x86";
                    break;
                case RuntimePlatform.WindowsEditor:
                    pathSDK = @"C:/Program Files (x86)/Android/android-sdk";
                    break;
            }

            UpdateAndroidSDKPaths();
        }

        void SelectAndroidSDKPaths()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    path = EditorUtility.OpenFolderPanel(string.Format("Path to {0}", KEY_PATH_ANDROID_SDK), pathSDK, "../android-sdk-mac_x86");
                    break;
                case RuntimePlatform.WindowsEditor:
                    path = EditorUtility.OpenFolderPanel(string.Format("Path to {0}", KEY_PATH_ANDROID_SDK), pathSDK, @"..\android-sdk");
                    break;
            }
            if (!string.IsNullOrEmpty(path))
            {
                pathSDK = path;
            }

            UpdateAndroidSDKPaths();
        }

        #endregion

        #region Unity Paths

        public const string KEY_PATH_UNITY_EDITOR = @"Unity Editor";
        public const string KEY_PATH_UNITY_PROJECT = @"Unity Project";

        private static string pathUnityEditor = string.Empty;
        private static string pathUnityProject = string.Empty;

        #endregion

        #region Java JDK

        public const string KEY_PATH_JAVA_TOOLS_JAR = @"Tools Jar";
        public const string KEY_PATH_JAVA_JAR = @"Jar Path";
        public const string KEY_PATH_JAVA_JAVAC = @"JavaC Path";
        public const string KEY_PATH_JAVA_JAVAP = @"JavaP Path";
        public const string KEY_PATH_JAVA_JDK = @"JDK Path";

        public const string REL_JAVA_PLATFORM_TOOLS = "bin";
        public const string FILE_JAR_WIN = "jar.exe";
        public const string FILE_JAR_MAC = "jar";
        public const string FILE_JAVAC_WIN = "javac.exe";
        public const string FILE_JAVAC_MAC = "javac";
        public const string FILE_JAVAP_WIN = "javap.exe";
        public const string FILE_JAVAP_MAC = "javap";

        public static string pathToolsJar = string.Empty;
        public static string pathJar = string.Empty;
        public static string pathJavaC = string.Empty;
        public static string pathJavaP = string.Empty;
        public static string pathJDK = string.Empty;

        void UpdateJavaJDKPaths()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    pathToolsJar = string.Format("{0}/Contents/Classes/classes.jar", pathJDK);
                    pathJar = string.Format("{0}/Contents/Commands/{1}", pathJDK, FILE_JAR_MAC);
                    pathJavaC = string.Format("{0}/Contents/Commands/{1}", pathJDK, FILE_JAVAC_MAC);
                    pathJavaP = string.Format("{0}/Contents/Commands/{1}", pathJDK, FILE_JAVAP_MAC);
                    break;
                case RuntimePlatform.WindowsEditor:
                    pathToolsJar = string.Format("{0}/lib/tools.jar", pathJDK);
                    pathJar = string.Format("{0}/{1}/{2}", pathJDK, REL_JAVA_PLATFORM_TOOLS, FILE_JAR_WIN);
                    pathJavaC = string.Format("{0}/{1}/{2}", pathJDK, REL_JAVA_PLATFORM_TOOLS, FILE_JAVAC_WIN);
                    pathJavaP = string.Format("{0}/{1}/{2}", pathJDK, REL_JAVA_PLATFORM_TOOLS, FILE_JAVAP_WIN);
                    break;
            }

            EditorPrefs.SetString(KEY_PATH_JAVA_JDK, pathJDK);
        }

        void ResetJavaJDKPaths()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    pathJDK = @"/System/Library/Java/JavaVirtualMachines/1.6.0.jdk";
                    break;
                case RuntimePlatform.WindowsEditor:
                    pathJDK = @"C:\Program Files\Java\jdk1.8.0_101";
                    break;
            }

            UpdateJavaJDKPaths();
        }

        void SelectJavaJDKPaths()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    path = EditorUtility.OpenFolderPanel(string.Format("Path to {0}", KEY_PATH_JAVA_JDK), pathJDK, "../jdk1.8.0_101");
                    break;
                case RuntimePlatform.WindowsEditor:
                    path = EditorUtility.OpenFolderPanel(string.Format("Path to {0}", KEY_PATH_JAVA_JDK), pathJDK, @"..\jdk1.8.0_101");
                    break;
            }
            if (!string.IsNullOrEmpty(path))
            {
                pathJDK = path;
            }

            UpdateJavaJDKPaths();
        }

        #endregion

        void OnEnable()
        {
            pathUnityEditor = new FileInfo(EditorApplication.applicationPath).Directory.FullName;
            pathUnityProject = new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;

            if (EditorPrefs.HasKey(KEY_PATH_ANDROID_SDK))
            {
                pathSDK = EditorPrefs.GetString(KEY_PATH_ANDROID_SDK);
            }

            if (EditorPrefs.HasKey(KEY_PATH_ANDROID_AAPT))
            {
                pathAAPT = EditorPrefs.GetString(KEY_PATH_ANDROID_AAPT);
            }

            if (EditorPrefs.HasKey(KEY_PATH_ANDROID_ADB))
            {
                pathADB = EditorPrefs.GetString(KEY_PATH_ANDROID_ADB);
            }

            if (string.IsNullOrEmpty(pathSDK))
            {
                pathAAPT = string.Empty;
                pathADB = string.Empty;
                ResetAndroidSDKPaths();
            }
            else
            {
                UpdateAndroidSDKPaths();
            }


            if (EditorPrefs.HasKey(KEY_PATH_JAVA_JDK))
            {
                pathJDK = EditorPrefs.GetString(KEY_PATH_JAVA_JDK);
            }

            if (string.IsNullOrEmpty(pathJDK))
            {
                ResetJavaJDKPaths();
            }
            else
            {
                UpdateJavaJDKPaths();
            }

            UpdatePluginPaths();
        }

        void Update()
        {
            Repaint();

            if (!string.IsNullOrEmpty(m_nextScene))
            {
                EditorApplication.OpenScene(m_nextScene);
                m_nextScene = string.Empty;
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            var sceneList = new List<string>();

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                    sceneList.Add(scene.path);
            }

            var sceneArray = sceneList.ToArray();

            if (m_toggleBuildAndRunApplication)
            {
                m_toggleBuildAndRunApplication = false;

                AssetDatabase.Refresh();

                BuildOptions options = BuildOptions.AutoRunPlayer;
                if (EditorUserBuildSettings.allowDebugging)
                {
                    options |= BuildOptions.Development | BuildOptions.AllowDebugging;
                }

                BuildPipeline.BuildPlayer(sceneArray, string.Format("{0}/{1}", pathUnityProject, apkName),
                                          BuildTarget.Android, options);
            }

            if (m_toggleOpenAndroidSDK)
            {
                m_toggleOpenAndroidSDK = false;

                string androidPath = string.Empty;

                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                        androidPath = string.Format("{0}/tools/android", pathSDK);
                        break;

                    case RuntimePlatform.WindowsEditor:
                        androidPath = string.Format("{0}/tools/android.bat", pathSDK);
                        break;
                }

                if (!string.IsNullOrEmpty(androidPath) &&
                    File.Exists(androidPath))
                {
                    //Debug.Log(androidPath);
                    string args = "sdk";
                    ProcessStartInfo ps = new ProcessStartInfo(androidPath, args);
                    Process p = new Process();
                    p.StartInfo = ps;
                    p.Exited += (object sender, EventArgs e) =>
                                    {
                                        p.Dispose();
                                    };
                    p.Start();
                }
            }
        }

        void GUIDisplayFolder(string label, string path)
        {
            bool dirExists = Directory.Exists(path);

            if (!dirExists)
            {
                GUI.enabled = false;
            }
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
            GUILayout.Space(25);
            GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
            GUILayout.Space(5);
            GUILayout.Label(path.Replace("/", @"\"), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
            GUILayout.EndHorizontal();
            if (!dirExists)
            {
                GUI.enabled = true;
            }
        }

        void GUIDisplayFile(string label, string path)
        {
            bool fileExists = File.Exists(path);

            if (!fileExists)
            {
                GUI.enabled = false;
            }
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
            GUILayout.Space(25);
            GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
            GUILayout.Space(5);
            GUILayout.Label(path.Replace("/", @"\"), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
            GUILayout.EndHorizontal();
            if (!fileExists)
            {
                GUI.enabled = true;
            }
        }

        void GUIDisplayUnityFile(string label, string path)
        {
            bool fileExists = File.Exists(path);

            if (!fileExists)
            {
                GUI.enabled = false;
            }
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
            GUILayout.Space(25);
            GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
            GUILayout.Space(5);
            if (string.IsNullOrEmpty(path))
            {
                EditorGUILayout.ObjectField(string.Empty, null, typeof(UnityEngine.Object), false);
            }
            else
            {
                try
                {
                    DirectoryInfo assets = new DirectoryInfo("Assets");
                    Uri assetsUri = new Uri(assets.FullName);
                    FileInfo fi = new FileInfo(path);
                    string relativePath = assetsUri.MakeRelativeUri(new Uri(fi.FullName)).ToString();
                    UnityEngine.Object fileRef = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));
                    EditorGUILayout.ObjectField(string.Empty, fileRef, typeof(UnityEngine.Object), false);
                }
                catch (System.Exception)
                {
                    Debug.LogError(string.Format("Path is invalid: label={0} path={1}", label, path));
                }
            }
            GUILayout.EndHorizontal();
            if (!fileExists)
            {
                GUI.enabled = true;
            }
        }

        private string GetLicenseInfo()
        {
            string license = UnityEditorInternal.InternalEditorUtility.GetLicenseInfo();
            if (license.Contains("Serial number"))
            {
                int index = license.IndexOf("Serial number");
                if (index > 0)
                {
                    return license.Substring(0, index);
                }
            }
            return license;
        }

        private void DisplayImagesFor(string title, BuildTargetGroup target)
        {
            var found = false;
            Texture2D[] textures = PlayerSettings.GetIconsForTargetGroup(target);
            int[] textureSizes = PlayerSettings.GetIconSizesForTargetGroup(target);
            for (var i = 0; i < textureSizes.Length; i++)
            {
                var texture2D = textures[i];
                if (texture2D == null) continue;
                if (!found)
                {
                    EditorGUILayout.LabelField(title);
                    found = true;
                }

                EditorGUILayout.LabelField(string.Format("[{1}] - {0}", texture2D.ToString(), textureSizes[i]));
                new GUIContent(texture2D);
                Rect pos = GUILayoutUtility.GetRect(textureSizes[i], textureSizes[i], EditorStyles.miniButton, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(pos, texture2D);
            }
        }

        private void ShowImage(string title, Texture2D image)
        {
            EditorGUILayout.LabelField(title);
            if (image != null)
            {
                GUIContent content = new GUIContent(image);
                Rect pos = GUILayoutUtility.GetRect(content, EditorStyles.miniButton, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(pos, image);
            }
        }

        // load the scene in the update method
        private string m_nextScene = string.Empty;

        private void SwitchToExampleScene(string sceneName)
        {
            EditorBuildSettingsScene[] scenes =
                {
                new EditorBuildSettingsScene(string.Format("Assets/Razer/Examples/Scenes/{0}.unity", sceneName), true),
            };

            File.Copy(string.Format("Assets/Razer/Examples/Icons/{0}/app_icon.png", sceneName),
                "Assets/Plugins/Android/res/drawable/app_icon.png",
                true);

            File.Copy(string.Format("Assets/Razer/Examples/Icons/{0}/icon.png", sceneName),
                "Assets/Plugins/Android/res/drawable/icon.png",
                true);

            File.Copy(string.Format("Assets/Razer/Examples/Icons/{0}/ouya_icon.png", sceneName),
                @"Assets/Plugins/Android/res/drawable-xhdpi/ouya_icon.png",
                true);

            SetupExample(scenes, sceneName);
        }

        private void SwitchToOuyaEverywhereIcons()
        {
            File.Copy("Assets/Razer/Examples/Icons/OuyaEverywhere/app_icon.png",
                "Assets/Plugins/Android/res/drawable/app_icon.png",
                true);

            File.Copy("Assets/Razer/Examples/Icons/OuyaEverywhere/icon.png",
                      "Assets/Plugins/Android/res/drawable/icon.png",
                      true);

            File.Copy("Assets/Razer/Examples/Icons/OuyaEverywhere//ouya_icon.png",
                @"Assets/Plugins/Android/res/drawable-xhdpi/ouya_icon.png",
                true);
        }

        private void SetupExample(EditorBuildSettingsScene[] scenes, string productName)
        {
            EditorBuildSettings.scenes = scenes;
            m_nextScene = scenes[0].path;

            apkName = string.Format("{0}.apk", productName);

            PlayerSettings.bundleIdentifier = string.Format("com.razerzone.store.sdk.engine.unity.example.{0}", productName);
            PlayerSettings.productName = productName;
        }

        private Vector2 m_scroll = Vector2.zero;

        private static int m_selectedExample = 0;

        private static string[] m_exampleScenes =
            {
            "OUYA Everywhere Icons",
            "ExampleAudio",
            "ExampleBilling",
            "ExampleCommunityContent",
            "ExampleDefaultInput",
            "ExampleJS",
            "ExampleSafeArea",
            "ExampleVirtualController",
            "ExampleVirtualControllerJS",
        };

        void OnGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling;

            m_scroll = GUILayout.BeginScrollView(m_scroll, GUILayout.MaxWidth(position.width));

            GUILayout.Label(string.Format("{0} UID: {1}", RazerSDK.PLUGIN_VERSION, UID));

            m_selectedToolSet = GUILayout.Toolbar(m_selectedToolSet, m_toolSets, GUILayout.MaxWidth(position.width));

            GUILayout.Space(20);

            switch (m_selectedToolSet)
            {
                case 0:

                    if (GUILayout.Button("Run", GUILayout.MaxWidth(position.width)))
                    {
                        string appPath = string.Format("{0}/{1}", pathUnityProject, apkName);
                        if (File.Exists(appPath))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format("shell am start -n {0}/com.razerzone.store.sdk.engine.unity.MainActivity", PlayerSettings.bundleIdentifier);
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                        }
                    }

                    if (GUILayout.Button("Stop", GUILayout.MaxWidth(position.width)))
                    {
                        string appPath = string.Format("{0}/{1}", pathUnityProject, apkName);
                        if (File.Exists(appPath))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format("shell am force-stop {0}", PlayerSettings.bundleIdentifier);
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                        }
                    }

                    if (GUILayout.Button("Reinstall", GUILayout.MaxWidth(position.width)))
                    {
                        string appPath = string.Format("{0}/{1}", pathUnityProject, apkName);
                        if (File.Exists(appPath))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format("install -r \"{0}\"", appPath);
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                        }
                    }

                    if (GUILayout.Button("Uninstall", GUILayout.MaxWidth(position.width)))
                    {
                        //Debug.Log(pathADB);
                        string args = string.Format("uninstall {0}", PlayerSettings.bundleIdentifier);
                        //Debug.Log(args);
                        ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                        using (Process p = new Process())
                        {
                            p.StartInfo = ps;
                            p.Start();
                        }
                    }

                    if (GUILayout.Button("Unity 4.X Unpack AARs", GUILayout.MaxWidth(position.width)))
                    {
                        CleanAARs();

                        String pathLibs = string.Format("{0}/Assets/Plugins/Android/libs", pathUnityProject);

                        using (Process p = new Process())
                        {
                            //Debug.Log(pathJDK);
                            string args = "xf store-sdk-standard-release.aar classes.jar";
                            //Debug.Log(args);
                            //Debug.Log(pathJar);
                            DeleteFile(pathClassesJar);
                            ProcessStartInfo ps = new ProcessStartInfo(pathJar, args);
                            ps.WorkingDirectory = pathLibs;
                            p.StartInfo = ps;
                            p.Start();
                            while (!p.HasExited)
                            {
                                Thread.Sleep(0);
                            }
                            Thread.Sleep(100);
                            if (File.Exists(pathClassesJar))
                            {
                                Debug.Log(string.Format("Create: {0}", pathRazerSDKJar));
                                File.Move(pathClassesJar, pathRazerSDKJar);
                            }
                        }

                        using (Process p = new Process())
                        {
                            //Debug.Log(pathJDK);
                            string args = "xf UnityPluginStoreSDK.aar classes.jar";
                            //Debug.Log(args);
                            //Debug.Log(pathJar);
                            DeleteFile(pathClassesJar);
                            ProcessStartInfo ps = new ProcessStartInfo(pathJar, args);
                            ps.WorkingDirectory = pathLibs;
                            p.StartInfo = ps;
                            p.Start();
                            while (!p.HasExited)
                            {
                                Thread.Sleep(0);
                            }
                            Thread.Sleep(100);
                            if (File.Exists(pathClassesJar))
                            {
                                Debug.Log(string.Format("Create: {0}", pathPluginJar));
                                File.Move(pathClassesJar, pathPluginJar);
                            }
                        }

                        using (Process p = new Process())
                        {
                            //Debug.Log(pathArmeabiv7a);
                            DirectoryInfo dir = new DirectoryInfo(pathArmeabiv7a);
                            if (!dir.Exists)
                            {
                                dir.Create();
                            }

                            //Debug.Log(pathJDK);
                            string args = "xf UnityPluginStoreSDK.aar jni";
                            //Debug.Log(args);
                            //Debug.Log(pathJar);
                            ProcessStartInfo ps = new ProcessStartInfo(pathJar, args);
                            ps.WorkingDirectory = pathLibs;
                            p.StartInfo = ps;
                            p.Start();
                            while (!p.HasExited)
                            {
                                Thread.Sleep(0);
                            }

                            Thread.Sleep(100);
                            if (File.Exists(pathJniArmeabiv7aNative))
                            {
                                Debug.Log(string.Format("Create: {0}", pathArmeabiv7aNative));
                                File.Move(pathJniArmeabiv7aNative, pathArmeabiv7aNative);
                            }

                            CleanAARsJni();

                        }

                        AssetDatabase.Refresh();
                    }

                    if (GUILayout.Button("Unity 5.X Clean AARs", GUILayout.MaxWidth(position.width)))
                    {
                        CleanAARs();
                        AssetDatabase.Refresh();
                    }

                    if (GUILayout.Button("Build and Run", GUILayout.MaxWidth(position.width)))
                    {
                        m_toggleBuildAndRunApplication = true;
                    }

                    GUILayout.Space(5);

                    #region Example scenes

                    GUILayout.Label("Build Settings:");

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width - 35));
                    m_selectedExample = EditorGUILayout.Popup(m_selectedExample, m_exampleScenes, GUILayout.MaxWidth(position.width));
                    if (GUILayout.Button("Switch to Example"))
                    {
                        if (m_selectedExample == 0)
                        {
                            SwitchToOuyaEverywhereIcons();
                        }
                        else
                        {
                            SwitchToExampleScene(m_exampleScenes[m_selectedExample]);
                        }
                    }
                    GUILayout.EndHorizontal();

                    #endregion

                    GUILayout.Label("Razer", EditorStyles.boldLabel);

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    GUILayout.Space(25);
                    GUILayout.Label("License:", GUILayout.Width(100));
                    GUILayout.Label(GetLicenseInfo(), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
                    GUILayout.EndHorizontal();

                    //ShowIcons();

                    // show splash screen settings

                    string error = string.Empty;

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    GUILayout.Space(25);
                    GUILayout.Label("Product Name", GUILayout.Width(100));
                    GUILayout.Space(5);
                    PlayerSettings.productName = GUILayout.TextField(PlayerSettings.productName, EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
                    GUILayout.EndHorizontal();

                    if ((PlayerSettings.bundleIdentifier.Contains(" ") ||
                        PlayerSettings.bundleIdentifier.Contains("\t") ||
                        PlayerSettings.bundleIdentifier.Contains("\r") ||
                        PlayerSettings.bundleIdentifier.Contains("\n") ||
                        PlayerSettings.bundleIdentifier.Contains("(") ||
                        PlayerSettings.bundleIdentifier.Contains(")")))
                    {
                        String fieldError = "[error] (bundle id has an invalid character)\n";
                        if (string.IsNullOrEmpty(error))
                        {
                            ShowNotification(new GUIContent(fieldError));
                            error = fieldError;
                        }
                        EditorGUILayout.Separator();
                        GUILayout.Label(fieldError, EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
                    }
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    GUILayout.Space(25);
                    GUILayout.Label("Bundle Identifier", GUILayout.Width(100));
                    GUILayout.Space(5);
                    PlayerSettings.bundleIdentifier = GUILayout.TextField(PlayerSettings.bundleIdentifier, EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
                    GUILayout.EndHorizontal();

                    GameObject go = GameObject.Find("RazerGameObject");
                    RazerGameObject razerGameObject = null;
                    if (go)
                    {
                        razerGameObject = go.GetComponent<RazerGameObject>();
                    }
                    if (null == razerGameObject)
                    {
                        GUI.enabled = false;
                    }
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    GUILayout.Space(25);
                    GUILayout.Label(string.Format("{0}:", "RazerGameObject"), GUILayout.Width(100));
                    GUILayout.Space(5);
                    EditorGUILayout.ObjectField(string.Empty, razerGameObject, typeof(RazerGameObject), true);
                    GUILayout.EndHorizontal();
                    if (null == razerGameObject)
                    {
                        GUI.enabled = true;
                    }

                    GUIDisplayUnityFile(KEY_PATH_RAZER_SDK, pathRazerSDKAar);
                    GUIDisplayUnityFile(KEY_PATH_AAR_RAZER_UNITY_PLUGIN, pathPluginAar);
                    GUIDisplayUnityFile("Manifest", pathManifestPath);
                    GUIDisplayFolder("Res", pathRes);

                    if (GUILayout.Button("Check for plugin updates"))
                    {
                        Application.OpenURL("https://github.com/razerofficial/unity-plugin-razer-sdk/releases");
                    }

                    if (GUILayout.Button("Developer Chat on Razer Forums"))
                    {
                        Application.OpenURL("https://insider.razerzone.com/index.php?forums/developer-chat.135/");
                    }

                    if (GUILayout.Button("Read Razer Unity Docs"))
                    {
                        Application.OpenURL("https://github.com/razerofficial/unity-plugin-razer-sdk");
                    }

                    if (GUILayout.Button("Razer Developer Portal"))
                    {
                        Application.OpenURL("https://devs.ouya.tv/developers");
                    }

                    break;
                case 1:
                    GUILayout.Label("Unity Paths", EditorStyles.boldLabel);

                    GUIDisplayFolder(KEY_PATH_UNITY_EDITOR, pathUnityEditor);
                    GUIDisplayFolder(KEY_PATH_UNITY_PROJECT, pathUnityProject);

                    if (GUILayout.Button("Download Unity3d"))
                    {
                        Application.OpenURL("http://unity3d.com/unity/download/");
                    }

                    if (GUILayout.Button("Unity3d Training"))
                    {
                        Application.OpenURL("http://unity3d.com/learn");
                    }

                    if (GUILayout.Button("Unity3d Scripting Reference"))
                    {
                        Application.OpenURL("http://docs.unity3d.com/Documentation/ScriptReference/index.html");
                    }

                    break;
                case 2:
                    GUILayout.Label("Java JDK Paths", EditorStyles.boldLabel);

                    GUIDisplayFile(KEY_PATH_JAVA_TOOLS_JAR, pathToolsJar);
                    GUIDisplayFile(KEY_PATH_JAVA_JAR, pathJar);
                    GUIDisplayFile(KEY_PATH_JAVA_JAVAC, pathJavaC);
                    GUIDisplayFile(KEY_PATH_JAVA_JAVAP, pathJavaP);
                    GUIDisplayFolder(KEY_PATH_JAVA_JDK, pathJDK);

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    if (GUILayout.Button("Select SDK Path..."))
                    {
                        pathAAPT = string.Empty;
                        pathADB = string.Empty;
                        SelectJavaJDKPaths();
                    }
                    if (GUILayout.Button("Reset Paths"))
                    {
                        pathAAPT = string.Empty;
                        pathADB = string.Empty;
                        ResetJavaJDKPaths();
                    }

                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Download JDK8"))
                    {
                        Application.OpenURL("http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html");
                    }

                    break;
                case 3:
                    GUILayout.Label("Android SDK", EditorStyles.boldLabel);

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    GUILayout.Space(25);
                    GUILayout.Label(string.Format("{0}:", "minSDKVersion"), GUILayout.Width(100));
                    GUILayout.Space(5);
                    GUILayout.Label(((int)(PlayerSettings.Android.minSdkVersion)).ToString(), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(position.width - 130));
                    GUILayout.EndHorizontal();

                    GUIDisplayFile(KEY_PATH_ANDROID_JAR, GetPathAndroidJar());
                    GUIDisplayFile(KEY_PATH_ANDROID_ADB, pathADB);
                    GUIDisplayFile(KEY_PATH_ANDROID_AAPT, pathAAPT);
                    GUIDisplayFolder(KEY_PATH_ANDROID_SDK, pathSDK);

                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
                    if (GUILayout.Button("Select SDK Path..."))
                    {
                        SelectAndroidSDKPaths();
                    }
                    if (GUILayout.Button("Reset Paths"))
                    {
                        ResetAndroidSDKPaths();
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Download Android SDK"))
                    {
                        Application.OpenURL("http://developer.android.com/sdk/index.html");
                    }

                    if (GUILayout.Button("Open Android SDK"))
                    {
                        m_toggleOpenAndroidSDK = true;
                    }

                    if (GUILayout.Button("Open Shell"))
                    {
                        string shellPath = @"c:\windows\system32\cmd.exe";
                        if (File.Exists(shellPath))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format(@"/k");
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(shellPath, args);
                            Process p = new Process();
                            ps.RedirectStandardOutput = false;
                            ps.UseShellExecute = true;
                            ps.CreateNoWindow = false;
                            ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                                            {
                                                p.Dispose();
                                            };
                            p.Start();
                        }
                        EditorGUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button("Take Screenshot"))
                    {
                        ThreadStart ts = new ThreadStart(() =>
                                                             {
                                                                 string currentDirectory = Directory.GetCurrentDirectory();
                                                                 if (File.Exists(pathADB))
                                                                 {
                                                                     //Debug.Log(appPath);
                                                                     //Debug.Log(pathADB);
                                                                     string args =
                                                                             string.Format(
                                                                                 @"shell /system/bin/screencap -p /sdcard/screenshot.png");
                                                                     //Debug.Log(args);
                                                                     ProcessStartInfo ps = new ProcessStartInfo(pathADB,
                                                                                                                    args);
                                                                     Process p = new Process();
                                                                     ps.RedirectStandardOutput = false;
                                                                     ps.UseShellExecute = true;
                                                                     ps.CreateNoWindow = false;
                                                                     ps.WorkingDirectory = currentDirectory;
                                                                     p.StartInfo = ps;
                                                                     p.Exited += (object sender, EventArgs e) =>
                                                                                     {
                                                                                         p.Dispose();
                                                                                     };
                                                                     p.Start();

                                                                     p.WaitForExit();


                                                                     string args2 =
                                                                         string.Format(
                                                                             @"pull /sdcard/screenshot.png screenshot.png");
                                                                     //Debug.Log(args2);
                                                                     ProcessStartInfo ps2 = new ProcessStartInfo(pathADB,
                                                                                                                     args2);
                                                                     Process p2 = new Process();
                                                                     ps2.RedirectStandardOutput = false;
                                                                     ps2.UseShellExecute = true;
                                                                     ps2.CreateNoWindow = false;
                                                                     ps2.WorkingDirectory = currentDirectory;
                                                                     p2.StartInfo = ps2;
                                                                     p2.Exited += (object sender, EventArgs e) =>
                                                                                      {
                                                                                          p2.Dispose();
                                                                                      };
                                                                     p2.Start();

                                                                     p2.WaitForExit();

                                                                     string shellPath = @"c:\windows\system32\cmd.exe";
                                                                     if (File.Exists(shellPath))
                                                                     {
                                                                         //Debug.Log(appPath);
                                                                         //Debug.Log(pathADB);
                                                                         string args3 =
                                                                                 string.Format(@"/c start screenshot.png");
                                                                         //Debug.Log(args3);
                                                                         ProcessStartInfo ps3 =
                                                                                 new ProcessStartInfo(shellPath, args3);
                                                                         Process p3 = new Process();
                                                                         ps3.RedirectStandardOutput = false;
                                                                         ps3.UseShellExecute = true;
                                                                         ps3.CreateNoWindow = false;
                                                                         ps3.WorkingDirectory = currentDirectory;
                                                                         p3.StartInfo = ps3;
                                                                         p3.Exited += (object sender, EventArgs e) =>
                                                                                          {
                                                                                              p3.Dispose();
                                                                                          };
                                                                         p3.Start();
                                                                         //p.WaitForExit();
                                                                     }
                                                                 }
                                                             });
                        Thread thread = new Thread(ts);
                        thread.Start();
                        EditorGUIUtility.ExitGUI();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Logcat");
                    if (GUILayout.Button("Open"))
                    {
                        if (File.Exists(pathADB))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format(@"shell logcat");
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            ps.RedirectStandardOutput = false;
                            ps.UseShellExecute = true;
                            ps.CreateNoWindow = false;
                            ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                            //p.WaitForExit();
                        }
                        EditorGUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Clear"))
                    {
                        if (File.Exists(pathADB))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            string args = string.Format(@"shell logcat -c");
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            ps.RedirectStandardOutput = false;
                            ps.UseShellExecute = true;
                            ps.CreateNoWindow = false;
                            ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                            //p.WaitForExit();
                        }
                        EditorGUIUtility.ExitGUI();
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reboot Console"))
                    {
                        Reboot();
                        EditorGUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button("Back"))
                    {
                        string args = string.Format("shell input keyevent BACK");

                        if (File.Exists(pathADB))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            ps.RedirectStandardOutput = false;
                            ps.UseShellExecute = true;
                            ps.CreateNoWindow = false;
                            ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                            //p.WaitForExit();
                        }
                        EditorGUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Home"))
                    {
                        string args = string.Format("shell input keyevent HOME");

                        if (File.Exists(pathADB))
                        {
                            //Debug.Log(appPath);
                            //Debug.Log(pathADB);
                            //Debug.Log(args);
                            ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                            Process p = new Process();
                            ps.RedirectStandardOutput = false;
                            ps.UseShellExecute = true;
                            ps.CreateNoWindow = false;
                            ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                            p.StartInfo = ps;
                            p.Exited += (object sender, EventArgs e) =>
                            {
                                p.Dispose();
                            };
                            p.Start();
                            //p.WaitForExit();
                        }
                        EditorGUIUtility.ExitGUI();
                    }

                    break;
            }

            GUILayout.EndScrollView();
        }

        #region Reboot

        private void Reboot()
        {
            if (File.Exists(pathADB))
            {
                //Debug.Log(appPath);
                //Debug.Log(pathADB);
                string args = string.Format(@"reboot");
                //Debug.Log(args);
                ProcessStartInfo ps = new ProcessStartInfo(pathADB, args);
                Process p = new Process();
                ps.RedirectStandardOutput = false;
                ps.UseShellExecute = true;
                ps.CreateNoWindow = false;
                ps.WorkingDirectory = Path.GetDirectoryName(pathADB);
                p.StartInfo = ps;
                p.Exited += (object sender, EventArgs e) =>
                {
                    p.Dispose();
                };
                p.Start();
                //p.WaitForExit();
            }
        }

        #endregion

        #region RUN PROCESS
        public static void RunProcess(string path, string arguments)
        {
            List<KeyValuePair<string, string>> environment = new List<KeyValuePair<string, string>>();
            RunProcess(environment, path, arguments);
        }

        public static void RunProcess(List<KeyValuePair<string, string>> environment, string path, string arguments)
        {
            string error = string.Empty;
            string output = string.Empty;
            RunProcess(environment, path, string.Empty, arguments, ref output, ref error);
        }

        public static void RunProcess(string path, string workingDirectory, string arguments)
        {
            List<KeyValuePair<string, string>> environment = new List<KeyValuePair<string, string>>();
            RunProcess(environment, path, workingDirectory, arguments);
        }

        public static void RunProcess(List<KeyValuePair<string, string>> environment, string path, string workingDirectory, string arguments)
        {
            string error = string.Empty;
            string output = string.Empty;
            RunProcess(environment, path, workingDirectory, arguments, ref output, ref error);
        }

        public static void RunProcess(string path, string workingDirectory, string arguments, string description)
        {
            List<KeyValuePair<string, string>> environment = new List<KeyValuePair<string, string>>();
            RunProcess(environment, path, workingDirectory, arguments, description);
        }

        public static void RunProcess(List<KeyValuePair<string, string>> environment, string path, string workingDirectory, string arguments, string description)
        {
            string error = string.Empty;
            string output = string.Empty;
            RunProcess(environment, path, workingDirectory, arguments, ref output, ref error, description);
        }

        public static void RunProcess(string path, string workingDirectory, string arguments, ref string output, ref string error)
        {
            List<KeyValuePair<string, string>> environment = new List<KeyValuePair<string, string>>();
            RunProcess(environment, path, workingDirectory, arguments, ref output, ref error);
        }

        public static void RunProcess(List<KeyValuePair<string, string>> environment, string path, string workingDirectory, string arguments, ref string output, ref string error)
        {
            RunProcess(environment, path, workingDirectory, arguments, ref output, ref error, string.Empty);
        }

        public static void RunProcess(List<KeyValuePair<string, string>> environment, string path, string workingDirectory, string arguments, ref string output, ref string error, string description)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                foreach (KeyValuePair<string, string> pair in environment)
                {
                    process.StartInfo.EnvironmentVariables[pair.Key] = pair.Value;
                }
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                DateTime startTime = DateTime.Now;

                if (string.IsNullOrEmpty(description))
                {
                    Debug.Log(string.Format("[Running Process] filename={0} arguments={1}", process.StartInfo.FileName,
                        process.StartInfo.Arguments));
                }
                else
                {
                    Debug.Log(string.Format("{0}\n[Running Process] filename={1} arguments={2}", description, process.StartInfo.FileName,
                        process.StartInfo.Arguments));
                }

                process.Start();

                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                float elapsed = (float)(DateTime.Now - startTime).TotalSeconds;
                if (string.IsNullOrEmpty(description))
                {
                    Debug.Log(string.Format("[Results] elapsedTime: {3} errors: {2}\noutput: {1}", process.StartInfo.FileName,
                        output, error, elapsed));

                }
                else
                {
                    Debug.Log(string.Format("{0}\n[Results] elapsedTime: {3} errors: {2}\noutput: {1}", description,
                        output, error, elapsed));
                }
                //if (output.Length > 0 ) Debug.Log("Output: " + output);
                //if (error.Length > 0 ) Debug.Log("Error: " + error); 
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(string.Format("Unable to run process: path={0} arguments={1} exception={2}", path, arguments, ex));
            }

        }
        #endregion

        #region File IO

        private static void GetAssets(string extension, List<string> files, DirectoryInfo directory)
        {
            if (null == directory)
            {
                return;
            }
            foreach (FileInfo file in directory.GetFiles(extension))
            {
                if (string.IsNullOrEmpty(file.FullName) ||
                    files.Contains(file.FullName))
                {
                    continue;
                }
                files.Add(file.FullName);
                //Debug.Log(string.Format("File: {0}", file.FullName));
            }
            foreach (DirectoryInfo subDir in directory.GetDirectories())
            {
                if (null == subDir)
                {
                    continue;
                }
                //Debug.Log(string.Format("Directory: {0}", subDir));
                GetAssets(extension, files, subDir);
            }
        }

        #endregion

        #region Unique identification

        public static string UID = GetUID();

        /// <summary>
        /// Get the machine name
        /// </summary>
        /// <returns></returns>
        private static string GetMachineName()
        {
            try
            {
                string machineName = System.Environment.MachineName;
                if (!string.IsNullOrEmpty(machineName))
                {
                    return machineName;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("GetMachineName: Failed to get machine name");
            }

            return "Unknown";
        }

        /// <summary>
        /// Get the IDE process IDE
        /// </summary>
        /// <returns></returns>
        private static int GetProcessID()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                if (null != process)
                {
                    return process.Id;
                }
            }
            catch
            {
                Debug.LogError("GetProcessID: Failed to get process id");
            }

            return 0;
        }

        /// <summary>
        /// Get a unique identifier for the Unity instance
        /// </summary>
        /// <returns></returns>
        private static string GetUID()
        {
            try
            {
                return string.Format("{0}_{1}", GetMachineName(), GetProcessID());
            }
            catch (System.Exception)
            {
                Debug.LogError("GetUID: Failed to create uid");
            }

            return string.Empty;
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                Debug.Log(string.Format("Delete: {0}", path));
                File.Delete(path);
            }
        }

        private static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Debug.Log(string.Format("Folder Removed: {0}", path));
                Directory.Delete(path, false);
            }
        }

        private static void CleanAARs()
        {
            DeleteFile(pathRazerSDKJar);
            DeleteFile(pathPluginJar);

            DeleteFile(pathClassesJar);

            DeleteFile(pathArmeabiv7aNative);
            DeleteFile(pathArmeabiv7aNativeMeta);
            DeleteFolder(pathArmeabiv7a);
            DeleteFile(pathArmeabiv7aMeta);

            CleanAARsJni();
        }

        private static void CleanAARsJni()
        {
            DeleteFile(pathJniArmeabiv7aNative);
            DeleteFile(pathJniArmeabiv7aNativeMeta);
            DeleteFolder(pathJniArmeabiv7a);
            DeleteFile(pathJniArmeabiv7aMeta);
            DeleteFolder(pathJni);
            DeleteFile(pathJniMeta);
        }

        #endregion
    }
}
