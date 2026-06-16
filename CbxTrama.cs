using System.IO.Ports;

namespace DesktopWeightViewer
{
    public class CbxTrama
    {
        public string Trama { get; private set; } = string.Empty;
        public string PesoStr { get; private set; } = string.Empty;
        public int timeoutSerial { get; set; }

        private SerialPort serialPort;

        public CbxTrama(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public void ReadWeight_XKR()
        {
            string text, rawPeso;
            int lon;
            double y;

            text = serialPort.ReadExisting();
            lon = text.Length;
            if (lon < 11)
                return;

            try
            {
                Trama = text.Substring(text.IndexOf('\x02'));
                char signo = Trama.ElementAt(1);
                rawPeso = Trama.Substring(2, 6);

                y = int.Parse(rawPeso);
                if (signo != '+')
                { y = y * (-1); }

                timeoutSerial = 0;

                PesoStr = y.ToString("0");
            }
            catch
            {
                return;
            }
        }
    }
}
