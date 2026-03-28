using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool rotateToDirection = true;

    public bool IsMoving { get; private set; }

    public IEnumerator FollowPath(List<TileNode> path)
    {
        if (path == null || path.Count == 0)
        {
            IsMoving = false;
            yield break;
        }

        IsMoving = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 targetPos = path[i].transform.position;
            targetPos.z = transform.position.z;

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

    public void ForceStopMovement()
    {
        IsMoving = false;
    }

    private void LookWhereYoureGoing(Vector3 targetPosition)
    {
        Vector3 scale = transform.localScale;

        // Reset scale first
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);

        float dx = targetPosition.x - transform.position.x;
        float dy = targetPosition.y - transform.position.y;

       /* // 8-direction comparison
        if (dy > 0 && Mathf.Abs(dx) < 0.1f) // UP
        {
            scale.x *= -1;
        }
        else if (dy < 0 && Mathf.Abs(dx) < 0.1f) // DOWN
        {
            // normal
        }
        else if (dx < 0 && Mathf.Abs(dy) < 0.1f) // LEFT
        {
            scale.y *= -1;
        }
        else if (dx > 0 && Mathf.Abs(dy) < 0.1f) // RIGHT
        {
            // normal
        }*/
         if (dx < 0 && dy > 0) // TOP LEFT
        {
            scale.x *= -1;
            scale.y *= -1;
        }
        else if (dx > 0 && dy > 0) // TOP RIGHT
        {
            scale.x *= -1;
        }
        else if (dx > 0 && dy < 0) // BOTTOM RIGHT
        {
            // normal
        }
        else if (dx < 0 && dy < 0) // BOTTOM LEFT
        {
            scale.y *= -1;
        }

        transform.localScale = scale;
    }
}