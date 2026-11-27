using UnityEngine;
using System;

public interface IVillageInteractable
{
    string Prompt { get; }
    bool CanInteract { get; }
    void Interact(GameObject player);
}

public class VillageGameManager : MonoBehaviour
{
    public static VillageGameManager Instance { get; private set; }
    public int Coins { get; private set; }
    public string Message { get; private set; }
    private float messageUntil;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        ShowMessage("+" + amount + (amount == 1 ? " moneda" : " monedas"));
    }

    public void ShowMessage(string message)
    {
        Message = message;
        messageUntil = Time.time + 2.5f;
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(16, 16, 170, 38), "Monedas: " + Coins);
        if (Time.time < messageUntil)
            GUI.Box(new Rect(Screen.width / 2f - 130, 55, 260, 34), Message);
    }
}

[RequireComponent(typeof(CharacterController))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private float interactionRadius = 1.1f;
    private readonly Collider[] hits = new Collider[32];
    private IVillageInteractable current;

    private void Update()
    {
        current = FindTarget();
        if (current != null && current.CanInteract && Input.GetKeyDown(KeyCode.E))
            current.Interact(gameObject);
    }

    private IVillageInteractable FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up, interactionRadius,
            hits, ~0, QueryTriggerInteraction.Collide);
        IVillageInteractable best = null;
        float bestDistance = interactionDistance;
        for (int i = 0; i < count; i++)
        {
            if (hits[i] == null) continue;
            MonoBehaviour[] components = hits[i].GetComponentsInParent<MonoBehaviour>();
            foreach (var component in components)
            {
                if (!(component is IVillageInteractable candidate) || !candidate.CanInteract) continue;
                float d = Vector3.Distance(transform.position, hits[i].ClosestPoint(transform.position));
                if (d <= bestDistance) { bestDistance = d; best = candidate; }
            }
        }
        return best;
    }

    private void OnGUI()
    {
        if (current == null || !current.CanInteract) return;
        GUI.Box(new Rect(Screen.width / 2f - 125, Screen.height - 92, 250, 38),
            "[E] " + current.Prompt);
    }
}

public class HousePortal : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private GameObject outsideRoot;
    [SerializeField] private GameObject insideRoot;
    [SerializeField] private Transform outsideSpawn;
    [SerializeField] private Transform insideSpawn;
    [SerializeField] private bool entering;

    public string Prompt => entering ? "Entrar a la casa" : "Salir de la casa";
    public bool CanInteract => true;

    public void Configure(GameObject outside, GameObject inside, Transform outsidePoint,
        Transform insidePoint, bool isEntering)
    {
        outsideRoot = outside; insideRoot = inside;
        outsideSpawn = outsidePoint; insideSpawn = insidePoint; entering = isEntering;
    }

    public void Interact(GameObject player)
    {
        if (outsideRoot == null || insideRoot == null) return;

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

        Transform destinationPoint;
        if (entering)
        {
            outsideRoot.SetActive(false);
            insideRoot.SetActive(true);
            destinationPoint = insideSpawn;
        }
        else
        {
            insideRoot.SetActive(false);
            outsideRoot.SetActive(true);
            destinationPoint = outsideSpawn;
        }

        Vector3 destination = destinationPoint.position;
        Physics.SyncTransforms();

        // Ajusta la altura usando el suelo real y el tamaño del CharacterController.
        // Así no aparece por debajo del terreno al salir ni suspendido dentro.
        RaycastHit[] groundHits = Physics.RaycastAll(
            destination + Vector3.up * 5f, Vector3.down, 15f, ~0,
            QueryTriggerInteraction.Ignore);
        float nearestGround = float.PositiveInfinity;
        foreach (RaycastHit hit in groundHits)
        {
            if (hit.collider == null || hit.collider.transform == player.transform ||
                hit.collider.transform.IsChildOf(player.transform)) continue;
            if (hit.distance < nearestGround) nearestGround = hit.distance;
        }

        if (!float.IsPositiveInfinity(nearestGround))
        {
            float groundY = destination.y + 5f - nearestGround;
            destination.y = groundY - bottomOffset + (controller != null ? controller.skinWidth : 0.08f) + 0.03f;
        }
        else
        {
            destination.y = Mathf.Max(destination.y, 1.2f);
        }

        player.transform.SetPositionAndRotation(destination, destinationPoint.rotation);
        Physics.SyncTransforms();
        if (controller != null) controller.enabled = true;

        if (movement != null)
        {
            movement.ResetVerticalMotion();
            movement.SetWorldBoundsActive(!entering);
        }
    }
}

public class SearchableFurniture : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private GameObject coinsRoot;
    [SerializeField] private string furnitureName = "mueble";
    private bool searched;

    public string Prompt => searched ? furnitureName + " (vacío)" : "Revisar " + furnitureName;
    public bool CanInteract => !searched;

    public void Configure(GameObject contents, string label)
    {
        coinsRoot = contents;
        furnitureName = label;
    }

    public void Interact(GameObject player)
    {
        if (searched) return;
        searched = true;
        if (coinsRoot != null) coinsRoot.SetActive(true);
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage("Encontraste algo en " + furnitureName);
    }
}

public class CoinPickup : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private int value = 1;
    public string Prompt => "Recoger moneda";
    public bool CanInteract => true;

    public void Interact(GameObject player)
    {
        if (VillageGameManager.Instance != null) VillageGameManager.Instance.AddCoins(value);
        Destroy(transform.root.gameObject);
    }
}