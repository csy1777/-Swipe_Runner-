using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace KrolStudio
{
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor
    {
        private const int CircleSegments = 32;

        private Level map;

        public void OnEnable()
        {
            map = target as Level;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            map.playerPathSpline = (SplineContainer)EditorGUILayout.ObjectField("Player path spline", map.playerPathSpline, typeof(SplineContainer), true);
            if (GUILayout.Button("Create PlayerPathSpline"))
                map.CreatePathSpline();

            EditorGUILayout.Space();

            map.sphereRadius = EditorGUILayout.FloatField("Sphere radius", map.sphereRadius);
            map.smallSphereRadius = EditorGUILayout.FloatField("Small sphere radius", map.smallSphereRadius);

            EditorGUILayout.Space();

            DrawEnemySpawnPointsGUI();

            SetMapDirty();
        }

        // Function for creating a texture used for the background
        static private Texture2D MakeTexture(int width, int height, Color color)
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

        private void DrawEnemySpawnPointsGUI()
        {
            if (map.enemySpawnPoints != null)
            {
                int toremove = -1;

                // Create a style for the border and background
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = MakeTexture(2, 2, new Color(0.8f, 0.8f, 0.8f));    // Light gray background
                boxStyle.normal.textColor = Color.black;                                        // Black text (if needed)
                boxStyle.border = new RectOffset(1, 1, 1, 1);                                   // Border around the block

                float fieldWidth = 160;

                for (int i = 0; i < map.enemySpawnPoints.Count; ++i)
                {
                    // Start a vertical block with a border
                    EditorGUILayout.BeginVertical(boxStyle);

                    #region EnemyType
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enemy type", GUILayout.Width(fieldWidth));
                    map.enemySpawnPoints[i].enemyType = (EnemyType)EditorGUILayout.EnumPopup(map.enemySpawnPoints[i].enemyType);

                    // Visibility icon
                    GUIContent eyeIcon = map.enemySpawnPoints[i].visible ? EditorGUIUtility.IconContent("d_VisibilityOn") : EditorGUIUtility.IconContent("d_VisibilityOff");

                    if (GUILayout.Button(eyeIcon, GUILayout.MaxWidth(32)))
                    {
                        map.enemySpawnPoints[i].visible = !map.enemySpawnPoints[i].visible;
                    }

                    // Button for deletion
                    GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
                    // Create a texture for the background
                    Texture2D redTexture = MakeTexture(20, 20, Color.red);
                    // Set the background for the button
                    redButtonStyle.normal.background = redTexture;  // For the normal state
                    redButtonStyle.active.background = redTexture;  // For the active state (when pressed)

                    if (GUILayout.Button("-", redButtonStyle, GUILayout.MaxWidth(32)))
                    {
                        toremove = i;
                    }

                    GUILayout.EndHorizontal();
                    #endregion

                    // Prematurely end the vertical block with a border
                    // The same logic applies at the end of the loop
                    if (!map.enemySpawnPoints[i].visible)
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                        continue;
                    }

                    #region EnemyCount
                    if (map.enemySpawnPoints[i].zoneType != ZoneType.RectAlong || !map.enemySpawnPoints[i].enableSpawnGradient)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Enemy count", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].enemyCount = EditorGUILayout.IntField(map.enemySpawnPoints[i].enemyCount);
                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region PositionOnSpline
                    if (map.enemySpawnPoints[i].zoneType != ZoneType.RectAlong)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Position on spline", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].position = EditorGUILayout.Slider(map.enemySpawnPoints[i].position, 0f, 1f);
                        GUILayout.EndHorizontal();
                    }               
                    #endregion

                    #region SpawnTriggerPosition
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Spawn trigger position", GUILayout.Width(fieldWidth));
                    map.enemySpawnPoints[i].spawnTrigger = EditorGUILayout.Slider(map.enemySpawnPoints[i].spawnTrigger, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region DespawnTriggerPosition
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Despawn trigger position", GUILayout.Width(fieldWidth));
                    map.enemySpawnPoints[i].deSpawnTrigger = EditorGUILayout.Slider(map.enemySpawnPoints[i].deSpawnTrigger, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region ZoneType
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Zone Type", GUILayout.Width(fieldWidth));
                    map.enemySpawnPoints[i].zoneType = (ZoneType)EditorGUILayout.EnumPopup(map.enemySpawnPoints[i].zoneType);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region EnableSpawnGradient
                    if (map.enemySpawnPoints[i].zoneType == ZoneType.RectAlong)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Enable spawn gradient", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].enableSpawnGradient = EditorGUILayout.Toggle(map.enemySpawnPoints[i].enableSpawnGradient);
                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region CustomGradient
                    if (map.enemySpawnPoints[i].zoneType == ZoneType.RectAlong && map.enemySpawnPoints[i].enableSpawnGradient)
                    {
                        DrawGradient(map.enemySpawnPoints[i]);
                    }
                    EditorGUILayout.Space();
                    #endregion

                    if (map.enemySpawnPoints[i].zoneType == ZoneType.Circle)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Spawn zone radius", GUILayout.Width(fieldWidth)); 
                        map.enemySpawnPoints[i].circleRadius = EditorGUILayout.FloatField("", map.enemySpawnPoints[i].circleRadius);
                        GUILayout.EndHorizontal();
                    }
                    else if(map.enemySpawnPoints[i].zoneType == ZoneType.Rect)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Spawn zone size", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].rectSize = EditorGUILayout.Vector2Field("", map.enemySpawnPoints[i].rectSize);
                        GUILayout.EndHorizontal();
                    }
                    else if (map.enemySpawnPoints[i].zoneType == ZoneType.RectAlong)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Start rect", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].startRect = EditorGUILayout.Slider(map.enemySpawnPoints[i].startRect, 0.0f, 1.0f);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("End rect", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].endRect = EditorGUILayout.Slider(map.enemySpawnPoints[i].endRect, 0.0f, 1.0f);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Number of segments", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].numberOfSegments = EditorGUILayout.IntField("", map.enemySpawnPoints[i].numberOfSegments);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Rect width", GUILayout.Width(fieldWidth));
                        map.enemySpawnPoints[i].rectWidth = EditorGUILayout.FloatField("", map.enemySpawnPoints[i].rectWidth);
                        GUILayout.EndHorizontal();
                    }

                    // End the vertical block with a border
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                // Delete the object if needed
                if (toremove != -1)
                    map.enemySpawnPoints.RemoveAt(toremove);
            }

            // Button for adding a new spawn point
            if (GUILayout.Button("Add enemy spawn point"))
            {
                map.enemySpawnPoints.Add(new EnemySpawnPoint(0.5f));
            }
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        public static void DrawCustomGizmo(Level map, GizmoType gizmoType)
        {
            DrawEnemySpawnPoints(map);
        }

        public static void DrawEnemySpawnPoints(Level map)
        {
            if (map.playerPathSpline == null || map.playerPathSpline.Spline == null || map.enemySpawnPoints == null)
                return;

            Color color = Gizmos.color;

            float3 pos, tangent, up;
            Vector3 worldPos, worldTangent, worldUp;

            string iconPath = "Assets/KrolStudio/SwipeGunner/Scripts/Editor/";

            foreach (var pointData in map.enemySpawnPoints)
            {
                if (pointData.visible)
                {
                    Gizmos.color = new Color(0, 158f/255f, 1f);                 
                    CalculateOrientation(pointData.spawnTrigger);
                    iconPath += "Icons/Enter.png";
                    Gizmos.DrawIcon(worldPos + Vector3.up * 1f, iconPath, true); // 'true' means the icon will rotate with the object
                    Gizmos.DrawSphere(worldPos, map.smallSphereRadius);

                    Gizmos.color = Color.red;                  
                    CalculateOrientation(pointData.deSpawnTrigger);
                    iconPath = "Icons/Exit.png";
                    Gizmos.DrawIcon(worldPos + Vector3.up * 1f, iconPath, true); // 'true' means the icon will rotate with the object
                    Gizmos.DrawSphere(worldPos, map.smallSphereRadius);

                    Gizmos.color = Color.green;
                    
                    if(pointData.zoneType != ZoneType.RectAlong)
                    {
                        CalculateOrientation(pointData.position);
                        Gizmos.DrawSphere(worldPos, map.sphereRadius);
                    }

                    if (pointData.zoneType == ZoneType.Circle)
                    {
                        DrawCircle(worldPos, pointData.circleRadius, worldTangent, worldUp, Color.green);
                    }
                    else if (pointData.zoneType == ZoneType.Rect)
                    {
                        DrawRectangle(worldPos, pointData.rectSize, worldTangent, worldUp);
                    }
                    else if (pointData.zoneType == ZoneType.RectAlong)
                    {
                        DrawRectangleAlongSpline(pointData, map);
                    }
                }
            }

            Gizmos.color = color;

            void CalculateOrientation(float t)
            {
                SplineUtility.Evaluate(map.playerPathSpline.Spline, t, out pos, out tangent, out up);

                worldPos = map.playerPathSpline.transform.TransformPoint(pos);
                worldTangent = map.playerPathSpline.transform.TransformDirection(tangent);
                worldUp = map.playerPathSpline.transform.TransformDirection(up);
            }
        }

        static private void DrawGradient(EnemySpawnPoint enemySpawn)
        {
            // Display the texture (gradient) using EditorGUI
            Texture2D gradientTexture = GenerateGradientTexture(enemySpawn);
            Rect textureRect = GUILayoutUtility.GetRect(300, 60);  // Set the size for the texture
            EditorGUI.DrawPreviewTexture(textureRect, gradientTexture);

            // Display the interface for editing color keys
            EditorGUILayout.LabelField("Color Keys", EditorStyles.boldLabel);
            for (int i = 0; i < enemySpawn.colorKeys.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Time", GUILayout.Width(32));
                enemySpawn.colorKeys[i].time = EditorGUILayout.Slider("", enemySpawn.colorKeys[i].time, 0f, 1f);

                EditorGUILayout.LabelField("Color", GUILayout.Width(32));
                enemySpawn.colorKeys[i].color = EditorGUILayout.ColorField("", enemySpawn.colorKeys[i].color, GUILayout.Width(50));

                EditorGUILayout.LabelField("EnemyCount", GUILayout.Width(75));
                enemySpawn.colorKeys[i].enemyCount = EditorGUILayout.IntField("", enemySpawn.colorKeys[i].enemyCount, GUILayout.Width(50));
       
                GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);    // Button for deleting a key
                Texture2D redTexture = MakeTexture(20, 20, Color.red);      // Create a texture for the background
                // Set the background for the button
                redButtonStyle.normal.background = redTexture;  // For the normal state
                redButtonStyle.active.background = redTexture;  // For the active state (when pressed)

                if (GUILayout.Button("-", redButtonStyle, GUILayout.MaxWidth(32)))
                {
                    enemySpawn.RemoveColorKey(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Color Key"))
            {
                enemySpawn.AddColorKey();
            }
        }

        // Generating gradient texture based on keys
        static private Texture2D GenerateGradientTexture(EnemySpawnPoint enemySpawn)
        {
            int width = 256; // Texture width
            Texture2D texture = new Texture2D(width, 1);
            Color[] colors = new Color[width];

            // Fill the color array based on the gradient color keys
            for (int i = 0; i < width; i++)
            {
                float time = i / (float)(width - 1);
                colors[i] = enemySpawn.GetColorAt(time);  // Get the color for this time
            }

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
        
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------

        static void DrawRectangleAlongSpline(EnemySpawnPoint pointData, Level map)
        {
            float3 pos, tangent, up;
            Vector3 worldPos, worldTangent, worldUp;

            List<Vector3> points = new(pointData.numberOfSegments * 2);

            float time;
            // Middle lines
            for (int i = 0; i < pointData.numberOfSegments; i++)
            {
                time = i / (float)(pointData.numberOfSegments - 1);

                if(pointData.enableSpawnGradient)
                    Gizmos.color = pointData.GetColorAt(time);  // Get color for this time

                CalculateOrientation(Mathf.Lerp(pointData.startRect, pointData.endRect, time));
                DrawLineTest(worldPos, pointData.rectWidth, worldTangent, worldUp, points);
            }

            if (pointData.enableSpawnGradient)
                Gizmos.color = Color.white;

            // Side lines
            int iter = points.Count / 2 - 1;

            for (int i = 0, k = 0; i < iter; i++, k += 2)
            {
                float t = (float)i / (iter - 1);

                Gizmos.DrawLine(points[k], points[k + 2]);
                Gizmos.DrawLine(points[k + 1], points[k + 3]);
            }

            void CalculateOrientation(float t)
            {
                SplineUtility.Evaluate(map.playerPathSpline.Spline, t, out pos, out tangent, out up);

                worldPos = map.playerPathSpline.transform.TransformPoint(pos);
                worldTangent = map.playerPathSpline.transform.TransformDirection(tangent);
                worldUp = map.playerPathSpline.transform.TransformDirection(up);
            }
        }
        
        static void DrawLineTest(Vector3 center, float width, Vector3 tangent, Vector3 up, List<Vector3> points)
        {
            Quaternion rotation = Quaternion.LookRotation(tangent, up);
            Vector3 halfWidth = rotation * new Vector3(width * 0.5f, 0f, 0f);

            points.Add(center + halfWidth);
            points.Add(center - halfWidth);

            Gizmos.DrawLine(center + halfWidth, center - halfWidth);
        }

        static void DrawRectangle(Vector3 center, Vector2 size, Vector3 tangent, Vector3 up)
        {
            // Calculate orientation for correct rectangle placement
            Quaternion rotation = Quaternion.LookRotation(tangent, up);

            // Find the 4 vertices of the rectangle relative to the center
            Vector3 halfWidth = rotation * new Vector3(size.x * 0.5f, 0f, 0f);
            Vector3 halfHeight = rotation * new Vector3(0f, 0f, size.y * 0.5f);

            // Vertical corners of the rectangle
            Vector3 topLeft = center + halfHeight + halfWidth;
            Vector3 topRight = center + halfHeight - halfWidth;
            Vector3 bottomLeft = center - halfHeight + halfWidth;
            Vector3 bottomRight = center - halfHeight - halfWidth;

            // Draw lines between points to create the rectangle
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        static void DrawCircle(Vector3 center, float radius, Vector3 tangent, Vector3 up, Color color)
        {
            Color currentColor = Gizmos.color;

            Gizmos.color = color;

            const int CircleSegments = 32;
            float angleStep = 360f / CircleSegments;

            Quaternion rotation = Quaternion.LookRotation(tangent, up);
            Vector3 prevPoint = center + rotation * new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)) * radius;

            for (int i = 1; i <= CircleSegments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 nextPoint = center + rotation * new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            Gizmos.color = currentColor;
        }
        
        private void SetMapDirty()
        {
            if (GUI.changed)
                EditorUtility.SetDirty(map);
        }
    }
}
