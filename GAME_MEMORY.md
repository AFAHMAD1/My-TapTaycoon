# Game Memory

Bu dosya oyunun mantigini, mimarisini ve birlikte aldigimiz kararları kalici olarak tutar.
Yeni bir goreve baslarken once bu dosyaya bakilacak ve sistemler buna gore gelistirilecek.

## Oyun Ozeti

- Tur: mobil idle / clicker / management oyunu
- Ana dongu:
  1. Oyuncu ekrana dokunur ve para uretir.
  2. Collector karakter sahnedeki paralari toplar.
  3. Toplanan para ile bina, upgrade ve ileride futbolcu sistemleri satin alinir.
  4. Binalar pasif gelir veya manuel toplama gelirleri uretir.
  5. Ileride futbolcu ve diger sekmeler ayni ekonomi altyapisini kullanir.

## Ana Tasarim Kararlari

- Tek bir is birden fazla yerde kullaniliyorsa ortak bir temel sinif veya ortak servis uzerinden cozulmeli.
- OOP temelli, esnek, tekrar kullanilabilir bir yapi tercih edilecek.
- Satin alma islemi tek mantik olacak:
  Kullanici para harcar, ilgili sistem level / adet kazanir, UI kendini gunceller.
- Upgrade mantigi tek yere toplanacak:
  Bina, tiklama gucu, futbolcu, ozel yetenek gibi sistemler ayni satin alma altyapisini kullanacak.
- ScrollView tab yapisi tekrar kullanilabilir olacak:
  Binalar, ekstra ozellikler, futbolcular ve benzeri sekmeler ortak listeleme mantigina dayanacak.
- Para gorselleri ve floating text objeleri `Instantiate/Destroy` yerine pooling ile yonetilecek.
- Para sayisi teorik olarak sinirsiza yakin buyuyebilir; UI kisaltmasi kademeli harflendirme ile ilerleyecek.

## Ekonomi Kurallari

- Ekrana tek dokunus = bir para uretimi.
- Coklu dokunus desteklenecek:
  Ayni anda 3, 4, 5 veya 10 parmak dokunursa her dokunus ayrica para uretecek.
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
- Hedef:
  bina tek seferde baska modele donusmek yerine dikey olarak buyuyormus hissi vermeli.
- Bu sistem gorsel tarafta prefab parcalari veya kat verileri ile surdurulebilir olmali.
- Mantik ve gorsel birbirinden ayrilmali:
  ekonomi level bilgisini verir, gorsel sistem buna gore kac kat/segment gosterilecegini belirler.

## UI ve Tab Hedefleri

- Bina sekmesindeki kart mantigi diger sekmelere de tasinabilir olmali.
- Tek tip liste / kart uretim mantigi ile farkli veri tipleri gosterilebilmeli.
- Satin al x1 / x10 / x100 / max mantigi ortak kullanilacak.

## Mevcut Teknik Yon

- `UpgradableEntity`: ortak gelistirilebilir veri tabani
- `ClickUpgradeEntity`: tiklama gucu upgrade modeli
- `IncomeBuilding`: bina ve pasif gelir modeli
- `CurrencyManager`: toplam para ve para harcama yonetimi
- `PassiveIncomeManager`: bina uretim dongusu
- `BuildingUIManager` ve `BuildingUIItem`: bina kart listesi

## Siradaki Oncelikler

1. Ortak satin alma mantigini merkezi hale getirmek
2. Para ve floating text objelerini pooling'e tasimak
3. ScrollView / kart yapisini sekmeler arasi tekrar kullanilabilir hale getirmek
4. Buyuk sayi formatlamasini sonsuz kisaltma mantigina genisletmek
5. Bina gorsel buyume sistemi icin kat/segment tabanli temel yapiyi kurmak
6. Kayit sistemi gelmeden once ekonomi modellerini kalici veriye uygun hale getirmek

## Not

Kullanicidan "git dosyaya bak ve ona gore hareket edelim" denildiginde bu dosya referans alinacak.
