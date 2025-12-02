using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MedicalImageProcessor.Mobile;

public partial class MainPage : ContentPage
{
    private string? _token;

    string baseUrl =
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5077"
            : "http://localhost:5077";
    
    public MainPage()
    {
        InitializeComponent();
        ModelTypePicker.SelectedIndex = 0;
        UploadButton.IsEnabled = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckToken();
    }
    
    private void CheckToken()
    {
        _token = Preferences.Get("token", null);
        if (!string.IsNullOrEmpty(_token))
        {
            UploadButton.IsEnabled = true;
            ResultLabel.Text = "Logged in";
            ResultLabel.TextColor = Colors.Green;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Auth("login");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Auth("register");
    }

    private async Task Auth(string type)
{
    var username = UsernameEntry.Text?.Trim();
    var password = PasswordEntry.Text?.Trim();

    // Перевірка, чи поля не порожні і не містять тільки підказки
    if (string.IsNullOrWhiteSpace(username) || 
        string.IsNullOrWhiteSpace(password))
    {
        await DisplayAlert("Помилка", "Введіть реальний логін та пароль", "OK");
        return;
    }

    LoginButton.IsEnabled = false;
    RegisterButton.IsEnabled = false;

    try
    {
        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(new { username, password });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{baseUrl}/api/auth/{type}";
        Console.WriteLine($"[AUTH] → {url}");
        Console.WriteLine($"[AUTH] Body: {json}");

        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            
            if (data?.TryGetValue("token", out var token) == true)
            {
                _token = token;
                Preferences.Set("token", _token);
    
                UploadButton.IsEnabled = true;
                ResultLabel.Text = $"Вітаємо, {username}!";
                ResultLabel.TextColor = Colors.Green;

                UsernameEntry.Text = "";
                PasswordEntry.Text = "";

                await DisplayAlert("Успіх", $"Ви успішно увійшли!", "OK");
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[AUTH] ERROR: {error}");
            ResultLabel.Text = "Неправильний логін або пароль";
            ResultLabel.TextColor = Colors.Red;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[AUTH] EXCEPTION: {ex}");
        ResultLabel.Text = "Помилка з'єднання з сервером";
        ResultLabel.TextColor = Colors.Red;
    }
    finally
    {
        LoginButton.IsEnabled = true;
        RegisterButton.IsEnabled = true;
    }
}

    private async void OnUploadClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("[UPLOAD] Starting upload");

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Choose image",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
            {
                Debug.WriteLine("[UPLOAD] No file selected");
                return;
            }

            Debug.WriteLine($"[UPLOAD] File selected: {result.FullPath}");

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            Debug.WriteLine($"[UPLOAD] File size: {bytes.Length} bytes");

            var model = ModelTypePicker.SelectedIndex == 0 ? "tumor" : "fracture";
            Debug.WriteLine($"[UPLOAD] Model: {model}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(bytes), "imageFile", result.FileName);

            Debug.WriteLine($"[UPLOAD] Sending request → {baseUrl}/api/Detection/detect?modelType={model}");

            var response = await client.PostAsync(
                $"{baseUrl}/api/Detection/detect?modelType={model}",
                content);

            Debug.WriteLine($"[UPLOAD] ResponseCode: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[UPLOAD] Raw response: {json}");

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<DetectionResult>(json)!;

                // Формуємо результат детекції
                var text =
                    $"Tumor: {data.brainTumorConfidence:P1}\n" +
                    $"Fracture: {data.fractureConfidence:P1}\n\n";

                // Логіка рекомендацій
                var hasRiskTumor = data.brainTumorConfidence >= 0.50f;
                var hasRiskFracture = data.fractureConfidence >= 0.50f;

                if (hasRiskTumor)
                {
                    text += "⚠️ *Можливі ознаки пухлинного процесу.*\n" +
                            "Рекомендації:\n" +
                            "• Запишіться на консультацію до невролога або нейрохірурга.\n" +
                            "• Бажано пройти МРТ з контрастом для уточнення діагнозу.\n" +
                            "• Якщо відчуваєте головний біль, нудоту чи порушення зору — не зволікайте.\n\n";
                }

                if (hasRiskFracture)
                {
                    text += "⚠️ *Можливі ознаки перелому.*\n" +
                            "Рекомендації:\n" +
                            "• Зверніться до травматолога для очного огляду.\n" +
                            "• Уникайте навантаження на потенційно травмовану ділянку.\n" +
                            "• За потреби пройдіть рентген для підтвердження.\n\n";
                }

                if (!hasRiskTumor && !hasRiskFracture)
                {
                    text += "🙂 *Ймовірних патологій не виявлено.*\n" +
                            "Все виглядає добре — однак якщо маєте дискомфорт чи симптоми, не соромтесь звернутися до лікаря.\n";
                }

                ResultLabel.Text = text;
                ResultLabel.TextColor = Colors.DarkGreen;
            }
            else
            {
                ResultLabel.Text = "Analysis failed";
                ResultLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UPLOAD] EXCEPTION: {ex}");
            LoadingIndicator.IsVisible = false;
            ResultLabel.Text = "Error";
            ResultLabel.TextColor = Colors.Red;
        }
    }
}

public class DetectionResult
{
    public bool hasBrainTumor { get; set; }
    public float brainTumorConfidence { get; set; }
    public bool hasFracture { get; set; }
    public float fractureConfidence { get; set; }
}
