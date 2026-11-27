using UnityEngine;

public class RpgEquipmentVisuals : MonoBehaviour
{
    private Transform equipmentRoot;
    private GameObject weaponModel;
    private GameObject shieldModel;
    private GameObject armorModel;
    private RpgEquipment equipment;

    private void Start()
    {
        equipment = GetComponent<RpgEquipment>();
        Transform oldArmor = transform.Find("ARMADURA_REINO_GENERADA");
        if (oldArmor != null) oldArmor.gameObject.SetActive(false);
        Refresh();
    }

    public void Refresh()
    {
        if (equipment == null) equipment = GetComponent<RpgEquipment>();
        if (equipmentRoot == null)
        {
            GameObject root = new GameObject("RPG_EQUIPO_VISUAL");
            equipmentRoot = root.transform;
            equipmentRoot.SetParent(transform, false);
        }

        ClearChild(weaponModel);
        ClearChild(shieldModel);
        ClearChild(armorModel);
        weaponModel = null;
        shieldModel = null;
        armorModel = null;

        if (equipment == null) return;
        if (equipment.EquippedWeapon != RpgWeaponType.None)
            weaponModel = BuildWeapon(equipment.EquippedWeapon);
        if (equipment.HasShield)
            shieldModel = BuildShield(equipment.HasEclipseShield);
        if (equipment.ArmorRating > 0)
            armorModel = BuildArmor(equipment.ArmorRating);
    }

    private void ClearChild(GameObject target)
    {
        if (target != null) Destroy(target);
    }

    private GameObject BuildWeapon(RpgWeaponType type)
    {
        GameObject root = new GameObject("Arma_" + type);
        root.transform.SetParent(equipmentRoot, false);
        root.transform.localPosition = new Vector3(.42f, .92f, .33f);
        root.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);

        Material metal = MakeMaterial("RPG_Weapon_Metal", new Color(.63f, .68f, .72f), .72f);
        Material dark = MakeMaterial("RPG_Weapon_Handle", new Color(.18f, .095f, .045f));
        Material gold = MakeMaterial("RPG_Weapon_Gold", new Color(.78f, .55f, .18f), .55f);
        Material wood = MakeMaterial("RPG_Weapon_Wood", new Color(.36f, .20f, .09f));

        if (type == RpgWeaponType.Bow || type == RpgWeaponType.AncientBow)
        {
            Part(root.transform, "Arco", PrimitiveType.Capsule, new Vector3(0, .33f, 0),
                new Vector3(.08f, .62f, .08f), type == RpgWeaponType.AncientBow ? gold : wood,
                Quaternion.Euler(0, 0, 90));
            Part(root.transform, "Cuerda", PrimitiveType.Cube, new Vector3(0, .33f, .055f),
                new Vector3(.025f, .86f, .025f), metal, Quaternion.identity);
        }
        else if (type == RpgWeaponType.Crossbow)
        {
            Part(root.transform, "Cuerpo ballesta", PrimitiveType.Cube, new Vector3(0, .1f, 0),
                new Vector3(.12f, .62f, .13f), dark, Quaternion.identity);
            Part(root.transform, "Arco ballesta", PrimitiveType.Cube, new Vector3(0, .42f, 0),
                new Vector3(.76f, .09f, .1f), wood, Quaternion.identity);
            Part(root.transform, "Virola", PrimitiveType.Cube, new Vector3(0, .18f, .09f),
                new Vector3(.08f, .28f, .06f), gold, Quaternion.identity);
        }
        else
        {
            bool dagger = type == RpgWeaponType.Dagger;
            bool katana = type == RpgWeaponType.Katana;
            bool legendary = type == RpgWeaponType.EclipseSword;
            float bladeLength = dagger ? .48f : katana ? 1.05f : .82f;
            Vector3 bladeScale = new Vector3(.075f, bladeLength, .055f);
            Part(root.transform, "Hoja", PrimitiveType.Cube, new Vector3(0, bladeLength * .5f + .16f, 0),
                bladeScale, legendary ? gold : metal, katana ? Quaternion.Euler(0, 0, -5) : Quaternion.identity);
            Part(root.transform, "Guarda", PrimitiveType.Cube, new Vector3(0, .13f, 0),
                new Vector3(.32f, .07f, .1f), gold, Quaternion.identity);
            Part(root.transform, "Empuñadura", PrimitiveType.Cylinder, new Vector3(0, -.06f, 0),
                new Vector3(.055f, .17f, .055f), dark, Quaternion.identity);
            Part(root.transform, "Pomo", PrimitiveType.Sphere, new Vector3(0, -.23f, 0),
                new Vector3(.11f, .11f, .11f), gold, Quaternion.identity);
        }

        return root;
    }

    private GameObject BuildShield(bool legendary)
    {
        GameObject root = new GameObject("Escudo_Equipado");
        root.transform.SetParent(equipmentRoot, false);
        root.transform.localPosition = new Vector3(-.43f, 1.08f, .26f);
        Material shield = MakeMaterial("RPG_Shield", legendary ? new Color(.12f, .18f, .3f) : new Color(.32f, .22f, .12f), .2f);
        Material trim = MakeMaterial("RPG_Shield_Trim", new Color(.78f, .56f, .2f), .55f);
        Part(root.transform, "Placa", PrimitiveType.Cube, Vector3.zero,
            new Vector3(.58f, .72f, .12f), shield, Quaternion.identity);
        Part(root.transform, "Emblema", PrimitiveType.Sphere, new Vector3(0, .02f, -.075f),
            new Vector3(.2f, .2f, .045f), trim, Quaternion.identity);
        Part(root.transform, "Borde superior", PrimitiveType.Cube, new Vector3(0, .36f, -.07f),
            new Vector3(.62f, .055f, .06f), trim, Quaternion.identity);
        return root;
    }

    private GameObject BuildArmor(int rating)
    {
        GameObject root = new GameObject("Armadura_Equipada");
        root.transform.SetParent(equipmentRoot, false);
        Material baseMat = MakeMaterial("RPG_Armor_Base",
            rating >= 90 ? new Color(.08f, .09f, .13f) :
            rating >= 35 ? new Color(.34f, .39f, .43f) : new Color(.30f, .17f, .09f),
            rating >= 35 ? .72f : .12f);
        Material trim = MakeMaterial("RPG_Armor_Trim", rating >= 90 ? new Color(.62f, .14f, .08f) : new Color(.72f, .55f, .25f), .5f);

        Part(root.transform, "Peto", PrimitiveType.Cube, new Vector3(0, 1.28f, .04f),
            new Vector3(.72f, .75f, .4f), baseMat, Quaternion.identity);
        Part(root.transform, "Peto frontal", PrimitiveType.Cube, new Vector3(0, 1.32f, .25f),
            new Vector3(.42f, .48f, .07f), trim, Quaternion.identity);
        for (int side = -1; side <= 1; side += 2)
        {
            Part(root.transform, "Hombrera", PrimitiveType.Sphere, new Vector3(side * .45f, 1.67f, 0),
                new Vector3(.4f, .28f, .39f), baseMat, Quaternion.identity);
            Part(root.transform, "Greba", PrimitiveType.Cube, new Vector3(side * .2f, .34f, .08f),
                new Vector3(.28f, .48f, .32f), baseMat, Quaternion.identity);
        }
        if (rating >= 35)
            Part(root.transform, "Yelmo", PrimitiveType.Sphere, new Vector3(0, 2.02f, 0),
                new Vector3(.43f, .46f, .39f), baseMat, Quaternion.identity);
        return root;
    }

    private void Part(Transform parent, string label, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, Quaternion rotation)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = label;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localRotation = rotation;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    private Material MakeMaterial(string name, Color color, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        return material;
    }
}
