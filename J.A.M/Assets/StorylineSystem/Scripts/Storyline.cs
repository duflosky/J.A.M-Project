﻿using System.Collections.Generic;

namespace SS
{
    using Enumerations;
    using ScriptableObjects;

    [System.Serializable]
    public class Storyline
    {
        public string ID;
        public SSNodeContainerSO StorylineContainer;
        public SSStoryStatus Status;
        public List<Timeline> Timelines;

        public Storyline(SSNodeContainerSO storylineContainer, List<SSNodeGroupSO> timelines)
        {
            ID = storylineContainer.ID;
            StorylineContainer = storylineContainer;
            Status = storylineContainer.StoryStatus;
            Timelines = new List<Timeline>();
            for (int index = 0; index < timelines.Count; index++)
            {
                Timelines.Add(new Timeline(timelines[index]));
            }
        }
    }
}