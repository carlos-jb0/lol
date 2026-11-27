#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KingdomExpansionBuilder
{
    private const string TownRootName = "PUEBLO_GENERADO";
    private const string ExpansionName = "REINO_EXPANSION";

    [MenuItem("Reino medieval/Expandir reino y configurar cámara")]
    public static void Build()
    {
        GameObject town = GameObject.Find(TownRootName);
        GameObject player = GameObject.Find("Player");
        if (town == null || player == null)
        {
            EditorUtility.DisplayDialog("Reino medieval",
                "Primero construye el pueblo explorable y asegúrate de que exista el objeto Player.", "OK");
            return;
        }

        Transform ground = town.transform.Find("Terreno del pueblo");
        if (ground != null) ground.localScale = new Vector3(220f, 0.8f, 220f);

        Transform mainRoad = town.transform.Find("Camino principal");
        if (mainRoad != null) mainRoad.localScale = new Vector3(5f, 0.12f, 190f);
        Transform crossRoad = town.transform.Find("Camino transversal");
        if (crossRoad != null) crossRoad.localScale = new Vector3(190f, 0.13f, 4f);

        Transform previous = town.transform.Find(ExpansionName);
        if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
        GameObject expansion = new GameObject(ExpansionName);
        Undo.RegisterCreatedObjectUndo(expansion, "Expandir reino medieval");
        expansion.transform.SetParent(town.transform, false);

        Material stone = MakeMaterial("Reino_Piedra", new Color(.31f, .32f, .34f), .12f);
        Material darkStone = MakeMaterial("Reino_PiedraOscura", new Color(.19f, .20f, .22f), .08f);
        Material roof = MakeMaterial("Reino_Tejado", new Color(.19f, .12f, .13f), .05f);
        Material wood = MakeMaterial("Reino_Madera", new Color(.25f, .14f, .075f));
        Material timber = MakeMaterial("Reino_Vigas", new Color(.12f, .075f, .045f));
        Material cloth = MakeMaterial("Reino_Estandarte", new Color(.42f, .035f, .055f));
        Material gold = MakeMaterial("Reino_Detalles", new Color(.72f, .48f, .16f), .55f);
        Material flame = MakeMaterial("Reino_Fuego", new Color(1f, .31f, .055f), .1f);
        Material water = MakeMaterial("Reino_Agua", new Color(.18f, .42f, .48f), .35f);

        BuildPerimeter(expansion.transform, stone, darkStone, cloth, gold, flame);
        BuildGatehouse(expansion.transform, stone, darkStone, wood, cloth, gold, flame);
        BuildKeep(expansion.transform, stone, darkStone, roof, cloth, gold, flame);
        BuildOuterDistrict(expansion.transform, stone, roof, wood, timber, cloth, gold);
        BuildMarketAndLandmarks(expansion.transform, stone, darkStone, roof, wood, timber, cloth, gold, water);
        BuildStreetLights(expansion.transform, darkStone, wood, flame);
        BuildCamera(player);
        BuildKnightAppearance(player.transform);

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.43f, .45f, .47f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.0035f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = expansion;
        EditorUtility.DisplayDialog("Reino expandido",
            "Se amplió el terreno, se construyeron murallas almenadas, torres, una puerta fortificada, una fortaleza, un distrito exterior y nuevos puntos de interés. También se configuró la cámara orbital y la armadura del jugador. Pulsa Play para probarlo.", "¡A explorar!");
    }

    private static void BuildPerimeter(Transform root, Material stone, Material darkStone,
        Material banner, Material gold, Material flame)
    {
        const float edge = 99f;
        const float segment = 12f;
        for (float p = -90f; p <= 90f; p += segment)
        {
            if (Mathf.Abs(p) > 10f)
                WallSegment(root, new Vector3(p, 5f, -edge), true, stone, darkStone);
            WallSegment(root, new Vector3(p, 5f, edge), true, stone, darkStone);
            WallSegment(root, new Vector3(-edge, 5f, p), false, stone, darkStone);
            WallSegment(root, new Vector3(edge, 5f, p), false, stone, darkStone);
        }

        Vector3[] towers =
        {
            new Vector3(-edge,0,-edge), new Vector3(edge,0,-edge),
            new Vector3(-edge,0,edge), new Vector3(edge,0,edge),
            new Vector3(0,0,-edge), new Vector3(0,0,edge),
            new Vector3(-edge,0,0), new Vector3(edge,0,0)
        };
        foreach (Vector3 p in towers) BuildTower(root, p, 10f, stone, darkStone, banner, gold, flame);
    }

    private static void WallSegment(Transform root, Vector3 center, bool alongX, Material stone, Material darkStone)
    {
        Vector3 size = alongX ? new Vector3(12.5f, 9f, 3.2f) : new Vector3(3.2f, 9f, 12.5f);
        Cube(root, "Muralla almenada", center, size, stone);
        for (int i = -1; i <= 1; i += 2)
        {
            Vector3 offset = alongX ? new Vector3(i * 3.4f, 5.25f, 0f) : new Vector3(0f, 5.25f, i * 3.4f);
            Vector3 merlon = alongX ? new Vector3(2.1f, 1.5f, 3.5f) : new Vector3(3.5f, 1.5f, 2.1f);
            Cube(root, "Almena", center + offset, merlon, darkStone);
        }
        Vector3 buttressOffset = alongX ? new Vector3(0f, -1.2f, 2f) : new Vector3(2f, -1.2f, 0f);
        Vector3 buttressSize = alongX ? new Vector3(1.2f, 7f, .8f) : new Vector3(.8f, 7f, 1.2f);
        Cube(root, "Contrafuerte", center + buttressOffset, buttressSize, darkStone);
    }

    private static void BuildTower(Transform root, Vector3 p, float scale, Material stone,
        Material darkStone, Material banner, Material gold, Material flame)
    {
        Cube(root, "Torre de muralla", p + Vector3.up * 6f,
            new Vector3(scale, 12f, scale), stone);
        Cube(root, "Coronamiento de torre", p + Vector3.up * 12.3f,
            new Vector3(scale + 1.2f, 1f, scale + 1.2f), darkStone);
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (scale * .37f);
            Cube(root, "Merlon de torre", p + Vector3.up * 13.3f + offset,
                new Vector3(2.2f, 1.2f, 2.2f), stone);
        }
        Cube(root, "Ventana de torre", p + new Vector3(0f, 7f, scale * .51f),
            new Vector3(1.1f, 2.3f, .18f), darkStone);
        BuildBanner(root, p + new Vector3(0f, 9f, scale * .65f), banner, gold);
        BuildTorch(root, p + new Vector3(scale * .65f, 2.5f, scale * .65f), darkStone, flame);
    }

    private static void BuildGatehouse(Transform root, Material stone, Material darkStone,
        Material wood, Material banner, Material gold, Material flame)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 p = new Vector3(side * 11f, 0f, -99f);
            BuildTower(root, p, 12f, stone, darkStone, banner, gold, flame);
            Cube(root, "Pilar de la puerta", p + new Vector3(-side * 2.5f, 5f, 0f),
                new Vector3(3f, 10f, 4f), darkStone);
        }
        Cube(root, "Arco superior de la puerta", new Vector3(0f, 10f, -99f),
            new Vector3(13f, 3f, 4f), stone);
        Cube(root, "Puerta de roble", new Vector3(0f, 4.5f, -98.5f),
            new Vector3(10f, 9f, .8f), wood);
        for (int x = -4; x <= 4; x += 2)
            Cube(root, "Refuerzo de puerta", new Vector3(x, 4.5f, -98f),
                new Vector3(.25f, 8.5f, 1f), gold);
        Cube(root, "Camino hacia la puerta", new Vector3(0f, .08f, -65f),
            new Vector3(12f, .16f, 65f), MakeMaterial("Camino_Puerta", new Color(.39f,.34f,.29f)));
    }

    private static void BuildKeep(Transform root, Material stone, Material darkStone,
        Material roof, Material banner, Material gold, Material flame)
    {
        Vector3 center = new Vector3(0f, 0f, 76f);
        Cube(root, "Fortaleza - cuerpo", center + Vector3.up * 8f,
            new Vector3(34f, 16f, 26f), stone);
        Cube(root, "Fortaleza - azotea", center + Vector3.up * 16.5f,
            new Vector3(36f, 1.2f, 28f), darkStone);
        Cube(root, "Porton de la fortaleza", center + new Vector3(0f, 4f, -13.2f),
            new Vector3(7f, 8f, .5f), roof);
        for (int x = -14; x <= 14; x += 4)
            Cube(root, "Almena de fortaleza", center + new Vector3(x, 17.5f, -13f),
                new Vector3(2f, 2f, 2f), darkStone);

        Vector3[] corners =
        {
            center + new Vector3(-17f,0,-13f), center + new Vector3(17f,0,-13f),
            center + new Vector3(-17f,0,13f), center + new Vector3(17f,0,13f)
        };
        foreach (Vector3 p in corners) BuildTower(root, p, 9f, darkStone, stone, banner, gold, flame);
        BuildBanner(root, center + new Vector3(0f, 20f, -15f), banner, gold);
        for (int x = -10; x <= 10; x += 5)
            Cube(root, "Ventana de fortaleza", center + new Vector3(x, 10f, -13.3f),
                new Vector3(1.3f, 2.8f, .2f), darkStone);
    }

    private static void BuildOuterDistrict(Transform root, Material stone, Material roof,
        Material wood, Material timber, Material banner, Material gold)
    {
        Vector3[] homes =
        {
            new Vector3(-48,0,-36), new Vector3(48,0,-36),
            new Vector3(-62,0,-8), new Vector3(62,0,-8),
            new Vector3(-48,0,38), new Vector3(48,0,38),
            new Vector3(-58,0,65), new Vector3(58,0,65),
            new Vector3(-25,0,-63), new Vector3(25,0,-63)
        };
        for (int i = 0; i < homes.Length; i++)
            BuildTownhouse(root, homes[i], i % 3 == 0 ? 8f : 7f, stone, roof, wood, timber, i);
    }

    private static void BuildTownhouse(Transform root, Vector3 p, float size, Material stone,
        Material roof, Material wood, Material timber, int index)
    {
        float w = size;
        float d = size * .78f;
        Cube(root, "Casa del distrito - muro", p + Vector3.up * 2.3f,
            new Vector3(w, 4.6f, d), stone);
        Cube(root, "Casa del distrito - tejado", p + Vector3.up * 5.1f,
            new Vector3(w + 1.2f, 1.3f, d + 1.2f), roof);
        Cube(root, "Puerta de casa", p + new Vector3(0f, 1.2f, -d * .51f),
            new Vector3(1.5f, 2.4f, .2f), wood);
        for (int side = -1; side <= 1; side += 2)
        {
            Cube(root, "Viga de fachada", p + new Vector3(side * (w * .42f), 2.3f, -d * .53f),
                new Vector3(.28f, 4.8f, .22f), timber);
            Cube(root, "Ventana de casa", p + new Vector3(side * (w * .25f), 2.8f, -d * .53f),
                new Vector3(1.15f, 1.25f, .22f), wood);
        }
        Cube(root, "Viga superior", p + new Vector3(0f, 4.35f, -d * .54f),
            new Vector3(w, .3f, .25f), timber);
        Cube(root, "Chimenea", p + new Vector3(w * .28f, 6.2f, d * .15f),
            new Vector3(.8f, 2f, .8f), stone);
        if (index % 2 == 0) BuildBanner(root, p + new Vector3(0f, 5.8f, -d * .7f), roof, wood);
    }

    private static void BuildMarketAndLandmarks(Transform root, Material stone, Material darkStone,
        Material roof, Material wood, Material timber, Material cloth, Material gold, Material water)
    {
        Vector3[] stalls =
        {
            new Vector3(-35,0,-8), new Vector3(35,0,-8),
            new Vector3(-35,0,24), new Vector3(35,0,24),
            new Vector3(-24,0,48), new Vector3(24,0,48)
        };
        foreach (Vector3 p in stalls)
        {
            Cube(root, "Puesto - mostrador", p + Vector3.up * .8f,
                new Vector3(5f, 1.6f, 2.4f), wood);
            Cube(root, "Puesto - toldo", p + Vector3.up * 3.2f,
                new Vector3(6f, .35f, 4f), cloth);
            for (int x = -1; x <= 1; x += 2)
                Cube(root, "Soporte de puesto", p + new Vector3(x * 2.6f, 2f, 0f),
                    new Vector3(.25f, 2.6f, .25f), timber);
            for (int i = 0; i < 3; i++)
                Cube(root, "Caja de mercancia", p + new Vector3(-1.5f + i * 1.4f, 1.8f, .2f),
                    new Vector3(.8f, .7f, .7f), i % 2 == 0 ? gold : timber);
        }

        Vector3 fountain = new Vector3(0f, .15f, 4f);
        Cylinder(root, "Fuente - base", fountain, new Vector3(4.5f,.5f,4.5f), darkStone);
        Cylinder(root, "Fuente - agua", fountain + Vector3.up * .35f,
            new Vector3(3.5f,.15f,3.5f), water);
        Cylinder(root, "Fuente - columna", fountain + Vector3.up * 1.3f,
            new Vector3(.65f,1.2f,.65f), stone);
        Sphere(root, "Fuente - remate", fountain + Vector3.up * 2.5f,
            new Vector3(.8f,.8f,.8f), gold);

        Vector3 well = new Vector3(0f,0f,-45f);
        Cylinder(root, "Pozo - brocal", well + Vector3.up * .7f,
            new Vector3(2.6f,1.4f,2.6f), stone);
        Cube(root, "Pozo - techo", well + Vector3.up * 3.2f,
            new Vector3(3.2f,.35f,2.8f), roof);
        for (int x = -1; x <= 1; x += 2)
            Cube(root, "Pozo - poste", well + new Vector3(x * 1.1f,2.1f,0f),
                new Vector3(.25f,3f,.25f), wood);
    }

    private static void BuildStreetLights(Transform root, Material stone, Material wood, Material flame)
    {
        for (int z = -78; z <= 78; z += 26)
        {
            BuildTorch(root, new Vector3(-7f,0,z), stone, flame);
            BuildTorch(root, new Vector3(7f,0,z), stone, flame);
        }
        for (int x = -78; x <= 78; x += 26)
        {
            BuildTorch(root, new Vector3(x,0,7f), stone, flame);
            BuildTorch(root, new Vector3(x,0,-7f), stone, flame);
        }
    }

    private static void BuildTorch(Transform root, Vector3 p, Material stone, Material flame)
    {
        Cylinder(root, "Antorcha - poste", p + Vector3.up * 1.35f,
            new Vector3(.22f,1.35f,.22f), stone);
        Sphere(root, "Antorcha - fuego", p + Vector3.up * 2.85f,
            new Vector3(.38f,.65f,.38f), flame);
        GameObject lightObject = new GameObject("Luz de antorcha");
        lightObject.transform.SetParent(root);
        lightObject.transform.localPosition = p + Vector3.up * 2.8f;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f,.48f,.2f);
        light.range = 9f;
        light.intensity = 1.1f;
    }

    private static void BuildBanner(Transform root, Vector3 p, Material cloth, Material gold)
    {
        Cylinder(root, "Mastil de estandarte", p + Vector3.up * .9f,
            new Vector3(.09f,1.1f,.09f), gold);
        Cube(root, "Estandarte del reino", p + new Vector3(0f,.35f,.12f),
            new Vector3(1.1f,1.5f,.12f), cloth);
        Cube(root, "Emblema del reino", p + new Vector3(0f,.35f,.2f),
            new Vector3(.28f,.45f,.08f), gold);
    }

    private static void BuildCamera(GameObject player)
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        CameraFollow oldFollow = camera.GetComponent<CameraFollow>();
        if (oldFollow != null) oldFollow.enabled = false;
        Behaviour[] behaviours = camera.GetComponents<Behaviour>();
        foreach (Behaviour behaviour in behaviours)
            if (behaviour != null && behaviour.GetType().Name == "CinemachineBrain")
                behaviour.enabled = false;

        SkyrimThirdPersonCamera orbit = camera.GetComponent<SkyrimThirdPersonCamera>();
        if (orbit == null) orbit = camera.gameObject.AddComponent<SkyrimThirdPersonCamera>();
        orbit.Configure(player.transform);
    }

    private static void BuildKnightAppearance(Transform player)
    {
        Transform old = player.Find("ARMADURA_REINO_GENERADA");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        Renderer[] oldRenderers = player.GetComponents<Renderer>();
        foreach (Renderer renderer in oldRenderers) renderer.enabled = false;

        GameObject armor = new GameObject("ARMADURA_REINO_GENERADA");
        armor.transform.SetParent(player, false);

        Material obsidian = MakeMaterial("Armadura_Obsidiana", new Color(.055f,.06f,.075f), .78f);
        Material edge = MakeMaterial("Armadura_AceroOscuro", new Color(.19f,.21f,.24f), .82f);
        Material red = MakeMaterial("Armadura_Carmesí", new Color(.36f,.015f,.035f), .42f);
        Material glow = MakeMaterial("Armadura_Runa", new Color(.8f,.035f,.025f), .25f);
        Material leather = MakeMaterial("Armadura_Cuero", new Color(.09f,.045f,.025f));

        ArmorCube(armor.transform,"Coraza",new Vector3(0,1.28f,0),new Vector3(.68f,.78f,.38f),obsidian);
        ArmorCube(armor.transform,"Peto central",new Vector3(0,1.35f,.205f),new Vector3(.42f,.54f,.08f),edge);
        ArmorCube(armor.transform,"Emblema carmesi",new Vector3(0,1.36f,.26f),new Vector3(.09f,.4f,.035f),red);
        ArmorCube(armor.transform,"Cinturon",new Vector3(0,.88f,0),new Vector3(.7f,.16f,.4f),leather);
        ArmorCube(armor.transform,"Hebilla",new Vector3(0,.9f,.22f),new Vector3(.15f,.15f,.06f),glow);

        for (int side=-1; side<=1; side+=2)
        {
            ArmorSphere(armor.transform,"Hombrera",new Vector3(side*.43f,1.68f,0),
                new Vector3(.42f,.3f,.43f),obsidian);
            ArmorCube(armor.transform,"Borde de hombrera",new Vector3(side*.43f,1.69f,.2f),
                new Vector3(.38f,.08f,.08f),edge);
            ArmorCapsule(armor.transform,"Brazo blindado",new Vector3(side*.47f,1.28f,0),
                new Vector3(.2f,.53f,.2f),obsidian);
            ArmorCube(armor.transform,"Guantelete",new Vector3(side*.48f,.91f,.04f),
                new Vector3(.25f,.28f,.27f),edge);
            ArmorCapsule(armor.transform,"Muslera",new Vector3(side*.2f,.57f,0),
                new Vector3(.24f,.55f,.27f),obsidian);
            ArmorCube(armor.transform,"Greba",new Vector3(side*.2f,.27f,.09f),
                new Vector3(.27f,.38f,.34f),edge);
            ArmorCube(armor.transform,"Bota",new Vector3(side*.2f,.09f,.13f),
                new Vector3(.3f,.18f,.45f),leather);
        }

        ArmorSphere(armor.transform,"Yelmo",new Vector3(0,2.03f,0),
            new Vector3(.43f,.48f,.39f),obsidian);
        ArmorCube(armor.transform,"Visera",new Vector3(0,1.99f,.34f),
            new Vector3(.32f,.13f,.09f),edge);
        ArmorCube(armor.transform,"Ranura roja",new Vector3(0,2.01f,.395f),
            new Vector3(.22f,.035f,.025f),glow);
        ArmorCube(armor.transform,"Cresta del yelmo",new Vector3(0,2.42f,-.02f),
            new Vector3(.12f,.32f,.15f),red);
        for (int side=-1; side<=1; side+=2)
        {
            ArmorCapsule(armor.transform,"Cuerno del yelmo",
                new Vector3(side*.32f,2.25f,-.02f),new Vector3(.13f,.42f,.13f),edge);
        }
        ArmorCube(armor.transform,"Capa carmesi",new Vector3(0,1.18f,-.27f),
            new Vector3(.58f,.95f,.08f),red);
        ArmorCube(armor.transform,"Placa dorsal",new Vector3(0,1.35f,-.22f),
            new Vector3(.62f,.62f,.12f),obsidian);
    }

    private static void ArmorCube(Transform p,string n,Vector3 pos,Vector3 scale,Material m)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=n; go.transform.SetParent(p,false); go.transform.localPosition=pos;
        go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
    }
    private static void ArmorSphere(Transform p,string n,Vector3 pos,Vector3 scale,Material m)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name=n; go.transform.SetParent(p,false); go.transform.localPosition=pos;
        go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
    }
    private static void ArmorCapsule(Transform p,string n,Vector3 pos,Vector3 scale,Material m)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name=n; go.transform.SetParent(p,false); go.transform.localPosition=pos;
        go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
    }

    private static GameObject Cube(Transform parent,string name,Vector3 pos,Vector3 size,Material mat)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=pos;
        go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }
    private static GameObject Cylinder(Transform parent,string name,Vector3 pos,Vector3 size,Material mat)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=pos;
        go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }
    private static GameObject Sphere(Transform parent,string name,Vector3 pos,Vector3 size,Material mat)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=pos;
        go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }
    private static Material MakeMaterial(string name,Color color,float metallic=0f)
    {
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");
        if(shader==null) shader=Shader.Find("Standard");
        Material material=new Material(shader); material.name=name; material.color=color;
        material.SetFloat("_Metallic",metallic); return material;
    }
}
#endif