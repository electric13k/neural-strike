using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon")]
    public Weapon    currentWeapon;
    public Transform weaponHolder;

    [Header("Input")]
    public KeyCode reloadKey = KeyCode.R;
    public bool    semiAuto  = false;

    private bool fireHeldLastFrame;

    private void Update()
    {
        HandleFireInput();
        HandleReloadInput();
    }

    private void HandleFireInput()
    {
        if (currentWeapon == null) return;

        bool pressed = Input.GetButtonDown("Fire1");
        bool held    = Input.GetButton("Fire1");

        if (semiAuto)
        {
            if (pressed) currentWeapon.TryFire(gameObject);
        }
        else
        {
            if (held) currentWeapon.TryFire(gameObject);
        }

        fireHeldLastFrame = held;
    }

    private void HandleReloadInput()
    {
        if (currentWeapon == null) return;
        if (Input.GetKeyDown(reloadKey))
            currentWeapon.TryReload();
    }

    /// <summary>Equip a weapon component directly.</summary>
    public void EquipWeapon(Weapon newWeapon)
    {
        currentWeapon = newWeapon;
    }

    /// <summary>Instantiate a weapon prefab into weaponHolder and equip it.</summary>
    public void EquipWeaponFromPrefab(GameObject prefab)
    {
        if (prefab == null || weaponHolder == null) return;
        foreach (Transform c in weaponHolder) Destroy(c.gameObject);
        var go = Instantiate(prefab, weaponHolder);
        go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        var w = go.GetComponent<Weapon>();
        if (w != null) currentWeapon = w;
    }
}
