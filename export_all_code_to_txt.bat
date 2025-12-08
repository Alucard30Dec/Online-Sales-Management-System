@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: ========= CẤU HÌNH =========
:: Thư mục gốc của solution ASP.NET Core MVC
set "ROOT=E:\Project\Online-Sales-Management-System"

:: File TXT tổng hợp code
set "OUT=%ROOT%\ALL_FILES_TEXT.txt"

:: CÁC ĐUÔI FILE THƯỜNG SẼ ĐƯỢC CHỈNH SỬA TRONG DỰ ÁN MVC
set "EXTS=.cs .cshtml .js .css .ts .scss .csproj .sln .json .config .xml .resx .md .bat .ps1"

:: Thư mục cần bỏ qua
set "SKIPFOLDERS=\bin\ \obj\ \packages\ \dist\ \build\ \out\ \node_modules\ .git\ .vs\"

:: ========= KHỞI TẠO =========
for %%D in ("%ROOT%") do if not exist "%%~fD" (
  echo [Loi] Khong tim thay thu muc ROOT: "%ROOT%"
  pause
  exit /b 1
)

:: Tạo thư mục cha của OUT nếu chưa có
for %%P in ("%OUT%") do if not exist "%%~dpP" mkdir "%%~dpP" >nul 2>&1

:: Xoá file OUT cũ (nếu có)
if exist "%OUT%" del /f /q "%OUT%" >nul 2>&1

:: Header
>> "%OUT%" echo ======= EXPORT CODE ASP.NET Core MVC =======
>> "%OUT%" echo Thoi gian: %date% %time%
>> "%OUT%" echo Thu muc goc: %ROOT%
>> "%OUT%" echo Chi gom cac file: %EXTS%
>> "%OUT%" echo Bo qua thu muc: %SKIPFOLDERS%
>> "%OUT%" echo.

echo Dang quet ma nguon trong "%ROOT%" ...
echo (co the mat vai giay...)
echo.

:: ========= QUÉT TẤT CẢ FILE =========
for /r "%ROOT%" %%F in (*) do (
  set "FULL=%%~fF"
  set "EXT=%%~xF"

  :: Bỏ qua nếu đường dẫn chứa thư mục skip
  set "SKIP=0"
  for %%S in (%SKIPFOLDERS%) do (
    echo(!FULL!| find /i "%%~S" >nul && set "SKIP=1"
  )

  if "!SKIP!"=="1" (
    rem Bo qua file nam trong thu muc bi skip
  ) else (
    :: Kiểm tra đuôi file có thuộc EXTS không
    set "MATCH=0"
    for %%E in (%EXTS%) do (
      if /i "%%E"=="!EXT!" set "MATCH=1"
    )

    if "!MATCH!"=="1" (
      :: *** LOG RA CONSOLE ***
      echo [EXPORTED] %%~fF

      :: Ghi vào file OUT
      >> "%OUT%" echo ---------- FILE ----------
      >> "%OUT%" echo %%~fF
      >> "%OUT%" echo --------------------------
      powershell -NoProfile -Command ^
        "Get-Content -LiteralPath '%%~fF' -Raw | Out-File -FilePath '%OUT%' -Append -Encoding utf8"
      >> "%OUT%" echo.
    )
  )
)

>> "%OUT%" echo ======= HET =======

echo.
echo Xong! Tat ca file da duoc xuat vao:
echo   "%OUT%"
echo.
echo Bam phim bat ky de thoat...
pause >nul
endlocal
