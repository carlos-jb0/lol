using UnityEngine;

public class RpgWorldPickup : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private RpgResourceKind resource = RpgResourceKind.Herb;
    [SerializeField] private int amount = 1;
    [SerializeField] private string displayName = "Recurso";
    private bool collected;

    public string Prompt => "Recoger " + displayName;
    public bool CanInteract => !collected;

    public void Configure(RpgResourceKind kind, int quantity, string label)
    {
        resource = kind;
        amount = Mathf.Max(1, quantity);
        displayName = label;
    }

    public void Interact(GameObject player)
    {
        if (collected) return;
        RpgInventory inventory = player.GetComponent<RpgInventory>();
        if (inventory == null) inventory = player.AddComponent<RpgInventory>();
        inventory.AddResource(resource, amount);

        RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
        if (stats != null) stats.AddExperience(2);
        collected = true;
        Destroy(gameObject);
    }
}

public class RpgSearchableLoot : MonoBehaviour, IVillageInteractable
{
    [SerializeField] private string furnitureName = "cofre";
    [SerializeField] private bool searched;

    public string Prompt => searched ? furnitureName + " (vacío)" : "Buscar " + furnitureName;
    public bool CanInteract => !searched;

    public void Configure(string label)
    {
        furnitureName = label;
    }

    public void Interact(GameObject player)
    {
        if (searched) return;
        searched = true;

        RpgInventory inventory = player.GetComponent<RpgInventory>();
        if (inventory == null) inventory = player.AddComponent<RpgInventory>();

        int roll = Random.Range(0, 5);
        if (roll == 0) inventory.AddResource(RpgResourceKind.AncientRelic, 1);
        else if (roll == 1) inventory.AddResource(RpgResourceKind.IronOre, Random.Range(1, 3));
        else if (roll == 2) inventory.AddResource(RpgResourceKind.Herb, Random.Range(2, 5));
        else if (roll == 3) inventory.AddResource(RpgResourceKind.Hide, 1);
        else
        {
            if (VillageGameManager.Instance != null)
                VillageGameManager.Instance.AddCoins(Random.Range(12, 36));
        }

        RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
        if (stats != null) stats.AddExperience(5);
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage("Revisaste " + furnitureName);
    }
}
