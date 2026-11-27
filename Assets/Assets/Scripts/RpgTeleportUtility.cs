using UnityEngine;

public static class RpgTeleportUtility
{
    public static void Teleport(GameObject player, Transform destination, bool restoreWorldBounds)
    {
        if (player == null || destination == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        float bottomOffset = controller != null
            ? controller.bounds.min.y - player.transform.position.y
            : 0f;

        if (movement != null)
        {
            movement.SetWorldBoundsActive(false);
            movement.ResetVerticalMotion();
        }
        if (controller != null) controller.enabled = false;

        Vector3 target = destination.position;
        Quaternion rotation = destination.rotation;
        Physics.SyncTransforms();

        // El rayo comienza justo encima del punto de aparición: así nunca toma
        // el techo como piso y solo acepta superficies orientadas hacia arriba.
        RaycastHit[] hits = Physics.RaycastAll(
            target + Vector3.up * 0.45f,
            Vector3.down,
            3.5f,
            ~0,
            QueryTriggerInteraction.Ignore);

        bool foundGround = false;
        float nearestDistance = float.PositiveInfinity;
        float groundY = target.y;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.normal.y < 0.65f) continue;
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == player.transform || hitTransform.IsChildOf(player.transform))
                continue;

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                groundY = hit.point.y;
                foundGround = true;
            }
        }

        if (foundGround)
            target.y = groundY - bottomOffset + (controller != null ? controller.skinWidth : 0.08f) + 0.02f;

        player.transform.SetPositionAndRotation(target, rotation);
        Physics.SyncTransforms();

        if (controller != null) controller.enabled = true;
        Physics.SyncTransforms();

        if (movement != null)
        {
            movement.ResetVerticalMotion();
            movement.SetWorldBoundsActive(restoreWorldBounds);
        }
    }
}
