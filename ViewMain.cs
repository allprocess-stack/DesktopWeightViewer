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
        private bool tramaCambiada;

        public ViewMain()
        {
            InitializeComponent();

            menuConfiguracion.Visible = false;

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

            btnCerrarTrama.Click += BtnCerrarTrama_Click;
            cerrarBalanza.Click += cerrarBalanza_Click;
            cbxComBalanza.DropDown += cbxComBalanza_DropDown;
            cbxTramas.SelectedIndexChanged += cbxTramas_SelectedIndexChanged;
            timer1.Tick += Timer1_Tick;
            Load += ViewMain_Load;
            FormClosing += ViewMain_FormClosing;
        }

        private void ViewMain_Load(object? sender, EventArgs e)
        {
            EnumerarPuertosCOM();
            PoblarTiposTrama();
            CargarConfiguracion();

            if (!string.IsNullOrEmpty(cbxComBalanza.Text))
                btnAbrirTrama_Click(null, EventArgs.Empty);
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
            cbxTramas.Items.AddRange(new[] { "XKR", "XK310", "FT11", "Generic" });
        }

        /// <summary>
        /// Abre el puerto COM indicado en cbxComBalanza, configura el tipo de trama
        /// seleccionado en cbxTramas e inicia el timer de lectura.
        /// </summary>
        private void btnAbrirTrama_Click(object? sender, EventArgs e)
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

                tramaCambiada = false;
                txtTrama.Text = "-----";
                RestaurarFuente();
                SetControlesHabilitados(false);
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

            txtTrama.Text = "-----";
            RestaurarFuente();
            SetControlesHabilitados(true);
        }

        /// <summary>
        /// Timer: lee la trama según el tipo seleccionado y actualiza txtTrama.
        /// </summary>
        private void Timer1_Tick(object? sender, EventArgs e)
        {
            if (!serialPort1.IsOpen || string.IsNullOrEmpty(cbxTramas.Text))
            {
                txtTrama.Text = "-----";
                RestaurarFuente();
                return;
            }

            try
            {
                tramaReader.Leer();

                if (!string.IsNullOrEmpty(tramaReader.PesoStr))
                {
                    tramaCambiada = false;
                    txtTrama.Text = tramaReader.PesoStr;
                    AjustarFuenteTrama();
                }
                else if (tramaCambiada)
                {
                    txtTrama.Text = "Trama incorrecta";
                    RestaurarFuente();
                }
            }
            catch
            {
                txtTrama.Text = "-----";
                RestaurarFuente();
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

                tramaCambiada = false;
                txtTrama.Text = "-----";
                RestaurarFuente();
                SetControlesHabilitados(false);
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

            txtTrama.Text = "-----";
            RestaurarFuente();
            SetControlesHabilitados(true);
        }

        private void cbxTramas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            tramaCambiada = true;
            tramaReader.TipoTrama = cbxTramas.Text;

            if (serialPort1.IsOpen)
            {
                txtTrama.Text = "-----";
                RestaurarFuente();
            }
        }

        private void SetControlesHabilitados(bool enabled)
        {
            cbxComBalanza.Enabled = enabled;
            cbxTramas.Enabled = enabled;
            btnAbrirTrama.Enabled = enabled;
            abrirBalanza.Enabled = enabled;
        }

        /// <summary>
        /// Reduce el tamaño de fuente de txtTrama si el texto supera los 6 caracteres.
        /// </summary>
        private void AjustarFuenteTrama()
        {
            int len = txtTrama.Text.Length;

            if (len <= 4)
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


        private void txtContrasena_TextChanged(object sender, EventArgs e)
        {
            ToolStripTextBox txt = sender as ToolStripTextBox;
            string realText = txt.Tag as string ?? "";
            if (txt.Text.Length > realText.Length)
            {
                realText += txt.Text.Substring(realText.Length);
            }
            else if (txt.Text.Length < realText.Length)
            {
                realText = realText.Substring(0, txt.Text.Length);
            }
            txt.Tag = realText;
            txt.Text = new string('*', realText.Length);
            txt.SelectionStart = txt.Text.Length;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string user = txtUsuario.Text;
                string password = txtContrasena.Tag as string ?? "";

                if (user == "root" && password == "adminconfig")
                {
                    menuConfiguracion.Visible = true;
                    MessageBox.Show("Se ha habilitado el menú de configuración.", "Acceso concedido",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso denegado",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
    }
}
