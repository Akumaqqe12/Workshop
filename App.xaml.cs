using System.Windows;

namespace MusicRepairShop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Инициализация базы данных
            using (var context = new Api.ApplicationContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}