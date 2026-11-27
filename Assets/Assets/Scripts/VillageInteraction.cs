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
        Coins = PlayerPrefs.GetInt("RpgCoins", 0);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        PlayerPrefs.SetInt("RpgCoins", Coins);
        PlayerPrefs.Save();
        ShowMessage("+" + amount + (amount == 1 ? " moneda" : " monedas"));
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount) return false;
        Coins -= amount;
        PlayerPrefs.SetInt("RpgCoins", Coins);
        PlayerPrefs.Save();
        return true;
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
        if (outsideRoot == null || insideRoot == null || outsideSpawn == null || insideSpawn == null)
            return;

        if (entering)
        {
            outsideRoot.SetActive(false);
            insideRoot.SetActive(true);
            Physics.SyncTransforms();
            RpgTeleportUtility.Teleport(player, insideSpawn, false);
        }
        else
        {
            insideRoot.SetActive(false);
            outsideRoot.SetActive(true);
            Physics.SyncTransforms();
            RpgTeleportUtility.Teleport(player, outsideSpawn, true);
        }

        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(entering ? "Entraste a la casa" : "Saliste de la casa");
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
        Destroy(gameObject);
    }
}