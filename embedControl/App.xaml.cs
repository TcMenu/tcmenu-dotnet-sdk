using System.Runtime.ConstrainedExecution;

namespace embedControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}