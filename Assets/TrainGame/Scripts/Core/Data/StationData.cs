using UnityEngine;

namespace TrainGame
{
    [CreateAssetMenu(menuName = "Train Game/Station", fileName = "Station")]
    public sealed class StationData : ScriptableObject
    {
        [SerializeField] private string stationId = "station";
        [SerializeField] private string displayName = "Unnamed Station";

        public string StationId => stationId;
        public string DisplayName => displayName;
    }
}
