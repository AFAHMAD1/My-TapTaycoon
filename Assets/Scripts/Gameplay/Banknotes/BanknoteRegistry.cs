using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BanknoteRegistry : MonoBehaviour
{
    private static readonly List<Banknote> banknotes = new List<Banknote>();

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public static void Register(Banknote banknote)
    {
        if (banknote == null) return;
        if (!banknotes.Contains(banknote))
        {
            // Bu satir: 'banknotes' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
            banknotes.Add(banknote);
        }
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public static void Cleanup()
    {
        // Bu satir: 'banknotes' objesi uzerindeki 'RemoveAll' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        banknotes.RemoveAll(b => b == null || b.isCollected);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public static void Unregister(Banknote banknote)
    {
        if (banknote == null) return;
        // Bu satir: 'banknotes' objesi uzerindeki 'Remove' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        banknotes.Remove(banknote);
    }

    // Artık hem maksimum mesafeyi (searchRadius) hem de maksimum taşıma kapasitesini (maxCount) alıyor
    public static List<Banknote> GetBestGroup(float maxRadius, int maxCount)
    {
        Cleanup();

        // Bu satir: 'banknotes' uzerindeki 'Where' metodunu cagirir ve donen sonucu 'validNotes' degiskenine kaydeder.
        var validNotes = banknotes.Where(b => !b.isCollected && !b.isReserved).ToList();

        if (validNotes.Count == 0)
            return null;

        List<Banknote> bestGroup = new List<Banknote>();

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var seed in validNotes)
        {
            Vector2 seedPos = seed.transform.position;
            List<Banknote> currentGroup = new List<Banknote>();

            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (var b in validNotes)
            {
                // Yakınlık kontrolü VE kapasite sınırını aşmama (maxCount)
                if (Vector2.Distance(seedPos, b.transform.position) <= maxRadius && currentGroup.Count < maxCount)
                {
                    // Bu satir: 'currentGroup' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                    currentGroup.Add(b);
                }
            }

            if (currentGroup.Count > bestGroup.Count)
            {
                bestGroup = currentGroup;
            }
        }

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var note in bestGroup)
        {
            note.isReserved = true;
        }

        return bestGroup;
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public static void ReleaseGroup(List<Banknote> group)
    {
        if (group == null) return;

        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < group.Count; i++)
        {
            if (group[i] != null && !group[i].isCollected)
            {
                group[i].isReserved = false;
            }
        }
    }
}
