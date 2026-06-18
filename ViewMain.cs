using System.IO.Ports;

namespace DesktopWeightViewer
{
    /// <summary>
    /// Ventana principal de la aplicación. Muestra el peso leído desde una báscula
    /// por puerto serie en un TextBox de gran tamaño (txtTrama).
    /// </summary>
    public partial class ViewMain : Form
    {
        private SerialPort serialPort1;
        private CbxTrama tramaReader;
        private float tamanoFuenteOriginal;

        public ViewMain()
        {
            InitializeComponent();

            txtTrama.Size = new Size(256, 128);
            txtTrama.ReadOnly = true;
            txtTrama.TabStop = false;
            tamanoFuenteOriginal = txtTrama.Font.Size;

            serialPort1 = new SerialPort
            {
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            tramaReader = new CbxTrama(serialPort1);

            btnGuardarConfiguracion.Click += btnGuardarConfiguracion_Click;
            btnAbrirTrama.Click += BtnAbrirTrama_Click;
            btnCerrarTrama.Click += BtnCerrarTrama_Click;
            abrirBalanza.Click += abrirBalanza_Click;
            cerrarBalanza.Click += cerrarBalanza_Click;
            cbxComBalanza.DropDown += cbxComBalanza_DropDown;
            timer1.Tick += Timer1_Tick;
            Load += ViewMain_Load;
            FormClosing += ViewMain_FormClosing;
        }

        private void ViewMain_Load(object? sender, EventArgs e)
        {
            CargarConfiguracion();
            EnumerarPuertosCOM();
            PoblarTiposTrama();
        }

        private void ViewMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                timer1.Stop();
                serialPort1.Close();
            }
        }

        /// <summary>
        /// Pobla el ComboBox de tramas con los tipos disponibles.
        /// </summary>
        private void PoblarTiposTrama()
        {
            cbxTramas.Items.Clear();
            cbxTramas.Items.AddRange(new[] { "XKR", "XK310", "Generic" });
        }

        /// <summary>
        /// Abre el puerto COM indicado en cbxComBalanza, configura el tipo de trama
        /// seleccionado en cbxTramas e inicia el timer de lectura.
        /// </summary>
        private void BtnAbrirTrama_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbxComBalanza.Text))
            {
                MessageBox.Show("Seleccione un puerto COM.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                CerrarPuertoSiAbierto();
                serialPort1.PortName = cbxComBalanza.Text;
                tramaReader.TipoTrama = cbxTramas.Text;
                tramaReader.Limpiar();
                serialPort1.Open();
                timer1.Interval = 200;
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir puerto serie: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCerrarTrama_Click(object? sender, EventArgs e)
        {
            timer1.Stop();

            if (serialPort1.IsOpen)
                serialPort1.Close();

            txtTrama.Text = string.Empty;
            RestaurarFuente();
        }

        /// <summary>
        /// Timer: lee la trama según el tipo seleccionado y actualiza txtTrama.
        /// </summary>
        private void Timer1_Tick(object? sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                timer1.Stop();
                return;
            }

            try
            {
                tramaReader.Leer();

                if (!string.IsNullOrEmpty(tramaReader.PesoStr))
                {
                    txtTrama.Text = tramaReader.PesoStr;
                    AjustarFuenteTrama();
                }
            }
            catch
            {
                // Si falla la lectura, se ignora este ciclo
            }
        }

        private void EnumerarPuertosCOM()
        {
            cbxComBalanza.Items.Clear();
            string[] puertos = SerialPort.GetPortNames();
            cbxComBalanza.Items.AddRange(puertos);
        }

        private void cbxComBalanza_DropDown(object? sender, EventArgs e)
        {
            string seleccionado = cbxComBalanza.Text;
            EnumerarPuertosCOM();

            if (cbxComBalanza.Items.Contains(seleccionado))
                cbxComBalanza.Text = seleccionado;
        }

        private void abrirBalanza_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbxComBalanza.Text))
            {
                MessageBox.Show("Seleccione un puerto COM.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                CerrarPuertoSiAbierto();
                serialPort1.PortName = cbxComBalanza.Text;
                serialPort1.Open();
                timer1.Interval = 200;
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir puerto serie: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cerrarBalanza_Click(object? sender, EventArgs e)
        {
            timer1.Stop();

            if (serialPort1.IsOpen)
                serialPort1.Close();

            txtTrama.Text = string.Empty;
            RestaurarFuente();
        }

        /// <summary>
        /// Reduce el tamaño de fuente de txtTrama si el texto supera los 6 caracteres.
        /// </summary>
        private void AjustarFuenteTrama()
        {
            int len = txtTrama.Text.Length;

            if (len <= 6)
            {
                RestaurarFuente();
                return;
            }

            float nuevoTamano = tamanoFuenteOriginal * 6f / len;
            nuevoTamano = Math.Max(nuevoTamano, 12f);

            if (Math.Abs(txtTrama.Font.Size - nuevoTamano) > 0.5f)
                txtTrama.Font = new Font(txtTrama.Font.FontFamily, nuevoTamano, txtTrama.Font.Style);
        }

        /// <summary>
        /// Restaura el tamaño de fuente original de txtTrama.
        /// </summary>
        private void RestaurarFuente()
        {
            if (Math.Abs(txtTrama.Font.Size - tamanoFuenteOriginal) > 0.5f)
                txtTrama.Font = new Font(txtTrama.Font.FontFamily, tamanoFuenteOriginal, txtTrama.Font.Style);
        }

        /// <summary>
        /// Cierra el puerto serie si está abierto (permite cambiar PortName).
        /// </summary>
        private void CerrarPuertoSiAbierto()
        {
            if (serialPort1.IsOpen)
            {
                timer1.Stop();
                serialPort1.Close();
            }
        }

        private void CargarConfiguracion()
        {
            var config = Configuracion.Cargar();
            if (config != null)
            {
                cbxTramas.Text = config.TipoTrama;
                cbxComBalanza.Text = config.COMBalanza;
            }
        }

        private void btnGuardarConfiguracion_Click(object? sender, EventArgs e)
        {
            var config = new Configuracion
            {
                TipoTrama = cbxTramas.Text,
                COMBalanza = cbxComBalanza.Text
            };
            config.Guardar();

            MessageBox.Show("Configuración guardada.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtTrama_Enter(object sender, EventArgs e)
        {
            txtTrama.SelectionLength = 0;
        }
    }
}
