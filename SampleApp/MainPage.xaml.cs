using Maui.BottomSheet;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            sheetOpen.Clicked += (s, e) => sh.Open();
            darkModeBtn.Clicked += (s, e) => Application.Current.UserAppTheme = AppTheme.Dark;
            lightModeBtn.Clicked += (s, e) => Application.Current.UserAppTheme = AppTheme.Light;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
