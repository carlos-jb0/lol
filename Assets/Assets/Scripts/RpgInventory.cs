using UnityEngine;

public enum RpgResourceKind
{
    Herb = 0,
    IronOre = 1,
    Wood = 2,
    Hide = 3,
    BeastFang = 4,
    AncientRelic = 5
}

public class RpgInventory : MonoBehaviour
{
    private const int ResourceCount = 6;
    private readonly int[] resources = new int[ResourceCount];
    private bool showInventory;

    private void Awake()
    {
        for (int i = 0; i < ResourceCount; i++)
            resources[i] = PlayerPrefs.GetInt("RpgResource_" + i, 0);
    }

    public int Count(RpgResourceKind kind)
    {
        return resources[(int)kind];
    }

    public void AddResource(RpgResourceKind kind, int amount)
    {
        if (amount <= 0) return;
        int index = (int)kind;
        resources[index] += amount;
        Save();
        if (kind == RpgResourceKind.AncientRelic && RpgStoryQuest.Instance != null)
            RpgStoryQuest.Instance.RegisterRelics(amount);

        string label = ResourceName(kind);
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage("+" + amount + " " + label);
    }

    public bool RemoveResource(RpgResourceKind kind, int amount)
    {
        if (amount <= 0 || Count(kind) < amount) return false;
        resources[(int)kind] -= amount;
        Save();
        return true;
    }

    public int SellAll(RpgResourceKind kind)
    {
        int amount = Count(kind);
        if (amount <= 0 || VillageGameManager.Instance == null) return 0;

        int value = amount * UnitValue(kind);
        resources[(int)kind] = 0;
        Save();
        VillageGameManager.Instance.AddCoins(value);
        VillageGameManager.Instance.ShowMessage("Vendiste " + amount + " " + ResourceName(kind) + " por " + value + " monedas");
        return value;
    }

    public int UnitValue(RpgResourceKind kind)
    {
        switch (kind)
        {
            case RpgResourceKind.Herb: return 3;
            case RpgResourceKind.IronOre: return 12;
            case RpgResourceKind.Wood: return 4;
            case RpgResourceKind.Hide: return 9;
            case RpgResourceKind.BeastFang: return 22;
            case RpgResourceKind.AncientRelic: return 75;
            default: return 1;
        }
    }

    public static string ResourceName(RpgResourceKind kind)
    {
        switch (kind)
        {
            case RpgResourceKind.Herb: return "hierbas";
            case RpgResourceKind.IronOre: return "mineral de hierro";
            case RpgResourceKind.Wood: return "madera";
            case RpgResourceKind.Hide: return "pieles";
            case RpgResourceKind.BeastFang: return "colmillos";
            case RpgResourceKind.AncientRelic: return "reliquias antiguas";
            default: return "recursos";
        }
    }

    private void Save()
    {
        for (int i = 0; i < ResourceCount; i++)
            PlayerPrefs.SetInt("RpgResource_" + i, resources[i]);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (RpgMerchant.IsAnyShopOpen) return;
        if (Input.GetKeyDown(KeyCode.I)) showInventory = !showInventory;
    }

    private void OnGUI()
    {
        if (!showInventory || RpgMerchant.IsAnyShopOpen) return;
        Rect panel = new Rect(Screen.width - 300, 18, 282, 250);
        GUI.Box(panel, "INVENTARIO  [I]");
        GUILayout.BeginArea(new Rect(panel.x + 14, panel.y + 32, panel.width - 28, panel.height - 42));
        for (int i = 0; i < ResourceCount; i++)
        {
            RpgResourceKind kind = (RpgResourceKind)i;
            GUILayout.Label(ResourceName(kind) + ": " + Count(kind));
        }
        GUILayout.Space(8);
        GUILayout.Label("El comerciante compra todos estos recursos.");
        GUILayout.EndArea();
    }
}
