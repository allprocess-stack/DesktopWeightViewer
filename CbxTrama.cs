using System.IO.Ports;
using System.Text.RegularExpressions;

namespace DesktopWeightViewer
{
    /// <summary>
    /// Interpreta tramas de peso provenientes de una báscula por puerto serie.
    /// Soporta múltiples formatos: XKR, XK310, Generic.
    /// </summary>
    public class CbxTrama
    {
        /// <summary>Trama completa desde STX hasta el final.</summary>
        public string Trama { get; private set; } = string.Empty;

        /// <summary>Peso formateado como texto (con signo negativo si corresponde).</summary>
        public string PesoStr { get; private set; } = string.Empty;

        /// <summary>Contador de timeout de lectura serie.</summary>
        public int timeoutSerial { get; set; }

        /// <summary>Formato de trama activo ("XKR", "XK310" o "Generic").</summary>
        public string TipoTrama { get; set; } = "XKR";

        private readonly SerialPort serialPort;

        public CbxTrama(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        /// <summary>
        /// Lee una trama del puerto serie usando el formato activo (<see cref="TipoTrama"/>).
        /// </summary>
        public void Leer()
        {
            switch (TipoTrama)
            {
                case "XK310":
                    ReadWeight_XK310();
                    break;
                case "Generic":
                    ReadWeight_Generic();
                    break;
                default:
                    ReadWeight_XKR();
                    break;
            }
        }

        /// <summary>
        /// Lee y decodifica una trama de peso con formato XKR.
        /// Formato esperado: STX + signo (1 char) + peso (dígitos variables).
        /// Lee todos los dígitos consecutivos después del signo.
        /// </summary>
        public void ReadWeight_XKR()
        {
            string text, rawPeso;
            int lon;

            text = serialPort.ReadExisting();
            lon = text.Length;

            if (lon < 9)
                return;

            try
            {
                Trama = text.Substring(text.IndexOf('\x02'));

                char signo = Trama.ElementAt(1);

                // Lee todos los dígitos consecutivos después del signo
                int idx = 2;
                while (idx < Trama.Length && char.IsDigit(Trama[idx]))
                    idx++;
                rawPeso = Trama.Substring(2, idx - 2);

                if (string.IsNullOrEmpty(rawPeso))
                    return;

                long peso = long.Parse(rawPeso);
                if (signo != '+')
                    peso = -peso;

                if (peso < -999)
                    peso = -999;

                timeoutSerial = 0;
                PesoStr = peso.ToString("0");
            }
            catch
            {
                // Si hay error de parsing, se ignora esta lectura
            }
        }

        /// <summary>
        /// Lee y decodifica una trama de peso con formato XK310.
        /// Formato esperado: STX + signo (1 char) + peso (dígitos variables).
        /// Lee todos los dígitos consecutivos después del signo.
        /// </summary>
        public void ReadWeight_XK310()
        {
            string text, rawPeso;
            int lon;

            text = serialPort.ReadExisting();
            lon = text.Length;

            if (lon < 9)
                return;

            try
            {
                Trama = text.Substring(text.IndexOf('\x02'));

                char signo = Trama.ElementAt(1);

                // Lee todos los dígitos consecutivos después del signo
                int idx = 2;
                while (idx < Trama.Length && char.IsDigit(Trama[idx]))
                    idx++;
                rawPeso = Trama.Substring(2, idx - 2);

                if (string.IsNullOrEmpty(rawPeso))
                    return;

                long peso = long.Parse(rawPeso);
                if (signo != '+')
                    peso = -peso;

                if (peso < -999)
                    peso = -999;

                timeoutSerial = 0;
                PesoStr = peso.ToString("0");
            }
            catch
            {
                // Si hay error de parsing, se ignora esta lectura
            }
        }

        /// <summary>
        /// Lee y extrae el primer número encontrado en el buffer del puerto serie.
        /// Formato: busca cualquier valor numérico con signo opcional.
        /// </summary>
        public void ReadWeight_Generic()
        {
            string text;

            text = serialPort.ReadExisting();

            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                Trama = text;

                // Busca el primer número con signo opcional en la trama
                Match match = Regex.Match(text, @"(-?\d+)");
                if (match.Success)
                {
                    long peso = long.Parse(match.Groups[1].Value);

                    if (peso < -999)
                        peso = -999;

                    timeoutSerial = 0;
                    PesoStr = peso.ToString("0");
                }
            }
            catch
            {
                // Si hay error de parsing, se ignora esta lectura
            }
        }

        /// <summary>
        /// Limpia los resultados de la última lectura.
        /// </summary>
        public void Limpiar()
        {
            Trama = string.Empty;
            PesoStr = string.Empty;
        }
    }
}
