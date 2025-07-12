@echo off
setlocal enabledelayedexpansion

REM Ordnerpfad eingeben (Quellordner)
set "folder_path=D:\8 - Unterlagen\GameDesign\GitHubResp\Little-Grinders\Assets\Resources\Sounds\Effects\Combat"

REM Zeichenfolge eingeben, die entfernt werden soll
set "string_to_remove=GameDesign "

REM Zielordner für kopierte Dateien
set "target_path=D:\8 - Unterlagen\GameDesign\GitHubResp\Little-Grinders\Assets\Resources\Sounds\Effects\Combat\Enemy"

REM Durch alle .mp3-Dateien im Ordner iterieren
for %%f in ("%folder_path%\*.mp3") do (
    set "filename=%%~nxf"
    set "new_filename=!filename:%string_to_remove%=!"
    if not "!filename!"=="!new_filename!" (
        ren "%%~dpfnxf" "!new_filename!"
    )
)

REM Dateien nach dem Umbenennen in Zielordner kopieren
for %%f in ("%folder_path%\*.mp3") do (
    copy "%%f" "%target_path%\"
)

endlocal
pause
