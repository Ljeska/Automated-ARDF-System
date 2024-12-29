using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace comp
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private List<string> targets;
        private DateTime startTime;
        private const int timeLimitSeconds = 30;
        private string firstTransmitter = null;
        private List<string> foundTargets = new List<string>(); // Lista za praćenje pronađenih predajnika
        private bool competitionFinished = false; // Da omogući novo takmičenje

        public Form1()
        {
            InitializeComponent();

            // Inicijalizacija serijskog porta
            serialPort = new SerialPort("COM7", 9600);  // Promijenite 'COM7' u odgovarajući serijski port

            // Povezivanje događaja klika na dugme s odgovarajućom metodom
            button1.Click += button1_Click;

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Vaša logika za inicijalizaciju forme
            //this.Size = this.BackgroundImage.Size;
            panel1.BackColor = Color.FromArgb(100,0,0,0);
        }
        private void button1_Click(object sender, EventArgs e)
        {

            targets = new List<string> { "A", "B", "C" }; // Inicijalizacija liste ciljeva
            Thread.Sleep(2000); // Čekanje nekoliko sekundi kako bi se serijski port stabilizirao
            if (!serialPort.IsOpen)
            {
                serialPort.Open(); // Otvorite serijski port
            }
            startTime = DateTime.Now; // Pokretanje vremena od početka takmičenja

            if (competitionFinished)
            {
                // Resetovanje promenljivih za novo takmičenje
                label1.Text = ""; // Brisemo sadrzaj labele 
                foundTargets.Clear(); // Resetujemo listu pronađenih predajnika
                firstTransmitter = null; // Postavljamo varijablu za prvi predajnik
                competitionFinished = false; // Postavljamo da takmičenje nije završeno
            }
            else
            {
                label1.Text = ""; // Brisemo sadrzaj labele
            }
            
            // Glavna petlja takmičenja
            Random random = new Random();
            while (foundTargets.Count < 3)
            {

                // Provjera vremena takmičenja
                TimeSpan elapsedTime = DateTime.Now - startTime;
                int remainingTimeSeconds = timeLimitSeconds - (int)elapsedTime.TotalSeconds;

                if (remainingTimeSeconds <= 0)
                {
                    remainingTimeSeconds = 0; // Postavite na minimalnu vrijednost
                }

                //if (remainingTimeSeconds <= 0)
                //{
                //Poruka o diskvalifikaciji
                // label1.Text += "Diskvalifikacija: Vrijeme je isteklo, niste pronašli sve predajnike u roku.\n";
                //label1.Refresh();
                //serialPort.Write("X"); // Slanje naredbe Arduino-u da isključi predajnik
                //serialPort.Close();

                // Prikazivanje dialoga za novo takmičenje
                //var result = MessageBox.Show("Takmičenje je završeno! Želite li pokrenuti novo takmičenje?", "Kraj takmičenja", MessageBoxButtons.YesNo);
                //if (result == DialogResult.Yes)
                //{
                //competitionFinished = true; // Postavljamo da takmičenje nije završeno
                //button1_Click(sender, e); // Ponovno pokretanje takmičenja
                //}
                //else if (result == DialogResult.No)
                //{
                //label1.Text = ""; // Brisemo sadrzaj labele
                //return; // Izlaz iz metode jer je takmičenje završeno
                //}
                //}

                // Postavljanje timeout-a na serijskom portu
                serialPort.ReadTimeout = remainingTimeSeconds * 1000; // Pretvaranje vremena u milisekunde

                // Odabir nasumičnog cilja iz preostalih ciljeva
                string target = targets[random.Next(targets.Count)];

                if (serialPort.IsOpen)
                {
                    // Slanje odabranog cilja na Arduina
                    serialPort.Write(target);
                    Thread.Sleep(1);
                }

                try
                {
                    if (serialPort.IsOpen)
                    {

                        // Čitanje odgovora sa serijskog porta
                        string response = serialPort.ReadLine().Trim();

                        // Provjera je li pronadjen novi cilj
                        if (response.StartsWith("Predajnik") && response.EndsWith("pronadjen") && response.Split()[1] == target)
                        {
                            // Izračunaj vrijeme proteklo od početka takmičenja do pronalaska trenutnog predajnika
                            double elapsedTimeSeconds = (DateTime.Now - startTime).TotalSeconds;

                            // Ispis informacija o pronadjenom predajniku
                            string logMessage = $"Pronađen predajnik {target} za {elapsedTimeSeconds:F2} sekundi.\n";

                            // Dodaj informaciju u tekst Label kontrole
                            label1.Text += logMessage;

                            // Osveži labelu da bi se prikazala ažuriranja
                            label1.Refresh();

                            // Ako je ovo prvi pronadjeni predajnik, spremi ga kao prvi predajnik
                            if (foundTargets.Count == 0)
                            {
                                firstTransmitter = target;
                            }

                            // Dodaj pronadjeni predajnik u listu pronadjenih ciljeva
                            foundTargets.Add(target);

                            // Ukloni pronadjeni cilj iz liste ciljeva
                            targets.Remove(target);
                        }

                        // Pauza između svake iteracije
                        Thread.Sleep(2000);

                        // Ako su pronadjeni svi ciljevi osim zadnjeg
                        if (foundTargets.Count == 2 && targets.Count == 1)
                        {
                            // Dodaj prvog izabranog predajnika nazad u listu ciljeva prije biranja posljednjeg preostalog predajnika
                            targets.Add(firstTransmitter);
                        }
                        if (foundTargets.Count == 3)
                        {
                            // Zatvaranje serijskog porta nakon završetka takmičenja
                            if (serialPort.IsOpen)
                            {
                                serialPort.Close();
                            }
                            // Ispis ukupnog vremena trajanja takmičenja
                            double totalTime = (DateTime.Now - startTime).TotalSeconds - 2;
                            label1.Text += $"Ukupno vrijeme takmičenja: {totalTime:F2} sekundi\n";

                            //serialPort.Close();

                            // Obavijest o završetku takmičenja
                            var result = MessageBox.Show("Takmičenje je završeno! Želite li pokrenuti novo takmičenje?", "Kraj takmičenja", MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                competitionFinished = true; // Postavljamo da takmičenje nije završeno
                                button1_Click(sender, e); // Ponovno pokretanje takmičenja
                                return; // Izlaz iz metode kako bi se spriječilo izvršavanje ostatka koda
                            }
                            else if (result == DialogResult.No)
                            {

                                label1.Text = ""; // Brisemo sadrzaj labele
                                Environment.Exit(0); // Pravilno zatvaranje aplikacije
                                //return; // Izlaz iz metode kako bi se spriječilo izvršavanje ostatka koda
                            }
                        }
                    }
            }
            catch (TimeoutException)
                {
                // Poruka o diskvalifikaciji
                label1.Text += "Diskvalifikacija: Vrijeme je isteklo, niste pronašli sve predajnike u roku.\n";
                label1.Refresh();

                    if (serialPort.IsOpen) {
                        serialPort.Write("X"); // Slanje naredbe Arduino-u da isključi predajnik
                        serialPort.Close();
                    }
                //Prikazivanje dialoga za novo takmičenje
                var result = MessageBox.Show("Takmičenje je završeno! Želite li pokrenuti novo takmičenje?", "Kraj takmičenja", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    competitionFinished = true; // Postavljamo da takmičenje nije završeno
                    button1_Click(sender, e); // Ponovno pokretanje takmičenja
                }
                else if (result == DialogResult.No)
                {
                        if (serialPort.IsOpen)
                        {
                            serialPort.Close();
                        }
                        Environment.Exit(0); 
                        // Pravilno zatvaranje aplikacije
                                             //Application.Exit();
                                             //label1.Text = ""; // Brisemo sadrzaj labele
                                             //return; // Izlaz iz metode jer je takmičenje završeno
                }
            }

        }
            
            
        }    
    }
}
