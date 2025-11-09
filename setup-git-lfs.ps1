# Git LFS Quick Setup Script für Unity RTS Projekt
# Führen Sie dieses Script aus, um Git LFS automatisch einzurichten

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Git LFS Setup für Unity RTS" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if Git is installed
Write-Host "Prüfe Git Installation..." -ForegroundColor Yellow
try {
    $gitVersion = git --version
    Write-Host "? Git gefunden: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "? Git ist nicht installiert!" -ForegroundColor Red
    Write-Host "Bitte installieren Sie Git von: https://git-scm.com/" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Check if Git LFS is installed
Write-Host "Prüfe Git LFS Installation..." -ForegroundColor Yellow
try {
    $lfsVersion = git lfs version
 Write-Host "? Git LFS gefunden: $lfsVersion" -ForegroundColor Green
} catch {
    Write-Host "? Git LFS ist nicht installiert!" -ForegroundColor Red
    Write-Host "Installation:" -ForegroundColor Yellow
    Write-Host "  1. Download von: https://git-lfs.github.com/" -ForegroundColor Yellow
    Write-Host "  2. Oder mit Winget: winget install Git.LFS" -ForegroundColor Yellow
    Write-Host "  3. Oder mit Chocolatey: choco install git-lfs" -ForegroundColor Yellow
    
    $install = Read-Host "Möchten Sie versuchen, Git LFS jetzt zu installieren? (j/n)"
    if ($install -eq "j" -or $install -eq "y") {
   Write-Host "Versuche Installation mit Winget..." -ForegroundColor Yellow
        try {
   winget install Git.LFS
            Write-Host "? Git LFS erfolgreich installiert!" -ForegroundColor Green
        } catch {
            Write-Host "? Automatische Installation fehlgeschlagen." -ForegroundColor Red
   Write-Host "Bitte installieren Sie Git LFS manuell und führen Sie dieses Script erneut aus." -ForegroundColor Yellow
    exit 1
        }
    } else {
        exit 1
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Git LFS Setup wird gestartet..." -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Initialize Git LFS
Write-Host "1. Initialisiere Git LFS..." -ForegroundColor Yellow
try {
    git lfs install
    Write-Host "? Git LFS erfolgreich initialisiert!" -ForegroundColor Green
} catch {
    Write-Host "? Fehler bei der LFS-Initialisierung!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Check if .gitattributes exists
Write-Host "2. Prüfe .gitattributes Datei..." -ForegroundColor Yellow
if (Test-Path ".gitattributes") {
    Write-Host "? .gitattributes Datei gefunden!" -ForegroundColor Green
    Write-Host "   Die Datei wurde bereits mit LFS-Regeln erstellt." -ForegroundColor Gray
} else {
    Write-Host "? .gitattributes nicht gefunden!" -ForegroundColor Red
    Write-Host "   Bitte stellen Sie sicher, dass die .gitattributes Datei existiert." -ForegroundColor Yellow
}

Write-Host ""

# Check Git repository
Write-Host "3. Prüfe Git Repository..." -ForegroundColor Yellow
if (Test-Path ".git") {
    Write-Host "? Git Repository gefunden!" -ForegroundColor Green
    
    # Check if .gitattributes is committed
    Write-Host ""
    Write-Host "4. Committe .gitattributes..." -ForegroundColor Yellow
    try {
        git add .gitattributes
        git add .gitignore
        $commitResult = git commit -m "Setup Git LFS tracking rules"
        Write-Host "? .gitattributes committed!" -ForegroundColor Green
    } catch {
 Write-Host "? .gitattributes bereits committed oder keine Änderungen." -ForegroundColor Yellow
    }
} else {
    Write-Host "? Kein Git Repository gefunden!" -ForegroundColor Yellow
    Write-Host "   Möchten Sie ein neues Repository initialisieren?" -ForegroundColor Yellow
    $initRepo = Read-Host "   Repository initialisieren? (j/n)"
    
    if ($initRepo -eq "j" -or $initRepo -eq "y") {
        git init
  Write-Host "? Git Repository initialisiert!" -ForegroundColor Green
        
    git add .gitattributes
    git add .gitignore
        git commit -m "Initial commit with Git LFS setup"
 Write-Host "? Initial commit erstellt!" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Git LFS Status" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Show tracked patterns
Write-Host ""
Write-Host "Getrackte LFS-Patterns:" -ForegroundColor Yellow
git lfs track

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Setup abgeschlossen! ?" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Nächste Schritte:" -ForegroundColor Cyan
Write-Host "  1. Assets hinzufügen: git add Assets/" -ForegroundColor White
Write-Host "  2. Committen: git commit -m 'Add Unity assets'" -ForegroundColor White
Write-Host "  3. Remote hinzufügen: git remote add origin <URL>" -ForegroundColor White
Write-Host "  4. Pushen: git push -u origin main" -ForegroundColor White
Write-Host ""

Write-Host "Nützliche Befehle:" -ForegroundColor Cyan
Write-Host "  • LFS-Dateien anzeigen: git lfs ls-files" -ForegroundColor White
Write-Host "  • LFS-Status: git lfs status" -ForegroundColor White
Write-Host "  • LFS-Größe: git lfs ls-files -s" -ForegroundColor White
Write-Host "  • LFS bereinigen: git lfs prune" -ForegroundColor White
Write-Host ""

Write-Host "Dokumentation: Siehe GIT_LFS_SETUP.md" -ForegroundColor Gray
Write-Host ""

# Offer to show current LFS files
$showFiles = Read-Host "Möchten Sie jetzt LFS-getrackte Dateien anzeigen? (j/n)"
if ($showFiles -eq "j" -or $showFiles -eq "y") {
  Write-Host ""
    Write-Host "Aktuell von LFS getrackte Dateien:" -ForegroundColor Yellow
    git lfs ls-files
    
    Write-Host ""
    Write-Host "LFS-Statistik:" -ForegroundColor Yellow
    git lfs ls-files -s | Measure-Object -Property Length -Sum | Select-Object Count, @{Name="Size (MB)";Expression={[math]::Round($_.Sum / 1MB, 2)}}
}

Write-Host ""
Write-Host "Fertig! ??" -ForegroundColor Green
