using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace SuperAlloyRangerTranslator
{
    public class InstallerForm : Form
    {
        private Image bgImage;
        private TextBox txtPath;
        private Button btnBrowse;
        private Label lblPathStatus;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnInstall;
        private Button btnRestore;
        private Button btnPlay;
        private Button btnClose;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }

        public InstallerForm()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            LoadEmbeddedAssets();
            InitializeComponent();
            CheckInitialPath();
        }

        private void LoadEmbeddedAssets()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream s = asm.GetManifestResourceStream("installer_bg.jpg"))
                {
                    if (s != null)
                    {
                        bgImage = Image.FromStream(s);
                    }
                }
                using (Stream s = asm.GetManifestResourceStream("app_icon.ico"))
                {
                    if (s != null)
                    {
                        this.Icon = new Icon(s);
                    }
                }
            }
            catch { }

            // Fallback if not embedded
            if (bgImage == null && File.Exists("installer_bg.jpg"))
            {
                try { bgImage = Image.FromFile("installer_bg.jpg"); } catch { }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Super Alloy Ranger - Instalador da Tradução PT-BR";
            this.ClientSize = new Size(760, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(12, 17, 28);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.ForeColor = Color.White;

            int cardY = 285;

            // Path Header Label
            Label lblPathTitle = new Label();
            lblPathTitle.Text = "Pasta de Instalação do Jogo:";
            lblPathTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblPathTitle.ForeColor = Color.FromArgb(0, 229, 255);
            lblPathTitle.BackColor = Color.Transparent;
            lblPathTitle.Location = new Point(36, cardY + 14);
            lblPathTitle.AutoSize = true;

            // Path TextBox
            txtPath = new TextBox();
            txtPath.Location = new Point(36, cardY + 38);
            txtPath.Size = new Size(560, 26);
            txtPath.BackColor = Color.FromArgb(8, 12, 22);
            txtPath.ForeColor = Color.White;
            txtPath.BorderStyle = BorderStyle.FixedSingle;
            txtPath.Font = new Font("Segoe UI", 9.5f);
            txtPath.TextChanged += (s, e) => ValidateCurrentPath();

            // Browse Button
            btnBrowse = new Button();
            btnBrowse.Text = "Procurar...";
            btnBrowse.Location = new Point(604, cardY + 36);
            btnBrowse.Size = new Size(116, 29);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.FlatAppearance.BorderSize = 1;
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            btnBrowse.BackColor = Color.FromArgb(28, 38, 60);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnBrowse.Click += (s, e) => BrowseGameFolder();

            // Status Badge Label
            lblPathStatus = new Label();
            lblPathStatus.Location = new Point(36, cardY + 72);
            lblPathStatus.Size = new Size(685, 20);
            lblPathStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPathStatus.BackColor = Color.Transparent;

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(36, cardY + 98);
            progressBar.Size = new Size(685, 12);
            progressBar.Style = ProgressBarStyle.Continuous;

            // Status text
            lblStatus = new Label();
            lblStatus.Location = new Point(36, cardY + 116);
            lblStatus.Size = new Size(685, 18);
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(0, 229, 255);
            lblStatus.BackColor = Color.Transparent;
            lblStatus.Text = "Pronto para instalar.";

            // Action Buttons
            btnInstall = new Button();
            btnInstall.Text = "Instalar Tradução";
            btnInstall.Location = new Point(36, cardY + 144);
            btnInstall.Size = new Size(200, 42);
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.BackColor = Color.FromArgb(0, 229, 255);
            btnInstall.ForeColor = Color.FromArgb(10, 15, 25);
            btnInstall.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnInstall.Cursor = Cursors.Hand;
            btnInstall.Click += (s, e) => DoInstall();

            btnRestore = new Button();
            btnRestore.Text = "Restaurar Original";
            btnRestore.Location = new Point(245, cardY + 144);
            btnRestore.Size = new Size(160, 42);
            btnRestore.FlatStyle = FlatStyle.Flat;
            btnRestore.FlatAppearance.BorderSize = 1;
            btnRestore.FlatAppearance.BorderColor = Color.FromArgb(70, 85, 115);
            btnRestore.BackColor = Color.FromArgb(25, 32, 50);
            btnRestore.ForeColor = Color.FromArgb(220, 225, 235);
            btnRestore.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            btnRestore.Cursor = Cursors.Hand;
            btnRestore.Click += (s, e) => DoRestore();

            btnPlay = new Button();
            btnPlay.Text = "Jogar Agora";
            btnPlay.Location = new Point(415, cardY + 144);
            btnPlay.Size = new Size(135, 42);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.FlatAppearance.BorderSize = 0;
            btnPlay.BackColor = Color.FromArgb(0, 200, 83);
            btnPlay.ForeColor = Color.White;
            btnPlay.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.Click += (s, e) => DoPlayGame();

            btnClose = new Button();
            btnClose.Text = "Sair";
            btnClose.Location = new Point(625, cardY + 144);
            btnClose.Size = new Size(95, 42);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 95);
            btnClose.BackColor = Color.FromArgb(22, 28, 42);
            btnClose.ForeColor = Color.FromArgb(175, 185, 200);
            btnClose.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblPathTitle);
            this.Controls.Add(txtPath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(lblPathStatus);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnInstall);
            this.Controls.Add(btnRestore);
            this.Controls.Add(btnPlay);
            this.Controls.Add(btnClose);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // 1. Draw Background Image
            if (bgImage != null)
            {
                g.DrawImage(bgImage, 0, 0, w, h);
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(12, 17, 28)))
                {
                    g.FillRectangle(b, 0, 0, w, h);
                }
            }

            // 2. Draw Top-Left Translucent Title Badge
            Rectangle badgeRect = new Rectangle(20, 16, 290, 52);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(215, 10, 15, 28)))
            {
                g.FillRectangle(b, badgeRect);
            }
            using (Pen p = new Pen(Color.FromArgb(150, 0, 229, 255), 1.5f))
            {
                g.DrawRectangle(p, badgeRect);
            }
            using (Font f1 = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (SolidBrush b1 = new SolidBrush(Color.FromArgb(0, 229, 255)))
            {
                g.DrawString("TRADUÇÃO PT-BR v1.1", f1, b1, 32, 22);
            }
            using (Font f2 = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush b2 = new SolidBrush(Color.FromArgb(210, 225, 240)))
            {
                g.DrawString("Super Alloy Ranger • Mod Nativo Integrado", f2, b2, 33, 44);
            }

            // 3. Draw Bottom Frosted Glass Card for Controls
            int cardY = 285;
            int cardH = 210;
            Rectangle cardRect = new Rectangle(20, cardY, w - 40, cardH);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 12, 17, 30)))
            {
                g.FillRectangle(b, cardRect);
            }
            using (Pen p = new Pen(Color.FromArgb(140, 0, 229, 255), 1.5f))
            {
                g.DrawRectangle(p, cardRect);
            }
        }

        private void CheckInitialPath()
        {
            string detected = DetectGamePath();
            if (!string.IsNullOrEmpty(detected))
            {
                txtPath.Text = detected;
            }
            ValidateCurrentPath();
        }

        private void BrowseGameFolder()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Selecione a pasta onde o Super Alloy Ranger está instalado:";
                if (!string.IsNullOrEmpty(txtPath.Text) && Directory.Exists(txtPath.Text))
                {
                    fbd.SelectedPath = txtPath.Text;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                    ValidateCurrentPath();
                }
            }
        }

        private bool ValidateCurrentPath()
        {
            string p = txtPath.Text.Trim();
            bool valid = IsValidGameFolder(p);

            if (valid)
            {
                lblPathStatus.ForeColor = Color.FromArgb(0, 230, 118);
                lblPathStatus.Text = "✓ Jogo encontrado e pronto para instalação!";
                btnInstall.Enabled = true;
                btnPlay.Enabled = true;

                string managedDir = Path.Combine(p, @"Super.Alloy.Ranger_Data\Managed");
                string bakPath = Path.Combine(managedDir, "Assembly-CSharp.dll.bak_ptbr");
                btnRestore.Enabled = File.Exists(bakPath);
            }
            else
            {
                lblPathStatus.ForeColor = Color.FromArgb(255, 82, 82);
                lblPathStatus.Text = "✗ Pasta inválida. Selecione a pasta com 'Super.Alloy.Ranger.exe'.";
                btnInstall.Enabled = false;
                btnRestore.Enabled = false;
                btnPlay.Enabled = false;
            }

            return valid;
        }

        private void DoInstall()
        {
            string gameDir = txtPath.Text.Trim();
            if (!IsValidGameFolder(gameDir))
            {
                MessageBox.Show("Selecione uma pasta válida do Super Alloy Ranger antes de instalar.", "Pasta Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Process[] procs = Process.GetProcessesByName("Super.Alloy.Ranger");
            if (procs == null || procs.Length == 0) procs = Process.GetProcessesByName("Super Alloy Ranger");
            if (procs != null && procs.Length > 0)
            {
                MessageBox.Show("O Super Alloy Ranger está aberto no momento.\nPor favor, feche o jogo antes de continuar.", "Jogo em Execução", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SetBusy(true);
                lblStatus.ForeColor = Color.FromArgb(0, 229, 255);
                lblStatus.Text = "Iniciando instalação...";
                progressBar.Value = 10;
                Application.DoEvents();

                string dataDir = Path.Combine(gameDir, "Super.Alloy.Ranger_Data");
                string managedDir = Path.Combine(dataDir, "Managed");
                if (!Directory.Exists(managedDir)) Directory.CreateDirectory(managedDir);

                string asmDest = Path.Combine(managedDir, "Assembly-CSharp.dll");
                string bakPath = Path.Combine(managedDir, "Assembly-CSharp.dll.bak_ptbr");

                // 1. Backup original DLL
                lblStatus.Text = "Criando backup seguro dos arquivos originais...";
                progressBar.Value = 25;
                Application.DoEvents();

                if (File.Exists(asmDest) && !File.Exists(bakPath))
                {
                    File.Copy(asmDest, bakPath, false);
                }

                // 2. Extract Assembly-CSharp.dll
                lblStatus.Text = "Instalando Assembly-CSharp.dll...";
                progressBar.Value = 50;
                Application.DoEvents();
                ExtractResource("Assembly-CSharp.dll", asmDest);

                // 3. Extract SARTranslator.dll
                lblStatus.Text = "Instalando SARTranslator.dll...";
                progressBar.Value = 70;
                Application.DoEvents();
                ExtractResource("SARTranslator.dll", Path.Combine(managedDir, "SARTranslator.dll"));

                // 4. Extract translation_ptbr.txt
                lblStatus.Text = "Instalando translation_ptbr.txt...";
                progressBar.Value = 90;
                Application.DoEvents();
                ExtractResource("translation_ptbr.txt", Path.Combine(managedDir, "translation_ptbr.txt"));
                ExtractResource("translation_ptbr.txt", Path.Combine(dataDir, "translation_ptbr.txt"));
                ExtractResource("translation_ptbr.txt", Path.Combine(gameDir, "translation_ptbr.txt"));

                // 5. Extract LEIA-ME.txt
                ExtractResource("LEIA-ME.txt", Path.Combine(gameDir, "LEIA-ME_TRADUCAO.txt"));

                progressBar.Value = 100;
                lblStatus.Text = "Tradução instalada com sucesso!";
                lblStatus.ForeColor = Color.FromArgb(0, 230, 118);
                btnRestore.Enabled = true;
                btnPlay.Enabled = true;

                MessageBox.Show(
                    "Tradução em Português do Brasil (PT-BR) instalada com sucesso!\n\nVocê já pode iniciar o jogo e desfrutar da aventura totalmente em português.",
                    "Instalação Concluída",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro durante a instalação.";
                lblStatus.ForeColor = Color.FromArgb(255, 82, 82);
                MessageBox.Show("Ocorreu um erro durante a instalação:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void DoRestore()
        {
            string gameDir = txtPath.Text.Trim();
            if (!IsValidGameFolder(gameDir)) return;

            string managedDir = Path.Combine(gameDir, @"Super.Alloy.Ranger_Data\Managed");
            string asmDest = Path.Combine(managedDir, "Assembly-CSharp.dll");
            string bakPath = Path.Combine(managedDir, "Assembly-CSharp.dll.bak_ptbr");

            if (!File.Exists(bakPath))
            {
                MessageBox.Show("Nenhum backup foi encontrado para restaurar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialog = MessageBox.Show(
                "Deseja restaurar os arquivos originais do jogo?\nA tradução será removida.",
                "Confirmar Restauração",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialog != DialogResult.Yes) return;

            try
            {
                SetBusy(true);
                lblStatus.ForeColor = Color.FromArgb(0, 229, 255);
                lblStatus.Text = "Restaurando arquivos originais...";
                progressBar.Value = 40;
                Application.DoEvents();

                File.Copy(bakPath, asmDest, true);

                string sarDll = Path.Combine(managedDir, "SARTranslator.dll");
                if (File.Exists(sarDll)) File.Delete(sarDll);

                progressBar.Value = 100;
                lblStatus.Text = "Jogo restaurado para os arquivos originais com sucesso!";
                lblStatus.ForeColor = Color.FromArgb(0, 230, 118);

                MessageBox.Show("Os arquivos originais foram restaurados com sucesso!", "Restauração Concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro ao restaurar arquivos.";
                lblStatus.ForeColor = Color.FromArgb(255, 82, 82);
                MessageBox.Show("Erro ao restaurar:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void DoPlayGame()
        {
            string gameDir = txtPath.Text.Trim();
            if (!IsValidGameFolder(gameDir)) return;

            string exe1 = Path.Combine(gameDir, "Super.Alloy.Ranger.exe");
            string exe2 = Path.Combine(gameDir, "Super Alloy Ranger.exe");

            string targetExe = File.Exists(exe1) ? exe1 : (File.Exists(exe2) ? exe2 : "");
            if (!string.IsNullOrEmpty(targetExe))
            {
                ProcessStartInfo psi = new ProcessStartInfo(targetExe);
                psi.WorkingDirectory = gameDir;
                Process.Start(psi);
                this.Close();
            }
            else
            {
                MessageBox.Show("Executável do jogo não encontrado nesta pasta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetBusy(bool busy)
        {
            btnInstall.Enabled = !busy;
            btnRestore.Enabled = !busy;
            btnBrowse.Enabled = !busy;
            txtPath.Enabled = !busy;
        }

        private static void ExtractResource(string resourceName, string outputPath)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(resourceName))
            {
                if (s == null)
                {
                    throw new Exception("Recurso interno não encontrado: " + resourceName);
                }
                string dir = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buf = new byte[65536];
                    int read;
                    while ((read = s.Read(buf, 0, buf.Length)) > 0)
                    {
                        fs.Write(buf, 0, read);
                    }
                }
            }
        }

        private static string DetectGamePath()
        {
            string cur = Application.StartupPath;
            if (IsValidGameFolder(cur)) return cur;

            if (IsValidGameFolder(@"X:\jogos\epic\SuperAlloyRangerKIpC3"))
                return @"X:\jogos\epic\SuperAlloyRangerKIpC3";

            try
            {
                string steamPath = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
                {
                    string p1 = Path.Combine(steamPath, @"steamapps\common\Super Alloy Ranger");
                    if (IsValidGameFolder(p1)) return p1;
                    string p2 = Path.Combine(steamPath, @"steamapps\common\SuperAlloyRanger");
                    if (IsValidGameFolder(p2)) return p2;

                    string vdf = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
                    if (File.Exists(vdf))
                    {
                        foreach (string line in File.ReadAllLines(vdf))
                        {
                            if (line.Contains("\"path\""))
                            {
                                int firstQuote = line.IndexOf('"', line.IndexOf("\"path\"") + 6);
                                if (firstQuote > 0)
                                {
                                    int secondQuote = line.IndexOf('"', firstQuote + 1);
                                    if (secondQuote > firstQuote)
                                    {
                                        string libPath = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Replace(@"\\", @"\");
                                        string cand1 = Path.Combine(libPath, @"steamapps\common\Super Alloy Ranger");
                                        if (IsValidGameFolder(cand1)) return cand1;
                                        string cand2 = Path.Combine(libPath, @"steamapps\common\SuperAlloyRanger");
                                        if (IsValidGameFolder(cand2)) return cand2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            try
            {
                string manifestsDir = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";
                if (Directory.Exists(manifestsDir))
                {
                    foreach (string file in Directory.GetFiles(manifestsDir, "*.item"))
                    {
                        string text = File.ReadAllText(file);
                        if (text.IndexOf("Super Alloy Ranger", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            text.IndexOf("SuperAlloyRanger", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            int idx = text.IndexOf("\"InstallLocation\":");
                            if (idx > 0)
                            {
                                int q1 = text.IndexOf('"', idx + 18);
                                int q2 = text.IndexOf('"', q1 + 1);
                                if (q1 > 0 && q2 > q1)
                                {
                                    string loc = text.Substring(q1 + 1, q2 - q1 - 1).Replace(@"\\", @"\");
                                    if (IsValidGameFolder(loc)) return loc;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            string[] commonRelative = new string[] {
                @"jogos\epic\SuperAlloyRangerKIpC3",
                @"Epic Games\SuperAlloyRangerKIpC3",
                @"Games\Super Alloy Ranger",
                @"SteamLibrary\steamapps\common\Super Alloy Ranger",
                @"Jogos\Super Alloy Ranger",
                @"Games\SuperAlloyRanger"
            };

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    foreach (string rel in commonRelative)
                    {
                        string test = Path.Combine(drive.RootDirectory.FullName, rel);
                        if (IsValidGameFolder(test)) return test;
                    }
                }
            }

            return "";
        }

        private static bool IsValidGameFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;

            string exe1 = Path.Combine(path, "Super.Alloy.Ranger.exe");
            string exe2 = Path.Combine(path, "Super Alloy Ranger.exe");
            string dataDir = Path.Combine(path, "Super.Alloy.Ranger_Data");
            string managedDir = Path.Combine(dataDir, "Managed");
            string asmDll = Path.Combine(managedDir, "Assembly-CSharp.dll");

            return File.Exists(exe1) || File.Exists(exe2) || (Directory.Exists(managedDir) && File.Exists(asmDll));
        }
    }
}
