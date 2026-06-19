# DesktopWeightViewer

Aplicación Windows Forms para leer y mostrar el peso de una báscula a través de puerto serie (COM). Soporta múltiples formatos de trama.

## Requisitos

- .NET 10.0 SDK o superior
- Windows (WinForms)
- Puerto COM con báscula o simulador serie

## Estructura del proyecto

```
DesktopWeightViewer/
├── Program.cs              # Punto de entrada de la aplicación
├── ViewMain.cs             # Ventana principal (UI, timer, eventos)
├── ViewMain.Designer.cs    # Diseñador de la ventana principal
├── CbxTrama.cs             # Parseo de tramas de peso (XKR, XK310, FT11, Generic)
├── Configuracion.cs        # Guardado/carga de config.json
├── DesktopWeightViewer.csproj
└── config.json             # Archivo de configuración (se genera al guardar)
```

## Formato de trama (puerto serie)

| Parámetro  | Valor      |
|------------|------------|
| BaudRate   | 9600       |
| DataBits   | 8          |
| Parity     | None       |
| StopBits   | One        |
| Timeout    | 500 ms     |

## Tipos de trama soportados

### XKR
- Inicia con STX (`\x02`)
- Siguiente byte: signo (`+` / `-`)
- Dígitos de peso consecutivos de longitud variable
- Finaliza con ETX (`\x03`) + checksum

### XK310
- Misma estructura que XKR

### FT11
- Inicia con STX (`\x02`)
- Bytes 1-2: código de signo y decimales
- Bytes 4-9: 6 dígitos de peso
- Longitud mínima: 16 bytes

### Generic
- Busca cualquier número con signo opcional en el buffer mediante expresión regular

## Funcionalidades

- **Conexión automática** al iniciar si hay configuración guardada
- **Indicador "-----"** cuando no hay conexión o no se reciben datos válidos
- **"Trama incorrecta"** al cambiar el tipo de trama si el formato no coincide
- **"Valor negativo excedido"** si el peso baja de -999
- **Ajuste automático de fuente** para valores largos (> 5 caracteres)
- **Persistencia de configuración** en `config.json` (puerto COM y tipo de trama)
- **Botones de apertura/cierre** con deshabilitado de controles mientras conectado
- **Detección de desconexión** con mensaje visual

## Uso

1. Conecta la báscula al puerto COM
2. Abre la aplicación
3. Selecciona el puerto COM en el menú
4. Selecciona el tipo de trama (XKR, XK310, FT11 o Generic)
5. Presiona **"Abrir Trama"** o **"Abrir Balanza"**
6. El peso se muestra en tiempo real en el centro de la pantalla
7. Presiona **"Cerrar"** para desconectar
8. Usa **"Guardar Configuración"** para persistir la configuración actual

## Configuración

La configuración se guarda en `config.json` en la raíz del proyecto:

```json
{
  "TipoTrama": "XKR",
  "COMBalanza": "COM3"
}
```

Al iniciar la aplicación, si existe `config.json`, se carga automáticamente y se intenta abrir el puerto COM configurado.

---

Desarrollado por: Anthony Josue Laura Perez
GitHub : https://github.com/anthony2004lp
