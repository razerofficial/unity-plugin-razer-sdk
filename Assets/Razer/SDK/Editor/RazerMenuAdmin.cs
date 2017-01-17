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
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.razerzone.store.sdk.engine.unity
{
    public class RazerMenuAdmin : MonoBehaviour
    {
        private static Vector3 m_pos = Vector3.zero;
        private static Vector3 m_euler = Vector3.zero;

        [MenuItem("Razer/Open Razer Panel", priority = 100)]
        private static void MenuRazerPanel()
        {
            RazerPanel.GetWindow<RazerPanel>("Razer Panel");
        }

        [MenuItem("Razer/Export Core Package", priority = 100)]
        public static void MenuPackageCore()
        {
            string[] paths =
                {
                "Assets/Razer/SDK",
                "Assets/Plugins/Razer/Bitmap.cs",
                "Assets/Plugins/Razer/BitmapDrawable.cs",
                "Assets/Plugins/Razer/BitmapFactory.cs",
                "Assets/Plugins/Razer/ByteArrayOutputStream.cs",
                "Assets/Plugins/Razer/Controller.cs",
                "Assets/Plugins/Razer/DebugInput.cs",
                "Assets/Plugins/Razer/Drawable.cs",
                "Assets/Plugins/Razer/InputStream.cs",
                "Assets/Plugins/Razer/JniHandleOwnership.cs",
                "Assets/Plugins/Razer/JSONArray.cs",
                "Assets/Plugins/Razer/JSONObject.cs",
                "Assets/Plugins/Razer/OutputStream.cs",
                "Assets/Plugins/Razer/GameMod.cs",
                "Assets/Plugins/Razer/GameModManager.cs",
                "Assets/Plugins/Razer/GameModScreenshot.cs",
                "Assets/Plugins/Razer/RazerSDK.cs",
                "Assets/Plugins/Razer/Plugin.cs",
                "Assets/Plugins/Razer/UnityPlayer.cs",
                "Assets/Plugins/Android/AndroidManifest.xml",
                "Assets/Plugins/Android/libs/store-sdk-standard-release.aar",
                "Assets/Plugins/Android/libs/UnityPluginStoreSDK.aar",
                "Assets/Plugins/Android/res/drawable/app_icon.png",
                "Assets/Plugins/Android/res/drawable/icon.png",
                "Assets/Plugins/Android/res/drawable-xhdpi/ouya_icon.png",
                "Assets/Plugins/Android/res/values/strings.xml",
                "Assets/Plugins/Android/res/values-zh-rCN/strings.xml",
            };
            AssetDatabase.ExportPackage(paths, "Unity-RazerSDK-Core.unitypackage", ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            Debug.Log(string.Format("Export Unity-RazerSDK-Core.unitypackage success in: {0}", Directory.GetCurrentDirectory()));
        }

        [MenuItem("Razer/Export Examples Package", priority = 110)]
        public static void MenuPackageExamples()
        {
            string[] paths =
                {
                "Assets/Razer/Examples",
            };
            AssetDatabase.ExportPackage(paths, "Unity-RazerSDK-Examples.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            Debug.Log(string.Format("Export Unity-RazerSDK-Examples.unitypackage success in: {0}", Directory.GetCurrentDirectory()));
        }

        [MenuItem("Razer/Copy Object Transform", priority = 2000)]
        public static void MenuCopyObjectTransform()
        {
            if (Selection.activeGameObject)
            {
                m_pos = Selection.activeGameObject.transform.position;
                m_euler = Selection.activeGameObject.transform.rotation.eulerAngles;
            }
        }

        [MenuItem("Razer/Unset Symbol (UNITY_EDITOR)", priority = 1000)]
        public static void MenuSymbolUnsetUnityEditor()
        {
            try
            {
                DirectoryInfo pathUnityProject = new DirectoryInfo(Directory.GetCurrentDirectory());
                foreach (FileInfo file in pathUnityProject.GetFiles())
                {
                    if (!file.Extension.ToUpper().Equals(".CSPROJ"))
                    {
                        continue;
                    }
                    //Debug.Log(string.Format("Examine: {0}", file.Name));
                    string content;
                    using (StreamReader sr = new StreamReader(file.FullName))
                    {
                        content = sr.ReadToEnd();
                        content = content.Replace("UNITY_EDITOR;", string.Empty);
                    }
                    using (StreamWriter sw = new StreamWriter(file.FullName))
                    {
                        sw.Write(content);
                        sw.Flush();
                    }
                    Debug.Log(string.Format("Updated: {0}", file.Name));
                }
            }
            catch (System.Exception)
            {

            }
        }

        [MenuItem("Razer/Copy Scene Transform", priority = 2010)]
        public static void MenuCopySceneTransform()
        {
            if (SceneView.currentDrawingSceneView &&
                SceneView.currentDrawingSceneView.camera &&
                SceneView.currentDrawingSceneView.camera.transform)
            {
                m_pos = SceneView.currentDrawingSceneView.camera.transform.position;
                m_euler = SceneView.currentDrawingSceneView.camera.transform.rotation.eulerAngles;
            }
        }

        [MenuItem("Razer/Paste Stored Transform", priority = 2020)]
        public static void MenuSetTransform()
        {
            if (Selection.activeGameObject)
            {
                Selection.activeGameObject.transform.position = m_pos;
                Selection.activeGameObject.transform.rotation = Quaternion.Euler(m_euler);
            }
        }
			
        private static string m_pathSDK = string.Empty;

        private static void UpdatePaths()
        {
            m_pathSDK = EditorPrefs.GetString(RazerPanel.KEY_PATH_ANDROID_SDK);
        }

        private static string GetPathAndroidJar()
        {
            return string.Format("{0}/platforms/android-{1}/android.jar", m_pathSDK, (int)PlayerSettings.Android.minSdkVersion);
        }

        public static void GetAssets(string extension, Dictionary<string, string> files, DirectoryInfo directory)
        {
            if (null == directory)
            {
                return;
            }
            foreach (FileInfo file in directory.GetFiles(extension))
            {
                if (string.IsNullOrEmpty(file.FullName) ||
                    files.ContainsKey(file.FullName.ToLower()))
                {
                    continue;
                }
                files.Add(file.FullName.ToLower(), file.FullName);
            }
            foreach (DirectoryInfo subDir in directory.GetDirectories())
            {
                if (null == subDir)
                {
                    continue;
                }
                if (subDir.Name.ToUpper().Equals(".SVN"))
                {
                    continue;
                }
                //Debug.Log(string.Format("Directory: {0}", subDir));
                GetAssets(extension, files, subDir);
            }
        }
    }
}
