using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace KrolStudio
{
    public class SlideMoverContext
    {
        List<SlideMover> movers = new();

        public void Register(SlideMover slideMover)
        {
            movers.Add(slideMover);
        }

        public async UniTask StartSlide()
        {
            var tasks = new List<UniTask>();
            foreach (var mover in movers)
            {
                tasks.Add(mover.StartSlide());
            }

            await UniTask.WhenAll(tasks);
        }
    }
}