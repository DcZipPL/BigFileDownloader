using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Speech.Synthesis;
using System.Net.NetworkInformation;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.ComponentModel;

namespace BigFileDownloader
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private float n1 = 0;
        private float n2 = 0;

        private int currentPart = 1;
        private int allParts = 1;

        private long lastbyteintime = 1; //64 bit
        private long byteintime = 0; //64 bit

        private bool CanStop = false;
        private bool isWrited = false;
        private bool doOnce = false;

        private WebClient client = new WebClient();
        private Uri[] addresses = {};

        public byte[] bytes;

        int i1 = 0;
        private bool isC;
        private long gc1;

        public MainWindow()
        {
            InitializeComponent();
            client.DownloadProgressChanged += DownloadProgressChanged;
            InfoBlock.Text = "";
        }

        private string byteEncoder(long n)
        {
            string dbytes = "";
            if (n >= 1073741824)
            {
                n2 = n / 1073741824;
                dbytes = n2 + " GiB";
            }
            if (n >= 1048576)
            {
                n2 = n / 1048576;
                dbytes = n2 + " MiB";
            }
            else if (n >= 1024)
            {
                n2 = n / 1024;
                dbytes = n2 + " KiB";
            }
            else
            {
                n2 = n;
                dbytes = n2 + " B";
            }
            return dbytes;
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Random rnd = new Random();
            int month = rnd.Next(1, 12); //16
            int month1 = rnd.Next(1, 6);
            string dbytes = "";
            string tbytes = "";

            byteintime = e.BytesReceived / lastbyteintime;

            //n1 += month1;

            dbytes = byteEncoder(e.BytesReceived);
            tbytes = byteEncoder(e.TotalBytesToReceive);

            if (netProgressBar.Value != netProgressBar.Maximum)
            {
                netProgressBar.Value = e.ProgressPercentage * 2;
                NETlabel.Text = "NET " + (int)(netProgressBar.Value / 2) + "% \\ " + dbytes + " of " + tbytes + " \\ Part " + currentPart + " of " + allParts; //(month1 * (month1 * 6))
            }
            lastbyteintime = e.BytesReceived;
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanStop == true)
            {
                bInstall.Content = "Install";
                bytes = null;
                CanStop = false;
            }
            else
            {
                try
                {
                    addresses = new Uri[255];
                    string[] addressStrings = addressbox.Text.Split('|');
                    foreach (string str in addressStrings)
                    {
                        addresses[i1] = new Uri(str);
                        i1++;
                    }
                    for (int i = 0; i <= 200; i++)
                    {
                        if (addresses[i + 1] != null)
                        {
                            allParts++;
                        }
                    }
                    if (addresses != null)
                    {
                        InfoBlock.Text = "";
                        bInstall.Content = "Stop";
                        CanStop = true;
                        await TimerTask();
                    }
                }
                catch (UriFormatException ex)
                {
                    InfoBlock.Text = ex.Message;
                }
            }
        }

        private async Task TimerTask()
        {
            Random rnd = new Random();
            if (netProgressBar.Value != netProgressBar.Maximum && !doOnce)
            {
                doOnce = true;
                await DownloadFile(addresses);
                gc1 = System.GC.GetTotalMemory(true);
            }
            else
            {
                if (rnd.Next(0, 16) == 4)
                    await Task.Delay(10);
                otherProgressBar.Value += 12;
                IOlabel.Text = "Writing " + (int)(otherProgressBar.Value / 6) + "%";
            }
            if (otherProgressBar.Value == otherProgressBar.Maximum)
            {
                isC = true;
            }
            await Task.Delay(1);

            if (isC == false)
                await TimerTask();
            else
            {
                bytes = null;
                System.GC.Collect();
                bInstall.Content = "Install";
                netProgressBar.Value = netProgressBar.Minimum;
                otherProgressBar.Value = otherProgressBar.Minimum;
                IOlabel.Text = "I/O 0%";
                NETlabel.Text = "NET 0%";
                InfoBlock.Text = "Download Complited!";
                isC = false;
                isWrited = false;
                doOnce = false;
                CanStop = false;
                currentPart = 1;
                allParts = 0;
            }
        }

        private async Task DownloadFile(Uri[] partsAddresses)
        {
            try
            {
                int i = 0;
                foreach (Uri part in partsAddresses)
                {
                    if (part != null)
                    {
                        netProgressBar.Value = netProgressBar.Minimum;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        bytes = await client.DownloadDataTaskAsync(part);
                        File.WriteAllBytes($"downloaded{i}", bytes);
                        i++;
                        currentPart++;
                    }
                }
                IOlabel.Text = "Writing " + 0 + "%";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void addressbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (addressbox.Text == "" | addressbox.Text == " ")
            {
                placeholderBox.Text = "Use | to Separate Downloads";
            }
            else
            {
                placeholderBox.Text = "";
            }
        }
    }
}