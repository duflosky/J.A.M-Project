using System.Collections.Generic;
using UnityEngine;

namespace SS.Data.Save
{
    using Enumerations;

    public class SSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SSStoryStatus StoryStatus { get; set; }
        [field: SerializeField] public List<SSGroupSaveData> Groups { get; set; }
        [field: SerializeReference] public List<SSNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName, SSStoryStatus storyStatus)
        {
            FileName = fileName;
            StoryStatus = storyStatus;
            Groups = new List<SSGroupSaveData>();
            Nodes = new List<SSNodeSaveData>();
        }
    }
}