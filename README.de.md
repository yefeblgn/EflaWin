# Efla-Sieg

**Diller / Languages:** [TR](README.md) · [EN](README.en.md) · [ES](README.es.md) · [RU](README.ru.md) · [DE](README.de.md) · [中文](README.zh.md) · [日本語](README.ja.md)

Efla Win sammelt beim Einrichten eines neuen Windows-Computers Anwendungsdownloads, Treibersteuerung und häufig verwendete Systemeinstellungen in einem einzigen Fenster. Entwickelt mit WinUI 3.

<img src="Assets/efla.png" alt="Efla Win logosu" width="72" height="72">

## Anfang

Öffnen Sie die `Efla Win 1.0.0.exe`-Datei in der 64-Bit-Version von Windows 11 und erteilen Sie die Genehmigung des Administrators. Kein Installationspaket erforderlich. .NET- und Windows App SDK-Komponenten sind in der EXE-Datei enthalten und werden bei der ersten Ausführung in den lokalen Cache dekomprimiert. Für Downloads und Windows Update-Scans ist eine Internetverbindung erforderlich.

### Laden Sie Apps herunter und installieren Sie sie

1. Beginnen Sie mit **Empfohlen** oder wählen Sie die Kategorie auf dem Bildschirm **Alle Apps** aus. **Strg+F** fokussiert das Suchfeld.
2. Für eine einzelne App verwenden Sie die Schaltfläche **Herunterladen**. Um einige in die Warteschlange zu stellen, aktivieren Sie die Kontrollkästchen neben dem Logo und klicken Sie auf die Schaltfläche **Ausgewählte herunterladen**.
3. Standardmäßig werden heruntergeladene Dateien in das Verzeichnis `Efla Win` im Download-Ordner von Windows geschrieben. Mit **Ändern** in der unteren Leiste können Sie einen anderen Ordner auswählen und mit **Ordner öffnen** die Dateien anzeigen.
4. Sobald der Download abgeschlossen ist, öffnet die Schaltfläche **Installieren** das Installationsprogramm. Wenn Windows die Anwendung als installiert meldet, erscheint die Schaltfläche **Deinstallieren** und nach der Bestätigung öffnet sich das Tool zum Entfernen der Anwendung.

Die Option **Open Source** im Rechtsklick-Menü einer Karte öffnet die Vertriebsseite im Katalog.

### Verfolgen Sie Downloads und Surfen

**Verfügbare Aktionen** im linken Menü zeigen die Download-Warteschlange und den Treiberscan an, auch wenn Sie die Seite wechseln. Die während des Downloads empfangene Datenmenge wird angezeigt. Wenn der Server die Gesamtgröße meldet, wird diese ebenfalls angezeigt. Die Schaltfläche ****** neben einem Prozess stoppt diesen Job.

Wenn das Fenster minimiert wird, gelangt es in den Benachrichtigungsbereich. Sie können das Fenster, den Download-Ordner oder den Speicherbildschirm über das Rechtsklickmenü des Efla Win-Symbols öffnen. **Beenden** schließt die Anwendung.

### Fenster bearbeiten

| Bildschirm | Was können Sie tun? |
| --- | --- |
| **Windows-Einstellungen** | Überprüfen Sie die Optionen für Suche, Taskleiste, Datenschutz, Leistung und Datei-Explorer. Wechseln Sie zu den Startup-Apps, Speicher-, Netzwerk- und Proxy-Seiten. |
| **Spieleinstellungen** | Schauen Sie sich die Optionen für den Spielemodus, die Hintergrundaufzeichnung und die Xbox Game Bar an. Öffnen Sie die Grafikeinstellungen und den Energieplan. |
| **Hardware** | Sehen Sie sich Ihren Prozessor, Ihre Grafikkarte, Ihre Windows-Version, Ihre RAM-Auslastung und Ihren freien Speicherplatz an. |
| **Treiber** | Hebt Intel-, AMD- und NVIDIA-Tools je nach Gerät hervor. Suchen Sie nach in Windows Update angebotenen Treibern und öffnen Sie den Bildschirm **Optionale Updates** zur Installation. |
| **Erinnerung** | Aktuelle RAM-Nutzung anzeigen. **Löschen**, um die Arbeitssätze der in Frage kommenden Anwendungen in dieser Sitzung zu reduzieren. |
| **Anwendungseinstellungen** | Das System passt das helle oder dunkle Thema, die Hervorhebungsfarbe, die Schriftgröße, den fetten Text, den Kontrast und die Bewegungsreduzierung an. |

Wenn neben einer Einstellung **Gesperrt** steht, zeigt die Informationsschaltfläche die Richtlinie und den Speicherpfad an, die Windows erzwingt. **Nicht verfügbar** gibt an, dass die Option auf dieser Windows-Version oder diesem Computer nicht unterstützt wird. Einige Einstellungen werden erst angezeigt, wenn Sie sich erneut anmelden.

## Aus dem Quellcode kompilieren

.NET 10 SDK und Windows SDK müssen unter Windows installiert sein. Öffnen Sie das Quellarchiv und führen Sie PowerShell in dem Ordner aus, der die `EflaWin.csproj`-Datei enthält.

```powershell
dotnet publish .\EflaWin.csproj -c Release -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

Die Broadcast-Ausgabe befindet sich im Pfad `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\EflaWin.exe`. Für die Verteilung reicht diese EXE-Datei aus.

## Rückmeldung

Geben Sie beim Melden eines Fehlers die Windows-Version, den Bildschirm, auf dem der Fehler aufgetreten ist, und die zu wiederholenden Schritte an. Wenn es sich um ein Downloadproblem handelt, geben Sie auch den App-Namen und den Fehlertext an, der auf der Karte angezeigt wird. [GitHub](https://github.com/yefeblgn/eflawin)

## Ressourcen und Lizenzierung

Bei der Vorbereitung der Windows-Optionen wurden [Optimizer](https://github.com/hellzerg/optimizer), [Sparkle](https://github.com/thedogecraft/sparkle), [ReviOS Playbook](https://github.com/meetrevision/playbook), [GTweak](https://github.com/Greedeks/GTweak) und [vacpp/windows11](https://github.com/vacpp/windows11) untersucht. [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) wurde als Beispiel in der Oberfläche des Benachrichtigungsbereichsmenüs verwendet.

© 2026 BLGN Studios. Die Nutzungsbedingungen finden Sie in der [LICENSE](LICENSE)-Datei. Die im Katalog aufgeführten Marken und Anwendungen gehören ihren jeweiligen Eigentümern.

Hergestellt von ❤️ EFLAKEEEP000TOKEN
Unterstützt von BLGN Studios
