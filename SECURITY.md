# Security

## Reporting a vulnerability

Please don't open a public issue for a security problem. Report it privately on GitHub instead: on this repository, open **Security**, then **Report a vulnerability**. Only the maintainer sees it.

Tell us what you found, how to reproduce it, and which 1v1 Timer version you used (Settings, at the foot of the window). We reply as soon as we can and credit you in the release notes if you like.

## Supported versions

Only the latest release gets security fixes. 1v1 Timer updates itself from inside the app, so the fix reaches you there.

## Getting 1v1 Timer safely

- Download 1v1 Timer only from [the releases page](https://github.com/yasirmz/1v1-Timer/releases).
- Each release lists the SHA-256 of its files. To check a download, run this in PowerShell and compare the result with the release page:

  ```powershell
  Get-FileHash .\1v1timer-setup.exe
  ```

- Updates come from the same releases page over HTTPS, and Velopack checks each update against the checksum the release lists before putting it in.

## What 1v1 Timer does with your input

The hotkeys are read with Windows' Raw Input and XInput, only to compare each key and button with the timer's own hotkeys. Nothing you type is kept, logged or sent anywhere. See [Privacy](README.md#privacy) in the README for the only times the timer goes online.

---

## الإبلاغ عن ثغرة أمنية

لا تفتح بلاغًا عامًا عن مشكلة أمنية. أبلغ عنها بشكل خاص على GitHub: افتح **Security** في هذا المستودع، ثم **Report a vulnerability**. لا يراها إلا المشرف على المشروع.

اذكر ما وجدته، وكيف يمكن تكرار المشكلة، ورقم إصدار 1v1 Timer الذي استخدمته. لا تصل الإصلاحات الأمنية إلا إلى أحدث إصدار، ويحدّث 1v1 Timer نفسه من داخل التطبيق.

نزّل 1v1 Timer من [صفحة الإصدارات](https://github.com/yasirmz/1v1-Timer/releases) فقط. تذكر كل نسخة بصمة SHA-256 لملفاتها، وتستطيع مطابقتها بالأمر `Get-FileHash .\1v1timer-setup.exe` في PowerShell.
