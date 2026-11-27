using UnityEngine;

public class RpgTeleportPortal : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private GameObject sourceRoot;
    [SerializeField] private GameObject destinationRoot;
    [SerializeField] private Transform destinationPoint;
    [SerializeField] private string prompt = "Entrar";
    [SerializeField] private bool restoreWorldBounds;
    [SerializeField] private bool beginArenaTrial;
    private bool transitioning;

    public string Prompt => prompt;
    public bool CanInteract => !transitioning && destinationPoint != null;

    public void Configure(GameObject source, GameObject destination, Transform point,
        string actionPrompt, bool worldBoundsAfter, bool startTrial = false)
    {
        sourceRoot = source;
        destinationRoot = destination;
        destinationPoint = point;
        prompt = actionPrompt;
        restoreWorldBounds = worldBoundsAfter;
        beginArenaTrial = startTrial;
    }

    public void Interact(GameObject player)
    {
        if (!CanInteract || player == null) return;
        transitioning = true;

        if (destinationRoot != null)
            destinationRoot.SetActive(true);

        Physics.SyncTransforms();
        RpgTeleportUtility.Teleport(player, destinationPoint, restoreWorldBounds);

        if (beginArenaTrial)
        {
            ArenaWaveManager manager = destinationRoot != null
                ? destinationRoot.GetComponentInChildren<ArenaWaveManager>(true)
                : FindObjectOfType<ArenaWaveManager>();
            if (manager != null) manager.BeginTrial();
        }

        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(prompt);

        transitioning = false;
        if (sourceRoot != null)
            sourceRoot.SetActive(false);
    }
}
