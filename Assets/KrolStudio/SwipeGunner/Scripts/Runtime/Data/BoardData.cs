using System.Collections.Generic;

namespace KrolStudio
{
    [System.Serializable]
    public class BoardData : DictionarySerializeContainer<int, PartSaveData>
    {
        public BoardData(Dictionary<int, PartSaveData> dictionary) : base(dictionary)
        {
        }
    }

    [System.Serializable]
    public class PartSaveData
    {
        public PartType PartType;
        public int Level;
    }
}
