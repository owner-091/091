using System;
using UnityEngine;

namespace TrainGame
{
    [Serializable]
    public struct JourneyChoice
    {
        public string label;
        [TextArea] public string resultText;
    }

    [CreateAssetMenu(menuName = "Train Game/Journey Event", fileName = "JourneyEvent")]
    public sealed class JourneyEventData : ScriptableObject
    {
        [SerializeField] private string title = "Untitled Event";
        [SerializeField, TextArea(3, 8)] private string description;
        [SerializeField] private JourneyChoice[] choices = Array.Empty<JourneyChoice>();

        public string Title => title;
        public string Description => description;
        public JourneyChoice[] Choices => choices;
    }
}
