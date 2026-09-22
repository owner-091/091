using UnityEngine;

namespace TrainGame
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Journey")]
        [SerializeField] private RouteData route;
        [SerializeField] private TrainMovementController train;

        [Header("Events")]
        [SerializeField] private JourneyEventData[] journeyEvents;

        private int currentStopIndex;

        public int CurrentStopIndex => currentStopIndex;

        private void Start()
        {
            if (route == null || train == null || route.StopCount == 0)
            {
                Debug.LogWarning("[TrainGame] Route or train is not configured.");
                return;
            }

            currentStopIndex = 0;
            train.transform.position = route.GetStop(0).worldPosition;
            Debug.Log($"[TrainGame] Journey begins at {GetStationName(route.GetStop(0))}.");

            Advance();
        }

        [ContextMenu("Advance Journey")]
        public void Advance()
        {
            if (route == null || train == null || train.IsMoving)
                return;

            int nextIndex = currentStopIndex + 1;
            if (nextIndex >= route.StopCount)
            {
                Debug.Log("[TrainGame] Route complete.");
                return;
            }

            RouteStop nextStop = route.GetStop(nextIndex);
            Debug.Log($"[TrainGame] Departing for {GetStationName(nextStop)}.");

            train.MoveTo(nextStop.worldPosition, () => Arrive(nextIndex));
        }

        private void Arrive(int stopIndex)
        {
            currentStopIndex = stopIndex;
            RouteStop stop = route.GetStop(stopIndex);
            Debug.Log($"[TrainGame] Arrived at {GetStationName(stop)}.");

            TriggerJourneyEvent();
        }

        private void TriggerJourneyEvent()
        {
            if (journeyEvents == null || journeyEvents.Length == 0)
            {
                Debug.Log("[TrainGame] No journey event configured. Use 'Advance Journey' to continue.");
                return;
            }

            JourneyEventData selected = journeyEvents[Random.Range(0, journeyEvents.Length)];
            if (selected == null)
                return;

            Debug.Log($"[TrainGame Event] {selected.Title}\n{selected.Description}");

            JourneyChoice[] choices = selected.Choices;
            for (int i = 0; i < choices.Length; i++)
                Debug.Log($"  Choice {i + 1}: {choices[i].label}");
        }

        private static string GetStationName(RouteStop stop)
        {
            return stop.station != null ? stop.station.DisplayName : "Unknown Station";
        }
    }
}
