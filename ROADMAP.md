# Game Project Roadmap

Bu dosya mevcut proje durumunu ve bundan sonra adim adim nasil ilerleyecegimizi takip etmek icin guncel tutulur.

Detayli mimari kararlar icin ana referans dosyasi:
- `GAME_MEMORY.md`

## 1. Mevcut Durum

Su an projede calisan veya temeli atilmis sistemler:

- Ekrana tiklayarak / dokunarak para uretimi
- Collector karakterin sahnedeki paralari toplamasI
- Toplanan paranin toplam bakiyeye eklenmesi
- Tiklama gucu upgrade sistemi
- Bina listesi, bina kart UI sistemi ve x1 / x10 / x100 / max satin alma secimi
- Binalardan pasif gelir veya manuel toplama mantigi

## 2. Aktif Gelistirme Asamasi

Su an odaklandigimiz alan:

- Ekonomi altyapisini esnek ve tekrar kullanilabilir hale getirmek

Bu asamada ciktigimiz hedefler:

- Ortak satin alma mantigi
- Pooling ile para ve floating text yonetimi
- Sonsuza giden para formatlama mantigi
- Diger sekmelerin de kullanabilecegi ortak liste / scrollview yaklasimi
- Bina gorsellerinin level arttikca dikey buyume mantigi

## 3. Asama Bazli Yol Haritasi

### Asama 1: Ekonomi Cekirdegi

- Ortak `UpgradableEntity` tabanini koru ve genislet
- Satin alma islemini merkezi hale getir
- Para ekleme / para harcama akislarini temizlestir
- Sayi formatlamasini buyuk degerler icin guclendir

Durum: Devam ediyor

### Asama 2: Tiklama ve Pooling

- Coklu dokunus para uretimini saglamlastir
- Para prefablarini pooling sistemine tasi
- Floating text objelerini pooling sistemine tasi
- Gereksiz `Instantiate/Destroy` kullanimlarini azalt

Durum: Devam ediyor

### Asama 3: Ortak UI Listesi

- Mevcut bina scrollview yapisini ortaklastir
- Diger sekmeler icin tekrar kullanilabilir kart/list mantigi kur
- Veri tipi degisse bile ayni UI uretim akisini kullan

Durum: Planlandi

### Asama 4: Bina Gorsel Buyume Sistemi

- Ilk satin alimda bina gorunsun
- Belirli level'larda bina ustune yeni kat / segment eklensin
- Gorsel buyume mantigi ekonomi kodundan ayri olsun

Durum: Planlandi

### Asama 5: Futbolcu ve Diger Sekmeler

- Futbolcu veri modeli
- Futbolcu satin alma ve upgrade mantigi
- Ekstra ozellikler sekmesi
- Ortak satin alma ve ortak UI altyapisi ile entegrasyon

Durum: Planlandi

### Asama 6: Kayit ve Kalicilik

- Para verisi kaydi
- Bina level verisi kaydi
- Upgrade verisi kaydi
- Oyuna geri girince kaldigi yerden devam

Durum: Planlandi

## 4. Simdiki Adimlar

Siradaki uygulanacak is paketi:

1. Proje hafiza dosyasini olusturmak ve referans haline getirmek
2. Ortak satin alma mantigini tek yerde toplamak
3. Para ve floating text pooling sistemini kurmak
4. Buyuk sayi formatini genisletmek
5. Ortak scrollview altyapisina gecis icin UI katmanini sadelestirmek

## 5. Notlar

- Bir sistem birden fazla yerde kullanilacaksa kopyala-yapistir yerine ortak sinif / ortak servis tercih edilecek.
- Bina, futbolcu ve upgrade gibi tum satin alma davranislari ayni mimari uzerinden yuruyen bir yapiya dogru tasinacak.
