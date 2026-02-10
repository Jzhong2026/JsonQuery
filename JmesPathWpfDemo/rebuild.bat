@echo off
echo ===================================
echo 清理并重新构建 JmesPathWpfDemo
echo ===================================
echo.

echo [1/4] 停止正在运行的进程...
taskkill /F /IM JmesPathWpfDemo.exe 2>nul
timeout /t 2 >nul

echo [2/4] 清理 obj 和 bin 文件夹...
cd /d "C:\Users\jason\source\repos\JsonQuery"
if exist "JmesPathWpfDemo\obj" rmdir /s /q "JmesPathWpfDemo\obj"
if exist "JmesPathWpfDemo\bin" rmdir /s /q "JmesPathWpfDemo\bin"

echo [3/4] 执行 dotnet clean...
dotnet clean JmesPathWpfDemo\JmesPathWpfDemo.csproj

echo [4/4] 执行 dotnet build...
dotnet build JmesPathWpfDemo\JmesPathWpfDemo.csproj

echo.
echo ===================================
echo 构建完成！
echo ===================================
echo.
echo 现在可以在 Visual Studio 中运行应用程序 (F5)
echo.
pause
