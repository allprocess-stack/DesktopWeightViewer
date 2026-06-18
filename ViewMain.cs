using System.IO.Ports;

namespace DesktopWeightViewer
{
    public partial class ViewMain : Form
    {
        private SerialPort serialPort1;
        private CbxTrama tramaReader;

        public ViewMain()
        {
            InitializeComponent();

            //Tamaño de Panel 
            txtTrama.Size = new Size(256, 128);
            //txtTrama no editable 
            txtTrama.ReadOnly = true;
            //Desactivacion de seleccion por defecto 
            txtTrama.TabStop = false;

            serialPort1 = new SerialPort();
            tramaReader = new CbxTrama(serialPort1);

            //CargarConfiguracion();

            btnGuardarConfiguracion.Click += BtnGuardarConfiguracion_Click;
            Load += ViewMain_Load;
        }

        private void ViewMain_Load(object? sender, EventArgs e)
        {
            //cbxTrama.Items.Clear();
            //cbxTrama.Items.AddRange(["XKR", "XK310", "Generic"]);
        }

        private void CargarConfiguracion()
        {
            var config = Configuracion.Cargar();
            if (config != null)
            {
                cbxTrama.Text = config.TipoTrama;
            }
        }

        private void BtnGuardarConfiguracion_Click(object? sender, EventArgs e)
        {
            var config = new Configuracion
            {
                TipoTrama = cbxTrama.Text
            };
            config.Guardar();

            MessageBox.Show("Configuración guardada.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtTrama_Enter(object sender, EventArgs e)
        {
            txtTrama.SelectionLength = 0; // Quita cualquier selección
        }
}

}
