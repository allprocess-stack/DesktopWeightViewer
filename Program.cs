namespace DesktopWeightViewer
{
    /// <summary>
    /// Punto de entrada de la aplicación Windows Forms.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilita estilos visuales modernos y DPI alto
            ApplicationConfiguration.Initialize();
            Application.Run(new ViewMain());
        }
    }
}