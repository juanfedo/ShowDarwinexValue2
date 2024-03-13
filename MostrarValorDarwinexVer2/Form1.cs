using System.Globalization;
using System.Text.Json;
using Tulpep.NotificationWindow;

namespace MostrarValorDarwinexVer2
{
    public partial class Form1 : Form
    {
        string darwinX = string.Empty;
        string limiteSuperior = string.Empty;
        string intervalo = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        async Task<ApiResponse?> ObtenerTasaDeCambio(string darwinX)
        {
            using var httpClient = new HttpClient();
            var url = $"https://www.darwinex.com/api/productquotes/quotes/name?productName={darwinX}";
            var response = await httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonString = responseContent.Replace("[", "").Replace("]", "");
            var responseObject = JsonSerializer.Deserialize<ApiResponse>(jsonString);
            return responseObject;
        }

        void MostrarNotificacion(string darwinX, string limiteSuperior, string valorActual)
        {
            this.Invoke((MethodInvoker)delegate
            {

                string texto = $"El limite superior es {limiteSuperior}" + Environment.NewLine
                + $"El valor actual es {valorActual}" + Environment.NewLine;

                PopupNotifier popup = new PopupNotifier();
                using (StreamWriter archivo = new StreamWriter("log.txt", true))
                {
                    archivo.WriteLine(texto.Replace(Environment.NewLine, " ") + $" {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
                    archivo.Close();
                }

                popup.TitleText = $"Spreed de {darwinX}";
                popup.ContentText = texto;
                notifyIcon1.Text = texto;

                popup.Popup();
            });
        }

        private async Task iniciarProceso()
        {
            darwinX = textBox2.Text.Trim();
            limiteSuperior = textBox1.Text.Trim();
            intervalo = textBox3.Text.Trim();

            //this.Hide();
            notifyIcon1.Visible = true;

            while (true)
            {
                var respuesta = await ObtenerTasaDeCambio(darwinX);
                var spreadActual = respuesta?.spread;
                var valorActual = respuesta?.value;

                if (float.Parse(valorActual, CultureInfo.InvariantCulture.NumberFormat) > float.Parse(limiteSuperior, CultureInfo.InvariantCulture.NumberFormat))
                    MostrarNotificacion(darwinX, limiteSuperior, valorActual);

                Thread.Sleep(int.Parse(intervalo));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread trd;
            trd = new Thread(async () => await iniciarProceso());
            trd.Start();
            button1.Hide();
            this.Text = "Corriendo";
        }
    }
}
