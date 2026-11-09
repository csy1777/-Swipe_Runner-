using System;
using System.Collections.Generic;

namespace KrolStudio
{
    public class MainThreadEventDispatcher
    {
        private List<Action> events = new List<Action>();
        private int eventCount;

        public void Enqueue(Action action)
        {
            if (action != null)
            {
                events.Add(action);
                eventCount++;
            }
        }

        public void ProcessEvents()
        {
            if (eventCount > 0)
            {
                for (int i = 0; i < eventCount; i++)
                {
                    events[i]?.Invoke();
                }

                events.Clear();
                eventCount = 0;
            }
        }
    }
}
