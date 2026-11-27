using UnityEngine;

public class RpgCombatController : MonoBehaviour
{
    [SerializeField] private float dodgeDistance = 3.1f;
    [SerializeField] private float dodgeDuration = 0.22f;
    [SerializeField] private float dodgeCooldown = 1.15f;
    private CharacterController characterController;
    private RpgEquipment equipment;
    private float nextAttackTime;
    private float nextDodgeTime;
    private float dodgeTimer;
    private Vector3 dodgeDirection;
    private bool blocking;

    public bool IsBlocking => blocking && equipment != null && equipment.HasShield;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        equipment = GetComponent<RpgEquipment>();
    }

    private void Update()
    {
        if (RpgMerchant.IsAnyShopOpen) { blocking = false; return; }

        HandleWeaponKeys();
        blocking = equipment != null && equipment.HasShield && Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0))
            Attack();

        if (Input.GetKeyDown(KeyCode.Q))
            TryDodge();

        if (dodgeTimer > 0f)
        {
            float step = dodgeDistance / Mathf.Max(.05f, dodgeDuration) * Time.deltaTime;
            if (characterController != null && characterController.enabled)
                characterController.Move(dodgeDirection * step);
            dodgeTimer -= Time.deltaTime;
        }
    }

    private void HandleWeaponKeys()
    {
        if (equipment == null) return;
        TryEquipKey(KeyCode.Alpha1, RpgWeaponType.Sword);
        TryEquipKey(KeyCode.Alpha2, RpgWeaponType.Dagger);
        TryEquipKey(KeyCode.Alpha3, RpgWeaponType.Katana);
        TryEquipKey(KeyCode.Alpha4, RpgWeaponType.Bow);
        TryEquipKey(KeyCode.Alpha5, RpgWeaponType.Crossbow);
        TryEquipKey(KeyCode.Alpha6, RpgWeaponType.EclipseSword);
        TryEquipKey(KeyCode.Alpha7, RpgWeaponType.AncientBow);
    }

    private void TryEquipKey(KeyCode key, RpgWeaponType weapon)
    {
        if (!Input.GetKeyDown(key)) return;
        if (equipment.EquipWeapon(weapon))
            ShowMessage("Equipaste: " + weapon);
        else
            ShowMessage("Todavía no tienes esa arma. Consíguela o cómprala.");
    }

    private void Attack()
    {
        if (equipment == null || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + equipment.AttackCooldown;

        if (equipment.IsRanged && equipment.EquippedWeapon != RpgWeaponType.None)
        {
            FireProjectile();
            return;
        }

        Vector3 direction = transform.forward;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            direction = mainCamera.transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude > .01f) direction.Normalize();
        }

        Vector3 origin = transform.position + Vector3.up * 1.05f + direction * .8f;
        Collider[] hits = Physics.OverlapSphere(origin, equipment.WeaponRange * .8f,
            ~0, QueryTriggerInteraction.Ignore);
        ArenaBeastAI best = null;
        float bestDistance = float.PositiveInfinity;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;
            ArenaBeastAI beast = hit.GetComponentInParent<ArenaBeastAI>();
            if (beast == null || beast.IsDefeated) continue;
            Vector3 toTarget = beast.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > .01f && Vector3.Dot(direction, toTarget.normalized) < .18f)
                continue;
            float distance = toTarget.sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = beast;
            }
        }

        if (best != null)
            best.TakeDamage(equipment.WeaponDamage);
    }

    private void FireProjectile()
    {
        Camera mainCamera = Camera.main;
        Vector3 direction = mainCamera != null ? mainCamera.transform.forward : transform.forward;
        Vector3 start = mainCamera != null
            ? mainCamera.transform.position + direction * .7f
            : transform.position + Vector3.up * 1.3f + direction;

        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrow.name = "Proyectil_RPG";
        arrow.transform.position = start;
        arrow.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        arrow.transform.localScale = new Vector3(.045f, .42f, .045f);
        Collider collider = arrow.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        Renderer renderer = arrow.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = new Color(.55f, .34f, .15f);
            renderer.sharedMaterial = material;
        }

        RpgArrowProjectile projectile = arrow.AddComponent<RpgArrowProjectile>();
        projectile.Configure(direction.normalized, equipment.WeaponDamage,
            equipment.EquippedWeapon == RpgWeaponType.Crossbow ? 58f : 42f);
    }

    private void TryDodge()
    {
        if (Time.time < nextDodgeTime) return;
        nextDodgeTime = Time.time + dodgeCooldown;
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude < .05f) input = -transform.forward;
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 forward = mainCamera.transform.forward;
                Vector3 right = mainCamera.transform.right;
                forward.y = 0f; right.y = 0f;
                input = forward.normalized * input.z + right.normalized * input.x;
            }
        }
        dodgeDirection = input.normalized;
        dodgeTimer = dodgeDuration;
    }

    private void ShowMessage(string value)
    {
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage(value);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(18, Screen.height - 72, 390, 24),
            "Combate: clic izq. atacar | clic der. bloquear | Q esquivar | 1-7 equipar");
        if (IsBlocking)
            GUI.Box(new Rect(Screen.width / 2f - 68, Screen.height - 150, 136, 28), "BLOQUEANDO");
    }
}

public class RpgArrowProjectile : MonoBehaviour
{
    private Vector3 direction;
    private int damage;
    private float speed;
    private float life = 2.2f;
    private float radius = .12f;

    public void Configure(Vector3 travelDirection, int hitDamage, float travelSpeed)
    {
        direction = travelDirection;
        damage = hitDamage;
        speed = travelSpeed;
    }

    private void Update()
    {
        float distance = speed * Time.deltaTime;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, radius, direction, out hit,
            distance, ~0, QueryTriggerInteraction.Ignore))
        {
            ArenaBeastAI beast = hit.collider.GetComponentInParent<ArenaBeastAI>();
            if (beast != null) beast.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        transform.position += direction * distance;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
