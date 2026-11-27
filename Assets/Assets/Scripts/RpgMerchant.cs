using UnityEngine;

public class RpgMerchant : MonoBehaviour, IVillageInteractable
{
    public static bool IsAnyShopOpen { get; private set; }
    private bool opened;
    private Rect window = new Rect(0, 0, 470, 570);
    private Vector2 scroll;

    public string Prompt => "Hablar con el comerciante";
    public bool CanInteract => !opened;

    public void Interact(GameObject player)
    {
        opened = true;
        IsAnyShopOpen = true;
        window.x = (Screen.width - window.width) * .5f;
        window.y = (Screen.height - window.height) * .5f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (opened && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void OnDisable()
    {
        if (opened) Close();
    }

    private void Close()
    {
        opened = false;
        IsAnyShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnGUI()
    {
        if (!opened) return;
        window = GUI.Window(GetInstanceID(), window, DrawShop, "COMERCIANTE DEL REINO");
    }

    private void DrawShop(int id)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            GUILayout.Label("No se encontró al jugador.");
            if (GUILayout.Button("Cerrar")) Close();
            return;
        }

        VillageGameManager wallet = VillageGameManager.Instance;
        int coins = wallet != null ? wallet.Coins : 0;
        GUILayout.Label("Monedas disponibles: " + coins);
        GUILayout.Label("Compra equipo, o vende los recursos que encuentres por el mapa.");
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(430));

        GUILayout.Label("ARMAS");
        OfferWeapon(player, "Espada de hierro", RpgWeaponType.Sword, 75);
        OfferWeapon(player, "Daga ligera", RpgWeaponType.Dagger, 60);
        OfferWeapon(player, "Katana de acero", RpgWeaponType.Katana, 280);
        OfferWeapon(player, "Arco de cazador", RpgWeaponType.Bow, 160);
        OfferWeapon(player, "Ballesta reforzada", RpgWeaponType.Crossbow, 240);
        OfferWeapon(player, "Espada del Eclipse (OP)", RpgWeaponType.EclipseSword, 4500);
        OfferWeapon(player, "Arco ancestral (OP)", RpgWeaponType.AncientBow, 3600);

        GUILayout.Space(10);
        GUILayout.Label("ARMADURAS Y DEFENSA");
        OfferArmor(player, "Armadura de cuero", 18, 150);
        OfferArmor(player, "Armadura de acero", 58, 650);
        OfferArmor(player, "Armadura del dragón (OP)", 135, 2600);
        OfferShield(player, "Escudo de guardia", false, 190);
        OfferShield(player, "Escudo del Eclipse (OP)", true, 1700);

        GUILayout.Space(10);
        GUILayout.Label("CONSUMIBLES");
        if (GUILayout.Button("Poción de salud +50 HP  —  45 monedas"))
            BuyPotion(player, 50, 45);
        if (GUILayout.Button("Poción superior +100 HP  —  110 monedas"))
            BuyPotion(player, 100, 110);

        GUILayout.Space(10);
        GUILayout.Label("VENDER RECURSOS");
        OfferSell(player, RpgResourceKind.Herb);
        OfferSell(player, RpgResourceKind.IronOre);
        OfferSell(player, RpgResourceKind.Wood);
        OfferSell(player, RpgResourceKind.Hide);
        OfferSell(player, RpgResourceKind.BeastFang);
        OfferSell(player, RpgResourceKind.AncientRelic);

        GUILayout.EndScrollView();
        if (GUILayout.Button("Cerrar tienda  [ESC]")) Close();
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    private void OfferWeapon(GameObject player, string label, RpgWeaponType type, int price)
    {
        RpgEquipment equipment = player.GetComponent<RpgEquipment>();
        if (equipment != null && equipment.OwnsWeapon(type))
        {
            GUILayout.Label(label + "  —  adquirido");
            return;
        }
        if (GUILayout.Button(label + "  —  " + price + " monedas"))
        {
            if (!Pay(price)) return;
            equipment.UnlockWeapon(type, true);
            Notify("Compraste " + label);
        }
    }

    private void OfferArmor(GameObject player, string label, int rating, int price)
    {
        RpgEquipment equipment = player.GetComponent<RpgEquipment>();
        if (GUILayout.Button(label + "  (+" + rating + " armadura)  —  " + price + " monedas"))
        {
            if (!Pay(price)) return;
            equipment.EquipArmor(label, rating);
            Notify("Equipaste " + label);
        }
    }

    private void OfferShield(GameObject player, string label, bool legendary, int price)
    {
        RpgEquipment equipment = player.GetComponent<RpgEquipment>();
        if (GUILayout.Button(label + "  —  " + price + " monedas"))
        {
            if (!Pay(price)) return;
            equipment.UnlockShield(legendary);
            Notify("Compraste " + label);
        }
    }

    private void BuyPotion(GameObject player, int healing, int price)
    {
        if (!Pay(price)) return;
        RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
        if (stats != null) stats.Heal(healing);
        Notify("Usaste una poción");
    }

    private void OfferSell(GameObject player, RpgResourceKind kind)
    {
        RpgInventory inventory = player.GetComponent<RpgInventory>();
        if (inventory == null) return;
        int amount = inventory.Count(kind);
        int value = amount * inventory.UnitValue(kind);
        if (amount <= 0)
            GUILayout.Label(RpgInventory.ResourceName(kind) + ": 0");
        else if (GUILayout.Button("Vender " + amount + " " + RpgInventory.ResourceName(kind) + "  —  " + value + " monedas"))
            inventory.SellAll(kind);
    }

    private bool Pay(int amount)
    {
        if (VillageGameManager.Instance == null || !VillageGameManager.Instance.SpendCoins(amount))
        {
            Notify("No tienes suficientes monedas.");
            return false;
        }
        return true;
    }

    private void Notify(string message)
    {
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(message);
    }
}
