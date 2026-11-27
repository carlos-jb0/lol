using UnityEngine;

public class RpgPlayerStats : MonoBehaviour
{
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private int health = 100;
    [SerializeField] private int level = 1;
    [SerializeField] private int experience;
    private Vector3 checkpoint;
    private bool hasCheckpoint;

    public int Health => health;
    public int MaxHealth { get; private set; }
    public int Level => level;
    public int Experience => experience;
    public int ArmorRating
    {
        get
        {
            RpgEquipment equipment = GetComponent<RpgEquipment>();
            return equipment != null ? equipment.ArmorRating : 0;
        }
    }

    private void Awake()
    {
        MaxHealth = PlayerPrefs.GetInt("RpgMaxHealth", baseMaxHealth);
        level = Mathf.Max(1, PlayerPrefs.GetInt("RpgLevel", 1));
        experience = Mathf.Max(0, PlayerPrefs.GetInt("RpgExperience", 0));
        health = PlayerPrefs.GetInt("RpgHealth", MaxHealth);
        health = Mathf.Clamp(health, 1, MaxHealth);
        checkpoint = transform.position;
        hasCheckpoint = true;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpoint = position;
        hasCheckpoint = true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Min(MaxHealth, health + amount);
        Save();
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage("+" + amount + " HP");
    }

    public void TakeDamage(int rawDamage)
    {
        if (rawDamage <= 0 || health <= 0) return;

        RpgCombatController combat = GetComponent<RpgCombatController>();
        float blockMultiplier = combat != null && combat.IsBlocking ? 0.28f : 1f;
        float armorMultiplier = 100f / (100f + ArmorRating * 1.1f);
        int finalDamage = Mathf.Max(1, Mathf.CeilToInt(rawDamage * blockMultiplier * armorMultiplier));
        health = Mathf.Max(0, health - finalDamage);
        Save();

        if (health <= 0)
        {
            if (VillageGameManager.Instance != null)
                VillageGameManager.Instance.ShowMessage("Has caído. Regresas al último punto seguro.");
            Invoke(nameof(Respawn), 1.2f);
        }
    }

    private void Respawn()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        if (hasCheckpoint) transform.position = checkpoint;
        if (controller != null) controller.enabled = true;
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ResetVerticalMotion();
            movement.SetWorldBoundsActive(true);
        }
        health = MaxHealth;
        Save();
        Physics.SyncTransforms();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        experience += amount;
        bool leveled = false;
        while (experience >= level * 100)
        {
            experience -= level * 100;
            level++;
            MaxHealth += 12;
            health = MaxHealth;
            leveled = true;
        }
        Save();
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(leveled
                ? "¡Nivel " + level + "! Tu salud máxima aumentó."
                : "+" + amount + " EXP");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("RpgHealth", health);
        PlayerPrefs.SetInt("RpgMaxHealth", MaxHealth);
        PlayerPrefs.SetInt("RpgLevel", level);
        PlayerPrefs.SetInt("RpgExperience", experience);
        PlayerPrefs.Save();
    }

    private void OnGUI()
    {
        const float x = 18f;
        const float width = 205f;
        const float height = 15f;
        float y = 64f;

        GUI.Box(new Rect(x - 5, y - 5, width + 10, 54), GUIContent.none);
        GUI.Label(new Rect(x, y - 2, width, 18), "HP  " + health + " / " + MaxHealth);
        GUI.Box(new Rect(x, y + 16, width, height), GUIContent.none);
        GUI.Box(new Rect(x, y + 16, width * (MaxHealth <= 0 ? 0 : (float)health / MaxHealth), height), GUIContent.none);
        GUI.Label(new Rect(x, y + 32, width, 18), "Armadura: " + ArmorRating + "     Nivel: " + level);
    }
}
