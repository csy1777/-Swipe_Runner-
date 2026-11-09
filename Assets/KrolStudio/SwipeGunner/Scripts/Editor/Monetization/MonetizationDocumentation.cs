using UnityEngine;
using UnityEditor;

namespace KrolStudio
{
    public class MonetizationDocumentation : EditorWindow
    {
        [MenuItem("Tools/Monetization/Documentation", false, 6)]
        public static void OpenDocumentation()
        {
            string documentationLink = "https://app.gitbook.com/o/ERmTZkISe4sXvWLWVVEl/s/OwQZwyXuabSq5AaeJsW0/monetization";
            Application.OpenURL(documentationLink);
        }        
    }
}
