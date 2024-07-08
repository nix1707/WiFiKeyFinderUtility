using CommunityToolkit.Maui.Storage;
using WiFiKeyFinderUtility.Models;
using WiFiKeyFinderUtility.Scripts;


namespace WiFiKeyFinderUtility;

public partial class MainPage : ContentPage
{
    private string _wifiInfosString = string.Empty;

    public MainPage()
    {
        InitializeComponent();
    }

    private void LoadWifiInfos()
    {
        try
        {
            List<WiFiInfo> wifiInfos = WiFiPasswordsRetriever.GetWiFiInfos();
            wifiListView.ItemsSource = wifiInfos;
            _wifiInfosString =
                string.Join("", wifiInfos.Select(i => $"SSID: {i.SSID},    PASSWORD: {i.Password}\n"));

            if (wifiInfos.Count > 0)
            {
                SaveAsTxtButton.IsEnabled = true;
                wifiListView.IsVisible = true;
                PlaceholderLabel.IsVisible = false;
            }
            else
            {
                SaveAsTxtButton.IsEnabled = false;
                wifiListView.IsVisible = false;
                PlaceholderLabel.IsVisible = true;
                PlaceholderLabel.Text = "We haven't found any WiFi yet...";
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void OnGetPasswordsClicked(object sender, EventArgs e)
    {
        LoadWifiInfos();
    }

    private async void OnSaveAsTextClicked(object sender, EventArgs e)
    {
        var result = await FolderPicker.Default.PickAsync();

        if (result.IsSuccessful && string.IsNullOrWhiteSpace(_wifiInfosString) == false)
        {
            string path = Path.Combine(result.Folder.Path, "wifi-info.txt");
            if (File.Exists(path) == false)
            {
                var created = File.Create(path);
                created.Close();
            }

            await File.WriteAllTextAsync(path, _wifiInfosString);
        }
        else
        {
            await DisplayAlert("Error", "We can't save your data, try to change directory", "Ok");
        }
    }

}
