using System.Text;
using System.Text.RegularExpressions;

namespace DesktopWeightViewer
{
    public class CbxTrama
    {
        public string Trama { get; private set; } = string.Empty;
        public string PesoStr { get; private set; } = string.Empty;
        public int timeoutSerial { get; set; }
        public string TipoTrama { get; set; } = "XKR";

        private readonly StringBuilder _buffer = new StringBuilder(512);
        private readonly object _bufferLock = new object();

        public void Alimentar(string datos)
        {
            if (string.IsNullOrEmpty(datos))
                return;
            lock (_bufferLock)
                _buffer.Append(datos);
        }

        public void LimpiarBuffer()
        {
            lock (_bufferLock)
                _buffer.Clear();
        }

        public void Limpiar()
        {
            Trama = string.Empty;
            PesoStr = string.Empty;
            LimpiarBuffer();
        }

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

        private void ParsearXK310(string buffer)
        {
            ParsearXKR(buffer);
        }

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
