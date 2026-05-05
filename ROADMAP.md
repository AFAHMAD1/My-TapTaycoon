# Game Project Roadmap

Bu dosya mevcut proje durumunu ve bundan sonra adim adim nasil ilerleyecegimizi takip etmek icin guncel tutulur.

Detayli mimari kararlar icin ana referans dosyasi:
- `GAME_MEMORY.md`

Scriptlerin klasor yapisi ve gorevleri icin pratik rehber:
- `SCRIPT_GUIDE.md`

GitHub uzerinden birlikte calisma kurallari:
- `CONTRIBUTING.md`

## 1. Mevcut Durum

Su an projede calisan veya temeli atilmis sistemler:

- Ekrana tiklayarak / dokunarak para uretimi.
- Coklu touch girdisi ve UI ustune basinca para uretimini engelleme.
- Collector karakterin sahnedeki paralari toplama davranisi.
- Toplanan paranin toplam bakiyeye eklenmesi.
- Tiklama gucu upgrade sistemi.
- Bina listesi, bina kart UI sistemi ve x1 / x10 / x100 / MAX satin alma secimi.
- Binalardan pasif gelir veya manuel toplama mantigi.
- Para ve floating text objeleri icin pooling altyapisi.
- Ortak satin alma servisi: `PurchaseService`.
- Ortak upgrade tabani: `UpgradableEntity`.
- Ortak UI kart/list altyapisi: `BaseUpgradePanel`, `UpgradeCard`, card adapter siniflari.
- Buyuk sayi formatlama: `NumberFormatter`.
- Temel kayit/yukleme sistemi: `SaveManager`.
- Script klasor yapisi ve anlasilir script isimleri.
- GitHub PR/issue sablonlari ve ekip calisma rehberi.

## 2. Aktif Gelistirme Asamasi

Su an odaklandigimiz alan:

- Mevcut ekonomi ve UI altyapisinin ustune yeni oyun sistemlerini guvenli sekilde eklemek.

Bu asamada ciktigimiz hedefler:

- Bina gorsellerinin level arttikca dikey buyume mantigi.
- Skill etkilerinin gercek ekonomiye baglanmasi.
- Kit/magaza odullerinin ve elmas sisteminin kalici hale gelmesi.
- Kayit sisteminin skill, kit, bina timer ve ayarlar icin genislemesi.
- Futbolcu ve diger sekmelerin ortak upgrade/UI altyapisina eklenmesi.

## 3. Asama Bazli Yol Haritasi

### Asama 1: Ekonomi Cekirdegi

- Ortak `UpgradableEntity` tabanini koru ve genislet.
- Satin alma islemini merkezi hale getir.
- Para ekleme / para harcama akislarini temizlestir.
- Sayi formatlamasini buyuk degerler icin guclendir.

Durum: Tamamlandi, genisletmeye hazir.

### Asama 2: Tiklama ve Pooling

- Coklu dokunus para uretimini saglamlastir.
- Para prefablarini pooling sistemine tasi.
- Floating text objelerini pooling sistemine tasi.
- Gereksiz `Instantiate/Destroy` kullanimlarini azalt.

Durum: Tamamlandi, sahne/prefab testleriyle ince ayar gerekebilir.

### Asama 3: Ortak UI Listesi

- Mevcut bina scrollview yapisini ortaklastir.
- Diger sekmeler icin tekrar kullanilabilir kart/list mantigi kur.
- Veri tipi degisse bile ayni UI uretim akisini kullan.

Durum: Tamamlandi, yeni sekmeler eklenirken ayni altyapi kullanilacak.

### Asama 4: Bina Gorsel Buyume Sistemi

- Ilk satin alimda bina gorunsun.
- Belirli level'larda bina ustune yeni kat / segment eklensin.
- Gorsel buyume mantigi ekonomi kodundan ayri olsun.

Durum: Planlandi.

### Asama 5: Skill ve Kit Sistemleri

- Skill etkilerini ekonomiye bagla.
- Skill level/cooldown bilgisini kayda ekle.
- Kit/magaza odullerini gercek oyun sistemlerine bagla.
- Elmas veya odeme katmanini tasarla.

Durum: Planlandi.

### Asama 6: Futbolcu ve Diger Sekmeler

- Futbolcu veri modeli.
- Futbolcu satin alma ve upgrade mantigi.
- Ekstra ozellikler sekmesi.
- Ortak satin alma ve ortak UI altyapisi ile entegrasyon.

Durum: Planlandi.

### Asama 7: Kayit ve Kalicilik

- Para verisi kaydi.
- Bina level verisi kaydi.
- Upgrade verisi kaydi.
- Oyuna geri girince kaldigi yerden devam.
- Skill, kit, ayar ve bina timer verilerinin kaydi.
- Offline kazanc hesabinin test edilmesi.

Durum: Temel sistem var, genisletilecek.

## 4. Simdiki Adimlar

Siradaki uygulanacak is paketi:

1. Unity Editor icinde Main sahnesini acip prefab/sahne referanslarini gorsel olarak test etmek.
2. Bina gorsel buyume sistemi icin `IncomeBuildingData` uzerine kat/segment veri modeli eklemek.
3. `PassiveIncomeManager` icindeki ekonomi mantigini bozmadan bina gorsel kontrol scripti tasarlamak.
4. Skill etkilerini `SkillEffectType` uzerinden gercek davranislara baglamak.
5. Kayit sistemini skill, kit ve bina timer bilgilerini kapsayacak sekilde genisletmek.
6. GitHub'da `main` branch korumasi acip Pull Request ile calisma duzenini zorunlu hale getirmek.

## 5. Notlar

- Bir sistem birden fazla yerde kullanilacaksa kopyala-yapistir yerine ortak sinif / ortak servis tercih edilecek.
- Bina, futbolcu ve upgrade gibi tum satin alma davranislari ayni mimari uzerinden yuruyen bir yapiya dogru tasinacak.
- Yeni script eklenirse once uygun klasor secilecek, sonra `SCRIPT_GUIDE.md` ve gerekirse bu roadmap guncellenecek.
- Ekip calismasinda branch ve Pull Request kurallari icin `CONTRIBUTING.md` takip edilecek.
