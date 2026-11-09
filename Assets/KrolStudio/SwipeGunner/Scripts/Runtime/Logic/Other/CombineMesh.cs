using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace KrolStudio
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CombineMesh : MonoBehaviour
    {
        void Start()
        {
            Vector3 originalPosition = transform.position;
            Quaternion originalRotation = transform.rotation;
            Vector3 originalScale = transform.localScale;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
            List<CombineInstance> combines = new List<CombineInstance>();

            foreach (MeshFilter mf in filters)
            {
                if (mf.transform == transform)
                    continue;

                if (mf.sharedMesh == null)
                    continue;

                CombineInstance ci = new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    transform = mf.transform.localToWorldMatrix
                };
                combines.Add(ci);

                mf.gameObject.SetActive(false);
            }

            Mesh mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };

            mesh.CombineMeshes(combines.ToArray(), mergeSubMeshes: true, useMatrices: true);

            GetComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.SetActive(true);

            transform.position = originalPosition;
            transform.rotation = originalRotation;
            transform.localScale = originalScale;
        }
    }
}