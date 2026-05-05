# Game Memory

Bu dosya oyunun mantigini, mimarisini ve birlikte aldigimiz kararlari kalici olarak tutar.
Yeni bir goreve baslarken once bu dosyaya bakilacak ve sistemler buna gore gelistirilecek.

Scriptlerin tek tek ne yaptigini anlamak icin tamamlayici rehber:
- `SCRIPT_GUIDE.md`

## Oyun Ozeti

- Tur: mobil idle / clicker / management oyunu.
- Ana dongu:
  1. Oyuncu ekrana dokunur ve para uretir.
  2. Collector karakter sahnedeki paralari toplar.
  3. Toplanan para ile bina, upgrade ve ileride futbolcu sistemleri satin alinir.
  4. Binalar pasif gelir veya manuel toplama gelirleri uretir.
  5. Ileride futbolcu ve diger sekmeler ayni ekonomi altyapisini kullanir.

## Ana Tasarim Kararlari

- Tek bir is birden fazla yerde kullaniliyorsa ortak bir temel sinif veya ortak servis uzerinden cozulmeli.
- OOP temelli, esnek, tekrar kullanilabilir bir yapi tercih edilecek.
- Satin alma islemi tek mantik olacak: kullanici para harcar, ilgili sistem level / adet kazanir, UI kendini gunceller.
- Upgrade mantigi tek yere toplanacak: bina, tiklama gucu, futbolcu, ozel yetenek gibi sistemler ayni satin alma altyapisini kullanacak.
- ScrollView tab yapisi tekrar kullanilabilir olacak: binalar, ekstra ozellikler, futbolcular ve benzeri sekmeler ortak listeleme mantigina dayanacak.
- Para gorselleri ve floating text objeleri `Instantiate/Destroy` yerine pooling ile yonetilecek.
- Para sayisi teorik olarak sinirsiza yakin buyuyebilir; UI kisaltmasi kademeli harflendirme ile ilerleyecek.

## GitHub Calisma Kararlari

- `main` branch'i calisir ve temiz tutulacak.
- Yeni isler `feature/...`, bug fixler `fix/...`, duzenlemeler `refactor/...`, dokuman isleri `docs/...` branch'lerinde yapilacak.
- `main` branch'ine dogrudan commit atilmadan Pull Request uzerinden merge edilecek.
- Unity `.meta` dosyalari ilgili asset/script ile birlikte commitlenecek.
- Ayni sahne veya prefab uzerinde iki kisi ayni anda calismamaya dikkat edecek.
- Ortak workflow detaylari icin `CONTRIBUTING.md` referans alinacak.

## Ekonomi Kurallari

- Ekrana tek dokunus = bir para uretimi.
- Coklu dokunus desteklenecek; ayni anda 3, 4, 5 veya 10 parmak dokunursa her dokunus ayrica para uretecek.
- UI ustune basildiginda para uretimi olmayacak.
- Parayi collector topladiginda toplam bakiyeye eklenir.
- Bir bina veya upgrade satin alindiginda para aninda dusmelidir.
- Satin alma butonu farkli sekmelerde kullanilacak ortak bir davranis olmali.
- Tiklama basina kazanilan para ayri upgrade'lerle artirilabilir.
- Iki kati para gibi boost mantiklari da ayni ekonomi altyapisina uyumlu olmali.

## Bina Sistemi Kurallari

- Binalar satin alinabilir ve level kazanir.
- Ilk satin alimdan sonra sahnede bina gorunur.
- Belirli levellerde ayni binanin ustune yeni bir kat / parca eklenir.
- Hedef: bina tek seferde baska modele donusmek yerine dikey olarak buyuyormus hissi vermeli.
- Bu sistem gorsel tarafta prefab parcalari veya kat verileri ile surdurulebilir olmali.
- Mantik ve gorsel birbirinden ayrilmali: ekonomi level bilgisini verir, gorsel sistem buna gore kac kat/segment gosterilecegini belirler.

## UI ve Tab Hedefleri

- Bina sekmesindeki kart mantigi diger sekmelere de tasinabilir olmali.
- Tek tip liste / kart uretim mantigi ile farkli veri tipleri gosterilebilmeli.
- Satin al x1 / x10 / x100 / MAX mantigi ortak kullanilacak.

## Mevcut Teknik Yon

- `UpgradableEntity`: ortak gelistirilebilir veri tabani.
- `ClickUpgradeEntity`: tiklama gucu upgrade modeli.
- `IncomeBuilding`: `PassiveIncomeManager.cs` icinde duran bina ve pasif gelir modeli.
- `CurrencyManager`: toplam para ve para harcama yonetimi.
- `PassiveIncomeManager`: bina uretim dongusu.
- `BaseUpgradePanel`, `UpgradeCard` ve card adapter siniflari: bina, oyuncu, kit ve beceri kart listeleri icin ortak UI altyapisi.
- `PurchaseService`: x1 / x10 / x100 / MAX satin alma akisinin merkezi.
- `ComponentPool`, `BanknotePool`, `FloatingTextPool`: para ve floating text pooling altyapisi.
- `SaveManager`: para, bina level'i, tiklama upgrade'i ve boost level'larini PlayerPrefs uzerinden kaydeder/yukler.
- `SkillManager` ve `SkillEntity`: beceri listesi, cooldown ve aktif sure durumlarini yonetir.

## Tamamlanan Duzenleme Kararlari

- Asil calisilan Unity projesi `Game_project` klasorudur.
- Scriptler amacina gore klasorlere ayrildi; yeni klasor yapisi `SCRIPT_GUIDE.md` icinde detayli anlatilir.
- `CollectorAI` adi `BanknoteCollectorAI` olarak degistirildi.
- `CollectorMover` adi `CollectorMovement` olarak degistirildi.
- `MoneyDisplay` adi `PassiveIncomeDisplay` olarak degistirildi.
- Script tasimalarinda `.meta` dosyalari korundu, Unity prefab/sahne referanslari yeni script isimlerine guncellendi.
- MAX satin alma mantigi max level sinirini asmayacak sekilde duzeltildi.
- Kayit yukleme sirasi duzeltildi: `SaveManager`, diger manager'lar `Awake` islemlerini tamamladiktan sonra `Start` icinde yukleme yapar.
- Para yuklemesi `CurrencyManager.SetMoney()` ile UI'i de gunceller.
- `CurrencyManager` ve `SkillManager` icin singleton ve null durumlari daha guvenli hale getirildi.

## Script Klasor Yonlendirmesi

- `Assets/Scripts/Core`: ortak yardimci altyapi.
- `Assets/Scripts/Economy`: para yonetimi.
- `Assets/Scripts/Gameplay/Banknotes`: sahnedeki para uretimi/toplama objeleri.
- `Assets/Scripts/Gameplay/Collector`: collector karakterin zeka ve hareket kodlari.
- `Assets/Scripts/Systems`: manager'lar, upgrade modelleri, kayit ve genel oyun sistemleri.
- `Assets/Scripts/Data`: ScriptableObject veri tanimlari.
- `Assets/Scripts/UI`: paneller, kartlar, sekmeler ve UI yardimcilari.
- `Assets/Scripts/Editor`: Unity Editor icinde calisan veri olusturma ve inspector araclari.

## Siradaki Oncelikler

1. Bina gorsel buyume sistemi icin kat/segment tabanli temel yapiyi kurmak.
2. Skill etkilerini gercek ekonomiye baglamak: gelir carpani, otomatik tiklama, anlik nakit gibi etkiler su an taslak seviyesinde.
3. Kit/magaza sistemine elmas, odeme veya odul kaliciligi eklemek.
4. Kayit sistemini genisletmek: skill level/cooldown, kit satin alma durumu, bina timer durumlari ve ayarlar.
5. Futbolcu ve diger sekmeleri mevcut `UpgradableEntity`, `PurchaseService`, `BaseUpgradePanel` altyapisina eklemek.
6. Unity Editor icinde sahne/prefab uzerinden oyun akisini test edip eksik referanslari temizlemek.

## Not

Kullanicidan "git dosyaya bak ve ona gore hareket edelim" denildiginde bu dosya referans alinacak.
