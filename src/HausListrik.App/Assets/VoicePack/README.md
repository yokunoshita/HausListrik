# Voice Pack

Taruh setiap voice pack di subfolder terpisah di dalam folder ini.

Contoh struktur:

- `Assets/VoicePack/Default`
- `Assets/VoicePack/DramaQueen`
- `Assets/VoicePack/KaryawanBurnout`

Penamaan file:

- `drop-01.wav`
- `drop-02.wav`
- `critical-01.wav`
- `critical-02.wav`
- `charging-01.wav`
- `charging-02.wav`

Behavior:

- `drop-*` dipakai saat baterai turun normal
- `critical-*` dipakai saat baterai masuk level kritis
- `charging-*` dipakai saat charger dicolok

Kalau file belum ada, app otomatis fallback ke Windows Speech.
