# Script Guide

Bu dosya projedeki scriptlerin nerede durdugunu ve ne is yaptigini hizli anlamak icin tutulur.

## Klasor Yapisi

- `Assets/Scripts/Core`: Genel yardimci altyapi. Oyunun herhangi bir sisteminden kullanilabilir.
- `Assets/Scripts/Economy`: Para/bakiye gibi ekonomi cekirdegi.
- `Assets/Scripts/Gameplay/Banknotes`: Tiklayinca sahneye dusen para objeleri ve para pooling sistemi.
- `Assets/Scripts/Gameplay/Collector`: Sahnedeki paralari toplayan karakterin davranisi ve hareketi.
- `Assets/Scripts/Systems`: Oyun genelindeki manager ve satin alma/yukseltme modelleri.
- `Assets/Scripts/Data`: Unity Inspector'dan doldurulan ScriptableObject veri tanimlari.
- `Assets/Scripts/UI`: Paneller, kartlar, sekmeler ve ekrandaki metin/animasyon yardimcilari.
- `Assets/Scripts/Editor`: Sadece Unity Editor icinde calisan veri olusturma ve inspector araclari.

## Core

- `ComponentPool.cs`: Ayni prefabdan cok sayida nesneyi tekrar kullanmak icin genel havuz sinifi.
- `NumberFormatter.cs`: Buyuk sayilari `1.25K`, `10M`, `4.2B` gibi okunur hale getirir ve geri parse eder.

## Economy

- `CurrencyManager.cs`: Oyuncunun parasini tutar, para ekler/harcar ve para UI metnini gunceller.

## Gameplay / Banknotes

- `Banknote.cs`: Sahnedeki tek bir para objesi. Degerini tiklama gucunden alir ve havuza geri donebilir.
- `BanknotePool.cs`: Para objelerini `Instantiate/Destroy` yerine tekrar kullanir.
- `BanknoteRegistry.cs`: Sahnedeki toplanabilir paralari listeler ve collector icin en iyi grubu secer.
- `BanknoteSpawner.cs`: Mouse/touch girdisini okur, UI ustune tiklanmadiysa para ve floating text uretir.

## Gameplay / Collector

- `BanknoteCollectorAI.cs`: Collector karakterin hangi para grubuna gidecegini secen otomatik toplama zekasi.
- `CollectorMovement.cs`: Collector karakterin saga/sola hareketini ve ziplama hareketini yonetir.

## Systems

- `UpgradableEntity.cs`: Satin alinip level atlayabilen her seyin ortak temel modeli.
- `PurchaseService.cs`: Ortak satin alma akisi. x1, x10, x100 ve MAX satin almayi burada cozer.
- `ScaledValue.cs`: Fiyat, gelir, cooldown gibi degerleri level'a gore linear veya exponential hesaplar.
- `UpgradeManager.cs`: Tiklama gucu ve boost upgrade'lerini yonetir.
- `ClickUpgradeEntity.cs`: Tiklama basina para uretimini level'a gore hesaplar.
- `BoostUpgrade.cs`: Bina geliri, collector hizi veya kapasitesi gibi boost durumunu tutar.
- `PassiveIncomeManager.cs`: Binalarin sure doldukca gelir uretmesini ve manuel/otomatik toplamayi yonetir.
- `SaveManager.cs`: PlayerPrefs ile para, bina level'i ve upgrade level'larini kaydeder/yukler.
- `SkillManager.cs`: Beceri listesini kurar, cooldown ve aktif sureleri azaltir.
- `SkillEntity.cs`: Tek bir becerinin level, cooldown ve aktiflik durumunu tutar.
- `EffectManager.cs`: Level up ve tiklama efektlerini oynatir.
- `ThemeManager.cs`: Arka plan ve zemin renk temasini degistirir.

## Data

- `UpgradableEntityData.cs`: Upgrade edilebilir data assetlerinin ortak tabani.
- `IncomeBuildingData.cs`: Bina fiyati, geliri, uretim suresi, kart rengi ve sahne gorseli.
- `ClickUpgradeData.cs`: Tiklama gucu upgrade verisi.
- `BoostUpgradeData.cs`: Boost hedefi ve boost carpani.
- `SkillData.cs`: Beceri tipi, gucu, suresi ve cooldown verisi.
- `StoreItemData.cs`: Kit/magaza kartlarinin ucret ve odul verisi.

## UI

- `UIManager.cs`: Ana panelleri acar/kapatir.
- `TabController.cs`: Bir panel icindeki alt sekmeleri acar/kapatir.
- `UIHelper.cs`: Sahne veya prefab icinde isimle UI elemani bulmaya yarayan yardimci.
- `UIBounce.cs`: Butonlara basinca/hover edince kucuk animasyon verir.
- `PassiveIncomeDisplay.cs`: Pasif kazanci ekrandaki `ProfitText` alanlarina yazar.
- `FloatingText.cs`: Para dusunce yukari cikip solan yaziyi yonetir.
- `FloatingTextPool.cs`: Floating text objelerini havuzla tekrar kullanir.
- `SummaryStatsPanel.cs`: Zemin uzerindeki ozet istatistik yazilarini olusturur ve gunceller.
- `Panels/BaseUpgradePanel.cs`: Bina, oyuncu, kit, beceri gibi listeli panellerin ortak kart uretim tabani.
- `Panels/BuildingPanel.cs`: Bina kartlarini olusturur ve zemin istatistik panelini kurar.
- `Panels/PlayerPanel.cs`: Tiklama gucu ve boost kartlarini olusturur.
- `Panels/KitPanel.cs`: Magaza/kit kartlarini olusturur.
- `Panels/SkillsPanel.cs`: Beceri kartlarini olusturur.
- `Panels/SettingsPanel.cs`: Ayarlar, ses toggle'lari ve ilerleme sifirlama paneli.
- `Cards/ICardDataProvider.cs`: Upgrade kartlarina veri saglayan ortak arayuz.
- `Cards/CardDisplayConfig.cs`: Kartta ikon, seviye, gelir, aciklama gibi alanlar gorunsun mu ayari.
- `Cards/CardDataAdapters.cs`: Bina, collector, boost ve store item verilerini ortak kart formatina cevirir.
- `Cards/SkillCardAdapter.cs`: Skill verisini ortak kart formatina cevirir.
- `Cards/UpgradeCard.cs`: Tek bir kartin isim, fiyat, seviye, progress ve satin alma UI'ini gunceller.

## Editor

- `DataMigrator.cs`: Varsayilan bina, collector ve boost data assetlerini olusturur.
- `GameDataGenerator.cs`: Kit/magaza ve boost data assetlerini otomatik olusturur.
- `ScaledValueDrawer.cs`: Inspector'da `ScaledValue` alanlarini `1.5K`, `62.4B` gibi yazabilmeni saglar.
