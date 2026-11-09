using UnityEngine;

namespace KrolStudio
{
    [ExecuteAlways]
    public class GLLineRenderer : MonoBehaviour
    {
        public Material lineMaterial;
        public int lineCount = 1000;
        public float spacing = 0.1f;

        private void OnRenderObject()
        {
            if (lineMaterial == null)
            {
                Debug.LogWarning("Line material not assigned.");
                return;
            }

            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);

            for (int i = 0; i < lineCount; i++)
            {
                float x = i * spacing;

                GL.Color(Color.Lerp(Color.red, Color.blue, i / (float)lineCount));
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, 1, 0);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}

/*
 Shader "Hidden/GLLineShader"
{
    SubShader
    {
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Lighting Off
            Fog { Mode Off }
            BindChannels
            {
                Bind "vertex", vertex
                Bind "color", color
            }
        }
    }
}
 
 */
