# GitHub Collaboration Guide

Bu proje iki kisi veya daha fazla kisiyle gelistirilirken `main` branch'i temiz ve calisir tutulacak.
Kod, sahne ve prefab degisiklikleri kucuk branch'ler uzerinden yapilip Pull Request ile birlestirilecek.

## Temel Kural

- `main` branch'ine dogrudan commit atilmaz.
- Her is icin ayri branch acilir.
- Is bitince Pull Request acilir.
- Pull Request en az bir kisi tarafindan kontrol edildikten sonra merge edilir.
- Unity `.meta` dosyalari asset/script ile birlikte commitlenir.
- `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `.vs`, `.csproj`, `.sln` commitlenmez.

## Branch Isimleri

- Yeni ozellik: `feature/kisa-aciklama`
- Bug fix: `fix/kisa-aciklama`
- Refactor/duzenleme: `refactor/kisa-aciklama`
- Dokuman: `docs/kisa-aciklama`

Ornek:

```bash
git switch main
git pull
git switch -c feature/building-visual-growth
```

## Gunluk Calisma Akisi

1. Calismaya baslamadan once:

```bash
git switch main
git pull
git switch -c feature/yapilacak-is
```

2. Is yaparken kucuk ve anlamli commitler at:

```bash
git status
git add .
git commit -m "Add building visual growth data"
```

3. Branch'i GitHub'a gonder:

```bash
git push -u origin feature/yapilacak-is
```

4. GitHub uzerinden Pull Request ac.

## Unity Icin Ozel Kurallar

- Ayni anda ayni sahneyi iki kisi duzenlememeye calisin. Sahne dosyalari merge conflict cikarmaya yatkindir.
- Prefab degistirirken mumkunse sahne yerine prefab dosyasini duzenleyin.
- Script tasirken `.cs.meta` dosyasini da beraber tasiyin. Unity referanslari GUID ile bagli kalir.
- Yeni asset eklerken ilgili `.meta` dosyasinin da git status'ta oldugunu kontrol edin.
- Buyuk binary assetler eklemeden once ekipce karar verin. Gerekirse Git LFS'e gecilir.

## Pull Request Kontrol Listesi

- Oyun aciliyor mu?
- Console'da yeni hata var mi?
- Degisen sahne/prefab bilerek mi degisti?
- Yeni script dogru klasorde mi?
- `SCRIPT_GUIDE.md`, `GAME_MEMORY.md` veya `ROADMAP.md` guncellenmeli mi?
- Commit icinde `Library`, `Temp`, `.csproj`, `.sln` gibi uretilen dosyalar yok mu?

## Conflict Cikarsa

- Once sakin kalin; Unity conflictleri normaldir.
- Hangi dosyada conflict oldugunu `git status` ile gorun.
- Sahne/prefab conflictlerinde dosyayi kimin degistirdigini konusun.
- Emin degilseniz conflictli dosyayi rastgele secmeyin; once dosyanin amacini kontrol edin.

## Dokuman Guncelleme Kurali

- Mimari karar degisirse `GAME_MEMORY.md` guncellenir.
- Yol haritasi veya siradaki isler degisirse `ROADMAP.md` guncellenir.
- Script eklenir, tasinir veya adi degisirse `SCRIPT_GUIDE.md` guncellenir.
