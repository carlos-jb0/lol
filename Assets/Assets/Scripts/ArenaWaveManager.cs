using System.Collections.Generic;
using UnityEngine;

public class ArenaWaveManager : MonoBehaviour
{
    public static ArenaWaveManager Instance { get; private set; }

    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float spawnRadius = 13f;
    [SerializeField] private int maximumWaves = 3;

    private readonly List<ArenaBeastAI> enemies = new List<ArenaBeastAI>();
    private int wave;
    private bool started;
    private bool completed;
    private bool waitingForNextWave;
    private float nextWaveAt;
    private string status = "Acércate a la puerta para iniciar la prueba.";

    public bool IsCompleted => completed;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    public void Configure(Transform center, float radius, int waveCount)
    {
        arenaCenter = center;
        spawnRadius = radius;
        maximumWaves = Mathf.Max(1, waveCount);
    }

    public void BeginTrial()
    {
        if (completed)
        {
            Show("La arena ya fue superada. El reino reconoce tu victoria.");
            return;
        }
        if (started) return;

        started = true;
        wave = 1;
        SpawnWave();
    }

    private void Update()
    {
        if (waitingForNextWave && Time.time >= nextWaveAt)
        {
            waitingForNextWave = false;
            wave++;
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        int count = wave + 2;
        status = "Oleada " + wave + "/" + maximumWaves + " — Bestias restantes: " + count;
        enemies.Clear();

        Vector3 center = arenaCenter != null ? arenaCenter.position : transform.position;
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;
            Vector3 spawn = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            ArenaBeastAI beast = CreateBeast(spawn, wave, this);
            enemies.Add(beast);
        }
        Show("¡Comienza la oleada " + wave + "!");
    }

    public void NotifyBeastDefeated(ArenaBeastAI beast)
    {
        enemies.Remove(beast);
        if (enemies.Count > 0) return;

        if (wave < maximumWaves)
        {
            int reward = 35 * wave;
            if (VillageGameManager.Instance != null)
                VillageGameManager.Instance.AddCoins(reward);
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
                if (stats != null) stats.AddExperience(25 * wave);
            }

            status = "Oleada superada. Siguiente oleada en unos segundos...";
            waitingForNextWave = true;
            nextWaveAt = Time.time + 3.2f;
        }
        else
        {
            completed = true;
            waitingForNextWave = false;
            status = "¡ARENA COMPLETADA!";
            if (VillageGameManager.Instance != null)
                VillageGameManager.Instance.AddCoins(250);
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
                if (stats != null) stats.AddExperience(150);
                RpgStoryQuest quest = player.GetComponent<RpgStoryQuest>();
                if (quest != null) quest.RegisterArenaVictory();
            }
            Show("¡Has vencido a todas las bestias! Recompensa: 250 monedas.");
        }
    }

    private ArenaBeastAI CreateBeast(Vector3 position, int difficulty, ArenaWaveManager manager)
    {
        GameObject root = new GameObject("Bestia_de_la_Arena");
        root.transform.position = position;
        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = .62f;
        controller.center = new Vector3(0f, .9f, 0f);
        controller.stepOffset = .25f;
        controller.slopeLimit = 45f;

        Color hide = difficulty >= 3 ? new Color(.28f, .12f, .11f) :
            difficulty == 2 ? new Color(.36f, .22f, .13f) : new Color(.43f, .31f, .2f);
        Color horn = new Color(.78f, .67f, .46f);
        Material hideMat = MakeMaterial(hide);
        Material hornMat = MakeMaterial(horn);
        Material eyeMat = MakeMaterial(new Color(.9f, .18f, .06f), .1f);

        Part(root.transform, "Cuerpo", PrimitiveType.Sphere, new Vector3(0, 1.05f, 0),
            new Vector3(1.45f, 1.05f, 1.9f), hideMat);
        Part(root.transform, "Cabeza", PrimitiveType.Sphere, new Vector3(0, 1.25f, .95f),
            new Vector3(.92f, .85f, .88f), hideMat);
        Part(root.transform, "Hocico", PrimitiveType.Cube, new Vector3(0, .98f, 1.42f),
            new Vector3(.58f, .42f, .58f), hideMat);
        for (int side = -1; side <= 1; side += 2)
        {
            Part(root.transform, "Cuerno", PrimitiveType.Capsule, new Vector3(side * .32f, 1.83f, .83f),
                new Vector3(.24f, .62f, .24f), hornMat);
            Part(root.transform, "Ojo", PrimitiveType.Sphere, new Vector3(side * .27f, 1.42f, 1.35f),
                new Vector3(.12f, .12f, .08f), eyeMat);
            Part(root.transform, "Pata", PrimitiveType.Capsule, new Vector3(side * .43f, .42f, .48f),
                new Vector3(.34f, .62f, .35f), hideMat);
            Part(root.transform, "Pata", PrimitiveType.Capsule, new Vector3(side * .43f, .42f, -.52f),
                new Vector3(.34f, .62f, .35f), hideMat);
        }

        ArenaBeastAI ai = root.AddComponent<ArenaBeastAI>();
        ai.Configure(48 + difficulty * 26, 7 + difficulty * 4, 2.2f + difficulty * .18f, manager);
        return ai;
    }

    private void Part(Transform parent, string label, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = label;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    private Material MakeMaterial(Color color, float metallic = 0f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        return material;
    }

    private void Show(string message)
    {
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(message);
    }

    private void OnGUI()
    {
        if (!started) return;
        GUI.Box(new Rect(Screen.width - 300, 18, 282, 38),
            "ARENA  |  Oleada " + wave + "/" + maximumWaves);
        GUI.Label(new Rect(Screen.width - 292, 58, 270, 28), status);
    }
}
