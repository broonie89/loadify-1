Imports Infralution.Localization.Wpf
Imports System.Globalization
Imports System.Threading

'' <summary>
'' Demonstrates a small WPF application localized using Globalizer.NET.  Also
'' demonstrates using the Infralution.Localization.ResourceEnumConverter
'' class to translate Enum values.
'' </summary>
Class MainWindow

    '' <summary>
    '' Create a new instance of the window
    '' </summary>
    Public Sub New()
        InitializeComponent()

        ' set the initial application UI Culture based on the users
        ' current regional settings
        '
        CultureManager.UICulture = Thread.CurrentThread.CurrentCulture
        AddHandler CultureManager.UICultureChanged, AddressOf CultureManager_UICultureChanged
        UpdateLanguageMenus()
    End Sub

    '' <summary>
    '' Detach from UICultureChanged event
    '' </summary>
    '' <param name="e"></param>
    '' <remarks>
    '' If we don't detach from the event then the window will not get garbage collected
    '' </remarks>
    Protected Overrides Sub OnClosed(ByVal e As System.EventArgs)
        MyBase.OnClosed(e)
        RemoveHandler CultureManager.UICultureChanged, AddressOf CultureManager_UICultureChanged
    End Sub

    '' <summary>
    '' Update the check state of the Language menu
    '' </summary>
    Private Sub UpdateLanguageMenus()
        Dim lang As String = CultureManager.UICulture.TwoLetterISOLanguageName.ToLower()
        _frenchMenuItem.IsChecked = (lang = "fr")
        _englishMenuItem.IsChecked = (lang = "en")
        _fileListBox.ItemsSource = System.Enum.GetValues(GetType(SampleEnum))
    End Sub

    '' <summary>
    '' Update the language menus when the UI culture changes
    '' </summary>
    '' <param name="sender"></param>
    '' <param name="e"></param>
    Private Sub CultureManager_UICultureChanged(ByVal sender As Object, ByVal e As EventArgs)
        UpdateLanguageMenus()
    End Sub
  
    '' <summary>
    '' Select English as the User Interface language
    '' </summary>
    '' <param name="sender"></param>
    '' <param name="e"></param>
    Private Sub _englishMenuItem_Clicked(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles _englishMenuItem.Click
        CultureManager.UICulture = New CultureInfo("en")
    End Sub

    '' <summary>
    '' Select French as the User Interface language
    '' </summary>
    '' <param name="sender"></param>
    '' <param name="e"></param>
    Private Sub _frenchMenuItem_Clicked(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles _frenchMenuItem.Click
        CultureManager.UICulture = New CultureInfo("fr")
    End Sub

    '' <summary>
    '' Exit the application
    '' </summary>
    '' <param name="sender"></param>
    '' <param name="e"></param>
    Private Sub _exitMenuItem_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles _exitMenuItem.Click
        Me.Close()
    End Sub

End Class
