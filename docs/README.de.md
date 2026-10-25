<p align="center">
  <img src="media/logo.png" alt="1v1 Timer" width="160">
</p>

<h3 align="center">Zwei Stoppuhren für 1v1-Matches, über Ihrem Spiel.</h3>

<p align="center">
  Timer starten, stoppen und wechseln – per Taste, Maustaste oder Controller, während das Spiel weiterhin jede Taste bekommt.<br>
  Kostenlos, quelloffen und offline.
</p>

<p align="center"><b>Entwickelt von Yasir 🇸🇦</b></p>

<p align="center">
  <a href="https://github.com/yasirmz/1v1-Timer/releases/latest/download/1v1timer-setup.exe"><img src="https://img.shields.io/badge/Download-Windows%2010%20%7C%2011-B41A23?style=for-the-badge&logo=windows&logoColor=E4DECB&labelColor=170F10" alt="Download for Windows"></a>
  <img src="https://img.shields.io/badge/license-GPL--2.0-3A2A2D?style=for-the-badge&labelColor=170F10" alt="License: GPL-2.0">
  <img src="https://img.shields.io/badge/languages-7-3A2A2D?style=for-the-badge&labelColor=170F10" alt="7 languages">
</p>

<p align="center">
  <a href="../README.md">English</a> ·
  <a href="README.ar.md">العربية</a> ·
  <a href="README.zh-Hans.md">简体中文</a> ·
  <a href="README.ru.md">Русский</a> ·
  <a href="README.es.md">Español</a> ·
  <a href="README.pt-BR.md">Português (Brasil)</a> ·
  <b>Deutsch</b>
</p>

## Warum 1v1 Timer

- **Zwei Timer nebeneinander.** Timer 1 links, Timer 2 rechts, immer über dem Spiel. Sekunden mit zwei Nachkommastellen (12.34), ab der ersten Minute mit Minuten (1:33.45).
- **Tastenkürzel, die nichts wegnehmen.** Der Timer hört nur zu: Das Spiel bekommt weiterhin jede Taste, Maustaste und Controller-Taste, die Sie drücken.
- **Tastatur, Maus oder Controller.** Jede Aktion hat eine Taste oder Maustaste und eine eigene Controller-Taste.
- **Sehen, wann es knapp wird.** Timer 2 wird in den letzten 20 Sekunden, bevor er die Zeit von Timer 1 erreicht, orange.
- **Nie im Weg.** Er nimmt Ihrem Spiel nie den Fokus. Ziehen Sie ihn an jede Stelle, und ziehen Sie an seinen Rändern oder an der Ecke, um ihn größer oder kleiner zu machen.
- **Schont Ihr Spiel.** Es nutzt nie die Grafikkarte, kaum CPU, solange die Timer stehen, und unter 1 % der CPU, während einer läuft.
- **Privat.** Kein Konto, keine Telemetrie. Er geht nur online, um nach Updates zu suchen.

## Download

1. Laden Sie **1v1timer-setup.exe** aus der [neuesten Version](https://github.com/yasirmz/1v1-Timer/releases/latest) herunter.
2. Starten Sie die Datei und folgen Sie den Schritten: Wählen Sie Sprache und Ordner (keine Administratorrechte nötig) und ob Sie eine Desktop-Verknüpfung möchten und ob der Timer mit Windows starten soll (aus, solange Sie es nicht ankreuzen).
3. Starten Sie ein Match und drücken Sie die Tastenkürzel unten.

Windows warnt eventuell vor einem unbekannten Herausgeber (der Installer ist noch nicht signiert): Wählen Sie **Weitere Informationen** und dann **Trotzdem ausführen**.
Updates kommen aus der App selbst, wenn Sie auf Aktualisieren klicken. Beim Deinstallieren bleiben Ihre Einstellungen erhalten.

**Voraussetzungen:** Windows 10 (2004 oder neuer) oder Windows 11, 64 Bit.

## So funktioniert es

| Aktion | Tastatur / Maus | Controller |
|---|---|---|
| Start / Stopp / Zurücksetzen | Maus: Zurück | A |
| Start / Stopp | H | B |
| Timer 1 wählen | T | Steuerkreuz links |
| Timer 2 wählen | Y | Steuerkreuz rechts |

- **Start / Stopp / Zurücksetzen** wirkt auf den gewählten Timer: Läuft er, stoppt er; steht er mit Zeit darauf, geht er auf 0.00 zurück; bei 0.00 startet er.
- **Start / Stopp** stoppt den gewählten Timer oder lässt ihn ab seiner Zeit weiterlaufen.
- **Timer beim Wechseln starten** (in den Einstellungen): Beim Wechsel zum anderen Timer stoppt der bisherige, und der neue startet im selben Moment – wie eine Schachuhr.
- **Farben:** Timer grün, gewählter Timer blau, letzte 20 Sekunden orange, Hintergrund schwarz. Wählen Sie in den Einstellungen jede Farbe und sehen Sie sie sofort am Timer.
- **Transparenter Hintergrund** zeigt nur die Ziffern mit einem feinen Rand. **Klicks durchlassen** lässt Klicks durch den Timer zum Spiel; beim Start ist es immer aus, und das Symbol neben der Uhr schaltet es aus.
- Ein Rechtsklick auf den Timer oder auf das Symbol neben der Uhr öffnet **Einstellungen**, **Klicks durchlassen** und **Beenden**. Ein erneuter Start der App öffnet ebenfalls die Einstellungen.
- Jede Einstellung gilt sofort und wird mit Platz und Größe des Timers in `%AppData%\1v1 Timer\settings.json` gespeichert.

## Spiele und Vollbild

- **Nutzen Sie den rahmenlosen Modus oder den Fenstermodus.** Ein Spiel im exklusiven Vollbild belegt den ganzen Bildschirm, und Windows zeichnet kein anderes Fenster darüber – dort kann also kein Overlay erscheinen. Rahmenlos („Vollbild im Fenster“) sieht gleich aus und zeigt den Timer.
- **Spiele mit Administratorrechten:** Läuft ein Spiel als Administrator, lässt Windows seine Tasten eventuell nicht zu einem Timer durch, der ohne diese Rechte läuft. Starten Sie 1v1 Timer dann ebenfalls als Administrator, oder nutzen Sie den Controller, der in beiden Fällen funktioniert.
- **Controller:** Xbox, PlayStation (DualSense, DualShock 4) und die meisten anderen Controller, ohne zusätzliche App. Bei einem PlayStation-Controller ist Kreuz A, Kreis B, Quadrat X und Dreieck Y.

## Anti-Cheat (Vanguard, Easy Anti-Cheat, VAC)

**Kann 1v1 Timer meinen Valorant-, Fortnite- oder Steam-Account sperren lassen?**

1v1 Timer tut nichts von dem, wofür Anti-Cheat sperrt. Es ist ein eigenes Programm mit eigenem Fenster, wie Discord oder OBS, und berührt nie ein Spiel:

- Es öffnet, liest oder ändert nie den Speicher eines Spiels und bringt nie Code in ein Spiel.
- Es drückt nie eine Taste, klickt nie und drückt nie eine Controller-Taste für Sie. Es hört Tastendrücke nur mit, wie Windows es jeder App erlaubt, und das Spiel bekommt weiterhin jeden.
- Es installiert keinen Treiber und braucht keine Administratorrechte.
- Es zeigt zwei Stoppuhren und tut im Spiel nichts für Sie.

Kein Anti-Cheat lässt eine App sich selbst als sicher anmelden, daher kann nur Riot, Epic oder Valve für sie sprechen. Aber eine Stoppuhr, die das Spiel nie berührt, ist nicht das, wonach sie suchen. Das wird bei jeder Änderung geprüft: Die [Anti-Cheat-Tests](../tests/OneVOneTimer.Tests/AntiCheatTests.cs) des Projekts listen jede Windows-Funktion auf, die der Timer nutzt, und schlagen fehl, sobald eine hinzukommt, die den Speicher eines anderen Programms liest, Code hineinbringt oder Eingaben vortäuscht.

Laden Sie 1v1 Timer nur über die [Releases](https://github.com/yasirmz/1v1-Timer/releases) dieser Seite herunter: Eine Kopie von anderswo könnte verändert sein.

## Datenschutz

- Keine Telemetrie, keine Analysen, kein Konto.
- 1v1 Timer geht nur online, um auf GitHub nach einer neueren Version zu suchen (einmal am Tag, abschaltbar) und um sie herunterzuladen, wenn Sie auf Aktualisieren klicken.
- Die Tastenkürzel werden nur auf Ihrem PC gelesen. Nichts, was Sie tippen, wird gespeichert oder irgendwohin gesendet: Der Timer vergleicht jede Taste nur mit seinen eigenen Tastenkürzeln.
- Ein Sicherheitsproblem gefunden? Bitte melden Sie es vertraulich: siehe [SECURITY.md](../SECURITY.md).

## Aus dem Quellcode bauen

<details>
<summary>Für Entwickler</summary>

1v1 Timer ist eine .NET-10-App (WPF und WinForms) mit dem Aussehen, den Einstellungen, der Farbauswahl, dem Infobereich-Menü und den Sprachen von [Capture](https://github.com/yasirmz/Capture).

```powershell
dotnet build src\OneVOneTimer              # die App
dotnet test tests\OneVOneTimer.Tests       # die Tests
.\tools\release.ps1                        # Installer, Update-Dateien und Quellcode-Zip in releases\ (braucht vpk 1.2.161 und Inno Setup 6)
```

Versionen baut und veröffentlicht der Workflow **Release build** (Actions → Run workflow).

</details>

## Lizenz

1v1 Timer ist freie Software unter der [GNU General Public License v2.0 oder neuer](../LICENSE). Jede Version enthält ihren vollständigen Quellcode. Komponenten von Drittanbietern und ihre Lizenzen stehen in [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).

<sub>1v1 Timer basiert auf [Kairos](https://github.com/BasilForge/Kairos) von BasilForge, unter derselben Lizenz.</sub>
