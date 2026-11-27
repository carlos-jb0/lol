#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VillageBuilder
{
    private const string RootName = "PUEBLO_GENERADO";
    private static readonly Vector3[] HousePositions =
    {
        new Vector3(-17f, 0f, 14f), new Vector3(17f, 0f, 14f),
        new Vector3(-17f, 0f, -15f), new Vector3(17f, 0f, -15f)
    };
    private static readonly string[] ModelNames =
    {
        "Casa_Pequena", "Casa_Mediana", "Casa_Dos_Pisos", "Casa_Piedra_Porche"
    };

    [MenuItem("Pueblo/Construir pueblo explorable")]
    public static void Build()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null || player.GetComponent<CharacterController>() == null)
        {
            EditorUtility.DisplayDialog("Pueblo", "No encontré el objeto Player con CharacterController. No se modificó la escena.", "OK");
            return;
        }

        GameObject[] models = new GameObject[ModelNames.Length];
        for (int i = 0; i < ModelNames.Length; i++)
        {
            string[] guids = AssetDatabase.FindAssets(ModelNames[i] + " t:Model");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Faltan modelos",
                    "Primero descomprime los cuatro OBJ + MTL en Assets/Models/Pueblo y espera a que Unity los importe. Falta: " + ModelNames[i], "OK");
                return;
            }
            models[i] = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);
        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Construir pueblo");

        Material grass = Mat("M_Grass_Village", new Color(.32f, .48f, .25f));
        Material road = Mat("M_Dirt_Road", new Color(.48f, .37f, .25f));
        Material stone = Mat("M_Stone_Plaza", new Color(.56f, .54f, .48f));
        Material wood = Mat("M_Wood_Furniture", new Color(.35f, .22f, .12f));
        Material wall = Mat("M_Interior_Wall", new Color(.78f, .72f, .58f));
        Material gold = Mat("M_Coin_Gold", new Color(1f, .72f, .08f), .25f);

        GameObject ground = Cube(root.transform, "Terreno del pueblo", new Vector3(0,-.45f,0),
            new Vector3(78,.8f,78), grass);
        ground.isStatic = true;
        Cube(root.transform, "Camino principal", new Vector3(0,.02f,0), new Vector3(5,.12f,66), road);
        Cube(root.transform, "Camino transversal", new Vector3(0,.03f,0), new Vector3(58,.13f,4), road);
        Cube(root.transform, "Plaza central", new Vector3(0,.1f,0), new Vector3(15,.18f,15), stone);
        CreateDecor(root.transform, wood, grass);

        if (Object.FindObjectOfType<VillageGameManager>() == null)
            new GameObject("VillageGameManager").AddComponent<VillageGameManager>();
        if (player.GetComponent<PlayerInteractor>() == null)
            Undo.AddComponent<PlayerInteractor>(player);

        for (int i = 0; i < models.Length; i++)
            BuildHouse(root.transform, models[i], ModelNames[i], HousePositions[i], i, wall, wood, gold);

        player.transform.position = new Vector3(0, 1.2f, -2f);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        EditorUtility.DisplayDialog("Pueblo listo",
            "Se creó el pueblo con cuatro casas, interiores, muebles y monedas. Pulsa Play y acércate a las puertas o muebles; usa E.", "¡Vamos!");
    }

    private static void BuildHouse(Transform parent, GameObject model, string label, Vector3 pos,
        int index, Material wall, Material wood, Material gold)
    {
        GameObject outside = new GameObject(label + "_Exterior");
        outside.transform.SetParent(parent); outside.transform.position = pos;
        GameObject mesh = (GameObject)PrefabUtility.InstantiatePrefab(model);
        if (mesh == null) mesh = Object.Instantiate(model);
        mesh.name = label + "_Modelo";
        mesh.transform.SetParent(outside.transform, false);
        mesh.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        mesh.transform.localScale = Vector3.one * .9f;

        GameObject interior = new GameObject(label + "_Interior");
        interior.transform.SetParent(parent);
        interior.SetActive(false);
        BuildRoom(interior.transform, wall, wood, gold, index);

        Vector3 door = pos + new Vector3(0,0, index % 2 == 0 ? 5f : 5.5f);
        Transform outsideSpawn = NewPoint(parent, label+"_PuntoExterior", door + new Vector3(0,0,2.5f), Quaternion.identity);
        Transform insideSpawn = NewPoint(interior.transform, "PuntoInterior", new Vector3(0,1f,-3.1f), Quaternion.identity);
        AddPortal(parent, label+"_Entrar", door, outside, interior, outsideSpawn, insideSpawn, true);
        AddPortal(interior.transform, label+"_Salir", new Vector3(0,1f,-3.8f), outside, interior, outsideSpawn, insideSpawn, false);
    }

    private static void BuildRoom(Transform root, Material wall, Material wood, Material gold, int index)
    {
        Cube(root,"Piso",new Vector3(0,0,0),new Vector3(10,.25f,9),Mat("RoomFloor_"+index,new Color(.48f,.34f,.22f)));
        Cube(root,"Pared trasera",new Vector3(0,1.8f,4.5f),new Vector3(10,3.6f,.2f),wall);
        Cube(root,"Pared izquierda",new Vector3(-5,1.8f,0),new Vector3(.2f,3.6f,9),wall);
        Cube(root,"Pared derecha",new Vector3(5,1.8f,0),new Vector3(.2f,3.6f,9),wall);
        Cube(root,"Techo",new Vector3(0,3.65f,0),new Vector3(10,.2f,9),wall);
        Cube(root,"Pared frontal izquierda",new Vector3(-3.6f,1.8f,-4.5f),new Vector3(2.8f,3.6f,.2f),wall);
        Cube(root,"Pared frontal derecha",new Vector3(3.6f,1.8f,-4.5f),new Vector3(2.8f,3.6f,.2f),wall);
        Cube(root,"Cama",new Vector3(-2.8f,.65f,1.8f),new Vector3(2.4f,.7f,3.2f),wood);
        Cube(root,"Colchón",new Vector3(-2.8f,1.08f,1.8f),new Vector3(2.25f,.18f,3.05f),Mat("Mattress_"+index,new Color(.72f,.68f,.56f)));
        MakeFurniture(root,"Cómoda",new Vector3(2.8f,.7f,2.7f),new Vector3(2.1f,1.4f,.9f),wood,gold,2,index);
        MakeFurniture(root,"Mesa",new Vector3(2.6f,.55f,-1.2f),new Vector3(2f,1.1f,1.5f),wood,gold,1,index+10);
        MakeFurniture(root,"Baúl",new Vector3(-2.8f,.55f,-1.6f),new Vector3(1.6f,1.1f,1.2f),wood,gold,2,index+20);
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name="Lámpara"; lamp.transform.SetParent(root); lamp.transform.localPosition=new Vector3(0,3.1f,0);
        lamp.transform.localScale=Vector3.one*.35f; lamp.GetComponent<Renderer>().material=Mat("Lamp_"+index,new Color(1f,.82f,.48f));
        Light light=lamp.AddComponent<Light>(); light.type=LightType.Point; light.range=10; light.intensity=1.2f;
    }

    private static void MakeFurniture(Transform root,string name,Vector3 pos,Vector3 size,Material wood,Material gold,int coinCount,int seed)
    {
        GameObject furniture=Cube(root,name,pos,size,wood);
        SphereCollider trigger=furniture.AddComponent<SphereCollider>(); trigger.isTrigger=true; trigger.radius=1.35f;
        GameObject contents=new GameObject(name+"_Monedas"); contents.transform.SetParent(root); contents.SetActive(false);
        SearchableFurniture search=furniture.AddComponent<SearchableFurniture>(); search.Configure(contents,name.ToLower());
        for(int i=0;i<coinCount;i++)
        {
            GameObject coin=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name="Moneda"; coin.transform.SetParent(contents.transform);
            coin.transform.localPosition=pos+new Vector3((i-(coinCount-1)*.5f)*.55f,1.1f,.25f);
            coin.transform.localScale=new Vector3(.32f,.07f,.32f);
            coin.GetComponent<Renderer>().material=gold;
            Collider c=coin.GetComponent<Collider>(); Object.DestroyImmediate(c);
            SphereCollider pickup=coin.AddComponent<SphereCollider>(); pickup.isTrigger=true; pickup.radius=.85f;
            coin.AddComponent<CoinPickup>();
        }
    }

    private static void AddPortal(Transform parent,string name,Vector3 pos,GameObject outside,GameObject inside,
        Transform outSpawn,Transform inSpawn,bool entering)
    {
        GameObject go=new GameObject(name); go.transform.SetParent(parent); go.transform.position=pos;
        SphereCollider c=go.AddComponent<SphereCollider>(); c.isTrigger=true; c.radius=1.7f;
        go.AddComponent<HousePortal>().Configure(outside,inside,outSpawn,inSpawn,entering);
    }

    private static Transform NewPoint(Transform parent,string name,Vector3 pos,Quaternion rot)
    {
        GameObject go=new GameObject(name); go.transform.SetParent(parent); go.transform.position=pos; go.transform.rotation=rot; return go.transform;
    }

    private static GameObject Cube(Transform parent,string name,Vector3 pos,Vector3 scale,Material mat)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent);
        go.transform.localPosition=pos; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }

    private static Material Mat(string name,Color color,float metallic=0f)
    {
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");
        if(shader==null) shader=Shader.Find("Standard");
        Material m=new Material(shader); m.name=name; m.color=color; m.SetFloat("_Metallic",metallic); return m;
    }

    private static void CreateDecor(Transform root,Material wood,Material grass)
    {
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4f; Vector3 p=new Vector3(Mathf.Cos(a)*28f,0,Mathf.Sin(a)*28f);
            GameObject trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder); trunk.name="Tronco";
            trunk.transform.SetParent(root); trunk.transform.position=p+Vector3.up*1.4f;
            trunk.transform.localScale=new Vector3(.45f,1.4f,.45f); trunk.GetComponent<Renderer>().sharedMaterial=wood;
            GameObject crown=GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.name="Copa";
            crown.transform.SetParent(root); crown.transform.position=p+Vector3.up*3.3f;
            crown.transform.localScale=new Vector3(3f,3.4f,3f); crown.GetComponent<Renderer>().sharedMaterial=grass;
        }
    }
}
#endif