Module mdlMain

    Public Function FindVisualParent(Of T As DependencyObject)(ByVal child As Object) As T
        If TypeOf child Is T Then Return child
        Dim parent As DependencyObject = If(child.Parent IsNot Nothing, child.Parent, VisualTreeHelper.GetParent(child))

        If parent IsNot Nothing Then
            If TypeOf parent Is T Then
                Return parent
            Else
                Return FindVisualParent(Of T)(parent)
            End If
        Else
            Return Nothing
        End If
    End Function


End Module
