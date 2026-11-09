using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace KrolStudio
{
    public class Level : MonoBehaviour
    {
        [HideInInspector] public List<EnemySpawnPoint> enemySpawnPoints = new();

        [Header("PathSpline")]
        [HideInInspector] public SplineContainer playerPathSpline;

        [Header("EnemySpawnPoint")]
        [HideInInspector][Min(0.1f)] public float sphereRadius = 0.5f;
        [HideInInspector][Min(0.1f)] public float smallSphereRadius = 0.2f;

        public void CreatePathSpline()
        {
            playerPathSpline = new GameObject("PlayerPathSpline").AddComponent<SplineContainer>();
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(playerPathSpline.gameObject, "Create Player Path Spline");   // For Undo in the editor
#endif
            playerPathSpline.transform.SetParent(transform, false);                                     // Set parent to the current object

#if UNITY_EDITOR
            // Refresh scenes to reflect changes
            EditorUtility.SetDirty(this);  // Mark the object as modified
#endif
        }
    }

    [System.Serializable]
    public struct ColorKey
    {
        public float time;   // Time (position along the path from 0 to 1)
        public Color color;  // Color at this time
        public int enemyCount;
    }

    [System.Serializable]
    public class EnemySpawnPoint
    {
        public EnemyType enemyType;
        public ZoneType zoneType;
        
        public int enemyCount;

        public bool enableSpawnGradient;
        public Gradient spawnCurve;
        public float position;

        public float circleRadius;
        public Vector2 rectSize;

        public float startRect;
        public float endRect;
        public float rectWidth;
        [Min(2)] public int numberOfSegments;
        public ColorKey[] colorKeys;

        public float spawnTrigger;
        public float deSpawnTrigger;

        public bool visible = true;

        [HideInInspector] public bool hasSpawned, hasDespawned;

        public float SpawnTrigger => Mathf.Lerp(0f, position, spawnTrigger);
        public float DeSpawnTrigger => Mathf.Lerp(position, 1f, deSpawnTrigger);

        public EnemySpawnPoint(float position, bool visible = true)
        {
            this.position = position;
            
            this.circleRadius = 5f;
            this.rectSize = new Vector2(10f, 10f);
            this.enemyCount = 1;

            this.numberOfSegments = 30;
            this.rectWidth = 10;

            colorKeys = new ColorKey[1];
            colorKeys[0].time = 1f;
            colorKeys[0].color = Color.black;

            this.visible = visible;
        }

        // Get color by normalized progress
        public Color GetColorAt(float t)
        {
            if (colorKeys.Length == 0)
                return Color.white;

            // Check for the first key
            if (t <= colorKeys[0].time)
            {
                return colorKeys[0].color;
            }

            // Interpolation between two adjacent keys
            for (int i = 0; i < colorKeys.Length - 1; i++)
            {
                if (t >= colorKeys[i].time && t <= colorKeys[i + 1].time)
                {
                    return colorKeys[i + 1].color;
                }
            }

            // Check for the last key
            if (t >= colorKeys[colorKeys.Length - 1].time)
            {
                return Color.white;
            }

            // If none of the conditions matched, return the last color
            return colorKeys[colorKeys.Length - 1].color;
        }

        // Function to add a new color key
        public void AddColorKey()
        {
            ColorKey[] newColorKeys = new ColorKey[colorKeys.Length + 1];
            colorKeys.CopyTo(newColorKeys, 0);
            newColorKeys[colorKeys.Length] = new ColorKey { time = 1f, color = Color.white };
            colorKeys = newColorKeys;
        }

        // Function to remove a color key
        public void RemoveColorKey(int index)
        {
            if (colorKeys.Length <= 1) return;

            ColorKey[] newColorKeys = new ColorKey[colorKeys.Length - 1];
            for (int i = 0, j = 0; i < colorKeys.Length; i++)
            {
                if (i != index)
                {
                    newColorKeys[j++] = colorKeys[i];
                }
            }
            colorKeys = newColorKeys;
        }
    }
}