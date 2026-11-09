using UnityEditor;
using UnityEngine;

namespace KrolStudio
{
    [CustomEditor(typeof(AdsConfig))]
    public class AdsConfigEditor : Editor
    {
        private SerializedProperty isModuleActive;
        private SerializedProperty logging;
        private SerializedProperty debugMode;

        private SerializedProperty bannerType;
        private SerializedProperty interstitialType;
        private SerializedProperty rewardedVideoType;

        private SerializedProperty adMobContainer;
        private SerializedProperty unityAdsContainer;
        private SerializedProperty levelPlayContainer;

        private SerializedProperty loadAdsOnStart;
        private SerializedProperty interstitialFirstStartDelay;
        private SerializedProperty interstitialStartDelay;
        private SerializedProperty interstitialShowingDelay;
        private SerializedProperty autoShowInterstitial;

       // private SerializedProperty isGDPREnabled;
      //  private SerializedProperty gdprPanelPrefab;

        private SerializedProperty consentDataContainer;

        private SerializedProperty isIDFAEnabled;
        private SerializedProperty trackingDescription;

        private bool showAdMob;
        private bool showUnityAds;

        private GUIStyle boxStyle;
        private GUIStyle sectionHeaderStyle;

        private void OnEnable()
        {
            isModuleActive = serializedObject.FindProperty("<IsModuleActive>k__BackingField");
            logging = serializedObject.FindProperty("<Logging>k__BackingField");
            debugMode = serializedObject.FindProperty("<DebugMode>k__BackingField");

            bannerType = serializedObject.FindProperty("bannerType");
            interstitialType = serializedObject.FindProperty("interstitialType");
            rewardedVideoType = serializedObject.FindProperty("rewardedVideoType");

            adMobContainer = serializedObject.FindProperty("adMobContainer");
            unityAdsContainer = serializedObject.FindProperty("unityAdsContainer");
            levelPlayContainer = serializedObject.FindProperty("levelPlayContainer");

            loadAdsOnStart = serializedObject.FindProperty("loadAdsOnStart");
            interstitialFirstStartDelay = serializedObject.FindProperty("interstitialFirstStartDelay");
            interstitialStartDelay = serializedObject.FindProperty("interstitialStartDelay");
            interstitialShowingDelay = serializedObject.FindProperty("interstitialShowingDelay");
            autoShowInterstitial = serializedObject.FindProperty("autoShowInterstitial");

            //isGDPREnabled = serializedObject.FindProperty("isGDPREnabled");
            //gdprPanelPrefab = serializedObject.FindProperty("gdprPanelPrefab");


            consentDataContainer = serializedObject.FindProperty("consentDataContainer");

            isIDFAEnabled = serializedObject.FindProperty("isIDFAEnabled");
            trackingDescription = serializedObject.FindProperty("trackingDescription");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0.8f, 0.8f, 0.8f));    // Light gray background
            boxStyle.normal.textColor = Color.black;                                        // Black text (if needed)
            boxStyle.border = new RectOffset(1, 1, 1, 1);                                   // Border around the block

            // Styles
            sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
            };
            sectionHeaderStyle.normal.textColor = Color.green;

            DrawModuleToggle();

            if (isModuleActive.boolValue)
            {
                EditorGUILayout.Space(8);

                EditorGUILayout.BeginVertical(boxStyle);
                DrawGeneralSettings();
                GUILayout.EndVertical();

                EditorGUILayout.BeginVertical(boxStyle);
                DrawAdTypes();
                GUILayout.EndVertical();

                EditorGUILayout.BeginVertical(boxStyle);
                DrawProviders();
                GUILayout.EndVertical();

                EditorGUILayout.BeginVertical(boxStyle);
                DrawAdSettings();
                GUILayout.EndVertical();

                EditorGUILayout.BeginVertical(boxStyle);
                DrawPrivacySettings();
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }
            texture.SetPixels(pix);
            texture.Apply();
            return texture;
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField(title, sectionHeaderStyle);
            EditorGUILayout.EndVertical();

            GUILayout.Space(4);
        }

        // ------------------------------------------------------
        private void DrawModuleToggle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(isModuleActive, new GUIContent("Ads Module Enabled"));
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralSettings()
        {
            DrawSectionHeader("General");

            EditorGUILayout.PropertyField(logging);
            EditorGUILayout.PropertyField(debugMode);
            EditorGUILayout.Space();
        }

        private void DrawAdTypes()
        {
            DrawSectionHeader("Ad Types");
            EditorGUILayout.PropertyField(bannerType);
            EditorGUILayout.PropertyField(interstitialType);
            EditorGUILayout.PropertyField(rewardedVideoType);
            EditorGUILayout.Space();
        }

        private void DrawProviders()
        {
            DrawSectionHeader("Providers");

            EditorGUILayout.PropertyField(unityAdsContainer, true);
            UnityAdsSpecialButtons();

            EditorGUILayout.PropertyField(adMobContainer, true);
            AdmobSpecialButtons();

            EditorGUILayout.PropertyField(levelPlayContainer, true);
            LevelPlaySpecialButtons();

            EditorGUILayout.Space();
        }

        void LevelPlaySpecialButtons()
        {
            if (GUILayout.Button("Download Ads Mediation"))
            {
                OpenPackageManagerAndSearch();
            }
            
            if (GUILayout.Button("Import Required Scripts"))
            {
                AssetDatabase.ImportPackage("Assets/KrolStudio/Plugins/MonetizationPackages/LevelPlayImplementation.unitypackage", true);
            }

            if (GUILayout.Button("Download Mobile Ads SDK Unity Plugin"))
            {
                Application.OpenURL(@"https://github.com/googleads/googleads-mobile-unity/releases");
            }

            GUILayout.Space(8);
        }

        void UnityAdsSpecialButtons()
        {
            GUILayout.Space(8);

            if (GUILayout.Button("Download Advertisement Legacy"))
            {
                OpenPackageManagerAndSearch();
            }

            if (GUILayout.Button("Import Required Scripts"))
            {
                AssetDatabase.ImportPackage("Assets/KrolStudio/Plugins/MonetizationPackages/UnityAdsImplementation.unitypackage", true);
            }

            if (GUILayout.Button("Unity Ads Dashboard"))
            {
                Application.OpenURL(@"https://operate.dashboard.unity3d.com");
            }

            if (GUILayout.Button("Unity Ads Quick Start Guide"))
            {
                Application.OpenURL(@"https://unityads.unity3d.com/help/monetization/getting-started");
            }

            GUILayout.Space(8);
        }

        private static void OpenPackageManagerAndSearch()
        {
            EditorApplication.ExecuteMenuItem("Window/Package Management/Package Manager");
        }

        void AdmobSpecialButtons()
        {
            if (GUILayout.Button("Download AdMob plugin"))
            {
                Application.OpenURL(@"https://github.com/googleads/googleads-mobile-unity/releases");
            }

            if (GUILayout.Button("Import Required Scripts"))
            {
                AssetDatabase.ImportPackage("Assets/KrolStudio/Plugins/MonetizationPackages/AdmobImplementation.unitypackage", true);
            }

            if (GUILayout.Button("AdMob Dashboard"))
            {
                Application.OpenURL(@"https://apps.admob.com/v2/home");
            }

            if (GUILayout.Button("AdMob Quick Start Guide"))
            {
                Application.OpenURL(@"https://developers.google.com/admob/unity/start");
            }

            GUILayout.Space(8);
        }

        private void DrawAdSettings()
        {
            DrawSectionHeader("Ad Behaviour");

            EditorGUILayout.PropertyField(loadAdsOnStart);
            EditorGUILayout.PropertyField(autoShowInterstitial);

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(interstitialFirstStartDelay);
            EditorGUILayout.PropertyField(interstitialStartDelay);
            EditorGUILayout.PropertyField(interstitialShowingDelay);

            EditorGUILayout.Space();
        }

        private void DrawPrivacySettings()
        {
            DrawSectionHeader("Privacy");

            //EditorGUILayout.PropertyField(isGDPREnabled);

            //using (new EditorGUI.DisabledScope(!isGDPREnabled.boolValue))
            //{
            //    EditorGUILayout.PropertyField(gdprPanelPrefab);
            //}
            //EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(consentDataContainer);


            EditorGUILayout.PropertyField(isIDFAEnabled);
            using (new EditorGUI.DisabledScope(!isIDFAEnabled.boolValue))
            {
                EditorGUILayout.PropertyField(trackingDescription);
            }
            EditorGUILayout.Space(4);
        }
    }
}