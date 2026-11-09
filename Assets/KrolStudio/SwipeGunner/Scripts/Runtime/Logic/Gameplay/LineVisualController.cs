using System;
using UnityEngine;

namespace KrolStudio
{
    public class LineVisualController : MonoBehaviour
    {
        [Serializable]
        class LineInfo
        {
            public LineRenderer line;
            public Transform startPos;
            public Transform endPos;
        }

        [SerializeField] LineInfo[] lines;

        private void Start()
        {
            foreach (var item in lines)
            {
                item.line.positionCount = 2;
                item.line.SetPosition(0, item.startPos.position);
                item.line.SetPosition(1, item.endPos.position);
            }
        }
    }
}
