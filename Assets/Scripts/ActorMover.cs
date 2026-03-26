using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class ActorMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool rotateToDirection = true;

    public bool IsMoving { get; private set; }

    public IEnumerator FollowPath(List<TileNode> path)
    {
        if (path == null || path.Count == 0)
            yield break;

        IsMoving = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 targetPos = path[i].transform.position;

            while (Vector3.Distance(transform.position, targetPos) > 0.02f)
            {
                Vector3 direction = (targetPos - transform.position).normalized;

                if (rotateToDirection && direction != Vector3.zero)
                {
                    LookWhereYoureGoing(direction);
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = targetPos;
        }

        IsMoving = false;
    }

    private void LookWhereYoureGoing(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
