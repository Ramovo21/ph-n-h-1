# Build & Run (WinForms) — HospitalApp

## Yêu cầu

- Windows 10/11
- .NET SDK 8.x (vì project target `net8.0-windows`)
  - Kiểm tra: `dotnet --version`
- (Khi chạy app) Có Oracle DB chạy trên máy người dùng (thường là `localhost:1521/...`)
  - Tên service/PDB **khác nhau tùy bản Oracle**:
    - Oracle XE hay gặp: `XEPDB1`
    - Oracle Database (cài mặc định) hay gặp: `ORCLPDB1`
  - App đọc cấu hình từ biến môi trường `HOSPITALAPP_ORACLE_DATASOURCE` (xem bên dưới)
  - Mã nằm trong [Service/DBConnection.cs](Service/DBConnection.cs)

> Package Oracle dùng `Oracle.ManagedDataAccess.Core` (managed) nên thường **không cần** cài Oracle Instant Client chỉ để build.

## Build bằng `dotnet` (khuyến nghị)

Chạy từ thư mục repo hoặc bất kỳ đâu, miễn là `cd` đúng vào thư mục winformcode:

```powershell
cd D:\DAC_BenhVien\ph-n-h-1\winformcode

# restore nuget
 dotnet restore .\HospitalApp.sln

# build Debug
 dotnet build .\HospitalApp.sln -c Debug

# hoặc build Release
 dotnet build .\HospitalApp.sln -c Release
```

Kết quả build sẽ nằm ở:

- `bin/Debug/net8.0-windows/`
- `bin/Release/net8.0-windows/`

## Run

```powershell
cd D:\DAC_BenhVien\ph-n-h-1\winformcode

# chạy trực tiếp project
 dotnet run --project .\HospitalApp.csproj
```

App sẽ mở form đăng nhập (`LoginForm`).

## Publish ra file `.exe` (để mang sang máy khác)

### Cách 1: Self-contained (khuyến nghị để “máy khác chạy luôn”, không cần cài .NET)

```powershell
cd D:\DAC_BenhVien\ph-n-h-1\winformcode

dotnet publish .\HospitalApp.csproj -c Release -r win-x64 \
  /p:PublishSingleFile=true \
  /p:SelfContained=true
```

File chạy sẽ nằm trong thư mục kiểu:

- `bin/Release/net8.0-windows/win-x64/publish/`

Chỉ cần copy **cả thư mục `publish/`** sang máy khác và chạy `HospitalApp.exe`.

> Lưu ý: Nếu bạn đang mở app (`dotnet run`) mà build/publish báo lỗi kiểu “file is being used by another process”, hãy tắt app trước rồi build lại.
> Có thể tắt nhanh bằng:
>
> ```powershell
> taskkill /IM HospitalApp.exe /F
> ```

### Cách 2: Framework-dependent (nhẹ hơn nhưng máy khác phải cài .NET Desktop Runtime)

```powershell
cd D:\DAC_BenhVien\ph-n-h-1\winformcode

dotnet publish .\HospitalApp.csproj -c Release -r win-x64 \
  /p:PublishSingleFile=true \
  /p:SelfContained=false
```

## Cấu hình kết nối Oracle cho máy người dùng

App **không hard-code** tên PDB/service theo máy bạn nữa. Trên máy người dùng, đặt biến môi trường:

```powershell
# Ví dụ Oracle Database thường gặp
setx HOSPITALAPP_ORACLE_DATASOURCE "localhost:1521/ORCLPDB1"

# Ví dụ Oracle XE thường gặp
setx HOSPITALAPP_ORACLE_DATASOURCE "localhost:1521/XEPDB1"
```

Sau khi setx, đóng/mở lại terminal (hoặc đăng xuất/đăng nhập Windows) để biến môi trường có hiệu lực.

## Build bằng Visual Studio (tuỳ chọn)

- Mở file `HospitalApp.sln`
- Build solution (Ctrl+Shift+B)
- Run (F5)

## Lỗi thường gặp

### 1) `msbuild` không nhận lệnh

Nếu bạn gõ `msbuild` trong terminal và bị báo không nhận diện, bạn có 2 cách:

- Dùng `dotnet build` như ở trên (đơn giản nhất), hoặc
- Cài Visual Studio / Build Tools và chạy trong “Developer Command Prompt for VS”.

### 2) Không đăng nhập được / không kết nối DB

Ứng dụng connect theo biến môi trường `HOSPITALAPP_ORACLE_DATASOURCE`.

Nếu chưa đặt biến này, app sẽ dùng mặc định: `localhost:1521/XEPDB1`.

### 3) Bị chặn đăng nhập SYS/SYSTEM

`LoginForm` có chặn một số user hệ thống (SYS/SYSTEM/LBACSYS) để tránh đăng nhập qua UI.
