using UnityEngine;

public class RpgStoryQuest : MonoBehaviour
{
    public static RpgStoryQuest Instance { get; private set; }
    private bool accepted;
    private int relics;
    private bool arenaWon;
    private bool completed;

    private void Awake()
    {
        Instance = this;
        accepted = PlayerPrefs.GetInt("RpgQuestAccepted", 0) == 1;
        relics = PlayerPrefs.GetInt("RpgQuestRelics", 0);
        arenaWon = PlayerPrefs.GetInt("RpgQuestArenaWon", 0) == 1;
        completed = PlayerPrefs.GetInt("RpgQuestCompleted", 0) == 1;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TalkToCaptain()
    {
        if (!accepted)
        {
            accepted = true;
            Save();
            Message("MISIÓN PRINCIPAL: reúne 3 reliquias antiguas y supera la arena del castillo.");
            return;
        }

        if (completed)
        {
            Message("El reino recuerda tu hazaña. Gracias, héroe.");
            return;
        }

        TryComplete();
        if (!completed)
            Message("Misión: reliquias " + Mathf.Min(relics, 3) + "/3 | Arena: " +
                (arenaWon ? "completada" : "pendiente") + ".");
    }

    public void RegisterRelics(int amount)
    {
        if (completed || amount <= 0) return;
        relics += amount;
        Save();
        TryComplete();
    }

    public void RegisterArenaVictory()
    {
        if (completed) return;
        arenaWon = true;
        Save();
        TryComplete();
    }

    private void TryComplete()
    {
        if (!accepted || completed || relics < 3 || !arenaWon) return;
        completed = true;
        Save();

        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.AddCoins(1200);
        RpgPlayerStats stats = GetComponent<RpgPlayerStats>();
        if (stats != null) stats.AddExperience(250);
        RpgEquipment equipment = GetComponent<RpgEquipment>();
        if (equipment != null)
        {
            equipment.UnlockWeapon(RpgWeaponType.Sword, true);
            equipment.EquipArmor("Armadura de la Guardia Real", 38);
        }
        Message("¡MISIÓN COMPLETADA! Recibiste 1200 monedas y la armadura de la Guardia Real.");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("RpgQuestAccepted", accepted ? 1 : 0);
        PlayerPrefs.SetInt("RpgQuestRelics", relics);
        PlayerPrefs.SetInt("RpgQuestArenaWon", arenaWon ? 1 : 0);
        PlayerPrefs.SetInt("RpgQuestCompleted", completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Message(string value)
    {
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(value);
    }

    private void OnGUI()
    {
        if (!accepted || completed) return;
        string line = "MISIÓN: Reliquias " + Mathf.Min(relics, 3) + "/3 | Arena " +
            (arenaWon ? "✓" : "pendiente");
        GUI.Box(new Rect(18, 112, 290, 28), line);
    }
}

public class RpgQuestNpc : MonoBehaviour, IVillageInteractable
{
    public string Prompt => "Hablar con el capitán";
    public bool CanInteract => true;

    public void Interact(GameObject player)
    {
        RpgStoryQuest quest = player.GetComponent<RpgStoryQuest>();
        if (quest == null) quest = player.AddComponent<RpgStoryQuest>();
        quest.TalkToCaptain();
    }
}
