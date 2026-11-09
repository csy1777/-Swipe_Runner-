using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KrolStudio
{
    public class IntegrationHelper : EditorWindow
    {
        private int selectedNetworkIndex = 0;
        
        private string[] networkDisplayNames  = new string[] { "Admob", "Unity Ads", "LevelPlay"};

        private string[] networkSetupUrls  = new string[] 
        {
            "https://developers.google.com/admob/unity/quick-start",
            "https://unityads.unity3d.com/help/unity/integration-guide-unity",
            "https://developers.is.com/ironsource-mobile/unity/unity-plugin/#step-1"
        };
        
        [MenuItem("Tools/Monetization/Integration", false, 0)]
        public static void OpenGettingStartedWindow()
        {
            IntegrationHelper window = GetWindow<IntegrationHelper>(true, "Ad Integration", true);
            window.ShowUtility();
        }
        
        private void OnGUI()
        {
            GUILayout.Space(10);
            DrawMobileAdsIntegration();
        }
        
        private Dictionary<string, string> networkUnityPackages = new Dictionary<string, string>()
        {
            { "Admob", "Assets/KrolStudio/Plugins/MonetizationPackages/AdmobImplementation.unitypackage" },
            { "LevelPlay", "Assets/KrolStudio/Plugins/MonetizationPackages/LevelPlayImplementation.unitypackage" },
            { "Unity Ads", "Assets/KrolStudio/Plugins/MonetizationPackages/UnityAdsImplementation.unitypackage" }
        };

        private void DrawMobileAdsIntegration()
        {
            selectedNetworkIndex = EditorGUILayout.Popup("Choose Ad", selectedNetworkIndex, networkDisplayNames );

            bool isAdMob = selectedNetworkIndex == 0;

            GUILayout.Label($"Step 1: Download {networkDisplayNames [selectedNetworkIndex]} SDK");
            DrawDownloadSdkButton();

            GUILayout.Space(10);

            if (isAdMob)
            {
                DrawAdMobSteps();
            }
            else
            {
                DrawGenericNetworkSteps();
            }

            GUILayout.Space(10);

            if (RequiresGdpr(selectedNetworkIndex))
            {
                if (GUILayout.Button("Install Google Mobile Ads SDK (GDPR / Consent)"))
                {
                    Application.OpenURL("https://developers.google.com/admob/unity/quick-start");
                }
            }
        }

        private void DrawDownloadSdkButton()
        {
            if (!GUILayout.Button($"Download {networkDisplayNames [selectedNetworkIndex]} SDK"))
                return;

            if (selectedNetworkIndex == 1)
            {
                OpenPackageManagerAndSearch();
                return;
            }

            if (selectedNetworkIndex >= 0 && selectedNetworkIndex < networkSetupUrls .Length)
            {
                Application.OpenURL(networkSetupUrls [selectedNetworkIndex]);
            }
            else
            {
                Debug.LogWarning("Please choose an Ad Network.");
            }
        }

        private void DrawAdMobSteps()
        {
            GUILayout.Label("Step 2: Download Admob Native SDK");

            if (GUILayout.Button("Download Admob Native SDK"))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/native");
            }

            GUILayout.Label("Step 3: Download Adapters For Mediation (Ignore this step if you only want to use Admob)");

            if (GUILayout.Button("Download Adapters"))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/choose-networks");
            }

            GUILayout.Label("Step 4: Import Mobile Monetization Pro Required Scripts");

            if (GUILayout.Button("Import Required Scripts"))
            {
                ImportRequiredScripts();
            }
        }

        private void DrawGenericNetworkSteps()
        {
            GUILayout.Label("Step 2: Import Mobile Monetization Pro Required Scripts");

            if (GUILayout.Button("Import Required Scripts"))
            {
                ImportRequiredScripts();
            }
        }

        private void ImportRequiredScripts()
        {
            string network = networkDisplayNames [selectedNetworkIndex];

            if (networkUnityPackages.TryGetValue(network, out var packagePath))
            {
                AssetDatabase.ImportPackage(packagePath, true);
                Debug.Log($"Imported {network} Unity package.");
            }
            else
            {
                Debug.LogWarning($"No Unity package found for {network}.");
            }
        }

        private bool RequiresGdpr(int networkIndex)
        {
            return networkIndex == 0 || networkIndex == 2 || networkIndex == 3;
        }

        private static void OpenPackageManagerAndSearch()
        {
            EditorApplication.ExecuteMenuItem("Window/Package Management/Package Manager");
        }
    }
} 