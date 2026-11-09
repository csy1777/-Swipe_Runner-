
namespace KrolStudio
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System.Collections.Generic;

    public static class RaycastUtility
    {
        /// <summary>
        /// Performs a Raycast from the mouse position and returns objects on specified layers.
        /// </summary>
        /// <param name="results">List of objects found on the specified layers.</param>
        /// <param name="layerMask">Layers to filter the Raycast.</param>
        /// <returns>True if objects were found on these layers.</returns>
        public static bool RaycastFromMouse(out List<RaycastResult> results, LayerMask layerMask)
        {
            results = new List<RaycastResult>();

            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem.current is null.");
                return false;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> allResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, allResults);

            // Filtering by LayerMask
            foreach (var result in allResults)
            {
                int objectLayer = result.gameObject.layer;
                if (((1 << objectLayer) & layerMask) != 0)
                {
                    results.Add(result);
                }
            }

            return results.Count > 0;
        }
    }

}