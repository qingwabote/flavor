using System;
using UnityEngine;

namespace Flavor
{
    [Serializable]
    public struct ReportEvent
    {
        public string Key;
        public int Type;
        public string ID;
    }

    [CreateAssetMenu(fileName = "ReportEvents", menuName = "Flavor/ReportEvents")]
    public class ReportEvents : ScriptableObject
    {
        public static ReportEvents Instance;

        public ReportEvent[] Events;

        public ReportEvent Get(string key, int type = 0)
        {
            var index = Array.FindIndex(Events, e => e.Key == key && (type == 0 || e.Type == type));
            Debug.Assert(index != -1);
            return Events[index];
        }

        void OnEnable()
        {
            Instance = this;
        }
    }
}