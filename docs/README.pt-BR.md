<p align="center">
  <img src="media/logo.png" alt="1v1 Timer" width="160">
</p>

<h3 align="center">Dois cronômetros para partidas 1v1, por cima do seu jogo.</h3>

<p align="center">
  Inicie, pare e troque de cronômetro com uma tecla, um botão do mouse ou o controle, enquanto o jogo continua recebendo todas as teclas.<br>
  Grátis, de código aberto e offline.
</p>

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
  <b>Português (Brasil)</b> ·
  <a href="README.de.md">Deutsch</a>
</p>

## Por que o 1v1 Timer

- **Dois cronômetros, lado a lado.** O cronômetro 1 à esquerda e o 2 à direita, sempre por cima do jogo. Segundos com duas casas decimais (12.34) e, a partir do primeiro minuto, minutos (1:33.45).
- **Atalhos que não tomam nada.** O cronômetro só escuta: o jogo continua recebendo cada tecla, botão do mouse e botão do controle que você aperta.
- **Teclado, mouse ou controle.** Cada ação tem uma tecla ou um botão do mouse, e um botão do controle só seu.
- **Saiba quando está perto.** O cronômetro 2 fica laranja nos últimos 20 segundos antes de chegar ao tempo do cronômetro 1.
- **Nunca atrapalha.** Ele nunca tira o foco do jogo. Arraste-o para qualquer lugar, e arraste as bordas ou o canto para aumentá-lo ou diminuí-lo.
- **Privado.** Sem conta, sem telemetria. Só se conecta à internet para procurar atualizações.

## Download

1. Baixe o **1v1timer-setup.exe** da [versão mais recente](https://github.com/yasirmz/1v1-Timer/releases/latest).
2. Execute-o e siga os passos: escolha o idioma e a pasta (sem precisar de direitos de administrador), e se quer um atalho na área de trabalho e iniciar o cronômetro com o Windows (desativado se você não marcar).
3. Comece uma partida e use os atalhos abaixo.

O Windows pode avisar sobre um editor desconhecido (o instalador ainda não é assinado): escolha **Mais informações** e depois **Executar assim mesmo**.
As atualizações vêm de dentro do app, quando você clica em Atualizar. Desinstalar mantém suas configurações.

**Requisitos:** Windows 10 (2004 ou mais recente) ou Windows 11, 64 bits.

## Como funciona

| Ação | Teclado / mouse | Controle |
|---|---|---|
| Iniciar / parar / zerar | Mouse: voltar | A |
| Iniciar / parar | H | B |
| Escolher cronômetro 1 | T | Direcional para a esquerda |
| Escolher cronômetro 2 | Y | Direcional para a direita |

- **Iniciar / parar / zerar** age no cronômetro escolhido: rodando, ele para; parado com tempo, volta para 0.00; em 0.00, ele começa.
- **Iniciar / parar** para o cronômetro escolhido ou o põe para rodar de novo a partir do seu tempo.
- **Iniciar ao trocar de cronômetro** (nas Configurações): ao trocar para o outro cronômetro, o que você deixa para e o novo começa no mesmo instante, como um relógio de xadrez.
- **Cores:** cronômetro verde, cronômetro escolhido azul, últimos 20 segundos laranja, fundo preto. Escolha qualquer cor nas Configurações e veja no cronômetro na hora.
- **Fundo transparente** mostra só os números, com um contorno fino. **Clique através** deixa os cliques passarem pelo cronômetro até o jogo; sempre começa desativado, e o ícone ao lado do relógio o desativa.
- Clique com o botão direito no cronômetro, ou no ícone ao lado do relógio, para **Configurações**, **Clique através** e **Sair**. Abrir o app de novo também abre as Configurações.
- Cada configuração vale na hora e fica guardada em `%AppData%\1v1 Timer\settings.json`, com a posição e o tamanho do cronômetro.

## Jogos e tela cheia

- **Use o modo sem bordas ou em janela.** Um jogo em tela cheia exclusiva ocupa a tela inteira, e o Windows não desenha nenhuma outra janela por cima dele, então nenhuma sobreposição aparece ali. O modo sem bordas («tela cheia em janela») fica igual e mostra o cronômetro.
- **Jogos executados como administrador:** se um jogo roda como administrador, o Windows pode impedir que as teclas dele cheguem a um cronômetro que não roda assim. Execute o 1v1 Timer como administrador também, ou use o controle, que funciona nos dois casos.
- **Controles:** controles de Xbox e qualquer controle que funcione como ele (XInput). Um controle de PlayStation funciona pelo Steam Input ou pelo DS4Windows.

## Privacidade

- Sem telemetria, sem análises, sem conta.
- O 1v1 Timer só se conecta para procurar uma versão mais nova no GitHub (uma vez por dia, e você pode desativar) e para baixá-la quando você clica em Atualizar.
- Os atalhos são lidos só no seu PC. Nada do que você digita é guardado ou enviado para lugar nenhum: o cronômetro só compara cada tecla com os atalhos dele.
- Encontrou um problema de segurança? Informe em particular: veja [SECURITY.md](../SECURITY.md).

## Compilar a partir do código

<details>
<summary>Para desenvolvedores</summary>

O 1v1 Timer é um app .NET 10 (WPF e WinForms), com a aparência, as configurações, o seletor de cores, o menu da bandeja e os idiomas do [Capture](https://github.com/yasirmz/Capture).

```powershell
dotnet build src\OneVOneTimer              # o app
dotnet test tests\OneVOneTimer.Tests       # os testes
.\tools\release.ps1                        # o instalador, os arquivos de atualização e o código-fonte em releases\ (precisa do vpk 1.2.161 e do Inno Setup 6)
```

As versões são compiladas e publicadas pelo fluxo de trabalho **Release build** (Actions → Run workflow).

</details>

## Licença

O 1v1 Timer é software livre sob a [Licença Pública Geral GNU v2.0 ou posterior](../LICENSE). Cada versão inclui o código-fonte completo. Os componentes de terceiros e suas licenças estão em [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).

<sub>O 1v1 Timer é baseado no [Kairos](https://github.com/BasilForge/Kairos), da BasilForge, sob a mesma licença.</sub>
