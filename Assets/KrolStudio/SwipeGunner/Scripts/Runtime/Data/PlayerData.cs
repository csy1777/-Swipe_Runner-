using System.Collections.Generic;

namespace KrolStudio
{
    [System.Serializable]
    public class PlayerData : DictionarySerializeContainer<PartType, int>
    {
        public PlayerData(Dictionary<PartType, int> dictionary) : base(dictionary)
        {
        }
    }
}

