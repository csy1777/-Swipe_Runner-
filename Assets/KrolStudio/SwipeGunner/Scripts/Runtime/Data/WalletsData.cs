using System.Collections.Generic;

namespace KrolStudio
{
    [System.Serializable]
    public class WalletsData : DictionarySerializeContainer<int, long>
    {
        public WalletsData(Dictionary<int, long> dictionary) : base(dictionary)
        {
        }
    }
}