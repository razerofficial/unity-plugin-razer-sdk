using UnityEngine;

namespace com.razerzone.store.sdk.engine.unity
{
    public class ExampleSafeArea : MonoBehaviour
    {
        private float _percentage = 1f;

        void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Height(Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Use DPAD LEFT and RIGHT to adjust the Safe Area");
            GUILayout.HorizontalSlider(_percentage, 0f, 1f, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void Update()
        {
            float percentage = _percentage;
            if (RazerSDK.ControllerInput.GetButtonDown(Controller.BUTTON_DPAD_LEFT))
            {
                percentage = Mathf.Max(0f, _percentage - 0.1f);
            }
            else if (RazerSDK.ControllerInput.GetButtonDown(Controller.BUTTON_DPAD_RIGHT))
            {
                percentage = Mathf.Min(1f, _percentage + 0.1f);
            }
            if (percentage != _percentage)
            {
                _percentage = percentage;
                RazerSDK.setSafeArea(percentage);
            }
        }
#endif
    }
}
