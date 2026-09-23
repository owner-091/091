using System;
using UnityEngine;

namespace TrainGame
{
    [Serializable]
    public struct RouteStop
    {
        public StationData station;
        public Vector3 worldPosition;
    }

    [CreateAssetMenu(menuName = "Train Game/Route", fileName = "Route")]
    public sealed class RouteData : ScriptableObject
    {
        [SerializeField] private RouteStop[] stops = Array.Empty<RouteStop>();

        public int StopCount => stops?.Length ?? 0;

        public RouteStop GetStop(int index)
        {
            if (stops == null || index < 0 || index >= stops.Length)
                throw new IndexOutOfRangeException($"Route stop index {index} is invalid.");

            return stops[index];
        }
    }
}
