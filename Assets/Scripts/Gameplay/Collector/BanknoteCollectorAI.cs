using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sahnedeki paraları otomatik olarak toplayan yapay zeka sınıfıdır.
/// En karlı para grubunu belirler, oraya gider ve toplar.
/// </summary>
[RequireComponent(typeof(CollectorMovement))]
public class BanknoteCollectorAI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private CollectorMovement mover;

    [Header("Algılama Ayarları")]
    [SerializeField] private float groundLevelThreshold = 0.15f; // Yerde olup olmadığını anlamak için eşik değer
    public float searchRadius = 2.5f; // Ne kadarlık bir alanda para arasın?

    [Header("Kapasite Ayarları")]
    public int baseCapacity = 4; // Başlangıç kapasitesi
    // Geliştirmelerle (UpgradeManager) artan güncel kapasite
    public int CurrentMaxCapacity => baseCapacity + (UpgradeManager.Instance != null ? UpgradeManager.Instance.GetCollectorCapacityBonus() : 0);

    private bool isBusy = false; // Şu an bir toplama işlemi yapıyor mu?
    private Vector3 basePosition; // Başlangıç (yer) pozisyonu
    private float lastSearchTime = 0f; // Son arama zamanı
    private const float searchCooldown = 0.25f; // Aramalar arası bekleme süresi (Saniyede 4 kez)

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (mover == null) mover = GetComponent<CollectorMovement>();
        basePosition = transform.position;
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        if (mover == null) return;

        // Eğer meşgul değilse ve bekleme süresi dolduysa yeni bir para grubu ara
        if (!isBusy && Time.time > lastSearchTime + searchCooldown)
        {
            lastSearchTime = Time.time;
            // BanknoteRegistry üzerinden en iyi (en yakın/kalabalık) para grubunu çek
            List<Banknote> group = BanknoteRegistry.GetBestGroup(searchRadius, CurrentMaxCapacity);

            if (group != null && group.Count > 0)
            {
                StartCoroutine(ProcessGroup(group));
            }
        }
    }

    /// <summary>
    /// Belirlenen para grubuna gidip toplama sürecini yöneten Coroutine.
    /// </summary>
    private System.Collections.IEnumerator ProcessGroup(List<Banknote> group)
    {
        isBusy = true;

        if (group == null || group.Count == 0)
        {
            BanknoteRegistry.ReleaseGroup(group);
            isBusy = false;
            yield break;
        }

        // 1. Hedef noktayı bul: Grubun tam merkez noktası
        Vector3 centerPos = Vector3.zero;
        int validCount = 0;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var b in group)
        {
            if (b != null)
            {
                centerPos += b.transform.position;
                validCount++;
            }
        }
        
        if (validCount == 0)
        {
            BanknoteRegistry.ReleaseGroup(group);
            isBusy = false;
            yield break;
        }
        
        centerPos /= validCount;

        // 2. Yatay eksende (X) hedefe doğru ilerle
        yield return mover.MoveToX(centerPos.x);

        // 3. Eğer hedef yukarıdaysa zıpla
        float heightDifference = Mathf.Abs(centerPos.y - basePosition.y);
        if (heightDifference > groundLevelThreshold)
        {
            Vector3 jumpTarget = new Vector3(centerPos.x, centerPos.y, transform.position.z);
            yield return mover.JumpToPoint(jumpTarget);
        }

        // 4. Hedefe varıldı! Gruptaki tüm paraları topla ve CurrencyManager'a ekle
        foreach (var banknote in group)
        {
            if (banknote != null && !banknote.isCollected)
            {
                banknote.isCollected = true;
                if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddMoney(banknote.Value);
                banknote.Release(); // Parayı havuza geri gönder
            }
        }

        // 5. Eğer havadaysa tekrar yere in
        if (Mathf.Abs(transform.position.y - basePosition.y) > groundLevelThreshold)
        {
            Vector3 downTarget = new Vector3(transform.position.x, basePosition.y, transform.position.z);
            yield return mover.JumpToPoint(downTarget);
        }

        // Pozisyonu tam olarak yere sabitle
        transform.position = new Vector3(transform.position.x, basePosition.y, transform.position.z);

        BanknoteRegistry.ReleaseGroup(group);
        isBusy = false;
    }
}
