# Haus Listrik

Haus Listrik adalah MVP aplikasi Windows yang bikin baterai laptop punya kepribadian dramatis:

- saat baterai turun, app bisa ngomel lewat Windows Speech
- saat charger dicolok, app teriak lega
- brightness layar bisa ikut menurun otomatis sesuai persentase baterai
- app jalan tray-first supaya terasa seperti utility beneran

## Stack

- C#
- .NET 8
- WPF
- `SystemInformation.PowerStatus` untuk status baterai
- WMI (`WmiMonitorBrightnessMethods`) untuk brightness internal display
- `System.Speech` untuk voice prototype tanpa asset audio tambahan
- `NotifyIcon` untuk tray app
- Windows Registry (`HKCU\...\Run`) untuk auto-start

## Struktur

- `src/HausListrik.App/Configuration`: settings dan persistence
- `src/HausListrik.App/Domain`: model inti
- `src/HausListrik.App/Infrastructure`: integrasi Windows-specific
- `src/HausListrik.App/Services`: orchestration logic
- `src/HausListrik.App/Presentation`: command dan view model

## Cara Menjalankan di Windows

1. Install .NET 8 SDK
2. Buka solution `HausListrik.sln`
3. Jalankan restore:

```bash
dotnet restore
```

4. Jalankan app:

```bash
dotnet run --project src/HausListrik.App/HausListrik.App.csproj
```

Default behavior:

- app langsung hidup di system tray
- double click tray icon untuk buka control panel
- close window akan minimize ke tray, bukan exit

## Voice Pack Kustom

Root voice pack ada di:

- `src/HausListrik.App/Assets/VoicePack`

Setiap voice pack sebaiknya ada di subfolder sendiri, misalnya:

- `Default`
- `DramaQueen`
- `KaryawanBurnout`

Isi file `.wav` di dalam subfolder itu dengan pola:

- `drop-01.wav`
- `critical-01.wav`
- `charging-01.wav`

Kalau file belum tersedia, app akan fallback ke TTS. Pack aktif bisa dipilih langsung dari UI.

## Packaging

Sudah disiapkan dua jalur publish:

- publish profile: `src/HausListrik.App/Properties/PublishProfiles/FolderProfile.pubxml`
- script PowerShell: `scripts/publish-win.ps1`

Contoh:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-win.ps1
```

## Catatan MVP

- Brightness via WMI biasanya hanya bekerja untuk layar laptop internal, bukan semua external monitor.
- Audio file kustom saat ini fokus ke `.wav` supaya tetap ringan dan native tanpa dependency tambahan.
- Settings user disimpan ke `%LocalAppData%\HausListrik\appsettings.json`.
- Auto-start disimpan di registry user saat setting di-save.

## Next Step yang Paling Masuk Akal

- tambah cooldown supaya voice line tidak terlalu rapat pada kondisi drain cepat
- tambahkan editor personality pack di dalam app
- buat installer MSI/Inno Setup
- tambah fallback brightness strategy untuk device yang tidak support WMI
