using System;
using System.Collections;
using UnityEngine;

namespace TrainGame
{
    public sealed class TrainMovementController : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float speed = 4f;

        private Coroutine movementRoutine;

        public bool IsMoving => movementRoutine != null;

        public void MoveTo(Vector3 targetPosition, Action onArrived = null)
        {
            if (movementRoutine != null)
                StopCoroutine(movementRoutine);

            movementRoutine = StartCoroutine(MoveRoutine(targetPosition, onArrived));
        }

        private IEnumerator MoveRoutine(Vector3 targetPosition, Action onArrived)
        {
            while ((transform.position - targetPosition).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    speed * Time.deltaTime);

                yield return null;
            }

            transform.position = targetPosition;
            movementRoutine = null;
            onArrived?.Invoke();
        }
    }
}
