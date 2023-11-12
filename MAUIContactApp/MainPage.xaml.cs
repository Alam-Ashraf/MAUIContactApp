using System.Text.Json;
using Communication = Microsoft.Maui.ApplicationModel.Communication;

namespace MAUIContactApp;

public partial class MainPage : ContentPage
{
    #region Properties
    private List<Contact> ContactList { get; set; }

    private string htmlSource = @"
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
    #endregion

    #region Constructor
    public MainPage()
    {
        InitializeComponent();
        ContactList = new List<Contact>();
    }
    #endregion

    #region Override Method
    protected override void OnParentSet()
    {
        base.OnParentSet();
        webView.Source = new HtmlWebViewSource() { Html = htmlSource };
        webView.JavaScriptAction += MyWebView_JavaScriptAction;
    }
    #endregion

    #region Private Methods
    private void MyWebView_JavaScriptAction(object sender, Controls.JavaScriptActionEventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            LoadingView.IsVisible = true;
            await CheckPermissionAvail();
        });
    }

    private async Task LoadContactsAsync()
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

            // Adding 3 Sec Delay to show indicator
            await Task.Delay(3000).ContinueWith(task =>
            {
                Dispatcher.Dispatch(async () =>
                {
                    await webView.EvaluateJavaScriptAsync(new EvaluateJavaScriptAsyncRequest("displayContacts(" + JsonSerializer.Serialize(ContactList) + ")"));
                    LoadingView.IsVisible = false;
                });
            });
        }
        catch (Exception ex)
        {
            // Handle exceptions here
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Permission
    private async Task CheckPermissionAvail()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();

        if (status == PermissionStatus.Granted)
        {
            // Permission has already been granted
            // Now you can read contacts
            await LoadContactsAsync();
        }
        else if (status == PermissionStatus.Denied)
        {
            // Permission has been denied
            // You may want to show a message explaining why you need the permission
            await RequestPermission();
        }
        else
        {
            await RequestPermission();
        }
    }

    private async Task RequestPermission()
    {
        // Permission has not been requested yet
        var requestResult = await Permissions.RequestAsync<Permissions.ContactsRead>();

        if (requestResult == PermissionStatus.Granted)
        {
            // Permission granted
            // Now you can read contacts
            await LoadContactsAsync();
        }
        else
        {
            // Permission denied
            // You may want to show a message explaining why you need the permission
        }
    }
    #endregion
}

public class Contact
{
    public string Name { get; set; }
    public string Phone { get; set; }
}

