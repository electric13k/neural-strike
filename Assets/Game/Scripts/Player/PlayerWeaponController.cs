using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon")]
    public Weapon currentWeapon;
    public Transform weaponHolder;

    [Header("Input")]
    public KeyCode reloadKey = KeyCode.R;
    public bool semiAuto = false; // true for pistols, false for automatic

    private bool fireHeldLastFrame;

    private void Update()
    {
        HandleFireInput();
        HandleReloadInput();
    }

    private void HandleFireInput()
    {
        bool firePressed = Input.GetButtonDown("Fire1");
        bool fireHeld = Input.GetButton("Fire1");

        if (currentWeapon == null) return;

        if (semiAuto)
        {
            if (firePressed)
            {
                currentWeapon.TryFire(gameObject);
            }
        }
        else
        {
            if (fireHeld)
            {
                if (!fireHeldLastFrame)
                {
                    currentWeapon.TryFire(gameObject);
                }
                else
                {
                    currentWeapon.TryFire(gameObject);
                }
            }
        }

        fireHeldLastFrame = fireHeld;
    }

    private void HandleReloadInput()
    {
        if (currentWeapon == null) return;

        if (Input.GetKeyDown(reloadKey))
        {
            currentWeapon.TryReload();
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        currentWeapon = newWeapon;
    }
}