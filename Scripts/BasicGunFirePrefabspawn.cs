using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class BasicGunFirePrefabspawn : MonoBehaviour
{

    private bool shouldShoot => Input.GetKey(shootKey);

    [Header("Gun variables")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float shootRate = 8f;
    private float nextShootTime;
    [SerializeField] private int ammoCount = 30;
    [SerializeField] private int maxAmmoCount = 30;
    [SerializeField] private float reloadTime = 2f;
    private bool isReloading = false;
    private bool canShoot = true;
    private bool isShooting ;

    [Header("Controlls")]
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;

    public Text AmmoText;

    private void Start()
    {

        GameObject ammoTextObject = GameObject.Find("AmmoText");
        AmmoText = ammoTextObject.GetComponent<Text>();

        UpdateAmmoText();
        
        
    }
    //--
    private void Update()
    {
    
    if (canShoot)
	    handleShoot();

    if (Input.GetKeyDown(KeyCode.R) && ammoCount < maxAmmoCount && !isReloading)
        {
        Reload();
        }
    }
    //--
    private void handleShoot(){
	 if (shouldShoot && !isShooting)
        StartCoroutine(Shoot());
	}

    //--

    private void Reload()
    {
        isReloading = true;
        UpdateAmmoText();
        Invoke("FinishReload", reloadTime);
    }

    private void FinishReload()
    {
        isReloading = false;
        ammoCount = maxAmmoCount;
        UpdateAmmoText();
    }

    private void UpdateAmmoText(){
        if (isReloading)
        {
        AmmoText.text = "Reloading...";
        Debug.Log("reloading");
        }else
        {
        AmmoText.text = ammoCount + " / " + maxAmmoCount;
        }
    }
private IEnumerator Shoot()
{
    isShooting = true;

    while (ammoCount > 0 && !isReloading && shouldShoot)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * speed;

        ammoCount--;
        UpdateAmmoText();

        yield return new WaitForSeconds(1f / shootRate);
    }

    isShooting = false;
}
}

