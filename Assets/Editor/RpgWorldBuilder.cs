#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class RpgWorldBuilder
{
    private const string ExpansionRootName = "RPG_ADVENTURE_EXPANSION";
    private static readonly string[] HouseModelNames =
    {
        "Casa_Pequena", "Casa_Mediana", "Casa_Dos_Pisos", "Casa_Piedra_Porche"
    };

    [MenuItem("RPG / Construir gran expansión RPG")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Expansión RPG",
                "Detén Play Mode antes de construir. No se modificó la escena.", "OK");
            return;
        }

        GameObject town = GameObject.Find("PUEBLO_GENERADO");
        GameObject player = GameObject.Find("Player");
        if (town == null || player == null || player.GetComponent<CharacterController>() == null)
        {
            EditorUtility.DisplayDialog("Falta la escena base",
                "Necesito el objeto PUEBLO_GENERADO y el Player con CharacterController. Primero abre la escena del reino medieval.", "OK");
            return;
        }

        GameObject[] houseModels = new GameObject[HouseModelNames.Length];
        for (int i = 0; i < HouseModelNames.Length; i++)
        {
            string[] guids = AssetDatabase.FindAssets(
                HouseModelNames[i] + " t:Model", new[] { "Assets/Models/Pueblo" });
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Falta un modelo original",
                    "No encontré " + HouseModelNames[i] +
                    ". Conserva/importa los cuatro modelos OBJ + MTL originales en Assets/Models/Pueblo y vuelve a ejecutar el constructor. No se modificó la escena.",
                    "OK");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            houseModels[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (houseModels[i] == null)
            {
                EditorUtility.DisplayDialog("Modelo no importado",
                    "Unity todavía no pudo cargar " + path + ". Espera a que termine de importar y vuelve a intentarlo.", "OK");
                return;
            }
        }

        Transform previous = town.transform.Find(ExpansionRootName);
        if (previous != null)
            Undo.DestroyObjectImmediate(previous.gameObject);

        GameObject root = new GameObject(ExpansionRootName);
        Undo.RegisterCreatedObjectUndo(root, "Construir expansión RPG");
        root.transform.SetParent(town.transform, false);

        Material grass = TexturedMaterial("RPG_Grass", new Color(.29f, .43f, .23f), 0);
        Material dirt = TexturedMaterial("RPG_Dirt", new Color(.42f, .31f, .21f), 1);
        Material stone = TexturedMaterial("RPG_Stone", new Color(.43f, .45f, .48f), 2, .18f);
        Material darkStone = TexturedMaterial("RPG_DarkStone", new Color(.20f, .22f, .25f), 3, .22f);
        Material wood = TexturedMaterial("RPG_Wood", new Color(.35f, .21f, .11f), 4);
        Material darkWood = TexturedMaterial("RPG_DarkWood", new Color(.18f, .105f, .055f), 5);
        Material gold = TexturedMaterial("RPG_Gold", new Color(.76f, .52f, .16f), 6, .65f);
        Material cloth = TexturedMaterial("RPG_CrimsonCloth", new Color(.34f, .035f, .045f), 7);
        Material floor = TexturedMaterial("RPG_InteriorFloor", new Color(.49f, .36f, .24f), 8);
        Material wall = TexturedMaterial("RPG_InteriorWall", new Color(.72f, .66f, .53f), 9);
        Material mattress = TexturedMaterial("RPG_Mattress", new Color(.58f, .55f, .46f), 10);
        Material iron = TexturedMaterial("RPG_Iron", new Color(.38f, .42f, .46f), 11, .62f);
        Material herb = TexturedMaterial("RPG_Herb", new Color(.22f, .55f, .24f), 12);

        GameObject homes = new GameObject("32_CASAS_ORIGINALES_FUNCIONALES");
        homes.transform.SetParent(root.transform, false);
        BuildAdditionalHouses(homes.transform, houseModels, wall, floor, wood, darkWood, mattress, gold);

        BuildMerchant(root.transform, wood, darkWood, gold, cloth);
        BuildWorldResources(root.transform, herb, iron, wood, gold);
        BuildCastleAndArena(root.transform, stone, darkStone, wood, darkWood, gold, cloth, floor, wall);
        ConfigurePlayer(player);
        if (Object.FindObjectOfType<VillageGameManager>() == null)
            new GameObject("VillageGameManager").AddComponent<VillageGameManager>();
        ApplyExistingWorldMaterials(town.transform, grass, dirt, stone);
        ApplyGraphics(root.transform);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog("Expansión RPG construida",
            "Se añadieron 32 casas explorables usando los cuatro modelos originales, interiores independientes, comerciante, recursos vendibles, castillo, arena de tres oleadas, misión principal, equipo y combate. La expansión se construyó sin reemplazar la escena base ni los modelos. Pulsa Play para probarla.",
            "¡A jugar!");
    }

    private static void ConfigurePlayer(GameObject player)
    {
        if (player.GetComponent<PlayerInteractor>() == null)
            Undo.AddComponent<PlayerInteractor>(player);
        if (player.GetComponent<RpgPlayerStats>() == null)
            Undo.AddComponent<RpgPlayerStats>(player);
        if (player.GetComponent<RpgInventory>() == null)
            Undo.AddComponent<RpgInventory>(player);
        if (player.GetComponent<RpgEquipment>() == null)
            Undo.AddComponent<RpgEquipment>(player);
        if (player.GetComponent<RpgEquipmentVisuals>() == null)
            Undo.AddComponent<RpgEquipmentVisuals>(player);
        if (player.GetComponent<RpgCombatController>() == null)
            Undo.AddComponent<RpgCombatController>(player);
        if (player.GetComponent<RpgStoryQuest>() == null)
            Undo.AddComponent<RpgStoryQuest>(player);

        Transform oldArmor = player.transform.Find("ARMADURA_REINO_GENERADA");
        if (oldArmor != null) oldArmor.gameObject.SetActive(false);

        // La armadura anterior era decorativa. Mostramos el modelo base y dejamos
        // que el nuevo sistema equipe piezas solo cuando el jugador las consiga.
        foreach (Renderer renderer in player.GetComponents<Renderer>())
            renderer.enabled = true;
    }

    private static void BuildAdditionalHouses(Transform parent, GameObject[] models,
        Material wall, Material floor, Material wood, Material darkWood, Material mattress, Material gold)
    {
        float[] mainColumns = { -78f, -64f, -50f, 50f, 64f, 78f };
        float[] rows = { -72f, -54f, 54f, 72f };
        Vector3[] positions = new Vector3[32];
        int cursor = 0;
        foreach (float z in rows)
            foreach (float x in mainColumns)
                positions[cursor++] = new Vector3(x, 0f, z);
        foreach (float z in rows)
        {
            positions[cursor++] = new Vector3(-36f, 0f, z);
            positions[cursor++] = new Vector3(36f, 0f, z);
        }

        for (int i = 0; i < positions.Length; i++)
            BuildHouse(parent, models[i % models.Length], positions[i], i,
                wall, floor, wood, darkWood, mattress, gold);
    }

    private static void BuildHouse(Transform parent, GameObject model, Vector3 position, int index,
        Material wall, Material floor, Material wood, Material darkWood, Material mattress, Material gold)
    {
        string houseName = "Casa_RPG_" + (index + 1).ToString("00");
        GameObject outside = new GameObject(houseName + "_Exterior");
        outside.transform.SetParent(parent, false);
        outside.transform.localPosition = position;

        GameObject modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (modelInstance == null) modelInstance = Object.Instantiate(model);
        modelInstance.name = houseName + "_Modelo_Original";
        modelInstance.transform.SetParent(outside.transform, false);
        modelInstance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        modelInstance.transform.localScale = Vector3.one * .9f;

        CreateHouseCollision(outside.transform);
        BuildDoorFrame(outside.transform, wood, gold);

        GameObject interior = new GameObject(houseName + "_Interior");
        interior.transform.SetParent(parent, false);
        interior.transform.localPosition = new Vector3(520f + index * 22f, 0f, 320f);
        BuildRoom(interior.transform, index, wall, floor, wood, darkWood, mattress, gold);
        interior.SetActive(false);

        Transform outsideSpawn = NewPoint(parent, houseName + "_SpawnExterior",
            position + new Vector3(0f, 0f, 8.2f), Quaternion.Euler(0f, 180f, 0f));
        Transform insideSpawn = NewPoint(interior.transform, "SpawnInterior",
            new Vector3(0f, 1f, -1.35f), Quaternion.identity);

        GameObject enter = new GameObject(houseName + "_Entrar");
        enter.transform.SetParent(parent, false);
        enter.transform.localPosition = position + new Vector3(0f, 1f, 3.5f);
        SphereCollider enterTrigger = enter.AddComponent<SphereCollider>();
        enterTrigger.isTrigger = true;
        enterTrigger.radius = 1.55f;
        enter.AddComponent<HousePortal>().Configure(outside, interior, outsideSpawn, insideSpawn, true);

        GameObject exit = new GameObject(houseName + "_Salir");
        exit.transform.SetParent(interior.transform, false);
        exit.transform.localPosition = new Vector3(0f, 1f, -4.1f);
        SphereCollider exitTrigger = exit.AddComponent<SphereCollider>();
        exitTrigger.isTrigger = true;
        exitTrigger.radius = 1.55f;
        exit.AddComponent<HousePortal>().Configure(outside, interior, outsideSpawn, insideSpawn, false);
    }

    private static void CreateHouseCollision(Transform parent)
    {
        WallCollider(parent, "MuroColision_Izquierdo", new Vector3(-3.35f, 1.45f, 0f), new Vector3(.28f, 2.9f, 5.8f));
        WallCollider(parent, "MuroColision_Derecho", new Vector3(3.35f, 1.45f, 0f), new Vector3(.28f, 2.9f, 5.8f));
        WallCollider(parent, "MuroColision_Trasero", new Vector3(0f, 1.45f, -2.8f), new Vector3(6.7f, 2.9f, .28f));
        WallCollider(parent, "MuroColision_FrontalIzq", new Vector3(-2.45f, 1.45f, 2.8f), new Vector3(1.8f, 2.9f, .28f));
        WallCollider(parent, "MuroColision_FrontalDer", new Vector3(2.45f, 1.45f, 2.8f), new Vector3(1.8f, 2.9f, .28f));
    }

    private static void BuildDoorFrame(Transform parent, Material wood, Material gold)
    {
        Cube(parent, "Marco_Puerta_Izq", new Vector3(-1.5f, 1.35f, 3.1f), new Vector3(.18f, 2.7f, .3f), darkWoodOr(wood));
        Cube(parent, "Marco_Puerta_Der", new Vector3(1.5f, 1.35f, 3.1f), new Vector3(.18f, 2.7f, .3f), darkWoodOr(wood));
        Cube(parent, "Marco_Puerta_Dintel", new Vector3(0f, 2.72f, 3.1f), new Vector3(3.2f, .18f, .3f), gold);
    }

    private static Material darkWoodOr(Material wood)
    {
        return wood;
    }

    private static void BuildRoom(Transform root, int index, Material wall, Material floor,
        Material wood, Material darkWood, Material mattress, Material gold)
    {
        Cube(root, "Suelo", new Vector3(0f, -.12f, 0f), new Vector3(11f, .24f, 10f), floor);
        Cube(root, "Pared_Fondo", new Vector3(0f, 1.8f, 4.9f), new Vector3(11f, 3.6f, .2f), wall);
        Cube(root, "Pared_Izquierda", new Vector3(-5.4f, 1.8f, 0f), new Vector3(.2f, 3.6f, 10f), wall);
        Cube(root, "Pared_Derecha", new Vector3(5.4f, 1.8f, 0f), new Vector3(.2f, 3.6f, 10f), wall);
        Cube(root, "Techo", new Vector3(0f, 3.65f, 0f), new Vector3(11f, .2f, 10f), darkWood);
        Cube(root, "Pared_Frontal_Izq", new Vector3(-4f, 1.8f, -4.9f), new Vector3(2.8f, 3.6f, .2f), wall);
        Cube(root, "Pared_Frontal_Der", new Vector3(4f, 1.8f, -4.9f), new Vector3(2.8f, 3.6f, .2f), wall);

        Cube(root, "MarcoInterior_Izq", new Vector3(-2.55f, 1.42f, -4.78f), new Vector3(.18f, 2.84f, .28f), wood);
        Cube(root, "MarcoInterior_Der", new Vector3(2.55f, 1.42f, -4.78f), new Vector3(.18f, 2.84f, .28f), wood);
        Cube(root, "MarcoInterior_Dintel", new Vector3(0f, 2.85f, -4.78f), new Vector3(5.25f, .18f, .28f), gold);

        Cube(root, "Cama_Base", new Vector3(-3.05f, .32f, 1.25f), new Vector3(2.2f, .55f, 3.1f), wood);
        Cube(root, "Cama_Colchon", new Vector3(-3.05f, .67f, 1.25f), new Vector3(2.08f, .18f, 2.95f), mattress);
        Cube(root, "Mesa", new Vector3(2.85f, .58f, 2.45f), new Vector3(2f, 1.05f, 1.3f), darkWood);
        Cube(root, "Baul", new Vector3(-2.9f, .55f, -1.65f), new Vector3(1.65f, 1.05f, 1.2f), wood);
        Cube(root, "Estante", new Vector3(3.9f, 1.1f, -1.15f), new Vector3(1.4f, 2.1f, .55f), darkWood);

        AddSearchable(root, "Buscar la mesa", new Vector3(2.85f, 1f, 2.45f), "mesa");
        AddSearchable(root, "Buscar el baul", new Vector3(-2.9f, 1f, -1.65f), "baúl");
        AddSearchable(root, "Buscar el estante", new Vector3(3.9f, 1.25f, -1.15f), "estante");

        GameObject lamp = Sphere(root, "Lampara", new Vector3(0f, 3.05f, 0f), new Vector3(.25f, .25f, .25f), gold);
        Light localLight = lamp.AddComponent<Light>();
        localLight.type = LightType.Point;
        localLight.color = new Color(1f, .72f, .42f);
        localLight.range = 9f;
        localLight.intensity = .85f;
    }

    private static void AddSearchable(Transform parent, string objectName, Vector3 position, string label)
    {
        GameObject target = new GameObject(objectName);
        target.transform.SetParent(parent, false);
        target.transform.localPosition = position;
        SphereCollider trigger = target.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.25f;
        target.AddComponent<RpgSearchableLoot>().Configure(label);
    }

    private static void BuildMerchant(Transform parent, Material wood, Material darkWood,
        Material gold, Material cloth)
    {
        GameObject merchant = new GameObject("COMERCIANTE_DEL_REINO");
        merchant.transform.SetParent(parent, false);
        merchant.transform.localPosition = new Vector3(-35f, 0f, -3.5f);

        Part(merchant.transform, "Cuerpo_Mercader", PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(.72f, 1f, .62f), cloth);
        Part(merchant.transform, "Cabeza_Mercader", PrimitiveType.Sphere,
            new Vector3(0f, 2.05f, 0f), new Vector3(.48f, .52f, .46f), wood);
        Part(merchant.transform, "Delantal", PrimitiveType.Cube,
            new Vector3(0f, .95f, .34f), new Vector3(.6f, .68f, .12f), darkWood);
        Part(merchant.transform, "Bolsa", PrimitiveType.Sphere,
            new Vector3(.48f, .72f, -.15f), new Vector3(.35f, .42f, .32f), gold);

        SphereCollider interaction = merchant.AddComponent<SphereCollider>();
        interaction.isTrigger = true;
        interaction.radius = 1.9f;
        merchant.AddComponent<RpgMerchant>();

        Cube(parent, "Puesto_Mercader_Mostrador", new Vector3(-35f, .8f, -7f),
            new Vector3(5.5f, 1.6f, 2.3f), wood);
        Cube(parent, "Puesto_Mercader_Toldo", new Vector3(-35f, 3.2f, -7f),
            new Vector3(6.4f, .3f, 4f), cloth);
        PointLight(parent, "Luz_Mercado", new Vector3(-35f, 3.7f, -3.5f),
            new Color(1f, .68f, .35f), 8f, .9f);
    }

    private static void BuildWorldResources(Transform parent, Material herb, Material iron,
        Material wood, Material gold)
    {
        Transform resources = NewRoot(parent, "RECURSOS_RECOLECTABLES");
        Vector3[] herbSpots =
        {
            new Vector3(-82,0,-20), new Vector3(-70,0,8), new Vector3(-55,0,22),
            new Vector3(-30,0,-75), new Vector3(28,0,-78), new Vector3(80,0,-38),
            new Vector3(82,0,22), new Vector3(72,0,48), new Vector3(42,0,78),
            new Vector3(-42,0,78), new Vector3(-82,0,48), new Vector3(18,0,-58)
        };
        for (int i = 0; i < herbSpots.Length; i++)
            CreatePickup(resources, "Hierba_" + i, herbSpots[i], PrimitiveType.Sphere,
                new Vector3(.45f, .7f, .45f), herb, RpgResourceKind.Herb, Random.Range(1, 3), "hierbas");

        Vector3[] oreSpots =
        {
            new Vector3(-83,0,30), new Vector3(-76,0,-48), new Vector3(-52,0,-78),
            new Vector3(52,0,-78), new Vector3(78,0,-50), new Vector3(86,0,2),
            new Vector3(62,0,82), new Vector3(-62,0,82)
        };
        for (int i = 0; i < oreSpots.Length; i++)
            CreatePickup(resources, "Mineral_" + i, oreSpots[i], PrimitiveType.Cube,
                new Vector3(.85f, .7f, .8f), iron, RpgResourceKind.IronOre, 1, "mineral");

        Vector3[] woodSpots =
        {
            new Vector3(-82,0,-66), new Vector3(-68,0,-78), new Vector3(-20,0,-82),
            new Vector3(20,0,-82), new Vector3(68,0,-78), new Vector3(82,0,-66),
            new Vector3(82,0,66), new Vector3(68,0,80), new Vector3(-68,0,80),
            new Vector3(-82,0,66)
        };
        for (int i = 0; i < woodSpots.Length; i++)
            CreatePickup(resources, "Madera_" + i, woodSpots[i], PrimitiveType.Cylinder,
                new Vector3(.65f, .35f, .65f), wood, RpgResourceKind.Wood, Random.Range(1, 3), "madera");

        Vector3[] relicSpots =
        {
            new Vector3(-72f, 0f, 24f),
            new Vector3(70f, 0f, -22f),
            new Vector3(40f, 0f, 58f)
        };
        for (int i = 0; i < relicSpots.Length; i++)
            CreatePickup(resources, "Reliquia_Antigua_" + i, relicSpots[i], PrimitiveType.Sphere,
                new Vector3(.55f, .55f, .55f), gold, RpgResourceKind.AncientRelic, 1, "reliquia antigua");

        Vector3[] chestSpots =
        {
            new Vector3(-55f, 0f, 55f), new Vector3(56f, 0f, 38f),
            new Vector3(-78f, 0f, -4f), new Vector3(78f, 0f, -4f)
        };
        for (int i = 0; i < chestSpots.Length; i++)
        {
            GameObject chest = Cube(resources, "Cofre_Exploracion_" + i,
                chestSpots[i] + Vector3.up * .5f, new Vector3(1.15f, .9f, .9f), darkWoodOr(wood));
            SphereCollider trigger = chest.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.3f;
            chest.AddComponent<RpgSearchableLoot>().Configure("cofre de exploración");
            Cube(resources, "Tapa_Cofre_" + i, chestSpots[i] + Vector3.up * 1.02f,
                new Vector3(1.2f, .2f, .95f), gold);
        }
    }

    private static void CreatePickup(Transform parent, string label, Vector3 position,
        PrimitiveType type, Vector3 scale, Material material, RpgResourceKind resource,
        int amount, string displayName)
    {
        GameObject pickup = GameObject.CreatePrimitive(type);
        pickup.name = label;
        pickup.transform.SetParent(parent, false);
        pickup.transform.localPosition = position + Vector3.up * .55f;
        pickup.transform.localScale = scale;
        Renderer renderer = pickup.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = pickup.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;
        RpgWorldPickup component = pickup.AddComponent<RpgWorldPickup>();
        component.Configure(resource, amount, displayName);
    }

    private static void BuildCastleAndArena(Transform parent, Material stone, Material darkStone,
        Material wood, Material darkWood, Material gold, Material cloth, Material floor, Material wall)
    {
        Transform outsideReturn = NewPoint(parent, "Castillo_Salida_Exterior",
            new Vector3(0f, 0f, 54f), Quaternion.identity);

        GameObject hall = new GameObject("CASTILLO_INTERIOR");
        hall.transform.SetParent(parent, false);
        hall.transform.localPosition = new Vector3(260f, 0f, 0f);
        BuildCastleHall(hall.transform, stone, darkStone, wood, darkWood, gold, cloth, floor, wall);

        Transform hallEntry = NewPoint(hall.transform, "Spawn_Entrada_Castillo",
            new Vector3(0f, 1f, -5f), Quaternion.identity);
        Transform hallReturn = NewPoint(hall.transform, "Spawn_Regreso_Arena",
            new Vector3(0f, 1f, 6f), Quaternion.Euler(0f, 180f, 0f));

        GameObject arena = new GameObject("ARENA_DE_BESTIAS");
        arena.transform.SetParent(parent, false);
        arena.transform.localPosition = new Vector3(340f, 0f, 0f);
        Transform arenaCenter = NewPoint(arena.transform, "Centro_Arena", Vector3.zero, Quaternion.identity);
        BuildArenaGeometry(arena.transform, stone, darkStone, wood, gold, cloth);
        ArenaWaveManager manager = arena.AddComponent<ArenaWaveManager>();
        manager.Configure(arenaCenter, 13f, 3);

        Transform arenaEntry = NewPoint(arena.transform, "Spawn_Entrada_Arena",
            new Vector3(0f, .1f, -18f), Quaternion.identity);
        Transform arenaExitTarget = NewPoint(arena.transform, "Spawn_Salida_Arena",
            new Vector3(0f, 1f, 6f), Quaternion.Euler(0f, 180f, 0f));

        GameObject castleEntrance = PortalObject(parent, "Portal_Entrar_Castillo",
            new Vector3(0f, 1f, 60f), 2.1f);
        castleEntrance.AddComponent<RpgTeleportPortal>().Configure(
            null, hall, hallEntry, "Entrar al castillo", false);

        GameObject hallExit = PortalObject(hall.transform, "Portal_Salir_Castillo",
            new Vector3(0f, 1f, -10f), 1.8f);
        hallExit.AddComponent<RpgTeleportPortal>().Configure(
            hall, null, outsideReturn, "Salir del castillo", true);

        Transform arenaDoorReturn = NewPoint(hall.transform, "Spawn_Regreso_Desde_Arena",
            new Vector3(0f, 1f, 7.2f), Quaternion.identity);
        GameObject hallToArena = PortalObject(hall.transform, "Portal_Entrar_Arena",
            new Vector3(0f, 1f, 10f), 2f);
        hallToArena.AddComponent<RpgTeleportPortal>().Configure(
            hall, arena, arenaEntry, "Entrar a la arena", false, true);

        GameObject arenaExit = PortalObject(arena.transform, "Portal_Salir_Arena",
            new Vector3(0f, 1f, -23f), 1.9f);
        arenaExit.AddComponent<RpgTeleportPortal>().Configure(
            arena, hall, arenaDoorReturn, "Regresar al castillo", false);

        hall.SetActive(false);
        arena.SetActive(false);
    }

    private static void BuildCastleHall(Transform root, Material stone, Material darkStone,
        Material wood, Material darkWood, Material gold, Material cloth, Material floor, Material wall)
    {
        Cube(root, "Suelo_Castillo", new Vector3(0f, -.15f, 0f), new Vector3(24f, .3f, 24f), floor);
        Cube(root, "Muro_Castillo_Izquierdo", new Vector3(-12f, 4f, 0f), new Vector3(.5f, 8f, 24f), stone);
        Cube(root, "Muro_Castillo_Derecho", new Vector3(12f, 4f, 0f), new Vector3(.5f, 8f, 24f), stone);
        Cube(root, "Muro_Castillo_Fondo", new Vector3(0f, 4f, 12f), new Vector3(24f, 8f, .5f), stone);
        Cube(root, "Muro_Castillo_Frontal_Izq", new Vector3(-8f, 4f, -12f), new Vector3(8f, 8f, .5f), stone);
        Cube(root, "Muro_Castillo_Frontal_Der", new Vector3(8f, 4f, -12f), new Vector3(8f, 8f, .5f), stone);
        Cube(root, "Techo_Castillo", new Vector3(0f, 8.1f, 0f), new Vector3(24f, .35f, 24f), darkStone);
        Cube(root, "Alfombra_Castillo", new Vector3(0f, .025f, 0f), new Vector3(5f, .05f, 17f), cloth);

        for (int x = -9; x <= 9; x += 6)
        {
            Cube(root, "Columna_Castillo", new Vector3(x, 3.9f, 3.2f),
                new Vector3(.9f, 7.8f, .9f), darkStone);
            Cube(root, "Capitel_Castillo", new Vector3(x, 7.8f, 3.2f),
                new Vector3(1.35f, .45f, 1.35f), gold);
        }

        Cube(root, "Trono_Base", new Vector3(0f, .55f, 8.7f), new Vector3(3f, 1.1f, 1.7f), wood);
        Cube(root, "Trono_Respaldo", new Vector3(0f, 2.1f, 9.3f), new Vector3(2.7f, 2.4f, .4f), darkWood);
        Cube(root, "Estandarte_Central", new Vector3(0f, 5.2f, 11.45f), new Vector3(2.5f, 3.8f, .18f), cloth);

        GameObject captain = new GameObject("Capitan_del_Reino");
        captain.transform.SetParent(root, false);
        captain.transform.localPosition = new Vector3(0f, 0f, 5.2f);
        Part(captain.transform, "Cuerpo_Capitan", PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(.75f, 1f, .6f), darkStone);
        Part(captain.transform, "Cabeza_Capitan", PrimitiveType.Sphere,
            new Vector3(0f, 2.05f, 0f), new Vector3(.44f, .48f, .43f), wood);
        Part(captain.transform, "Capa_Capitan", PrimitiveType.Cube,
            new Vector3(0f, 1.1f, -.35f), new Vector3(.72f, 1.25f, .12f), cloth);
        SphereCollider captainTrigger = captain.AddComponent<SphereCollider>();
        captainTrigger.isTrigger = true;
        captainTrigger.radius = 2.0f;
        captain.AddComponent<RpgQuestNpc>();

        AddCastleTorch(root, new Vector3(-8f, 2.5f, -3f), stone, gold);
        AddCastleTorch(root, new Vector3(8f, 2.5f, -3f), stone, gold);
        AddCastleTorch(root, new Vector3(-8f, 2.5f, 8f), stone, gold);
        AddCastleTorch(root, new Vector3(8f, 2.5f, 8f), stone, gold);

        Cube(root, "Armero", new Vector3(-8.8f, 1.2f, 5f), new Vector3(1.5f, 2.4f, .7f), wood);
        Cube(root, "Cofre_Castillo", new Vector3(8.8f, .55f, 5f), new Vector3(1.5f, 1.1f, 1.1f), darkWood);
        GameObject chestTrigger = new GameObject("Buscar_Cofre_Castillo");
        chestTrigger.transform.SetParent(root, false);
        chestTrigger.transform.localPosition = new Vector3(8.8f, 1f, 5f);
        SphereCollider chestSphere = chestTrigger.AddComponent<SphereCollider>();
        chestSphere.isTrigger = true;
        chestSphere.radius = 1.4f;
        chestTrigger.AddComponent<RpgSearchableLoot>().Configure("cofre del castillo");

        PointLight(root, "Luz_Castillo_1", new Vector3(-7f, 6f, 0f), new Color(1f, .68f, .4f), 13f, 1.1f);
        PointLight(root, "Luz_Castillo_2", new Vector3(7f, 6f, 0f), new Color(1f, .68f, .4f), 13f, 1.1f);
        PointLight(root, "Luz_Castillo_3", new Vector3(0f, 6f, 9f), new Color(1f, .72f, .46f), 14f, 1.2f);
    }

    private static void BuildArenaGeometry(Transform root, Material stone, Material darkStone,
        Material wood, Material gold, Material cloth)
    {
        Cylinder(root, "Plataforma_Arena", new Vector3(0f, -.35f, 0f),
            new Vector3(50f, .35f, 50f), darkStone);
        Cylinder(root, "Piso_Interior_Arena", new Vector3(0f, -.12f, 0f),
            new Vector3(42f, .12f, 42f), stone);

        const int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 24f, 3f, Mathf.Sin(angle) * 24f);
            GameObject wallSegment = Cube(root, "Muralla_Arena",
                position, new Vector3(8.2f, 6f, 1.25f), darkStone);
            wallSegment.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
            Cube(root, "Almena_Arena", position + Vector3.up * 3.35f,
                new Vector3(1.4f, .8f, 1.5f), stone);
        }

        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4f;
            Vector3 p = new Vector3(Mathf.Cos(angle) * 18f, 0f, Mathf.Sin(angle) * 18f);
            Cube(root, "Pilar_Arena", p + Vector3.up * 2.5f,
                new Vector3(1.15f, 5f, 1.15f), stone);
            Cube(root, "Remate_Pilar_Arena", p + Vector3.up * 5.2f,
                new Vector3(1.65f, .55f, 1.65f), gold);
            AddCastleTorch(root, p + Vector3.up * .3f, wood, gold);
        }

        Cube(root, "Porton_Arena", new Vector3(0f, 2.2f, -23.2f),
            new Vector3(5f, 4.4f, .3f), wood);
        for (int x = -2; x <= 2; x += 2)
            Cube(root, "Refuerzo_Porton_Arena", new Vector3(x, 2.2f, -23f),
                new Vector3(.16f, 4.1f, .2f), gold);

        Cylinder(root, "Plataforma_Centro_Arena", new Vector3(0f, .13f, 0f),
            new Vector3(5f, .25f, 5f), gold);
        Cube(root, "Emblema_Arena", new Vector3(0f, .4f, 0f),
            new Vector3(2.2f, .18f, 2.2f), cloth);
        PointLight(root, "Luz_Arena_Centro", new Vector3(0f, 5f, 0f),
            new Color(1f, .48f, .25f), 18f, 1.1f);
    }

    private static GameObject PortalObject(Transform parent, string label, Vector3 position, float radius)
    {
        GameObject portal = new GameObject(label);
        portal.transform.SetParent(parent, false);
        portal.transform.localPosition = position;
        SphereCollider trigger = portal.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = radius;
        return portal;
    }

    private static void AddCastleTorch(Transform parent, Vector3 position, Material metal, Material flame)
    {
        Cylinder(parent, "Antorcha_Base", position + Vector3.up * .8f,
            new Vector3(.22f, .8f, .22f), metal);
        Sphere(parent, "Antorcha_Llama", position + Vector3.up * 1.8f,
            new Vector3(.38f, .62f, .38f), flame);
        PointLight(parent, "Antorcha_Luz", position + Vector3.up * 1.8f,
            new Color(1f, .55f, .28f), 8f, .8f);
    }

    private static void ApplyGraphics(Transform root)
    {
        Light sun = RenderSettings.sun;
        if (sun == null)
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light light in lights)
                if (light.type == LightType.Directional) { sun = light; break; }
        }
        if (sun == null)
        {
            GameObject sunObject = new GameObject("RPG_Sol_Direccional");
            sunObject.transform.SetParent(root, false);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        sun.color = new Color(1f, .91f, .79f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = .78f;
        sun.shadowResolution = LightShadowResolution.Medium;
        RenderSettings.sun = sun;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.43f, .51f, .62f);
        RenderSettings.ambientEquatorColor = new Color(.40f, .39f, .34f);
        RenderSettings.ambientGroundColor = new Color(.19f, .17f, .14f);
        RenderSettings.ambientIntensity = 1.05f;
        RenderSettings.reflectionIntensity = .85f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(.53f, .57f, .61f);
        RenderSettings.fogDensity = .0022f;

        QualitySettings.shadowDistance = 65f;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.antiAliasing = 2;
    }

    private static void ApplyExistingWorldMaterials(Transform town, Material grass, Material dirt, Material stone)
    {
        ApplyMaterial(town.Find("Terreno del pueblo"), grass);
        ApplyMaterial(town.Find("Camino principal"), dirt);
        ApplyMaterial(town.Find("Camino transversal"), dirt);
        ApplyMaterial(town.Find("Plaza central"), stone);

        Transform expansion = town.Find("REINO_EXPANSION");
        if (expansion != null)
        {
            ApplyMaterial(expansion.Find("Camino hacia la puerta"), dirt);
            // Mantener los materiales originales de las murallas y modelos del castillo.
        }
    }

    private static void ApplyMaterial(Transform target, Material material)
    {
        if (target == null || material == null) return;
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static Material TexturedMaterial(string name, Color tint, int seed, float metallic = 0f)
    {
        EnsureFolder("Assets/RPGGenerated/Textures");
        EnsureFolder("Assets/RPGGenerated/Materials");
        string texturePath = "Assets/RPGGenerated/Textures/T_" + name + ".asset";
        string materialPath = "Assets/RPGGenerated/Materials/M_" + name + ".mat";

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            const int size = 64;
            texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            texture.name = "T_" + name;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float broad = Mathf.PerlinNoise((x + seed * 11) * .075f, (y + seed * 7) * .075f);
                    float fine = Mathf.PerlinNoise((x + seed * 3) * .24f, (y + seed * 13) * .24f);
                    float factor = .68f + broad * .25f + fine * .17f;
                    texture.SetPixel(x, y, new Color(
                        Mathf.Clamp01(tint.r * factor),
                        Mathf.Clamp01(tint.g * factor),
                        Mathf.Clamp01(tint.b * factor), 1f));
                }
            }
            texture.Apply(true, false);
            AssetDatabase.CreateAsset(texture, texturePath);
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            material.name = "M_" + name;
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.color = Color.white;
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .28f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Transform NewRoot(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Transform NewPoint(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        return go.transform;
    }

    private static void WallCollider(Transform parent, string name, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = position;
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
    }

    private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        return go;
    }

    private static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        return go;
    }

    private static GameObject Sphere(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        return go;
    }

    private static void Part(Transform parent, string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    private static void PointLight(Transform parent, string name, Vector3 position,
        Color color, float range, float intensity)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
    }
}
#endif
