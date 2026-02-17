using LocalAchievements.Services;
using Playnite.SDK;
using System.Windows;

namespace LocalAchievements
{
    public partial class ManualMetadataWindow : Window
    {
        public string PastedText { get; private set; }
        //public string Title { get; set; }
        public string Instructions { get; set; }
        public string ButtonText { get; set; }
        public string CopyButton { get; set; }
        public string PasteHere { get; set; }

        private readonly string appId;


        public ManualMetadataWindow(string appId)
        {
            InitializeComponent();
            this.Title = Localization.Get("ManualInputTitle");
            this.Instructions = Localization.Get("ManualInputInstructions");
            this.ButtonText = Localization.Get("ManualInputButton");
            this.CopyButton = Localization.Get("ManualInputCopyButton");
            this.PasteHere = Localization.Get("ManualInputPasteHere");
            this.DataContext = this;
            this.appId = appId;
            GenerateCommandString();
        }

        private void GenerateCommandString()
        {
            string command = $"fetch('https://steamdb.info/api/RenderAppSection/?section=stats&appid={this.appId}', {{ headers: {{ 'X-Requested-With': 'XMLHttpRequest' }} }}).then(r => r.text()).then(data => console.log(data));";
            CommandTextBox.Text = command;
        }

        private void BtnCopyCommand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(CommandTextBox.Text);
                API.Instance.Dialogs.ShowMessage(Localization.Get("ManualInputCopySuccess"), "Local Achievements");
                InputBox.Focus();
            }
            catch
            {
                API.Instance.Dialogs.ShowMessage(Localization.Get("ManualInputCopyFailure"), "Local Achievements");
            }
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            this.PastedText = InputBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}