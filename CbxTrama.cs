using System.Text;
using System.Text.RegularExpressions;

namespace DesktopWeightViewer
{
    /// <summary>
    /// Interpreta tramas de peso desde un buffer acumulado thread-safe
    /// (ya no lee directamente del SerialPort para no bloquear el hilo de UI).
    /// Los datos se insertan vía <see cref="Alimentar"/> desde el hilo de
    /// <see cref="System.IO.Ports.SerialPort.DataReceived"/> y se parsean
    /// desde el hilo de UI en <see cref="Leer"/>.
    /// </summary>
    public class CbxTrama
    {
        public string Trama { get; private set; } = string.Empty;
        public string PesoStr { get; private set; } = string.Empty;
        public int timeoutSerial { get; set; }
        public string TipoTrama { get; set; } = "XKR";

        // Buffer circular protegido con lock, thread-safe para productor/consumidor
        // entre DataReceived (hilo de pool) y Leer (hilo de UI vía BeginInvoke)
        private readonly StringBuilder _buffer = new StringBuilder(512);
        private readonly object _bufferLock = new object();

        /// <summary>
        /// Alimenta el buffer interno con datos crudos del puerto serie.
        /// Llamado desde SerialPort_DataReceived (hilo de pool).
        /// </summary>
        public void Alimentar(string datos)
        {
            if (string.IsNullOrEmpty(datos))
                return;
            lock (_bufferLock)
                _buffer.Append(datos);
        }

        /// <summary>
        /// Limpia el buffer interno. Se usa al cambiar el tipo de trama
        /// para evitar que datos viejos de otro formato causen falsos positivos.
        /// </summary>
        public void LimpiarBuffer()
        {
            lock (_bufferLock)
                _buffer.Clear();
        }

        /// <summary>
        /// Limpia todos los datos: trama parseada, peso y buffer interno.
        /// Se usa al abrir un puerto nuevo o cambiar de tipo de trama.
        /// </summary>
        public void Limpiar()
        {
            Trama = string.Empty;
            PesoStr = string.Empty;
            LimpiarBuffer();
        }

        /// <summary>
        /// Parsea el buffer según el formato activo.
        /// Se llama desde ActualizarPesoDesdeTrama (hilo de UI).
        /// Si encuentra una trama válida, la consume del buffer y actualiza PesoStr.
        /// Si no hay suficientes datos (trama parcial), no consume nada y espera.
        /// </summary>
        public void Leer()
        {
            string buffer;
            lock (_bufferLock)
                buffer = _buffer.ToString();

            if (string.IsNullOrEmpty(buffer))
            {
                PesoStr = string.Empty;
                return;
            }

            switch (TipoTrama)
            {
                case "XK310":
                    ParsearXK310(buffer);
                    break;
                case "FT11":
                    ParsearFT11(buffer);
                    break;
                case "Generic":
                    ParsearGeneric(buffer);
                    break;
                default:
                    ParsearXKR(buffer);
                    break;
            }
        }

        /// <summary>
        /// Formato XKR: STX (\x02) + signo (+/-) + dígitos de peso variables.
        /// Busca el STX en el buffer, extrae signo y dígitos hasta el primer no-dígito.
        /// </summary>
        private void ParsearXKR(string buffer)
        {
            int stxIdx = buffer.IndexOf('\x02');
            if (stxIdx < 0)
                return;

            if (buffer.Length < stxIdx + 3)
                return;

            char signo = buffer[stxIdx + 1];

            int idx = stxIdx + 2;
            while (idx < buffer.Length && char.IsDigit(buffer[idx]))
                idx++;

            if (idx == stxIdx + 2)
                return;

            string rawPeso = buffer.Substring(stxIdx + 2, idx - (stxIdx + 2));

            try
            {
                long peso = long.Parse(rawPeso);
                if (signo != '+')
                    peso = -peso;

                if (peso < -999)
                {
                    PesoStr = "Valor negativo excedido";
                    ConsumirBuffer(idx);
                    return;
                }

                Trama = buffer.Substring(stxIdx, idx - stxIdx);
                PesoStr = peso.ToString("0");
                ConsumirBuffer(idx);
            }
            catch { }
        }

        /// <summary>
        /// XK310 tiene la misma estructura que XKR.
        /// </summary>
        private void ParsearXK310(string buffer)
        {
            ParsearXKR(buffer);
        }

        /// <summary>
        /// FT11: STX (\x02) + codec (2 bytes: signo y decimales) + 6 dígitos de peso.
        /// Requiere al menos 10 bytes desde STX.
        /// </summary>
        private void ParsearFT11(string buffer)
        {
            int stxIdx = buffer.IndexOf('\x02');
            if (stxIdx < 0)
                return;

            if (buffer.Length < stxIdx + 10)
                return;

            string rawPeso = buffer.Substring(stxIdx + 4, 6);
            string codecStr = buffer.Substring(stxIdx + 1, 2);

            int signo;
            if ((codecStr[1] & 0x02) == 2)
                signo = -1;
            else
                signo = 1;

            try
            {
                long peso = long.Parse(rawPeso) * signo;

                if (peso < -999)
                {
                    PesoStr = "Valor negativo excedido";
                    ConsumirBuffer(stxIdx + 10);
                    return;
                }

                Trama = buffer.Substring(stxIdx, 10);
                PesoStr = peso.ToString();
                ConsumirBuffer(stxIdx + 10);
            }
            catch { }
        }

        /// <summary>
        /// Generic: busca el primer número con signo opcional vía regex.
        /// Consume hasta el final del número encontrado.
        /// </summary>
        private void ParsearGeneric(string buffer)
        {
            try
            {
                Match match = Regex.Match(buffer, @"(-?\d+)");
                if (!match.Success)
                    return;

                long peso = long.Parse(match.Groups[1].Value);

                if (peso < -999)
                {
                    PesoStr = "Valor negativo excedido";
                    ConsumirBuffer(buffer.Length);
                    return;
                }

                Trama = match.Value;
                PesoStr = peso.ToString("0");
                ConsumirBuffer(match.Index + match.Length);
            }
            catch { }
        }

        /// <summary>
        /// Remueve los primeros <paramref name="cantidad"/> caracteres del buffer.
        /// Thread-safe: usa el mismo lock que Alimentar.
        /// Si cantidad >= tamaño del buffer, lo limpia completamente para evitar
        /// acumulación de basura no parseable.
        /// </summary>
        private void ConsumirBuffer(int cantidad)
        {
            if (cantidad <= 0)
                return;
            lock (_bufferLock)
            {
                if (cantidad >= _buffer.Length)
                    _buffer.Clear();
                else
                    _buffer.Remove(0, cantidad);
            }
        }
    }
}
