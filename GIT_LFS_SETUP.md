# Git LFS Setup für Unity RTS Projekt

## ?? Übersicht

Git LFS (Large File Storage) speichert große Dateien effizient außerhalb des Git-Repositories
und ersetzt sie durch kleine Pointer-Dateien. Das hält Ihr Repository klein und schnell.

## ? Voraussetzungen

1. Git installiert (bereits vorhanden)
2. Git LFS installieren

## ?? Installation

### Windows:

**Option 1: Über Git for Windows Installer**
- Git LFS ist bereits in modernen Git for Windows Versionen enthalten
- Überprüfen mit: `git lfs version`

**Option 2: Manueller Download**
1. Download von: https://git-lfs.github.com/
2. Installer ausführen
3. Terminal öffnen und testen: `git lfs version`

**Option 3: Mit Winget (Windows Package Manager)**
```powershell
winget install Git.LFS
```

**Option 4: Mit Chocolatey**
```powershell
choco install git-lfs
```

### Überprüfung:
```bash
git lfs version
# Sollte etwas ausgeben wie: git-lfs/3.4.0
```

## ?? Setup im Projekt

### Schritt 1: Git LFS aktivieren

```bash
# Im Projektordner (D:\Unity\Harvest\)
cd D:\Unity\Harvest

# Git LFS für dieses Repository aktivieren
git lfs install
```

**Ausgabe sollte sein:**
```
Updated Git hooks.
Git LFS initialized.
```

### Schritt 2: File Tracking konfigurieren

Erstellen/Bearbeiten Sie `.gitattributes` mit den zu trackenden Dateitypen:

```bash
# Unity Asset Dateien für LFS
git lfs track "*.psd"
git lfs track "*.psb"
git lfs track "*.ai"
git lfs track "*.png"
git lfs track "*.jpg"
git lfs track "*.jpeg"
git lfs track "*.tga"
git lfs track "*.tif"
git lfs track "*.tiff"
git lfs track "*.gif"
git lfs track "*.bmp"
git lfs track "*.exr"
git lfs track "*.hdr"

# Audio Dateien
git lfs track "*.mp3"
git lfs track "*.wav"
git lfs track "*.ogg"
git lfs track "*.aif"
git lfs track "*.aiff"

# Video Dateien
git lfs track "*.mp4"
git lfs track "*.mov"
git lfs track "*.avi"
git lfs track "*.flv"
git lfs track "*.webm"

# 3D Modelle
git lfs track "*.fbx"
git lfs track "*.obj"
git lfs track "*.max"
git lfs track "*.blend"
git lfs track "*.blend1"
git lfs track "*.blend2"
git lfs track "*.blend3"
git lfs track "*.ma"
git lfs track "*.mb"
git lfs track "*.c4d"

# Unity spezifische Binärdateien
git lfs track "*.unity"
git lfs track "*.prefab"
git lfs track "*.asset"
git lfs track "*.cubemap"
git lfs track "*.unitypackage"

# Fonts
git lfs track "*.ttf"
git lfs track "*.otf"

# Substance Designer/Painter
git lfs track "*.sbsar"
git lfs track "*.sbs"

# Andere große Dateien
git lfs track "*.pdf"
git lfs track "*.zip"
git lfs track "*.rar"
git lfs track "*.7z"

# .gitattributes zur Versionskontrolle hinzufügen
git add .gitattributes
```

### Schritt 3: Existierende große Dateien migrieren (Optional)

Wenn Sie bereits große Dateien im Repository haben:

```bash
# Alle Dateien im Repository nach LFS migrieren
git lfs migrate import --include="*.psd,*.fbx,*.png,*.jpg,*.mp3,*.wav" --everything

# Oder nur für bestimmte Branches
git lfs migrate import --include="*.psd,*.fbx,*.png,*.jpg,*.mp3,*.wav" --include-ref=refs/heads/main
```

?? **Achtung:** Dies ändert die Git-Historie! Nur verwenden wenn:
- Repository ist neu oder noch nicht geteilt
- Sie mit allen Teammitgliedern abgesprochen haben

### Schritt 4: Committen

```bash
# .gitattributes committen
git add .gitattributes
git commit -m "Setup Git LFS für Unity Assets"

# Alle Assets hinzufügen (werden jetzt über LFS getrackt)
git add Assets/
git commit -m "Add Unity assets via Git LFS"
```

## ?? Überprüfung

### LFS Status prüfen:
```bash
# Welche Dateien werden von LFS getrackt?
git lfs ls-files

# LFS Tracking-Regeln anzeigen
git lfs track

# Größe des LFS-Storages
git lfs ls-files -s
```

### Testen ob es funktioniert:
```bash
# Eine große Datei hinzufügen
# Zum Beispiel: Assets/Textures/BigTexture.png

git add Assets/Textures/BigTexture.png
git commit -m "Add test texture"

# Prüfen ob es als LFS-Datei getrackt wird
git lfs ls-files
# Sollte die Datei auflisten!
```

## ?? Push zu Remote Repository

### GitHub:
```bash
# Repository auf GitHub erstellen (mit LFS Support)
git remote add origin https://github.com/USERNAME/REPO.git
git push -u origin main
```

**GitHub LFS Limits:**
- Kostenlos: 1 GB Storage, 1 GB Bandwidth/Monat
- Pro: 50 GB Storage, 50 GB Bandwidth/Monat
- Mehr Info: https://docs.github.com/en/billing/managing-billing-for-git-large-file-storage

### GitLab:
```bash
git remote add origin https://gitlab.com/USERNAME/REPO.git
git push -u origin main
```

**GitLab LFS Limits:**
- Kostenlos: 10 GB Storage
- Mehr Info: https://docs.gitlab.com/ee/topics/git/lfs/

### Bitbucket:
```bash
git remote add origin https://bitbucket.org/USERNAME/REPO.git
git push -u origin main
```

**Bitbucket LFS Limits:**
- 1 GB Storage (kostenlos)

## ?? Empfohlene .gitattributes für Unity

Hier ist eine optimierte `.gitattributes` Datei:

```gitattributes
# Auto detect text files and perform LF normalization
* text=auto

# Unity YAML
*.mat merge=unityyamlmerge eol=lf
*.anim merge=unityyamlmerge eol=lf
*.unity merge=unityyamlmerge eol=lf
*.prefab merge=unityyamlmerge eol=lf
*.physicsMaterial2D merge=unityyamlmerge eol=lf
*.physicMaterial merge=unityyamlmerge eol=lf
*.asset merge=unityyamlmerge eol=lf
*.meta merge=unityyamlmerge eol=lf
*.controller merge=unityyamlmerge eol=lf

# LFS - Image files
*.jpg filter=lfs diff=lfs merge=lfs -text
*.jpeg filter=lfs diff=lfs merge=lfs -text
*.png filter=lfs diff=lfs merge=lfs -text
*.gif filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.ai filter=lfs diff=lfs merge=lfs -text
*.tif filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.bmp filter=lfs diff=lfs merge=lfs -text
*.exr filter=lfs diff=lfs merge=lfs -text
*.hdr filter=lfs diff=lfs merge=lfs -text

# LFS - Audio
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.aif filter=lfs diff=lfs merge=lfs -text

# LFS - Video
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.mov filter=lfs diff=lfs merge=lfs -text
*.avi filter=lfs diff=lfs merge=lfs -text
*.webm filter=lfs diff=lfs merge=lfs -text

# LFS - 3D Models
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.max filter=lfs diff=lfs merge=lfs -text
*.blend filter=lfs diff=lfs merge=lfs -text
*.dae filter=lfs diff=lfs merge=lfs -text
*.mb filter=lfs diff=lfs merge=lfs -text
*.ma filter=lfs diff=lfs merge=lfs -text

# LFS - Unity Binaries
*.unitypackage filter=lfs diff=lfs merge=lfs -text
*.cubemap filter=lfs diff=lfs merge=lfs -text

# LFS - Fonts
*.ttf filter=lfs diff=lfs merge=lfs -text
*.otf filter=lfs diff=lfs merge=lfs -text

# LFS - Substance
*.sbsar filter=lfs diff=lfs merge=lfs -text
*.sbs filter=lfs diff=lfs merge=lfs -text

# LFS - Archives
*.zip filter=lfs diff=lfs merge=lfs -text
*.7z filter=lfs diff=lfs merge=lfs -text
*.rar filter=lfs diff=lfs merge=lfs -text
```

## ?? Nützliche Befehle

### Status & Info:
```bash
# Alle LFS-getrackten Dateien anzeigen
git lfs ls-files

# LFS-Statistiken
git lfs ls-files -s

# Welche Patterns werden getrackt?
git lfs track

# LFS Environment Info
git lfs env
```

### Problembehebung:
```bash
# LFS-Cache leeren
git lfs prune

# LFS-Dateien erneut herunterladen
git lfs pull

# Nur LFS-Dateien fetchen
git lfs fetch

# LFS neu installieren
git lfs install --force
```

### Performance:
```bash
# Nur Pointer-Dateien clonen (schneller)
git clone --no-checkout https://github.com/USER/REPO.git
cd REPO
git lfs install --skip-smudge
git checkout main

# Später die tatsächlichen Dateien holen
git lfs pull
```

## ?? Häufige Probleme

### Problem: "This repository is over its data quota"
**Lösung:** 
- GitHub/GitLab Plan upgraden
- Alte LFS-Dateien aus Historie entfernen
- Alternatives Hosting (Git Annex, eigener LFS-Server)

### Problem: Push dauert sehr lange
**Lösung:**
```bash
# Nur bestimmte Dateien pushen
git lfs push origin main --object-id <OID>

# Oder: LFS Batch-Size erhöhen
git config lfs.concurrenttransfers 10
```

### Problem: "pointer file" Fehler
**Lösung:**
```bash
# LFS-Dateien reparieren
git lfs fetch --all
git lfs checkout
```

## ?? Best Practices

### ? DO:
- **Große Binärdateien** (>100 KB) via LFS tracken
- **.gitattributes** committen
- **Team informieren** über LFS-Nutzung
- **Regelmäßig `git lfs prune`** ausführen
- **LFS vor dem ersten Commit** einrichten

### ? DON'T:
- **Kleine Textdateien** über LFS (unnötiger Overhead)
- **Code-Dateien** (.cs, .js) über LFS
- **JSON/XML** Dateien über LFS (außer sehr groß)
- **Nach dem Push** LFS-Tracking ändern (Probleme für andere)

## ?? Unity-spezifische Tipps

### Was SOLLTE über LFS:
```
? Texturen (.png, .jpg, .psd)
? Audio Files (.wav, .mp3, .ogg)
? 3D Models (.fbx, .obj, .blend)
? Videos (.mp4, .mov)
? Fonts (.ttf, .otf)
? Asset Bundles
? Sehr große Prefabs (>1 MB)
```

### Was NICHT über LFS:
```
? Scripts (.cs)
? Scenes (.unity) - meist klein
? Materials (.mat) - meist klein
? Meta-Dateien (.meta)
? Kleine Prefabs (<100 KB)
? ProjectSettings
```

## ?? Git LFS mit privaten Repositories

### Self-Hosted LFS Server:
```bash
# LFS URL anpassen
git config lfs.url "https://your-lfs-server.com"

# Mit Authentifizierung
git config lfs.url "https://username:token@your-lfs-server.com"
```

## ?? Monitoring & Maintenance

### Monatliche Wartung:
```bash
# 1. Alte LFS-Objekte bereinigen
git lfs prune

# 2. Nicht mehr benötigte Dateien identifizieren
git lfs ls-files -s | sort -k3 -n -r | head -20

# 3. LFS Cache-Größe prüfen
du -sh .git/lfs
```

## ? Fertig!

Nach diesem Setup sind Ihre großen Unity-Assets optimal für Git versioniert!

**Testen Sie es:**
```bash
# Eine große Texture hinzufügen
git add Assets/Textures/MyBigTexture.png
git commit -m "Add texture via LFS"
git push

# Überprüfen
git lfs ls-files
```

---

**Viel Erfolg mit Git LFS! ????**
