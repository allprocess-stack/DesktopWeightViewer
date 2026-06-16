using System.Text.Json;

namespace DesktopWeightViewer
{
    public class Configuracion
    {
        public string TipoTrama { get; set; } = string.Empty;

        private static string RutaArchivo =>
            Path.Combine(AppContext.BaseDirectory, "config.json");

        public void Guardar()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RutaArchivo)!);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RutaArchivo, json);
        }

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
