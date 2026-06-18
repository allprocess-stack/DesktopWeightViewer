using System.Text.Json;

namespace DesktopWeightViewer
{
    /// <summary>
    /// Almacena y recupera la configuración de la aplicación en un archivo JSON.
    /// </summary>
    public class Configuracion
    {
        /// <summary>Tipo de trama seleccionado (ej. "XKR", "XK310", "Generic").</summary>
        public string TipoTrama { get; set; } = string.Empty;

        public string COMBalanza { get; set; } = string.Empty;

        /// <summary>Ruta del archivo de configuración en la raíz del proyecto.</summary>
        private static string RutaArchivo =>
            Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.FullName, "config.json");

        /// <summary>
        /// Guarda la configuración actual como JSON en el archivo.
        /// </summary>
        public void Guardar()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RutaArchivo)!);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RutaArchivo, json);
        }

        /// <summary>
        /// Carga la configuración desde el archivo JSON.
        /// </summary>
        /// <returns>Objeto <see cref="Configuracion"/> o null si no existe o hay error.</returns>
        public static Configuracion? Cargar()
        {
            if (!File.Exists(RutaArchivo))
                return null;

            try
            {
                string json = File.ReadAllText(RutaArchivo);
                return JsonSerializer.Deserialize<Configuracion>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
