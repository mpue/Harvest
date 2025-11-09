@echo off
REM Git LFS Setup Script für Unity RTS Projekt
REM Einfache Batch-Version für schnelles Setup

echo ================================
echo Git LFS Quick Setup
echo ================================
echo.

REM Check Git
echo Pruefe Git Installation...
git --version >nul 2>&1
if errorlevel 1 (
    echo [FEHLER] Git ist nicht installiert!
    echo Bitte installieren Sie Git von: https://git-scm.com/
 pause
    exit /b 1
)
echo [OK] Git gefunden!
echo.

REM Check Git LFS
echo Pruefe Git LFS Installation...
git lfs version >nul 2>&1
if errorlevel 1 (
    echo [FEHLER] Git LFS ist nicht installiert!
    echo.
    echo Installation Optionen:
    echo   1. Download: https://git-lfs.github.com/
    echo   2. Winget: winget install Git.LFS
    echo   3. Chocolatey: choco install git-lfs
    echo.
    pause
    exit /b 1
)
echo [OK] Git LFS gefunden!
echo.

echo ================================
echo Starte Git LFS Setup...
echo ================================
echo.

REM Initialize Git LFS
echo 1. Initialisiere Git LFS...
git lfs install
if errorlevel 1 (
    echo [FEHLER] LFS Installation fehlgeschlagen!
    pause
 exit /b 1
)
echo [OK] Git LFS initialisiert!
echo.

REM Check .gitattributes
echo 2. Pruefe .gitattributes...
if exist ".gitattributes" (
    echo [OK] .gitattributes gefunden!
) else (
    echo [WARNUNG] .gitattributes nicht gefunden!
    echo Bitte stellen Sie sicher, dass die Datei existiert.
)
echo.

REM Check Git repo
echo 3. Pruefe Git Repository...
if exist ".git" (
echo [OK] Git Repository gefunden!
    
    REM Add and commit .gitattributes
    echo.
    echo 4. Committe LFS-Konfiguration...
    git add .gitattributes
    git add .gitignore
    git commit -m "Setup Git LFS tracking rules"
  echo [OK] Konfiguration committed!
) else (
    echo [WARNUNG] Kein Git Repository gefunden!
 echo.
    set /p init="Git Repository initialisieren? (j/n): "
    if /i "%init%"=="j" (
    git init
        echo [OK] Repository initialisiert!
        git add .gitattributes
    git add .gitignore
        git commit -m "Initial commit with Git LFS"
        echo [OK] Initial commit erstellt!
)
)

echo.
echo ================================
echo Setup abgeschlossen!
echo ================================
echo.

echo Naechste Schritte:
echo   1. Assets hinzufuegen: git add Assets/
echo   2. Committen: git commit -m "Add Unity assets"
echo   3. Remote: git remote add origin [URL]
echo   4. Pushen: git push -u origin main
echo.

echo Nuetzliche Befehle:
echo   git lfs ls-files    - LFS-Dateien anzeigen
echo   git lfs status      - LFS Status
echo   git lfs ls-files -s - LFS Groesse
echo   git lfs prune       - LFS bereinigen
echo.

echo Dokumentation: GIT_LFS_SETUP.md
echo.

pause
