# Template UTS Augmented Reality

> Tutorial UTS lengkap berbasis Modul 4, 5, 6, 9, dan 11 ada di `README.md` root proyek. Dokumen ini adalah panduan teknis template (generator, script, dan struktur scene).
>
> Ingin membuat dari nol tanpa template? Lihat [README_TANPA_TEMPLATE.md](../../README_TANPA_TEMPLATE.md).

Template ini dibuat untuk memenuhi requirement UTS pada gambar:

**Nilai B**
1. Menu utama (desain bebas) dengan 1 tombol menuju AR Camera.
2. AR Camera dapat menampilkan objek 3D.

**Nilai A**
1. Menu utama dengan 1 tombol menuju AR Camera.
2. AR Camera dapat menampilkan objek 3D.
3. Menambahkan salah satu interaksi: zoom atau rotasi.

Template ini sudah mencakup **keduanya**: rotasi memakai slider, zoom memakai pinch dua jari, tombol `+`/`-`, dan scroll mouse saat di Editor. Ada juga tombol reset, tombol kembali ke menu, dan indikator status marker.

Sesuai ketentuan UTS, **desain 3D, database marker, dan license berasal dari dosen**. Template ini memakai aset yang sudah ada di proyek sebagai contoh, lalu menyediakan langkah untuk menggantinya dengan aset dari dosen.

---

## 1. Ringkasan template

| Bagian | Isi |
|---|---|
| Scene menu | `Assets/UTS_Template/Scenes/MainMenu.unity` |
| Scene AR | `Assets/UTS_Template/Scenes/ARScene.unity` |
| Script menu | `Assets/UTS_Template/Scripts/MainMenuController.cs` |
| Script AR | `Assets/UTS_Template/Scripts/ARInteractionController.cs` |
| Generator scene | `Assets/UTS_Template/Editor/UTSTemplateBuilder.cs` |
| Material contoh | `Assets/UTS_Template/Materials/Konten3D.mat` |
| Marker/dataset | Memakai dataset yang sudah ada di proyek: `Assets/StreamingAssets/Vuforia/DataRafi.xml` |
| Target yang dipakai | `cover-sui` |
| Objek 3D contoh | Kubus `Konten3D` (placeholder, ganti dengan model dari dosen) |
| Vuforia | Vuforia Engine 11.4.4 |
| Unity | Unity 6000.0.34f1 (Unity 6) |
| Input | Input System baru dan Input Manager lama didukung lewat define |

Alur scene:

```text
MainMenu.unity
      |
      |  Tombol "MULAI AR"
      v
ARScene.unity
      |
      |  Tombol "KEMBALI"
      v
MainMenu.unity
```

---

## 2. Asumsi dan catatan penting

1. Template ini adalah **proyek Unity terpisah** di folder `UTS_AR_Template`, di luar `DataRafi_new`. Proyek ini sudah berisi Vuforia 11.4.4, salinan license, dataset marker, dan template scene. Membuka proyek ini tetap akan membuat Library baru, jadi pastikan ruang disk cukup.
2. Secara lokal, proyek ini berisi **salinan license** di `Assets/Resources/VuforiaConfiguration.asset` dan **salinan dataset** di `Assets/StreamingAssets/Vuforia/DataRafi.xml`. File license **tidak disimpan di repositori** karena berisi kunci rahasia; setelah clone, tambahkan license dari dosen. Dataset tetap disimpan.
3. Marker yang dipakai contoh adalah `cover-sui`. Jika dosen memberikan marker/dataset lain, ganti sesuai langkah di bagian 7.
4. Objek 3D yang tampil awalnya adalah **kubus placeholder**. Ganti dengan model 3D dari dosen sesuai langkah di bagian 6.
5. Jangan menyalin atau mempublikasikan license key Vuforia ke repositori publik.
6. Scene dibuat oleh **generator Editor**. Jalankan generator satu kali, lalu scene dapat diedit seperti scene Unity biasa.
7. Template memakai **Built-in Render Pipeline** dan shader `Standard`. Jika proyek Anda memakai URP, sesuaikan material.
8. UI memakai **Text UGUI biasa**, bukan TextMeshPro, agar tidak perlu setup TMP Essentials.
9. Template memaksa orientasi **portrait**. Jika ingin landscape, ubah `forcePortrait` menjadi `false` pada kedua script, lalu sesuaikan referensi Canvas.
10. Template tidak memakai Lean Touch. Pinch dan scroll ditangani langsung oleh `ARInteractionController`.
11. Template memakai API Unity 6 (`FindObjectsByType`, `FindFirstObjectByType`, `LegacyRuntime.ttf`, Input System 1.12). Gunakan Unity 6000.0.34f1 atau Unity 6 lain yang kompatibel; jangan gunakan Unity 2021/2022.

---

## 3. Struktur folder

```text
Assets/UTS_Template/
├── Editor/
│   └── UTSTemplateBuilder.cs
├── Scripts/
│   ├── MainMenuController.cs
│   └── ARInteractionController.cs
├── Scenes/                  (dibuat otomatis oleh generator)
│   ├── MainMenu.unity
│   └── ARScene.unity
├── Materials/               (dibuat otomatis oleh generator)
│   └── Konten3D.mat
└── README.md
```

Folder `Scenes` dan `Materials` belum ada sebelum generator dijalankan. Generator akan membuatnya.

---

## 4. Langkah cepat: dari nol sampai bisa dicoba

### 4.1 Prasyarat

- Unity 6 (`6000.0.34f1` atau versi Unity 6 lain yang kompatibel).
- Paket Vuforia Engine 11.4.4 sudah terpasang di proyek.
- Paket Input System sudah terpasang (proyek ini sudah memilikinya).
- Android Build Support terpasang jika ingin build ke HP.
- Ruang disk kosong yang cukup. **Sangat disarankan** mengosongkan beberapa GB sebelum membuka Unity atau build.
- Gambar marker dari dosen (atau marker `cover-sui` yang sudah ada di dataset) untuk diuji.
- Model 3D dari dosen (FBX/OBJ/GLB) untuk mengganti kubus placeholder.

### 4.2 Buka proyek

1. Buka Unity Hub.
2. Tambahkan proyek `UTS_AR_Template` jika belum terdaftar.
3. Buka proyek dengan Unity 6.
4. Tunggu proses import dan kompilasi selesai. Jangan jalankan generator sebelum Console bersih dari error kompilasi.

### 4.3 Jalankan generator

1. Pada menu bar Unity, pilih:

```text
UTS AR > Build Template Scenes
```

2. Jika muncul pertanyaan untuk menimpa scene, pilih **Ya, timpa** pada percobaan pertama. Peringatan ini muncul jika `MainMenu.unity` atau `ARScene.unity` sudah ada.
3. Generator akan:
   - membuat `Assets/UTS_Template/Scenes/MainMenu.unity`;
   - membuat `Assets/UTS_Template/Scenes/ARScene.unity`;
   - membuat material `Konten3D.mat`;
   - menambahkan kedua scene ke **Build Settings** dengan urutan:
     - index 0: `MainMenu`
     - index 1: `ARScene`
   - membuka `MainMenu.unity`.
4. Baca dialog konfirmasi, lalu klik **Siap**.

Jika menu `UTS AR` tidak muncul:

- Buka **Console** dan pastikan tidak ada error kompilasi.
- Pastikan script `UTSTemplateBuilder.cs` berada di folder `Assets/UTS_Template/Editor/`.
- Tunggu Unity selesai mengimpor paket Vuforia dan Input System.
- Coba restart Unity Editor.

### 4.4 Uji di Editor

1. Buka scene `Assets/UTS_Template/Scenes/MainMenu.unity`.
2. Tekan **Play**.
3. Menu utama akan muncul dengan tombol **MULAI AR** dan **KELUAR**.
4. Klik **MULAI AR**.
5. Scene AR akan terbuka. Vuforia akan mencoba memakai webcam laptop.
6. Arahkan marker ke webcam. Jika marker terdeteksi:
   - teks status berubah menjadi **Marker terdeteksi**;
   - kubus 3D muncul di atas marker.
7. Uji interaksi:
   - geser slider **ROTASI OBJEK** untuk memutar objek;
   - klik tombol **+** dan **-** untuk zoom;
   - gunakan **scroll mouse** untuk zoom saat di Editor;
   - klik **RESET** untuk mengembalikan posisi, rotasi, dan skala awal;
   - klik **KEMBALI** untuk kembali ke menu.
8. Jika webcam tidak tersedia atau marker tidak terbaca di Editor, uji lewat build Android. Menu utama tetap bisa diuji tanpa kamera.

Catatan: Vuforia di Editor membutuhkan webcam dan marker fisik/tampilan marker di layar lain. Jika tidak ada webcam, scene AR akan menampilkan latar hitam, tetapi UI tetap berfungsi.

### 4.5 Build ke Android

1. Buka **File > Build Profiles** (Unity 6) atau **File > Build Settings**.
2. Pilih platform **Android**.
3. Klik **Switch Platform** jika platform aktif belum Android.
4. Pastikan kedua scene tercentang dan urutannya:
   - `Assets/UTS_Template/Scenes/MainMenu.unity` (index 0);
   - `Assets/UTS_Template/Scenes/ARScene.unity` (index 1).
5. Buka **Player Settings > Other Settings**, lalu atur:
   - **Package Name**: misalnya `com.nama.nim.utsar`;
   - **Minimum API Level**: Android 7.0 (API 24) atau lebih tinggi;
   - **Target API Level**: Automatic (highest installed);
   - **Scripting Backend**: IL2CPP;
   - **Target Architectures**: ARM64 (centang ARMv7 hanya jika diperlukan);
   - **Active Input Handling**: `Input System Package (New)` atau `Both`.
6. Klik **Build** atau **Build and Run**.
7. Simpan file APK di luar folder `Assets`, misalnya di `Builds/UTS_AR.apk`.
8. Pasang APK di HP Android, lalu buka aplikasinya.
9. Izinkan akses kamera saat diminta.
10. Arahkan kamera ke marker. Kubus 3D akan muncul jika marker terdeteksi.

Tips build:

- Jika build gagal karena ukuran, hapus arsip besar yang tidak dipakai dari folder `Assets` (lihat bagian 11).
- Jika muncul error license Vuforia, periksa kembali license key dan koneksi internet perangkat.
- Jika kamera tidak muncul, periksa izin kamera aplikasi di pengaturan Android.

---

## 5. Cara kerja fitur

### 5.1 Menu utama

Scene `MainMenu` berisi:

- Background gelap dengan aksen lingkaran.
- Kartu menu dengan judul **AR UTS**.
- Placeholder identitas mahasiswa.
- Tombol **MULAI AR**.
- Tombol **KELUAR**.
- `EventSystem` dengan `InputSystemUIInputModule`.

`MainMenuController` akan:

- mengatur orientasi portrait;
- menghubungkan tombol **MULAI AR** ke `SceneManager.LoadScene("ARScene")`;
- menghubungkan tombol **KELUAR** ke `Application.Quit()` (di Editor, menghentikan Play Mode);
- menampilkan error di Console jika scene `ARScene` belum terdaftar di Build Settings.

### 5.2 AR Camera dan marker

Scene `ARScene` berisi:

- `ARCamera` dengan `VuforiaBehaviour` dan `DefaultInitializationErrorHandler`;
- `ImageTarget` dengan `ImageTargetBehaviour` dan `DefaultObserverEventHandler`;
- objek `Konten3D` (kubus placeholder) sebagai child dari `ImageTarget`;
- directional light;
- Canvas UI AR;
- `ARController` dengan script `ARInteractionController`;
- `EventSystem`.

Saat marker tidak terdeteksi, `DefaultObserverEventHandler` menonaktifkan renderer objek anak. Saat marker terdeteksi, renderer diaktifkan kembali. Karena itu, kubus hanya terlihat ketika marker berhasil dilacak.

### 5.3 Rotasi

- Slider `SliderRotasi` memiliki rentang 0–360 derajat.
- `ARInteractionController.OnRotationChanged` menerapkan rotasi sebagai **offset** dari rotasi awal model:

```csharp
targetModel.localRotation = baseRotation * Quaternion.Euler(0f, value, 0f);
```

- Rotasi tidak menimpa orientasi asli model, sehingga model yang memiliki rotasi koreksi (misalnya -90 derajat pada sumbu X) tetap berdiri dengan benar.
- Nilai rotasi ditampilkan pada teks `TeksNilaiRotasi`.

### 5.4 Zoom

Zoom dapat dilakukan dengan tiga cara:

1. **Pinch dua jari** pada layar HP.
2. **Tombol `+` dan `-`** pada panel bawah.
3. **Scroll mouse** saat menguji di Editor.

Semua zoom melewati `ZoomBy(float factor)` dan dibatasi:

```text
skala minimum = skala awal x minScaleMultiplier  (default 0.25)
skala maksimum = skala awal x maxScaleMultiplier (default 4.00)
```

Nilai dapat diubah di Inspector pada komponen `ARInteractionController`.

### 5.5 Reset, kembali, dan status

- **RESET** mengembalikan posisi, rotasi, dan skala ke kondisi awal, serta mengembalikan slider ke 0.
- **KEMBALI** memuat scene `MainMenu`.
- Teks status di bagian atas menampilkan:
  - **Arahkan kamera ke marker** saat marker belum terdeteksi;
  - **Marker terdeteksi** saat marker berhasil dilacak.

---

## 6. Mengganti kubus dengan model 3D dari dosen

Ini langkah yang paling penting untuk nilai UTS, karena dosen memberikan desain 3D.

### 6.1 Siapkan model

1. Buat folder `Assets/UTS_Template/Models/` jika belum ada.
2. Impor model dari dosen (FBX/OBJ/GLB) ke folder tersebut.
3. Pastikan tekstur dan material model ikut terimpor.
4. Jika model berformat FBX, atur **Scale Factor** dan **Import Materials** pada Inspector model.

### 6.2 Pasang model di scene AR

1. Buka `Assets/UTS_Template/Scenes/ARScene.unity`.
2. Pada Hierarchy, cari `ImageTarget`.
3. Hapus atau nonaktifkan kubus `Konten3D`.
4. Buat GameObject kosong sebagai wadah:
   - klik kanan `ImageTarget` > **Create Empty**;
   - beri nama `Konten3D`.
5. Seret model dari Project window ke dalam `Konten3D`.
6. Atur `Konten3D`:
   - `Position`: `(0, 0, 0)` untuk percobaan awal;
   - `Rotation`: `(0, 0, 0)`;
   - `Scale`: `(1, 1, 1)`.
7. Jika model terlalu kecil atau besar, ubah skala `Konten3D`. Target contoh memiliki lebar 20 unit dan tinggi sekitar 31,8 unit. Model dengan tinggi sekitar 10–15 unit biasanya terlihat proporsional.
8. Jika model berdiri miring atau tidur, putar `Konten3D` pada sumbu X:
   - model dengan sumbu Z-up biasanya perlu `Rotation X = -90`;
   - model dengan sumbu Y-up biasanya tidak perlu rotasi.
9. Pilih `ARController`, lalu pada komponen `ARInteractionController`:
   - isi field **Target Model** dengan `Konten3D`;
   - jika field kosong, script akan mencari child bernama `Konten3D` secara otomatis.
10. Tekan Play dan uji marker.

### 6.3 Tips agar model rapi

- Gunakan GameObject wadah `Konten3D` agar rotasi/zoom tidak mengubah transform internal model.
- Jika model memiliki pivot di tengah, letakkan `Konten3D` pada `Y = 0` lalu sesuaikan hingga model terlihat berdiri di atas marker.
- Jika model memiliki banyak bagian, pastikan semuanya berada di bawah `Konten3D`.
- Jika material terlihat pink, ganti shader material ke `Standard` (Built-in) atau `Universal Render Pipeline/Lit` (URP).
- Jika model terlalu berat, kurangi jumlah poligon dan ukuran tekstur sebelum dimasukkan ke proyek.

### 6.4 Jika ingin memakai model contoh dari proyek lama

Proyek `DataRafi_new` memiliki beberapa model contoh. Karena proyek UTS ini terpisah, impor model tersebut ke proyek ini terlebih dahulu:

- `../DataRafi_new/Assets/Black Dragon/Dragon_Baked_Actions_fbx_7.4_binary.fbx`
- `../DataRafi_new/Assets/elaina/source/SummerWhiteDress_GYRemix_FL.fbx`
- `../DataRafi_new/Assets/source/tripo_convert_ca66fc79-c220-4030-a1c3-59d08fabcfda.fbx`

Impor salah satunya ke folder `Assets/UTS_Template/Models/`, lalu seret ke dalam `Konten3D`. Namun, untuk UTS sebaiknya gunakan model yang diberikan dosen.

---

## 7. Mengganti marker, dataset, dan license

### 7.1 Jika dosen memberikan dataset baru

1. Salin file `.dat` dan `.xml` dari dosen ke:

```text
Assets/StreamingAssets/Vuforia/
```

2. Buka file `.xml` dan catat:
   - nama dataset, misalnya `MarkerDosen.xml`;
   - nama target, misalnya `marker-uts`;
   - ukuran target (width dan height).
3. Buka `Assets/UTS_Template/Scenes/ARScene.unity`.
4. Pilih GameObject `ImageTarget`.
5. Pada komponen `Image Target Behaviour`, isi:
   - **Image Target** dengan nama target dari XML;
   - **Data Set Path** dengan path relatif terhadap `StreamingAssets`, misalnya `Vuforia/MarkerDosen.xml`;
   - **Width** dan **Height** sesuai ukuran pada XML.
6. Jika ingin mengubah generator agar membuat scene baru dengan dataset tersebut, edit konstanta di `Assets/UTS_Template/Editor/UTSTemplateBuilder.cs`:

```csharp
private const string DataSetPath = "Vuforia/DataRafi.xml";
private const string TargetName = "cover-sui";
private const float TargetWidth = 20f;
private const float TargetHeight = 31.846153f;
```

7. Jalankan lagi `UTS AR > Build Template Scenes` jika ingin regenerate. **Perhatian:** regenerate akan menimpa scene dan menghilangkan edit manual. Jika sudah melakukan banyak edit, cukup ubah ImageTarget di scene tanpa regenerate.

### 7.2 Jika dosen memberikan license baru

1. Buka `Assets/Resources/VuforiaConfiguration.asset` di Project window.
2. Pada Inspector, cari bagian **Vuforia Configuration**.
3. Tempelkan **App License Key** dari dosen.
4. Simpan proyek.
5. Jika license terikat pada package name, samakan **Package Name** di Player Settings dengan package yang terdaftar pada license tersebut.
6. Uji kembali di perangkat.

License key yang sudah ada di proyek adalah license yang diberikan dosen. Jangan mengunggah file `VuforiaConfiguration.asset` ke repositori publik.

### 7.3 Ukuran target dan skala AR

- Nilai `Width`/`Height` pada ImageTarget memengaruhi skala dunia AR.
- Jika ingin ukuran AR realistis, gunakan ukuran cetak marker dalam satuan meter. Contoh: marker 20 cm x 31,8 cm sebaiknya diisi `0.20` dan `0.318`.
- Jika hanya untuk demo, nilai besar seperti `20` dan `31.8` tetap bisa dipakai asalkan model diskalakan relatif terhadap target.
- Pastikan nilai `Width`/`Height` di scene konsisten dengan ukuran di file XML dataset.

---

## 8. Kustomisasi desain menu dan UI AR

### 8.1 Mengubah teks menu

1. Buka `MainMenu.unity`.
2. Pada Hierarchy, pilih `Canvas > KartuMenu > JudulUtama` untuk judul.
3. Pilih `InfoMahasiswa` untuk nama, NIM, kelas, dan mata kuliah.
4. Ubah teks pada komponen `Text` di Inspector.
5. Ubah `SubJudul` jika ingin mengganti tagline.

### 8.2 Mengubah warna

Warna utama ada di dua tempat:

1. **Di scene**: pilih Image/Text/Button, lalu ubah properti `Color`.
2. **Di generator**: edit warna berikut pada `UTSTemplateBuilder.cs`, lalu regenerate:

```csharp
private static readonly Color BackgroundColor = new Color(0.043f, 0.071f, 0.125f, 1f);
private static readonly Color CardColor       = new Color(0.067f, 0.110f, 0.180f, 0.97f);
private static readonly Color AccentColor     = new Color(0.133f, 0.827f, 0.933f, 1f);
private static readonly Color SuccessColor    = new Color(0.200f, 0.850f, 0.580f, 1f);
```

### 8.3 Menambahkan logo

1. Impor logo ke folder `Assets/UTS_Template/Textures/`.
2. Buka `MainMenu.unity`.
3. Klik kanan `KartuMenu` > **UI > Image**.
4. Beri nama `Logo`.
5. Pada komponen Image, masukkan sprite logo.
6. Atur posisi dan ukuran di RectTransform.

### 8.4 Mengubah teks dan posisi UI AR

1. Buka `ARScene.unity`.
2. Semua UI berada di bawah `CanvasAR`.
3. Pilih `PanelKontrol` untuk mengubah panel bawah.
4. Pilih `SliderRotasi`, `TombolZoomIn`, `TombolZoomOut`, `TombolReset`, atau `TombolKembali` untuk mengubah tampilan.
5. Untuk mengubah perilaku, edit `ARInteractionController.cs` atau ubah nilai di Inspector.

### 8.5 Mengubah orientasi

- Secara default, kedua script memaksa portrait.
- Untuk mengizinkan rotasi layar, set `Force Portrait` menjadi `false` pada `MainMenuController` dan `ARInteractionController`.
- Sesuaikan juga `Reference Resolution` pada CanvasScaler jika ingin layout landscape.

---

## 9. Penjelasan script

### 9.1 `MainMenuController.cs`

| Method/Field | Fungsi |
|---|---|
| `startButton` | Referensi tombol MULAI AR. |
| `quitButton` | Referensi tombol KELUAR. |
| `arSceneName` | Nama scene AR yang dimuat. Default `ARScene`. |
| `Awake()` | Mengatur orientasi, mencari tombol jika kosong, dan memasang listener. |
| `StartAR()` | Memeriksa Build Settings lalu memuat scene AR. |
| `QuitGame()` | Keluar dari aplikasi; di Editor menghentikan Play Mode. |

### 9.2 `ARInteractionController.cs`

| Method/Field | Fungsi |
|---|---|
| `imageTarget` | Referensi ke `ImageTargetBehaviour`. |
| `targetModel` | Root objek yang diputar dan di-zoom. |
| `rotationSlider` | Slider rotasi 0–360 derajat. |
| `rotationValueText` | Teks nilai rotasi. |
| `zoomInButton`, `zoomOutButton` | Tombol zoom. |
| `resetButton` | Tombol reset transform. |
| `backButton` | Tombol kembali ke menu. |
| `statusText` | Teks status marker. |
| `minScaleMultiplier`, `maxScaleMultiplier` | Batas zoom relatif terhadap skala awal. |
| `buttonZoomStep` | Besar zoom per klik tombol. |
| `pinchSensitivity` | Sensitivitas pinch. |
| `scrollZoomStep` | Besar zoom per scroll mouse. |
| `Awake()` | Mencari referensi, menyimpan transform awal, memasang UI, dan berlangganan event target. |
| `OnRotationChanged()` | Menerapkan rotasi offset pada sumbu Y. |
| `ZoomBy()` | Menerapkan zoom dengan batas minimum/maksimum. |
| `ResetTransform()` | Mengembalikan transform awal. |
| `BackToMenu()` | Memuat scene menu. |
| `HandlePinchAndScroll()` | Membaca pinch dua jari dan scroll mouse. |
| `HandleTargetStatusChanged()` | Memperbarui teks status marker. |

### 9.3 `UTSTemplateBuilder.cs`

Script ini hanya berjalan di Editor dan tidak ikut ke build. Fungsinya:

- membuat scene `MainMenu` dan `ARScene`;
- membuat Canvas, teks, tombol, slider, panel, dan EventSystem;
- membuat ARCamera Vuforia, ImageTarget, dan objek contoh;
- membuat material `Konten3D.mat`;
- mendaftarkan scene ke Build Settings;
- menyediakan menu validasi setup.

---

## 10. Troubleshooting

| Gejala | Kemungkinan penyebab | Solusi |
|---|---|---|
| Menu `UTS AR` tidak muncul | Script belum selesai dikompilasi atau ada error | Buka Console, perbaiki error, tunggu kompilasi, restart Unity |
| `Scene 'ARScene' belum terdaftar` | Scene belum masuk Build Settings | Jalankan `UTS AR > Build Template Scenes` atau tambahkan manual di Build Settings |
| Marker tidak terdeteksi | Target/dataset salah, marker buram, cahaya kurang, kamera kotor | Periksa `ImageTargetBehaviour`, cetak marker dengan jelas, tambah cahaya, bersihkan lensa |
| Kubus tidak muncul | Marker belum terdeteksi atau renderer dimatikan oleh Vuforia | Pastikan status **Marker terdeteksi**; cek posisi/skala `Konten3D` |
| Objek 3D terlalu kecil/besar | Skala model tidak sesuai ukuran target | Ubah skala `Konten3D` atau ubah `Width`/`Height` ImageTarget |
| Rotasi aneh | Pivot/orientasi model tidak standar | Gunakan wadah `Konten3D`, atur rotasi koreksi pada wadah, bukan pada model |
| Tombol UI tidak bisa diklik | EventSystem hilang atau input module salah | Jalankan generator; pastikan ada `EventSystem` dengan `InputSystemUIInputModule` |
| `Vuforia initialization error` | License salah, package name tidak cocok, atau kamera tidak tersedia | Periksa `VuforiaConfiguration`, package name, izin kamera, dan koneksi internet |
| Material pink | Shader material tidak cocok dengan render pipeline | Ganti ke `Standard` (Built-in) atau `Universal Render Pipeline/Lit` (URP) |
| Build Android gagal | Android Build Support belum terpasang, package name kosong, atau storage penuh | Pasang Android module, isi package name, kosongkan disk |
| Aplikasi crash saat dibuka | License/package name salah, arsitektur tidak cocok | Cek logcat, samakan package name, gunakan ARM64 |
| Pinch tidak bekerja di Editor | Editor tidak memiliki layar sentuh | Gunakan scroll mouse; pinch diuji di HP |
| Preview marker di Scene view kosong | Generator tidak menambahkan komponen preview editor Vuforia | Abaikan; tracking runtime tetap berjalan selama target dan dataset benar |
| Scene AR hitam di Editor | Webcam tidak tersedia | Build ke HP Android untuk menguji tracking |

---

## 11. Catatan performa dan ruang disk

- Proyek UTS ini sengaja ringan: hanya berisi template, salinan license, dataset, input actions, dan kubus placeholder.
- Aset besar tetap berada di proyek `DataRafi_new` dan **tidak disalin**:
  - `Assets/elaina/source/SummerWhiteDress_GYRemix_FL.fbx` sekitar **100 MB**;
  - `Assets/elaina/elaina-the-witchs-journeysummerwhitedress.zip` sekitar **176 MB**;
  - `Assets/source/tripo_convert_....fbx` sekitar **54 MB**;
  - `Assets/Unity/Dragon Unity 5.unitypackage` sekitar **12 MB**;
  - `Assets/39-stl/stl/Dragon 2.5_stl.stl` sekitar **1,9 MB**.
- Jika ingin memakai model contoh dari proyek lama, impor manual ke proyek ini. Tekstur Elaina berukuran 4096 x 4096; untuk build Android, pertimbangkan menurunkan ukuran tekstur atau memakai kompresi ASTC.
- Paket Vuforia 138 MB di `Packages/` adalah **hard link** ke proyek lama, sehingga tidak menambah pemakaian disk saat ini. Saat proyek dibuka, Unity tetap akan membuat `Library` baru yang cukup besar; kosongkan beberapa GB terlebih dahulu.
- Folder `Library`, `Logs`, `Temp`, dan `UserSettings` adalah folder generated. Jangan commit folder tersebut jika menggunakan Git.
- Jika akan memakai Git, buat `.gitignore` yang mencakup:

```text
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
*.csproj
*.sln
*.zip
*.unitypackage
```

- File `Assets/Resources/VuforiaConfiguration.asset` berisi license key. Jangan publikasikan file ini ke repositori publik.

---

## 12. Checklist penilaian UTS

### Nilai B

- [ ] Scene `MainMenu.unity` ada dan bisa dibuka.
- [ ] Menu utama memiliki desain (background, judul, kartu, tombol).
- [ ] Ada 1 tombol **MULAI AR** yang membuka scene AR.
- [ ] Scene `ARScene.unity` memiliki ARCamera Vuforia.
- [ ] Marker dapat dilacak.
- [ ] Objek 3D muncul di atas marker.

### Nilai A

- [ ] Semua poin Nilai B terpenuhi.
- [ ] Rotasi objek berfungsi melalui slider.
- [ ] Zoom berfungsi melalui pinch, tombol, atau scroll.
- [ ] Ada tombol reset.
- [ ] Ada tombol kembali ke menu.
- [ ] Ada indikator status marker.
- [ ] Build APK berhasil dan diuji di HP.

### Sebelum dikumpulkan

- [ ] Ganti kubus placeholder dengan model 3D dari dosen.
- [ ] Samakan marker/dataset/license dengan yang diberikan dosen.
- [ ] Isi nama, NIM, kelas, dan mata kuliah pada menu.
- [ ] Uji di HP Android, bukan hanya di Editor.
- [ ] Simpan backup proyek.
- [ ] Pastikan APK dapat dibuka dan kamera berfungsi.

---

## 13. Setup manual jika generator gagal

Jika menu generator tidak bisa dijalankan, scene dapat dibuat manual.

### 13.1 Membuat MainMenu manual

1. Buat scene baru: **File > New Scene > Basic (Built-in)**.
2. Buat Canvas: **GameObject > UI > Canvas**.
3. Atur CanvasScaler:
   - UI Scale Mode: `Scale With Screen Size`;
   - Reference Resolution: `1080 x 1920`;
   - Match: `0.5`.
4. Tambahkan background Image, judul Text, dan tombol Button.
5. Buat GameObject kosong `MainMenuController`, tambahkan script `MainMenuController`.
6. Isi field `Start Button` dan `Quit Button` dengan tombol yang dibuat.
7. Pastikan ada `EventSystem` dengan `InputSystemUIInputModule`.
8. Simpan sebagai `Assets/UTS_Template/Scenes/MainMenu.unity`.

### 13.2 Membuat ARScene manual

1. Buat scene baru.
2. Tambahkan **GameObject > Vuforia Engine > AR Camera**.
3. Tambahkan **GameObject > Vuforia Engine > Image Target**.
4. Pada ImageTarget, isi target dan dataset dari dosen.
5. Tambahkan kubus sebagai child ImageTarget, beri nama `Konten3D`.
6. Tambahkan Canvas dengan slider rotasi dan tombol zoom.
7. Buat GameObject kosong `ARController`, tambahkan script `ARInteractionController`.
8. Isi semua field referensi di Inspector.
9. Pastikan ada `EventSystem`.
10. Simpan sebagai `Assets/UTS_Template/Scenes/ARScene.unity`.

### 13.3 Daftarkan scene

1. Buka **File > Build Settings**.
2. Klik **Add Open Scenes** untuk setiap scene.
3. Pastikan `MainMenu` berada di index 0 dan `ARScene` di index 1.

---

## 14. Ringkasan singkat untuk demo

Saat demo di depan dosen, urutan yang disarankan:

1. Buka aplikasi.
2. Tunjukkan menu utama dan jelaskan desainnya.
3. Klik **MULAI AR**.
4. Arahkan kamera ke marker.
5. Tunjukkan objek 3D muncul.
6. Geser slider rotasi.
7. Lakukan pinch zoom atau tekan tombol `+`/`-`.
8. Tekan **RESET** untuk menunjukkan objek kembali ke posisi awal.
9. Tekan **KEMBALI** untuk kembali ke menu.
10. Jelaskan bahwa aplikasi menggunakan Vuforia Image Target dan objek 3D dari dosen.

Dengan template ini, requirement Nilai B dan Nilai A sudah tercakup, tinggal mengganti model 3D, marker, dan license sesuai yang diberikan dosen.
