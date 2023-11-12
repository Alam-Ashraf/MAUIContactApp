using System.Text.Json;
using Communication = Microsoft.Maui.ApplicationModel.Communication;

namespace MAUIContactApp;

public partial class MainPage : ContentPage
{
    public List<Contact> ContactList { get; set; }

    string htmlSource = @"
<!DOCTYPE html>
<html>
<head></head>
<body>

<script>
    var counter = 1;

    function buttonClicked(e) {
        invokeCSharpAction(counter++);
    }

    function displayContacts(data) {
        var table = document.getElementById('dataTable');
        table.innerHTML = ''; // Clear existing table content

        // Create a header row
        var headerRow = table.insertRow(0);
        var headerCellName = headerRow.insertCell(0);
        headerCellName.innerHTML = '<b>Name</b>';
        var headerCellPhone = headerRow.insertCell(1);
        headerCellPhone.innerHTML = '<b>Phone</b>';

        // Iterate over the list and create a row for each contact
        for (var i = 0; i < data.length; i++) {
            var row = table.insertRow(i + 1);

            var cellName = row.insertCell(0);
            cellName.innerHTML = data[i].Name;

            var cellPhone = row.insertCell(1);
            cellPhone.innerHTML = data[i].Phone;
        }
    }
</script>

<button style='justify-content: center; text-align: center; height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='javascript:buttonClicked(event)'>Load Contacts</button>
<div>
    <table id='dataTable' style='font-family: script; border-collapse: collapse; width: 300px; margin-top: 10px;'>
        <!-- Table content will be dynamically added here -->
    </table>
</div>

</body>
</html>
";
    public MainPage()
    {
        InitializeComponent();
        ContactList = new List<Contact>();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        webView.Source = new HtmlWebViewSource() { Html = htmlSource };
        webView.JavaScriptAction += MyWebView_JavaScriptAction;
    }

    private void MyWebView_JavaScriptAction(object sender, Controls.JavaScriptActionEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            _ = LoadContactsAsync();
        });
    }

    async Task LoadContactsAsync()
    {
        try
        {
            var contacts = await Communication.Contacts.Default.GetAllAsync();
            ContactList.Clear();
            foreach (var contact in contacts)
            {
                ContactList.Add(new Contact()
                {
                    Name = contact.DisplayName,
                    Phone = contact.Phones.FirstOrDefault().PhoneNumber,
                });
            }

            Dispatcher.Dispatch(async () =>
            {
                await webView.EvaluateJavaScriptAsync(new EvaluateJavaScriptAsyncRequest("displayContacts(" + JsonSerializer.Serialize(ContactList) + ")"));
            });

        }
        catch (Exception ex)
        {
            // Handle exceptions here
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}

public class Contact
{
    public string Name { get; set; }
    public string Phone { get; set; }
}

