using UnityEngine;

public enum RpgWeaponType
{
    None = 0,
    Sword = 1,
    Dagger = 2,
    Katana = 3,
    Bow = 4,
    Crossbow = 5,
    EclipseSword = 6,
    AncientBow = 7
}

public class RpgEquipment : MonoBehaviour
{
    [SerializeField] private RpgWeaponType equippedWeapon = RpgWeaponType.None;
    [SerializeField] private int armorRating;
    [SerializeField] private string armorName = "Sin armadura";
    [SerializeField] private bool hasShield;
    [SerializeField] private bool hasEclipseShield;
    private bool[] ownedWeapons = new bool[8];

    public RpgWeaponType EquippedWeapon => equippedWeapon;
    public int ArmorRating => armorRating;
    public string ArmorName => armorName;
    public bool HasShield => hasShield;
    public bool HasEclipseShield => hasEclipseShield;

    private void Awake()
    {
        ownedWeapons[0] = true;
        for (int i = 1; i < ownedWeapons.Length; i++)
            ownedWeapons[i] = PlayerPrefs.GetInt("RpgWeaponOwned_" + i, 0) == 1;
        equippedWeapon = (RpgWeaponType)Mathf.Clamp(
            PlayerPrefs.GetInt("RpgEquippedWeapon", 0), 0, ownedWeapons.Length - 1);
        if (!ownedWeapons[(int)equippedWeapon]) equippedWeapon = RpgWeaponType.None;

        armorRating = Mathf.Max(0, PlayerPrefs.GetInt("RpgArmorRating", 0));
        armorName = PlayerPrefs.GetString("RpgArmorName", "Sin armadura");
        hasShield = PlayerPrefs.GetInt("RpgHasShield", 0) == 1;
        hasEclipseShield = PlayerPrefs.GetInt("RpgEclipseShield", 0) == 1;
    }

    public bool OwnsWeapon(RpgWeaponType weapon)
    {
        int index = (int)weapon;
        return index >= 0 && index < ownedWeapons.Length && ownedWeapons[index];
    }

    public void UnlockWeapon(RpgWeaponType weapon, bool equipNow)
    {
        int index = (int)weapon;
        if (index <= 0 || index >= ownedWeapons.Length) return;
        ownedWeapons[index] = true;
        PlayerPrefs.SetInt("RpgWeaponOwned_" + index, 1);
        if (equipNow) EquipWeapon(weapon);
        PlayerPrefs.Save();
    }

    public bool EquipWeapon(RpgWeaponType weapon)
    {
        if (!OwnsWeapon(weapon)) return false;
        equippedWeapon = weapon;
        PlayerPrefs.SetInt("RpgEquippedWeapon", (int)weapon);
        PlayerPrefs.Save();
        RpgEquipmentVisuals visuals = GetComponent<RpgEquipmentVisuals>();
        if (visuals != null) visuals.Refresh();
        return true;
    }

    public void EquipArmor(string name, int rating)
    {
        armorName = string.IsNullOrEmpty(name) ? "Armadura" : name;
        armorRating = Mathf.Max(0, rating);
        PlayerPrefs.SetString("RpgArmorName", armorName);
        PlayerPrefs.SetInt("RpgArmorRating", armorRating);
        PlayerPrefs.Save();
        RpgEquipmentVisuals visuals = GetComponent<RpgEquipmentVisuals>();
        if (visuals != null) visuals.Refresh();
    }

    public void UnlockShield(bool eclipse)
    {
        hasShield = true;
        if (eclipse) hasEclipseShield = true;
        PlayerPrefs.SetInt("RpgHasShield", 1);
        PlayerPrefs.SetInt("RpgEclipseShield", hasEclipseShield ? 1 : 0);
        PlayerPrefs.Save();
        RpgEquipmentVisuals visuals = GetComponent<RpgEquipmentVisuals>();
        if (visuals != null) visuals.Refresh();
    }

    public int WeaponDamage
    {
        get
        {
            switch (equippedWeapon)
            {
                case RpgWeaponType.Sword: return 18;
                case RpgWeaponType.Dagger: return 13;
                case RpgWeaponType.Katana: return 34;
                case RpgWeaponType.Bow: return 22;
                case RpgWeaponType.Crossbow: return 34;
                case RpgWeaponType.EclipseSword: return 115;
                case RpgWeaponType.AncientBow: return 82;
                default: return 4;
            }
        }
    }

    public float WeaponRange
    {
        get
        {
            switch (equippedWeapon)
            {
                case RpgWeaponType.Dagger: return 1.65f;
                case RpgWeaponType.Katana: return 2.65f;
                case RpgWeaponType.Sword:
                case RpgWeaponType.EclipseSword: return 2.2f;
                case RpgWeaponType.Bow:
                case RpgWeaponType.Crossbow:
                case RpgWeaponType.AncientBow: return 42f;
                default: return 1.35f;
            }
        }
    }

    public float AttackCooldown
    {
        get
        {
            switch (equippedWeapon)
            {
                case RpgWeaponType.Dagger: return 0.32f;
                case RpgWeaponType.Katana: return 0.55f;
                case RpgWeaponType.Bow:
                case RpgWeaponType.AncientBow: return 0.78f;
                case RpgWeaponType.Crossbow: return 1.05f;
                case RpgWeaponType.EclipseSword: return 0.48f;
                default: return 0.65f;
            }
        }
    }

    public bool IsRanged
    {
        get
        {
            return equippedWeapon == RpgWeaponType.Bow ||
                   equippedWeapon == RpgWeaponType.Crossbow ||
                   equippedWeapon == RpgWeaponType.AncientBow;
        }
    }
}
