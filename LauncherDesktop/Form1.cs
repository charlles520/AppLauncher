﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LauncherDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Change COLOR ListViewGroup Header

            // né class api
        public class ListViewAPI
        {
            public const int LVM_FIRST = 4096;
            public const int LVM_SETGROUPMETRICS = (LVM_FIRST + 155);
            public const int LVGMF_NONE = 0;
            public const int LVGMF_BORDERSIZE = 1;
            public const int lVGMF_BORDERCOLOR = 2;
            public const int LVGMF_TEXTCOLOR = 0x4;

          




            //import SendMessage
            [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        //Layout Senquencia crHeader
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LVGROUPMETRICS
        {
            public int cbSize;
            public int mask;
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int crLeft;
            public int crTop;
            public int crRight;
            public int crBottom;
            public int crHeader;
            public int crFooter;
            }

        public static void SetGroupHeaderColor(IntPtr handle, int icolor)
        {
            var groupMetrics = new LVGROUPMETRICS();
            Int32 ptrRetVal;
            IntPtr wparam = new IntPtr();
            IntPtr lparam = new IntPtr();

            groupMetrics.cbSize = Marshal.SizeOf(groupMetrics);
            groupMetrics.mask = ListViewAPI.LVGMF_TEXTCOLOR;
            groupMetrics.crHeader = icolor;

            lparam = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Runtime.InteropServices.Marshal.SizeOf(groupMetrics));
            System.Runtime.InteropServices.Marshal.StructureToPtr(groupMetrics, lparam, false);

            ptrRetVal = SendMessage(handle, LVM_SETGROUPMETRICS, wparam, lparam);

            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(lparam);
        }

        }

        //Global vars
        readonly ListBox mylist = new ListBox();
        readonly ListBox ConfigGroups = new ListBox();
        readonly ListBox ConfigAdmin = new ListBox();
        readonly ListBox MyGroups = new ListBox();


        public string NewVersion = string.Empty;

        string result = string.Empty;
        bool change = false;
        readonly string myFile = Application.StartupPath + "\\DATA.bin";
        readonly string Ver = "20.05.03";
        string TitleProgram = string.Empty;
        bool question = false;
        public class IconExtractor
        {
            [DllImport("shell32.dll", EntryPoint = "ExtractIconEx")]
            public static extern int ExtractIconExA(string lpszFile, int nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

            [DllImport("user32")]
            private static extern int DestroyIcon(IntPtr hIcon);

            //Attempts to extract the small-version of the applicaiton's icon
            public static Icon ExtractIconSmall(string path)
            {
                IntPtr largeIcon = IntPtr.Zero;
                IntPtr smallIcon = IntPtr.Zero;
                ExtractIconExA(path, 0, ref largeIcon, ref smallIcon, 1);

                //Transform the bits into the icon image
                Icon returnIcon = null;
                if (smallIcon != IntPtr.Zero)
                    returnIcon = Icon.FromHandle(smallIcon);

                //clean up
                DestroyIcon(largeIcon);

                return returnIcon;
            }

            //Attempts to extract the large-version of the application's icon
            public static Icon ExtractIconLarge(string path)
            {
                IntPtr largeIcon = IntPtr.Zero;
                IntPtr smallIcon = IntPtr.Zero;
                ExtractIconExA(path, 0, ref largeIcon, ref smallIcon, 1);

                //Transform the bits into the icon image
                Icon returnIcon = null;
                if (largeIcon != IntPtr.Zero)
                    returnIcon = Icon.FromHandle(largeIcon);

                //clean up
                DestroyIcon(smallIcon);

                return returnIcon;
            }

            //Returns the large icon if found, if not the small icon
            public static Icon ExtractIcon(string path)
            {
                Icon largeIcon = ExtractIconLarge(path);

                if (largeIcon == null)
                    return ExtractIconSmall(path);
                else
                    return largeIcon;
            }
        }

        //InputBox because C# dont have only VB because? I DONT KNOW
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(12, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor |= AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK)
                value = textBox.Text;
            else
                value = string.Empty;

            return dialogResult;
        }

        void AddFile(string filename)
        {
            string type = Path.GetExtension(filename);
            result = Path.GetFileNameWithoutExtension(filename);
            mylist.Items.Add(filename);
            try 
            { 
                Icon largeIcon = IconExtractor.ExtractIconLarge(filename);
                lista_icons.Images.Add(largeIcon.ToBitmap());
                listitens.Items.Add(result, lista_icons.Images.Count - 1);
            }
            catch{ listitens.Items.Add(result, 0); }
                

        }
        void OpenDirFile()
        {
            int indexforfile = listitens.SelectedIndices[0];
            mylist.SelectedIndex = indexforfile;
            string pasta = Path.GetDirectoryName(mylist.SelectedItem.ToString());
            System.Diagnostics.Process.Start(pasta);
        }
        void Run(bool adm)
        {
            int indexlist = listitens.SelectedIndices[0];
            mylist.SelectedIndex = indexlist;
            string arquivo = mylist.SelectedItem.ToString();
            string caminho = Path.GetDirectoryName(arquivo);
            string nome = Path.GetFileName(arquivo);
            var processInfo = new System.Diagnostics.ProcessStartInfo();
            if (adm)
            {
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
            }
            processInfo.WorkingDirectory = caminho;
            processInfo.FileName = nome;
            try
            {
                System.Diagnostics.Process.Start(processInfo);
            }
            catch
            {
                // Sem permissao
            }
        }   
        void Remove()
        {
            try
            {
                int val = listitens.SelectedIndices[0];
                try
                { 
                ConfigGroups.Items.RemoveAt(ConfigGroups.FindString(val.ToString()));
                }
                catch
                {
                    // N tem grupo
                }
                try
                { 
                    ConfigAdmin.Items.RemoveAt(ConfigAdmin.FindString(val.ToString()));
                }
                catch
                {
                    //N tem edicao admin
                }
               
                mylist.Items.RemoveAt(val);
                listitens.Items.RemoveAt(val);
                cm_itens.Items.RemoveAt(val);

            }
            catch { }
        }
        void ClearAll()
        {
            listitens.Clear();
            mylist.Items.Clear();
            Image defaulticon = lista_icons.Images[0];
            lista_icons.Images.Clear();
            lista_icons.Images.Add(defaulticon);
        }
        void SaveList()
        {
            Properties.Settings.Default.Save();
            System.IO.File.WriteAllLines(myFile, mylist.Items.OfType<string>().ToArray());
            System.IO.File.AppendAllLines(myFile, MyGroups.Items.OfType<string>().ToArray());
            System.IO.File.AppendAllLines(myFile, ConfigAdmin.Items.OfType<string>().ToArray());
            System.IO.File.AppendAllLines(myFile, ConfigGroups.Items.OfType<string>().ToArray());

            this.Text = TitleProgram;
            change = false;
        }
        void LoadFile(string myFile)
        {
          
            //conditions save
            _ = ConfigGroups.Items.Add("<ITENS_G>");
            _ = ConfigAdmin.Items.Add("<ITENS_C>");
            _ = MyGroups.Items.Add("<GROUPS>");


            if (File.Exists(myFile))
            {
                bool config_itens = false;
                bool AD_Groups = false;
                bool groups = false;
                bool admins = false;
                string[] lines = System.IO.File.ReadAllLines(myFile);
                for (int i = 0; i < lines.Count(); ++i)
                {
                    if (string.Compare(lines[i], "<GROUPS>") == 0)
                    {
                        AD_Groups = true;
                        continue;
                    }
                    if (string.Compare(lines[i], "<ITENS_G>") == 0)
                    {
                        groups = true;
                        AD_Groups = false;
                        continue;
                    }
                    if (string.Compare(lines[i], "<ITENS_C>") == 0)
                    {
                        AD_Groups = false;
                        groups = false;
                        admins = true;
                        continue;
                    }
                    if ((string.Compare(lines[i], string.Empty) == 0))
                        continue;

                    if (!groups && !admins && !AD_Groups && !config_itens)
                    {
                        mylist.Items.Add(lines[i]);
                        string type = Path.GetExtension(lines[i]);
                        result = Path.GetFileNameWithoutExtension(lines[i]);
                        try
                        {
                                Icon largeIcon = IconExtractor.ExtractIconLarge(lines[i]);
                                lista_icons.Images.Add(largeIcon.ToBitmap());
                                listitens.Items.Add(result, (lista_icons.Images.Count - 1));
                                cm_itens.Items.Add(result, lista_icons.Images[lista_icons.Images.Count - 1]);

                        }
                        catch
                        {
                            listitens.Items.Add(result, 0);
                            cm_itens.Items.Add(result, lista_icons.Images[0]);
                        }


                        
                       

                    }
                    else if (AD_Groups)
                    {
                        string header = lines[i];
                        if (string.Compare(header, "''") != 0)
                        {

                            // txt 230, 240, 250
                      
                            ListViewGroup newgroups = new ListViewGroup
                            {
                                Header = header,
                                HeaderAlignment = HorizontalAlignment.Center
                               
                                
                            };
                            

                            MyGroups.Items.Add(header);
                            listitens.Groups.Add(newgroups);
                            (cms_viewer.Items[4] as ToolStripMenuItem).DropDownItems.Add(header);
                            removerGrupoToolStripMenuItem.DropDownItems.Add(header);
                        }
                    }
                    else if (groups)
                    {
                        ListViewAPI.SetGroupHeaderColor(listitens.Handle, 0xC00056);
                        try
                        {
                            string[] file_groups = new string[2];
                            ConfigGroups.Items.Add(lines[i]);
                            file_groups = lines[i].Split(':');
                            for (int x = 0; x < listitens.Groups.Count; ++x)
                            {
                                if (string.Compare(listitens.Groups[x].Header, file_groups[1]) == 0)
                                 listitens.Items[listitens.Items.IndexOf(listitens.FindItemWithText(file_groups[0]))].Group
                                        = listitens.Groups[x];
                                   
                            }  
                        }catch
                        { }

                    }
                    else if (admins)
                    {
                        try
                        {
                            string[] file_admin = new string[2];
                            ConfigAdmin.Items.Add(lines[i]);
                            file_admin = lines[i].Split(':');
                            listitens.Items[listitens.Items.IndexOf(
                                listitens.FindItemWithText(file_admin[0]))].ForeColor = Color.Green;
                            listitens.Items[listitens.Items.IndexOf(
                                listitens.FindItemWithText(file_admin[0]))].Checked = true;

                        }
                        catch
                        { }

                    }
                }
            }

        }
        void ChangueItens()
        {
            this.Text = TitleProgram + "*";
            change = true;
        }
        void QuestionHide()
        {
            if (!question)
            {
                question = true;
                var ButtonsResult = MessageBox.Show("Deseja Esconder?", "Hide me", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ButtonsResult == DialogResult.Yes)
                    escondeAoAbrirAlgoToolStripMenuItem.Checked = true;   
                
            }
            if (escondeAoAbrirAlgoToolStripMenuItem.Checked)
            { 
                this.Hide();
                notifyIcon1.Visible = true;
            }


        }
        bool VerChange()
        {
            WebRequest request = WebRequest.Create("https://github.com/SrShadowy/AppLauncher/tags");
            WebResponse response = request.GetResponse();


            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                string[] splitResponse = responseFromServer.Split('<');
                string version;

                foreach (string lines in splitResponse)
                {
                    if (lines.Contains(@"a href=""/SrShadowy/AppLauncher/releases/tag/"))
                    {
                        version = lines.Replace(@"a href=""/SrShadowy/AppLauncher/releases/tag/v", "");
                        version = version.Remove(8);
                        if (string.Compare(version, Ver) == 0)
                        {
                            NewVersion = version;
                            return false;
                           


                        }
                        else if (string.Compare(version, Ver) != 0)
                        {   
                            NewVersion = version;
                            return true;
                
                        }
                           

                    }

                }


            }
            return false;

        }
        void ChangeGroup(string newGroup)
        {
            var itemIndex = listitens.Items.IndexOf(listitens.SelectedItems[0]);
            int removeAt = ConfigGroups.FindString(listitens.Items[itemIndex].Text);
            if (removeAt > 0)
                ConfigGroups.Items.RemoveAt(removeAt);

            ConfigGroups.Items.Add(newGroup);
        }

        void RemoveGroup(int index)
        {
            for (int x = 0; x < listitens.Groups.Count; ++x)
            {
                MessageBox.Show(MyGroups.Items[index+1].ToString());
                if (string.Compare(listitens.Groups[x].Header, MyGroups.Items[index+1].ToString()) == 0)
                {

                    listitens.Groups.RemoveAt(x);
                    break;
                }
            }
            MyGroups.Items.RemoveAt(index+1);
            moverParaOGrupoToolStripMenuItem.DropDownItems.RemoveAt(index);
            removerGrupoToolStripMenuItem.DropDownItems.RemoveAt(index);
        }

        //LIST 
        private void ListItens_DragDrop(object sender, DragEventArgs e)
        {
            string filename = string.Empty;
            if (e.Data.GetData(DataFormats.FileDrop) is string[] arquivos && arquivos.Any())
                filename = arquivos.First();

            AddFile(filename);
            ChangueItens();
        }
        private void ListItens_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void AdicionarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            string filename;
            if (abrir.ShowDialog() == DialogResult.OK)
            {
                filename = abrir.FileName;
                AddFile(filename);
                ChangueItens();
            }
        }
        private void SairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Listitens_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                cms_viewer.Show(Cursor.Position);
            }
        }
        private void AbrirLocalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenDirFile();
            }
            catch
            {

            }

        }
        private void AbrirLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenDirFile();
            }
            catch
            {

            }
        }
        private void Listitens_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (listitens.SelectedItems.Count > 0)
                {
                    Run(false);
                }
            }
            if (e.KeyCode == Keys.Delete)
            {
                Remove();
                ChangueItens();
            }
        }

        //Tipos de lista
        private void SmallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listitens.View = View.LargeIcon;
        }

        private void SmallIconsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listitens.View = View.SmallIcon;
        }

        private void DetalhesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listitens.View = View.Details;
        }

        private void ListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listitens.View = View.List;
        }

        private void TitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listitens.View = View.Tile;
        }

        private void RemoverToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Remove();
            ChangueItens();
        }

        private void RemoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Remove();
            ChangueItens();
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run(false);
            QuestionHide();
        }

        private void AbrirComoAdmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run(true);
            QuestionHide();
        }

        private void Listitens_DoubleClick(object sender, EventArgs e)
        {
            Run(listitens.Items[listitens.Items.IndexOf(listitens.SelectedItems[0])].Checked);
            QuestionHide();

        }

        private void LimparListaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearAll();
            ChangueItens();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TitleProgram = this.Text + " v" + Ver;
            this.Text = TitleProgram;

            LoadFile(myFile);
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                var kaka = key.GetValue("LauncherApps");
                inicializarComOOSToolStripMenuItem.Checked = (kaka != null);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (change)
            {
                var ButtonResult = MessageBox.Show("Existe alteração, deseja salvar?", "Alteração",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (ButtonResult == DialogResult.Yes)
                    SaveList();

            }

        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void EsconderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            notifyIcon1.Visible = true;
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
            }

        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible == true)
            { notifyIcon1.Visible = false; }
        }

        private void AbrirListaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(myFile))
                Process.Start("notepad", myFile);
        }

        private void SalvarListaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveList();
        }
        private void InicializarComOOSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inicializarComOOSToolStripMenuItem.Checked)
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        key.SetValue("LauncherApps", "\"" + Application.ExecutablePath + "\"");
                        //Properties.Settings.Default.autoIni = true;

                    }
                }
                catch
                {
                    MessageBox.Show("Não foi possivel adicionar tente novamente executando como administrador", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        key.DeleteValue("LauncherApps", false);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
        private void SobreToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("\tCriado por Sr.Shadowy @2016 @2020" + Environment.NewLine +
                "\t\tVer " + Ver + Environment.NewLine + "APP LAUNCHER inspirado e construido graças ao Smoll_iCe", "Sobre...");

        }
        private void VerificarUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VerChange())
            {
                var butonsResult = MessageBox.Show("Existe uma nova versão deseja baixar?", "Nova versão disponivel", MessageBoxButtons.YesNo);
                if (butonsResult == DialogResult.Yes)
                {
                    File.WriteAllText("dat.bin",NewVersion + Environment.NewLine + Ver);
                    if (File.Exists("update.exe"))
                    {
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "update.exe",
                        UseShellExecute = true,
                        Verb = "runas",
                        Arguments = Ver
                    };
                    Process.Start(processInfo);
                    }else
                    {
                        Process.Start("https://github.com/SrShadowy/AppLauncher/releases/");
                    }
                }
            }
            else
            {
                MessageBox.Show("Não foi possivel ou você está usando a versão atual.", "versão");
            }
        }
        private void Cm_itens_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = cm_itens.Items.IndexOf(e.ClickedItem);
            // MessageBox.Show(e.ClickedItem.Text + " " + index);
            listitens.Items[index].Selected = true;
            Run(false);

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "Estamos aqui na barra basta clica para abrir novamente :)";
                notifyIcon1.ShowBalloonTip(1000);
            }
        }
        private void AdicionarGrupoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string x = string.Empty;

            ListViewGroup NewGroup = new ListViewGroup();

            var resultDialog = InputBox("Novo grupo", "Insira o nome do grupo", ref x);
            if (resultDialog == DialogResult.OK)
            {
                NewGroup.Header = x;
                NewGroup.HeaderAlignment = HorizontalAlignment.Center;
                listitens.Groups.Add(NewGroup);
                (cms_viewer.Items[4] as ToolStripMenuItem).DropDownItems.Add(listitens.Groups[listitens.Groups.Count - 1].Header);
                MyGroups.Items.Add(x);
                ChangueItens();
            }
        }
        private void MoverParaOGrupoToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = moverParaOGrupoToolStripMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            var itemIndex = listitens.Items.IndexOf(listitens.SelectedItems[0]);
            listitens.Items[itemIndex].Group = listitens.Groups[index];
            string newGroup = listitens.Items[itemIndex].Text + ":" + listitens.Groups[index];
            ChangeGroup(newGroup);
            ChangueItens();
        }

        private void RemoverGrupoToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int indx = removerGrupoToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem);
            editarToolStripMenuItem.HideDropDown();
            var diagResult = MessageBox.Show("Tem certeza que deseja excluir o grupo: " + removerGrupoToolStripMenuItem.DropDownItems[indx].Text,
                "Excluir grupo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (diagResult == DialogResult.Yes)
                RemoveGroup(indx);

            ChangueItens();

        }

        private void DefinirComoADMINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var itemIndex = listitens.Items.IndexOf(listitens.SelectedItems[0]);
            listitens.Items[itemIndex].Checked = !listitens.Items[itemIndex].Checked;
            int removeAt = ConfigAdmin.FindString(listitens.Items[itemIndex].Text);
            if (removeAt > 0)
                ConfigAdmin.Items.RemoveAt(removeAt);

            ConfigAdmin.Items.Add(listitens.Items[itemIndex].Text + ":" + Convert.ToInt32( listitens.Items[itemIndex].Checked));
            ChangueItens();
            if (listitens.Items[itemIndex].Checked)
                listitens.Items[itemIndex].ForeColor = Color.Red;
            else
                listitens.Items[itemIndex].ForeColor = Color.Black;
        }

        private void ImportaListaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog();
            var dialogResult = file.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                try
                {
                    LoadFile(file.FileName);
                    ChangueItens();
                }
                catch
                {
                    //Nada acontece :V
                }
            }
            
        }

    }
}
