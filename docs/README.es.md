<p align="center">
  <img src="media/logo.png" alt="1v1 Timer" width="160">
</p>

<h3 align="center">Dos cronómetros para partidas 1v1, encima de su juego.</h3>

<p align="center">
  Inicie, detenga y cambie de cronómetro con una tecla, un botón del ratón o el mando, mientras el juego sigue recibiendo todas las teclas.<br>
  Gratis, de código abierto y sin conexión.
</p>

<p align="center"><b>Creado por Yasir 🇸🇦</b></p>

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
  <b>Español</b> ·
  <a href="README.pt-BR.md">Português (Brasil)</a> ·
  <a href="README.de.md">Deutsch</a>
</p>

## Por qué 1v1 Timer

- **Dos cronómetros, uno al lado del otro.** El cronómetro 1 a la izquierda y el 2 a la derecha, siempre encima del juego. Segundos con dos decimales (12.34) y, a partir del primer minuto, minutos (1:33.45).
- **Atajos que no quitan nada.** El cronómetro solo escucha: el juego sigue recibiendo cada tecla, botón del ratón y botón del mando que pulse.
- **Teclado, ratón o mando.** Cada acción tiene una tecla o un botón del ratón, y un botón del mando propio.
- **Sepa cuándo está cerca.** El cronómetro 2 se vuelve naranja en los últimos 20 segundos antes de alcanzar el tiempo del cronómetro 1.
- **Nunca estorba.** Nunca le quita el foco al juego. Arrástrelo a cualquier sitio, y arrastre sus bordes o su esquina para agrandarlo o achicarlo.
- **Ligero para tu juego.** Nunca usa la tarjeta gráfica, casi no usa la CPU con los temporizadores parados y menos del 1% de la CPU mientras uno corre.
- **Privado.** Sin cuenta ni telemetría. Solo se conecta a internet para buscar actualizaciones.

## Descarga

1. Descargue **1v1timer-setup.exe** de la [última versión](https://github.com/yasirmz/1v1-Timer/releases/latest).
2. Ejecútelo y siga los pasos: elija el idioma y la carpeta (no hacen falta permisos de administrador), y si quiere un acceso directo en el escritorio y que el cronómetro se inicie con Windows (desactivado si no lo marca).
3. Empiece una partida y pulse los atajos de abajo.

Windows puede avisar de un editor desconocido (el instalador aún no está firmado): elija **Más información** y luego **Ejecutar de todas formas**.
Las actualizaciones llegan desde la propia aplicación, cuando pulsa Actualizar. Al desinstalar se conserva su configuración.

**Requisitos:** Windows 10 (2004 o posterior) o Windows 11, de 64 bits.

## Cómo funciona

| Acción | Teclado / ratón | Mando |
|---|---|---|
| Iniciar / detener / reiniciar | Ratón: atrás | A |
| Iniciar / detener | H | B |
| Elegir cronómetro 1 | T | Cruceta izquierda |
| Elegir cronómetro 2 | Y | Cruceta derecha |

- **Iniciar / detener / reiniciar** actúa sobre el cronómetro elegido: si está en marcha, se detiene; si está detenido con tiempo, vuelve a 0.00; en 0.00, se inicia.
- **Iniciar / detener** detiene el cronómetro elegido o lo vuelve a poner en marcha desde su tiempo.
- **Iniciar al cambiar de cronómetro** (en Configuración): al cambiar al otro cronómetro, se detiene el que deja y se inicia el nuevo en el mismo instante, como un reloj de ajedrez.
- **Colores:** cronómetro verde, cronómetro elegido azul, últimos 20 segundos naranja, fondo negro. Elija cualquier color en Configuración y véalo en el cronómetro al momento.
- **Fondo transparente** muestra solo los números, con un borde fino. **Clic a través** deja que los clics atraviesen el cronómetro y lleguen al juego; siempre está desactivado al iniciar, y el icono junto al reloj lo desactiva.
- Haga clic derecho en el cronómetro, o en el icono junto al reloj, para **Configuración**, **Clic a través** y **Salir**. Abrir la aplicación otra vez también abre la configuración.
- Cada ajuste se aplica al momento y se guarda en `%AppData%\1v1 Timer\settings.json`, con la posición y el tamaño del cronómetro.

## Juegos y pantalla completa

- **Use el modo sin bordes o en ventana.** Un juego en pantalla completa exclusiva ocupa toda la pantalla y Windows no dibuja ninguna otra ventana encima, así que ninguna superposición puede verse ahí. El modo sin bordes («pantalla completa en ventana») se ve igual y muestra el cronómetro.
- **Juegos ejecutados como administrador:** si un juego se ejecuta como administrador, Windows puede impedir que sus teclas lleguen a un cronómetro que no lo hace. Ejecute 1v1 Timer como administrador también, o use el mando, que funciona en ambos casos.
- **Mandos:** Xbox, PlayStation (DualSense, DualShock 4) y la mayoría de los demás mandos, sin ninguna otra aplicación. En un mando de PlayStation, la X es A, el círculo B, el cuadrado X y el triángulo Y.

## En directo (OBS)

Captura de juego (Game Capture) graba solo el juego, así que una ventana sobre el juego (también el temporizador) nunca llega a tu directo. Por eso 1v1 Timer le da a OBS su propia copia:

1. En 1v1 Timer, abre **Configuración → Directo (OBS)** y pulsa **Copiar enlace** (es `http://127.0.0.1:47123/`).
2. En OBS: **Fuentes → + → Navegador**, pega el enlace como URL y pon ancho **700** y alto **140**.
3. Colócalo donde quieras en tu escena. Muestra los dos temporizadores con tus colores, transparente con **Fondo transparente**, y al compás del temporizador.

El enlace solo funciona en tu PC: nada puede llegar a él desde internet.

## Antitrampas (Vanguard, Easy Anti-Cheat, VAC)

**¿Puede 1v1 Timer hacer que baneen mi cuenta de Valorant, Fortnite o Steam?**

1v1 Timer no hace nada de lo que el antitrampas castiga. Es un programa aparte con su propia ventana, como Discord u OBS, y nunca toca el juego:

- Nunca abre, lee ni cambia la memoria de un juego, ni le mete código.
- Nunca pulsa teclas, hace clic ni pulsa botones del mando por ti. Solo escucha las pulsaciones, como Windows permite a cualquier aplicación, y el juego sigue recibiendo cada una.
- No instala ningún controlador (driver) ni necesita permisos de administrador.
- Muestra dos cronómetros; no hace nada en el juego por ti.

Ningún antitrampas deja que una aplicación se registre como segura, así que solo Riot, Epic o Valve pueden hablar por ellos. Pero un cronómetro que nunca toca el juego no es lo que buscan. Se comprueba en cada cambio: las [pruebas antitrampas](../tests/OneVOneTimer.Tests/AntiCheatTests.cs) del proyecto listan cada función de Windows que usa el temporizador y fallan si alguna vez se añade una para leer la memoria de otro programa, meterle código o falsificar pulsaciones.

Descarga 1v1 Timer solo desde la [página de versiones](https://github.com/yasirmz/1v1-Timer/releases): una copia de cualquier otro sitio podría estar modificada.

## Privacidad

- Sin telemetría, sin analíticas, sin cuenta.
- 1v1 Timer solo se conecta para buscar una versión nueva en GitHub (una vez al día, y puede desactivarlo) y para descargarla cuando pulsa Actualizar.
- Los atajos se leen solo en su PC. Nada de lo que escribe se guarda ni se envía a ningún sitio: el cronómetro solo compara cada tecla con sus atajos.
- ¿Encontró un problema de seguridad? Infórmelo en privado: vea [SECURITY.md](../SECURITY.md).

## Compilar desde el código

<details>
<summary>Para desarrolladores</summary>

1v1 Timer es una aplicación .NET 10 (WPF y WinForms), con el aspecto, la configuración, el selector de color, el menú de la bandeja y los idiomas de [Capture](https://github.com/yasirmz/Capture).

```powershell
dotnet build src\OneVOneTimer              # la aplicación
dotnet test tests\OneVOneTimer.Tests       # las pruebas
.\tools\release.ps1                        # el instalador, los archivos de actualización y el código fuente en releases\ (necesita vpk 1.2.161 e Inno Setup 6)
```

Las versiones las compila y publica el flujo de trabajo **Release build** (Actions → Run workflow).

</details>

## Licencia

1v1 Timer es software libre bajo la [Licencia Pública General de GNU v2.0 o posterior](../LICENSE). Cada versión incluye su código fuente completo. Los componentes de terceros y sus licencias se enumeran en [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).

<sub>1v1 Timer se basa en [Kairos](https://github.com/BasilForge/Kairos), de BasilForge, con la misma licencia.</sub>
