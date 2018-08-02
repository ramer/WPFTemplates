Imports System.Windows.Controls.Primitives

Class MainWindow

    Private Sub ToggleRadioButton_Checked(sender As Object, e As RoutedEventArgs)
        If CType(sender, ToggleButton).Content = "TreeView" Then frm.Navigate(New pgTreeView)
        If CType(sender, ToggleButton).Content = "Tiles" Then frm.Navigate(New pgTiles)
    End Sub

End Class
