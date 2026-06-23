using System.IO.Ports;
using Microsoft.Win32;

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
        private int reintentosApertura;

        /// <summary>
        /// Constructor de la ventana principal. Inicializa componentes, configura el puerto serie
        /// y asocia los manejadores de eventos.
        /// </summary>
        public ViewMain()
        {
            InitializeComponent();

            // Posicion esquina superior izquierda
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);

            menuConfiguracion.Visible = false;

            txtTrama.Size = new Size(256, 128);
            txtTrama.ReadOnly = true;
            txtTrama.TabStop = false;
            tamanoFuenteOriginal = txtTrama.Font.Size;

            serialPort1 = new SerialPort(components)
            {
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                // DTR/RTS habilitados: resetea el buffer interno del chip
                // USB-serial al abrir el puerto, evitando datos basura acumulados
                // durante el arranque de Windows (causa común de bloqueo)
                DtrEnable = true,
                RtsEnable = true
            };
            // DataReceived reemplaza al timer1: corre en un hilo de pool,
            // nunca bloquea el hilo de UI aunque ReadExisting() se demore
            serialPort1.DataReceived += SerialPort_DataReceived;
            // Delegado cacheado para BeginInvoke (evita crear uno nuevo por trama)
            _actualizarPesoDelegate = ActualizarPesoDesdeTrama;
            tramaReader = new CbxTrama();

            btnCerrarTrama.Click += BtnCerrarTrama_Click;
            btnCerrarPrograma.Click += btnCerrarPrograma_Click;
            cerrarBalanza.Click += cerrarBalanza_Click;
            cbxComBalanza.DropDown += cbxComBalanza_DropDown;
            cbxTramas.SelectedIndexChanged += cbxTramas_SelectedIndexChanged;
            timer2.Tick += Timer2_Tick;
            Load += ViewMain_Load;
            FormClosing += ViewMain_FormClosing;
            SystemEvents.SessionEnded += SystemEvents_SessionEnded;
        }

        /// <summary>
        /// Se ejecuta cuando Windows está finalizando la sesión (apagado o reinicio).
        /// Cierra el puerto serie inmediatamente y desactiva DTR/RTS para que el
        /// driver libere el handle antes de que el proceso sea terminado.
        /// </summary>
        private void SystemEvents_SessionEnded(object? sender, SessionEndedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.DtrEnable = false;
                serialPort1.RtsEnable = false;
                serialPort1.Close();
            }
        }

        /// <summary>
        /// Evento que se ejecuta al cargar el formulario. Enumera los puertos COM disponibles,
        /// carga la configuración guardada e inicia el timer de reintento si hay un puerto configurado.
        /// </summary>
        private void ViewMain_Load(object? sender, EventArgs e)
        {
            EnumerarPuertosCOM();
            PoblarTiposTrama();
            CargarConfiguracion();

            if (!string.IsNullOrEmpty(cbxComBalanza.Text))
            {
                reintentosApertura = 0;
                // 5000ms: un delay más largo al arranque evita que el primer
                // intento de apertura coincida con la inicialización del driver
                // USB-serial o con datos entrantes del arranque
                timer2.Interval = 5000;
                timer2.Start();
            }
        }

        /// <summary>
        /// Evento que se ejecuta al cerrar el formulario. Detiene el timer de reintento
        /// y cierra el puerto serie inmediatamente.
        /// </summary>
        private void ViewMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            timer2.Stop();
            CerrarPuertoSiAbierto();
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
        /// Intenta abrir el puerto serie configurado con watchdog de 2 fases:
        ///
        /// Fase 1 — El puerto no aparece en GetPortNames() (driver aún inicializando):
        ///          reintenta cada 3s hasta 30 veces (~90s).
        /// Fase 2 — El puerto existe pero Open() falla (driver ocupado con datos):
        ///          reintenta con backoff 2.5s→10s hasta 40 intentos totales.
        ///
        /// Entre fase 1 y 2, fuerza un reset del driver abriendo/cerrando temporalmente
        /// el puerto con DTR/RTS activos (ResetearPuerto).
        /// </summary>
        private void Timer2_Tick(object? sender, EventArgs e)
        {
            reintentosApertura++;

            // Fase 1: el driver aún no expone el puerto en el sistema
            string[] puertos = SerialPort.GetPortNames();
            if (!puertos.Contains(cbxComBalanza.Text))
            {
                if (reintentosApertura >= 30)
                {
                    timer2.Stop();
                    MessageBox.Show(
                        $"El puerto {cbxComBalanza.Text} no está disponible tras {reintentosApertura} intentos.\n\n" +
                        "Verifique que:\n" +
                        "- El dispositivo esté conectado.\n" +
                        "- El driver USB-serial esté correctamente instalado.",
                        "Puerto no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                timer2.Interval = 3000;
                return;
            }

            // Fase 2: el puerto existe, intentar abrir (ResetearPuerto se llama dentro)
            var (exito, mensajeError) = AbrirPuerto(false);
            if (exito)
            {
                timer2.Stop();
            }
            else
            {
                if (reintentosApertura >= 40)
                {
                    timer2.Stop();
                    MessageBox.Show($"No se pudo abrir el puerto {cbxComBalanza.Text} tras {reintentosApertura} intentos.\n" +
                        $"Error: {mensajeError}\n\n" +
                        "Verifique que:\n" +
                        "- El dispositivo esté conectado al puerto.\n" +
                        "- Ningún otro programa esté usando el puerto.\n" +
                        "- El driver del puerto serie esté correctamente instalado.",
                        "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Backoff progresivo: 2.5s, 3s, 3.5s, 4s... máx 10s
                    timer2.Interval = Math.Min(2500 + reintentosApertura * 500, 10000);
                }
            }
        }

        /// <summary>
        /// Abre el puerto COM indicado en cbxComBalanza y configura el tipo de trama.
        /// La lectura se inicia automáticamente vía el evento DataReceived.
        /// </summary>
        private void btnAbrirTrama_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbxComBalanza.Text))
            {
                MessageBox.Show("Seleccione un puerto COM.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (exito, mensajeError) = AbrirPuerto(true);
            if (!exito)
            {
                MessageBox.Show($"Error al abrir puerto serie: {mensajeError}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cierra el puerto serie asociado al formato Trama.
        /// </summary>
        private void BtnCerrarTrama_Click(object? sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();

            txtTrama.Text = "-----";
            RestaurarFuente();
            SetControlesHabilitados(true);
        }

        /// <summary>
        /// Delegado cacheado para ActualizarPesoDesdeTrama.
        /// Almacenarlo como field evita crear un nuevo objeto Action
        /// en cada llamada a BeginInvoke desde DataReceived.
        /// </summary>
        private readonly Action _actualizarPesoDelegate;

        /// <summary>
        /// Se dispara desde un hilo de pool cuando llegan datos al puerto serie.
        /// Lee los datos crudos con ReadExisting (que ya no bloquea el UI),
        /// los acumula en el buffer thread-safe de CbxTrama y programa la
        /// actualización de la UI en el hilo principal vía BeginInvoke.
        ///
        /// Esto reemplaza al antiguo Timer1_Tick que ejecutaba ReadExisting
        /// directamente en el hilo de UI, causando bloqueos al arrancar
        /// con Windows (driver USB-serial en estado inconsistente).
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort1.ReadExisting();
                if (string.IsNullOrEmpty(data))
                    return;

                tramaReader.Alimentar(data);

                if (!IsDisposed)
                    BeginInvoke(_actualizarPesoDelegate);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Se ejecuta en el hilo de UI (vía BeginInvoke desde DataReceived).
        /// Parsea el buffer acumulado y actualiza txtTrama con el peso.
        /// </summary>
        private void ActualizarPesoDesdeTrama()
        {
            if (!serialPort1.IsOpen)
                return;

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

        /// <summary>
        /// Abre/cierra temporalmente el puerto con DTR/RTS activos para forzar
        /// al driver USB-serial a resetear su estado interno. Esto ayuda cuando
        /// el driver rechaza CreateFile() porque está procesando interrupciones
        /// causadas por datos entrantes durante su inicialización.
        /// </summary>
        private void ResetearPuerto(string portName)
        {
            try
            {
                using var tempPort = new SerialPort(portName)
                {
                    DtrEnable = true,
                    RtsEnable = true,
                    ReadTimeout = 100,
                    WriteTimeout = 100
                };
                tempPort.Open();
                Thread.Sleep(50);
                tempPort.DiscardInBuffer();
                tempPort.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Abre el puerto serie con la configuración actual. Si <paramref name="mostrarError"/>
        /// es true, muestra un mensaje de error detallado en caso de fallo.
        /// Antes de abrir, fuerza un reset del driver mediante <see cref="ResetearPuerto"/>.
        /// </summary>
        /// <returns>
        /// Una tupla (bool, string): el primer elemento indica si se abrió correctamente;
        /// el segundo contiene el mensaje de error si falló, o string.Empty si fue exitoso.
        /// </returns>
        private (bool Exito, string MensajeError) AbrirPuerto(bool mostrarError)
        {
            try
            {
                CerrarPuertoSiAbierto();
                serialPort1.PortName = cbxComBalanza.Text;
                tramaReader.TipoTrama = cbxTramas.Text;
                tramaReader.Limpiar();
                ResetearPuerto(cbxComBalanza.Text);
                serialPort1.Open();
                // Descarta datos basura acumulados en el buffer del driver
                // durante el arranque de Windows (especialmente crítico cuando
                // la báscula ya estaba transmitiendo antes de abrir el puerto)
                serialPort1.DiscardInBuffer();

                tramaCambiada = false;
                txtTrama.Text = "-----";
                RestaurarFuente();
                SetControlesHabilitados(false);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                if (mostrarError)
                    MessageBox.Show($"Error al abrir puerto serie: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de puertos COM disponibles y actualiza el ComboBox.
        /// </summary>
        private void EnumerarPuertosCOM()
        {
            cbxComBalanza.Items.Clear();
            string[] puertos = SerialPort.GetPortNames();
            cbxComBalanza.Items.AddRange(puertos);
        }

        /// <summary>
        /// Al desplegar el ComboBox de puertos, vuelve a enumerar los puertos disponibles
        /// y mantiene la selección actual si sigue existiendo.
        /// </summary>
        private void cbxComBalanza_DropDown(object? sender, EventArgs e)
        {
            string seleccionado = cbxComBalanza.Text;
            EnumerarPuertosCOM();

            if (cbxComBalanza.Items.Contains(seleccionado))
                cbxComBalanza.Text = seleccionado;
        }

        /// <summary>
        /// Abre el puerto COM seleccionado en modo balanza.
        /// La lectura se inicia automáticamente vía el evento DataReceived.
        /// </summary>
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
                // Descarta buffer basura del driver (misma razón que en AbrirPuerto)
                serialPort1.DiscardInBuffer();

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

        /// <summary>
        /// Cierra el puerto COM de la balanza.
        /// </summary>
        private void cerrarBalanza_Click(object? sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();

            txtTrama.Text = "-----";
            RestaurarFuente();
            SetControlesHabilitados(true);
        }

        /// <summary>
        /// Se ejecuta al cambiar el tipo de trama. Marca que hubo un cambio,
        /// limpia el buffer interno (para evitar que datos viejos de otro formato
        /// provoquen falsos positivos) y, si el puerto está abierto, limpia la pantalla.
        /// </summary>
        private void cbxTramas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            tramaCambiada = true;
            tramaReader.TipoTrama = cbxTramas.Text;
            // Limpia el buffer acumulado para que el nuevo parser no se
            // confunda con datos del formato anterior
            tramaReader.LimpiarBuffer();

            if (serialPort1.IsOpen)
            {
                txtTrama.Text = "-----";
                RestaurarFuente();
            }
        }

        /// <summary>
        /// Habilita o deshabilita los controles de configuración de puerto y trama.
        /// </summary>
        /// <param name="enabled">True para habilitar, False para deshabilitar.</param>
        private void SetControlesHabilitados(bool enabled)
        {
            cbxComBalanza.Enabled = enabled;
            cbxTramas.Enabled = enabled;
            btnAbrirTrama.Enabled = enabled;
            abrirBalanza.Enabled = enabled;
        }

        /// <summary>
        /// Reduce el tamaño de fuente de txtTrama si el texto supera los 6 caracteres,
        /// para que el peso completo sea visible en el TextBox.
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
        /// Cierra el puerto serie si está abierto.
        /// Se usa antes de cambiar PortName, ya que SerialPort requiere
        /// que el puerto esté cerrado para modificar sus propiedades.
        /// </summary>
        private void CerrarPuertoSiAbierto()
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();
        }

        /// <summary>
        /// Carga la configuración guardada (puerto COM y tipo de trama) desde el archivo JSON.
        /// </summary>
        private void CargarConfiguracion()
        {
            var config = Configuracion.Cargar();
            if (config != null)
            {
                cbxTramas.Text = config.TipoTrama;
                cbxComBalanza.Text = config.COMBalanza;
            }
        }

        /// <summary>
        /// Muestra un mensaje de confirmación y cierra la aplicación si el usuario acepta.
        /// </summary>
        private void btnCerrarPrograma_Click(object? sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de que desea cerrar el programa?",
                "Confirmar cierre",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
                Close();
        }

        /// <summary>
        /// Guarda la configuración actual (puerto COM y tipo de trama) en un archivo JSON
        /// ubicado en la carpeta de datos local del usuario (%LOCALAPPDATA%\DesktopWeightViewer\config.json).
        /// </summary>
        private void btnGuardarConfiguracion_Click(object? sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evita que se seleccione texto en txtTrama al recibir el foco.
        /// </summary>
        private void txtTrama_Enter(object sender, EventArgs e)
        {
            txtTrama.SelectionLength = 0;
        }

        /// <summary>
        /// Reemplaza cada carácter escrito en el campo de contraseña por asteriscos,
        /// almacenando el texto real en la propiedad Tag del control.
        /// </summary>
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

        /// <summary>
        /// Verifica las credenciales del usuario. Si son correctas (root/adminconfig),
        /// muestra el menú de configuración.
        /// </summary>
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
