using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Data.Save
{
    using Enumerations;

    [Serializable]
    public class SSGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SSStoryStatus StoryStatus { get; set; }
        [field: SerializeField] public bool IsFirstToPlay { get; set; }
        [field: SerializeField] public ConditionSO Condition { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}