# ?? Git LFS - Quick Reference Card

## ?? Schnellstart

```bash
# 1. LFS installieren (einmalig)
git lfs install

# 2. Dateitypen tracken
git lfs track "*.psd"
git lfs track "*.fbx"

# 3. .gitattributes committen
git add .gitattributes
git commit -m "Setup Git LFS"

# 4. Normal arbeiten
git add Assets/
git commit -m "Add assets"
git push
```

## ?? Status & Info

```bash
# LFS-getrackte Dateien anzeigen
git lfs ls-files

# LFS-Status
git lfs status

# Dateigröße anzeigen
git lfs ls-files -s

# Tracking-Regeln anzeigen
git lfs track

# Welche Dateien würden getrackt?
git lfs track --list

# LFS Environment Info
git lfs env
```

## ?? Häufige Befehle

### Tracking hinzufügen:
```bash
# Einzelner Dateityp
git lfs track "*.png"

# Mehrere Dateitypen
git lfs track "*.png" "*.jpg" "*.psd"

# Bestimmter Ordner
git lfs track "Assets/Textures/**"

# Bestimmte Datei
git lfs track "Assets/HugeFile.fbx"
```

### Tracking entfernen:
```bash
# Tracking-Regel entfernen
git lfs untrack "*.png"

# Alle Regeln anzeigen
cat .gitattributes
```

### Dateien holen:
```bash
# Alle LFS-Dateien herunterladen
git lfs pull

# Nur bestimmte Dateien
git lfs pull --include="Assets/Textures/*"

# Außer bestimmte Dateien
git lfs pull --exclude="Assets/Videos/*"
```

### Aufräumen:
```bash
# Alte LFS-Objekte löschen
git lfs prune

# Alte Objekte älter als X Tage
git lfs prune --verify-remote --older-than 7d

# Was würde gelöscht?
git lfs prune --dry-run --verbose
```

## ?? Diagnose

### Problem: Dateien sind Pointer
```bash
# LFS-Dateien reparieren
git lfs fetch --all
git lfs checkout

# Oder einzelne Datei
git lfs pull --include="path/to/file.png"
```

### Problem: Große Repository-Größe
```bash
# Größe prüfen
du -sh .git/lfs

# Cache bereinigen
git lfs prune

# Historie bereinigen (VORSICHT!)
git lfs migrate import --everything
```

### Problem: Push schlägt fehl
```bash
# Nur LFS-Dateien pushen
git lfs push origin main

# Mit bestimmtem Objekt
git lfs push origin main --object-id <OID>

# Alle Objekte
git lfs push origin --all
```

## ?? Migration

### Existierende Dateien zu LFS migrieren:
```bash
# Alle Branches
git lfs migrate import --include="*.psd,*.fbx" --everything

# Nur main Branch
git lfs migrate import --include="*.psd,*.fbx" --include-ref=refs/heads/main

# Mit Voransicht
git lfs migrate info --include="*.psd"
```

## ?? Konfiguration

### Globale Einstellungen:
```bash
# Concurrent Transfers erhöhen (schnellerer Upload)
git config lfs.concurrenttransfers 10

# Batch-Size anpassen
git config lfs.batch true

# LFS-URL anpassen
git config lfs.url "https://your-lfs-server.com"
```

### Repository-spezifisch:
```bash
# In .lfsconfig
[lfs]
    url = "https://your-lfs-server.com"
    concurrent = 10
```

## ?? Dateien ausschließen

### .gitattributes anpassen:
```gitattributes
# LFS für alle PNGs
*.png filter=lfs diff=lfs merge=lfs -text

# ABER nicht in diesem Ordner
Assets/Icons/*.png !filter !diff !merge
```

## ?? Performance

### Schnelleres Clonen:
```bash
# Nur Pointer-Dateien clonen
git clone --no-checkout https://github.com/user/repo.git
cd repo
git lfs install --skip-smudge
git checkout main

# Später: Dateien bei Bedarf holen
git lfs pull --include="Assets/Textures/*"
```

### Partielle Checkouts:
```bash
# LFS Download überspringen
GIT_LFS_SKIP_SMUDGE=1 git clone ...

# Nur bestimmte Dateien
git lfs fetch --include="Assets/Audio/*"
git lfs checkout Assets/Audio/
```

## ?? Statistiken

```bash
# Anzahl LFS-Dateien
git lfs ls-files | wc -l

# Gesamtgröße
git lfs ls-files -s | awk '{s+=$1} END {print s/1024/1024 " MB"}'

# Top 10 größte Dateien
git lfs ls-files -s | sort -k1 -n -r | head -10

# Nach Dateityp
git lfs ls-files | grep -o '\.[^.]*$' | sort | uniq -c | sort -rn
```

## ??? Troubleshooting

### "Repository over quota"
```bash
# Überprüfen Sie Ihr LFS-Kontingent
git lfs ls-files -s

# Alte Dateien aus Geschichte entfernen
git lfs migrate export --include="*.old" --everything
```

### "Pointer file corruption"
```bash
# Einzelne Datei reparieren
rm Assets/broken-file.psd
git checkout Assets/broken-file.psd
git lfs pull --include="Assets/broken-file.psd"
```

### "Smudge error"
```bash
# LFS Cache zurücksetzen
rm -rf .git/lfs
git lfs fetch --all
git lfs checkout
```

## ?? Best Practices

### ? Empfohlen:
- Dateien >100 KB via LFS
- .gitattributes vor erstem Commit
- Regelmäßig `git lfs prune`
- Team über LFS informieren

### ? Vermeiden:
- Code-Dateien über LFS
- Häufige Änderungen an LFS-Dateien
- Tracking nach erstem Push ändern
- Zu viele kleine Dateien (<100 KB)

## ?? Workflow

### Tägliche Arbeit:
```bash
# 1. Pullen (mit LFS)
git pull
git lfs pull

# 2. Arbeiten...
# Assets bearbeiten, hinzufügen, etc.

# 3. Committen
git add .
git commit -m "Update assets"

# 4. Pushen
git push
# LFS-Dateien werden automatisch gepusht!
```

### Bei Team-Kollaboration:
```bash
# Vor dem Pullen
git lfs fetch

# Status prüfen
git lfs status

# Dann pullen
git pull
git lfs pull
```

## ?? Nützliche Links

- **Offizielle Docs**: https://git-lfs.github.com/
- **GitHub LFS**: https://docs.github.com/en/repositories/working-with-files/managing-large-files
- **GitLab LFS**: https://docs.gitlab.com/ee/topics/git/lfs/
- **Atlassian Tutorial**: https://www.atlassian.com/git/tutorials/git-lfs

## ?? Pro-Tipps

### Tip 1: LFS Log
```bash
# Detailliertes LFS-Logging aktivieren
GIT_TRACE=1 git lfs push origin main
GIT_CURL_VERBOSE=1 git lfs push origin main
```

### Tip 2: Partial Clone (Git 2.22+)
```bash
# Nur Tree ohne Blobs
git clone --filter=blob:none --no-checkout <url>
git lfs install --skip-smudge
git checkout main
```

### Tip 3: Pre-push Hook
```bash
# .git/hooks/pre-push
#!/bin/sh
git lfs pre-push "$@"
```

### Tip 4: Automatisches Prune
```bash
# Nach jedem Pull aufräumen
git config --global alias.lpull '!git pull && git lfs prune'
# Nutzen: git lpull
```

## ?? Checkliste - Neues Projekt

- [ ] Git LFS installiert (`git lfs version`)
- [ ] LFS initialisiert (`git lfs install`)
- [ ] .gitattributes erstellt und getrackt
- [ ] .gitignore konfiguriert
- [ ] Tracking-Regeln definiert
- [ ] .gitattributes committed
- [ ] Erstes Commit mit Assets
- [ ] Remote Repository konfiguriert
- [ ] Team informiert über LFS-Nutzung
- [ ] LFS-Quote geprüft (GitHub/GitLab)

---

**Quick Help:**
```bash
git lfs help           # Hilfe anzeigen
git lfs help [command] # Hilfe für Befehl
git lfs version        # Version prüfen
```

**Schnell-Setup:**
```bash
git lfs install && \
git lfs track "*.psd" "*.fbx" "*.png" && \
git add .gitattributes && \
git commit -m "Setup LFS"
```
