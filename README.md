# DesktopWeightViewer

Aplicación Windows Forms para leer y mostrar el peso de una báscula a través de
puerto serie (COM). Corre en .NET 10, ocupa toda la pantalla sin bordes, y se
posiciona automáticamente en la esquina superior izquierda al iniciar.

## Requisitos

- .NET 10.0 SDK o superior
- Windows (WinForms)
- Puerto COM con báscula o simulador serie

## Estructura del proyecto

```
DesktopWeightViewer/
├── Program.cs              # Punto de entrada (Application.Run)
├── ViewMain.cs             # Ventana principal (560 líneas)
├── ViewMain.Designer.cs    # Controles del formulario (diseñador)
├── CbxTrama.cs             # Parser de tramas con buffer thread-safe
├── Configuracion.cs        # Persistencia JSON de configuración
├── DesktopWeightViewer.csproj
└── bin/config.json         # Config guardada (se genera en %LOCALAPPDATA%)
```

## Flujo de la aplicación

```
Program.Main()
  └─> ViewMain() — constructor
        ├─> Configura SerialPort (9600 8N1, DTR/RTS=true)
        ├─> Suscribe DataReceived (reemplaza al antiguo timer1)
        └─> Suscribe eventos de UI
  └─> ViewMain_Load()
        ├─> EnumerarPuertosCOM()
        ├─> PoblarTiposTrama() — XKR, XK310, FT11, Generic
        ├─> CargarConfiguracion()
        └─> Si hay puerto configurado → timer2 cada 4s (máx 10 reintentos)
  └─> Timer2_Tick → AbrirPuerto() → Open() + DiscardInBuffer()
  └─> SerialPort_DataReceived (hilo de pool)
        ├─> ReadExisting() → datos crudos
        ├─> CbxTrama.Alimentar() → buffer thread-safe
        └─> BeginInvoke(ActualizarPesoDesdeTrama)
  └─> ActualizarPesoDesdeTrama (hilo de UI)
        └─> CbxTrama.Leer() → txtTrama.Text = peso
  └─> [Login] → user: root / pass: adminconfig → muestra menú Configuración
  └─> [Cerrar App] → confirmación → Close()
  └─> FormClosing / SessionEnded
        └─> CerrarPuertoEnSegundoPlano() (hilo aparte + 1s de delay)
```

## Arquitectura de lectura serie

### Antes (timer — causaba el bloqueo)

```
Timer1_Tick (hilo de UI, cada 200ms)
  └─> ReadExisting()  ← se bloqueaba si el driver USB-serial estaba inestable
       El hilo de UI congelado → ventana no responde
```

### Después (DataReceived — no bloquea)

```
DataReceived (hilo de pool del threadpool)
  ├─> ReadExisting() — nunca bloquea el UI
  ├─> CbxTrama.Alimentar() — acumula en StringBuilder con lock
  └─> BeginInvoke(ActualizarPesoDesdeTrama)

ActualizarPesoDesdeTrama (hilo de UI)
  └─> CbxTrama.Leer() — parsea desde el buffer acumulado
  └─> txtTrama.Text = peso
```

### Mejoras aplicadas

| Cambio | Problema que resuelve |
|---|---|
| `DataReceived` en lugar de `Timer1_Tick` | `ReadExisting()` ya no bloquea el hilo de UI |
| `DtrEnable=true`, `RtsEnable=true` | Resetea el buffer interno del chip USB-serial al abrir el puerto |
| `DiscardInBuffer()` tras `Open()` | Descarta datos basura acumulados durante el boot de Windows |
| Buffer thread-safe en `CbxTrama` (StringBuilder + lock) | Separa recepción (background) de parseo (UI) sin condiciones de carrera |
| Primer reintento a 4 segundos (vs 2) | Más margen para que el driver USB-serial se estabilice después del arranque |
| `ReadTimeout`/`WriteTimeout` eliminados | Innecesarios con DataReceived; evitaban excepciones espurias |
| `CbxTrama` ya no depende de `SerialPort` | Puede testearse sin conexión física; recibe datos vía `Alimentar()` |

## Formato de trama (puerto serie)

| Parámetro  | Valor      |
|------------|------------|
| BaudRate   | 9600       |
| DataBits   | 8          |
| Parity     | None       |
| StopBits   | One        |
| DTR        | Enabled    |
| RTS        | Enabled    |

## Tipos de trama soportados

### XKR (predeterminado)
- Inicia con STX (`\x02`)
- Siguiente byte: signo (`+` / `-`)
- Dígitos de peso consecutivos de longitud variable
- Se parsea desde el buffer recorriendo dígitos hasta el primer no-dígito

### XK310
- Misma estructura que XKR; comparten el mismo parser (`ParsearXKR`)

### FT11
- Inicia con STX (`\x02`)
- Bytes 1-2: codec (bit 1 del byte 2 indica signo negativo)
- Bytes 4-9: 6 dígitos de peso
- Longitud mínima desde STX: 10 bytes

### Generic
- Busca el primer número con signo opcional mediante regex `(-?\d+)`
- Útil para básculas que envían texto plano con el peso

## Funcionalidades

### Ventana
- **Sin bordes ni botón de cerrar** (`FormBorderStyle.None`, `ControlBox=false`)
- **Posición fija** en la esquina superior izquierda (0,0)
- **Color invertido**: TextBox negro con texto naranja/rojo en negrita (48pt)

### Conexión serie
- **Conexión automática** al iniciar si hay configuración guardada
- **Reintentos**: hasta 10 intentos cada 4 segundos si falla la apertura
- **Detección de conexión**: muestra "-----" cuando no hay datos
- **Dos modos de apertura**: "Trama" (configura tipo) y "Balanza" (abre directo)
- **Cambio de formato en caliente**: al seleccionar otro tipo, se limpia el buffer interno

### Seguridad
- **Login oculto**: botón "Login" en la barra de estado
- **Usuario**: `root` / Contraseña: `adminconfig` (hardcodeado)
- **Menú de configuración** se muestra solo tras login exitoso
- **Campo de contraseña** con enmascaramiento propio (Tag + asteriscos)

### Cierre
- **Botón "Cerrar App"** en la barra (alineado a la derecha) con confirmación Sí/No
- **Cierre seguro al apagar Windows**: escucha `SystemEvents.SessionEnded`, cierra el puerto en un hilo aparte con 1 segundo de retraso para que el driver libere correctamente el handle

### UI
- **Ajuste dinámico de fuente**: si el peso tiene más de 4 caracteres, reduce la fuente proporcionalmente (mínimo 12pt)
- **Deshabilitado de controles**: mientras el puerto está abierto, los ComboBox y botones de apertura se desactivan
- **Indicador "Trama incorrecta"** al cambiar de formato si los datos no coinciden
- **"Valor negativo excedido"** si el peso baja de -999

### Persistencia
- **Configuración** guardada en `%LOCALAPPDATA%\DesktopWeightViewer\config.json`
- Campos: `TipoTrama` (string) y `COMBalanza` (string)

## Uso

1. Conecta la báscula al puerto COM
2. Abre la aplicación (si está configurada para inicio automático con Windows, se conectará sola)
3. Si no se conecta automáticamente, haz clic en **Login** → ingresa credenciales → accede al menú **Configuración**
4. Selecciona el puerto COM y el tipo de trama
5. Presiona **"Abrir Trama"** o **"Abrir Balanza"**
6. El peso se muestra en tiempo real en el centro de la pantalla
7. Presiona **"Cerrar"** para desconectar
8. Usa **"Guardar Configuración"** para persistir
9. Presiona **"Cerrar App"** para salir (confirmación requerida)

## Configuración

```json
{
  "TipoTrama": "XKR",
  "COMBalanza": "COM3"
}
```

Al iniciar la aplicación, si existe el archivo en `%LOCALAPPDATA%\DesktopWeightViewer\config.json`,
se carga automáticamente y se intenta abrir el puerto COM configurado con reintentos.

---

Desarrollado por: Anthony Josue Laura Perez
GitHub : https://github.com/anthony2004lp
