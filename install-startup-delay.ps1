<#
.SYNOPSIS
    Registra DesktopWeightViewer en el Task Scheduler de Windows con un retraso
    de 30 segundos después del inicio de sesión. Reemplaza la entrada de registro
    HKCU\...\Run que ejecuta el programa inmediatamente.

.DESCRIPTION
    Usar el Task Scheduler con retraso evita que el programa intente abrir el
    puerto serie durante la ventana crítica en que los drivers USB-serial
    (CH340, CP2102, FTDI) aún están inicializándose.

    Si el programa está actualmente en HKCU\...\Run, este script lo elimina
    de allí y crea la tarea programada equivalente con 30s de retraso.

.EXAMPLE
    .\install-startup-delay.ps1
#>

$taskName = "DesktopWeightViewer"
$exePath = "C:\Users\antho\source\repos\DesktopWeightViewer\bin\Debug\net10.0-windows\DesktopWeightViewer.exe"

# --- Validar que el ejecutable exista ---
if (-not (Test-Path -LiteralPath $exePath))
{
    Write-Warning "No se encontró el ejecutable en: $exePath"
    Write-Warning "Compila el proyecto con 'dotnet build' antes de ejecutar este script."
    exit 1
}

# --- Eliminar entrada existente de HKCU\...\Run si existe ---
$runPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$existing = Get-ItemProperty -Path $runPath -Name $taskName -ErrorAction SilentlyContinue
if ($existing -ne $null)
{
    Remove-ItemProperty -Path $runPath -Name $taskName
    Write-Host "Eliminada entrada '$taskName' de HKCU\...\Run" -ForegroundColor Yellow
}

# --- Eliminar tarea existente si ya existe ---
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask -ne $null)
{
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
    Write-Host "Eliminada tarea programada '$taskName' existente" -ForegroundColor Yellow
}

# --- Crear la nueva tarea ---
$action = New-ScheduledTaskAction -Execute $exePath

$trigger = New-ScheduledTaskTrigger -AtLogOn
$trigger.Delay = "PT30S"  # 30 segundos de retraso

$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -RunLevel Limited

Register-ScheduledTask -TaskName $taskName `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Description "DesktopWeightViewer — visor de peso de báscula por puerto serie. Inicia con 30s de retraso para evitar conflictos con la inicialización del driver USB-serial."

Write-Host ""
Write-Host "Tarea '$taskName' registrada correctamente." -ForegroundColor Green
Write-Host "  Ejecutable : $exePath"
Write-Host "  Disparador  : Al iniciar sesión ($env:USERNAME)"
Write-Host "  Retraso     : 30 segundos"
Write-Host ""
Write-Host "Para desinstalar: Unregister-ScheduledTask -TaskName '$taskName' -Confirm:`$false"
