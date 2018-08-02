Imports System.Collections.ObjectModel

Class pgTiles

    Dim tiles As ObservableCollection(Of Tile)


    Private Sub pgTiles_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        tiles = New ObservableCollection(Of Tile)
        icTiles.ItemsSource = tiles

        tiles.Add(New Tile("tile 1"))
        tiles.Add(New Tile("tile 2"))
        tiles.Add(New Tile("tile 3"))
        tiles.Add(New Tile("tile 4"))
        tiles.Add(New Tile("tile 5"))
        tiles.Add(New Tile("tile 6"))
        tiles.Add(New Tile("tile 7"))
        tiles.Add(New Tile("tile 8"))
        tiles.Add(New Tile("tile 9"))
        tiles.Add(New Tile("tile 10"))
        tiles.Add(New Tile("tile 11"))
        tiles.Add(New Tile("tile 12"))
    End Sub

    Private _startpoint As Point
    Private _offset As Point
    Private _isdragging As Boolean
    Private _cursor As Cursor
    Private _adorner As DragAdorner

    Private Sub ItemsControl_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
        _startpoint = e.GetPosition(Me)

        Dim currentitem As Border = FindVisualParent(Of Border)(e.OriginalSource)
        If currentitem Is Nothing OrElse TypeOf currentitem.DataContext IsNot Tile Then Exit Sub

        _offset = e.GetPosition(currentitem)
        Me.Title = _offset.X & "    " & _offset.Y
    End Sub

    Private Sub ItemsControl_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
        _startpoint = Nothing
    End Sub

    Private Sub ItemsControl_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        If e.LeftButton = MouseButtonState.Pressed AndAlso Not _isdragging Then
            Dim position As Point = e.GetPosition(Me)
            If Math.Abs(position.X - _startpoint.X) > SystemParameters.MinimumHorizontalDragDistance OrElse Math.Abs(position.Y - _startpoint.Y) > SystemParameters.MinimumVerticalDragDistance Then
                StartDragProcess(e.OriginalSource, _offset)
            End If
        End If
    End Sub

    Private Sub StartDragProcess(OriginalSource As Object, Position As Point)
        Dim currentitem As Border = FindVisualParent(Of Border)(OriginalSource)
        If currentitem Is Nothing OrElse TypeOf currentitem.DataContext IsNot Tile Then Exit Sub

        Dim DragScope = TryCast(Window.GetWindow(Me).Content, FrameworkElement)
        Dim previousDrop As Boolean = DragScope.AllowDrop
        DragScope.AllowDrop = True
        Dim feedbackhandler As GiveFeedbackEventHandler = New GiveFeedbackEventHandler(AddressOf DragSource_GiveFeedback)
        AddHandler Me.GiveFeedback, feedbackhandler
        Dim dragoverhandler As DragEventHandler = New DragEventHandler(AddressOf DragTarget_PreviewDragOver)
        AddHandler DragScope.PreviewDragOver, dragoverhandler
        'Dim dragleavehandler As DragEventHandler = New DragEventHandler(DragScope_DragLeave)
        'DragScope.DragLeave += dragleavehandler
        Dim queryhandler As QueryContinueDragEventHandler = New QueryContinueDragEventHandler(AddressOf DragSource_QueryContinueDrag)
        AddHandler DragScope.QueryContinueDrag, queryhandler
        _adorner = New DragAdorner(Me, CreateAdornerContent(currentitem.DataContext), Position)
        '_adorner = New DragAdorner(Me, CreateAdornerContent(currentitem.DataContext), Position)
        AdornerLayer.GetAdornerLayer(DragScope).Add(_adorner)

        currentitem.Visibility = Visibility.Hidden

        _isdragging = True
        Dim dragData As New DataObject(currentitem.DataContext)
        Dim de As DragDropEffects = DragDrop.DoDragDrop(Me, dragData, DragDropEffects.All)

        DragScope.AllowDrop = previousDrop

        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner)
        _adorner = Nothing

        RemoveHandler Me.GiveFeedback, feedbackhandler
        'RemoveHandler DragScope.DragLeave, dragleavehandler
        RemoveHandler DragScope.QueryContinueDrag, queryhandler
        RemoveHandler DragScope.PreviewDragOver, dragoverhandler
        _isdragging = False

        currentitem.Visibility = Visibility.Visible

    End Sub

    Private Sub DragSource_GiveFeedback(ByVal sender As Object, ByVal e As GiveFeedbackEventArgs)
        'Try
        '    Try
        '        Using cursorStream As Stream = New MemoryStream(My.Resources.deny)
        '            _cursor = New Cursor(cursorStream)
        '        End Using
        '        Mouse.SetCursor(_cursor)
        '    Catch ex As Exception
        '        Debug.Print(ex.Message)
        '    End Try
        '    e.UseDefaultCursors = False
        '    e.Handled = True
        'Finally
        'End Try
    End Sub

    Private Sub DragTarget_PreviewDragOver(sender As Object, e As DragEventArgs)
        If _adorner IsNot Nothing Then
            _adorner.LeftOffset = e.GetPosition(Me).X
            _adorner.TopOffset = e.GetPosition(Me).Y
            Me.Title = _adorner.LeftOffset & "    " & _adorner.TopOffset
        End If
    End Sub

    Private Sub DragSource_QueryContinueDrag(ByVal sender As Object, ByVal e As QueryContinueDragEventArgs)
        If e.EscapePressed Then e.Action = DragAction.Cancel
    End Sub

    Private Function CreateAdornerContent(item As Tile) As UIElement
        Dim brd As New ContentPresenter
        brd.ContentTemplate = FindResource("ItemsControlItemDataTemplate")
        brd.Content = item

        'Dim brd As New Border With {
        '    .CornerRadius = New CornerRadius(5, 5, 5, 5),
        '    .Background = Brushes.AliceBlue,
        '    .BorderThickness = New Thickness(2),
        '    .BorderBrush = Brushes.Black,
        '    .Opacity = 0.7,
        '    .Width = 78,
        '    .Height = 78}
        'Dim sp As New StackPanel With {
        '    .Orientation = Orientation.Vertical}
        'sp.Children.Add(New Image() With {.Source = item.Icon})
        'sp.Children.Add(New TextBlock() With {.Text = item.Name})

        'brd.Child = sp
        brd.Measure(New Size(80, 80))
        brd.Arrange(New Rect(brd.DesiredSize))
        brd.UpdateLayout()
        Return brd
    End Function

    Private Class DragAdorner
        Inherits Adorner

        Private _child As Rectangle
        Private _leftoffset As Double
        Private _topoffset As Double
        Private _startoffset As Point

        Public Sub New(ByVal adornedElement As UIElement, ByVal content As UIElement, Optional startOffset As Point = Nothing)
            MyBase.New(adornedElement)

            _startoffset = startOffset
            _child = New Rectangle()
            _child.Width = content.RenderSize.Width
            _child.Height = content.RenderSize.Height
            _child.Fill = New VisualBrush(content)
        End Sub

        Protected Overrides Function MeasureOverride(ByVal constraint As System.Windows.Size) As System.Windows.Size
            _child.Measure(constraint)
            Return _child.DesiredSize
        End Function

        Protected Overrides Function ArrangeOverride(ByVal finalSize As System.Windows.Size) As System.Windows.Size
            _child.Arrange(New Rect(finalSize))
            Return finalSize
        End Function

        Protected Overrides Function GetVisualChild(ByVal index As Integer) As System.Windows.Media.Visual
            Return _child
        End Function

        Protected Overrides ReadOnly Property VisualChildrenCount() As Integer
            Get
                Return 1
            End Get
        End Property

        'With this bit Of code, we can already show the rectangle we wanted.
        'We'll want to allow the drag/drop code we wrote in the window to update the adorner to follow the mouse, so we'll add a couple of properties for this.

        Public Property LeftOffset() As Double
            Get
                Return _leftoffset
            End Get
            Set(ByVal value As Double)
                _leftoffset = value
                UpdatePosition()
            End Set
        End Property

        Public Property TopOffset() As Double
            Get
                Return _topoffset
            End Get
            Set(ByVal value As Double)
                _topoffset = value
                UpdatePosition()
            End Set
        End Property

        Private Sub UpdatePosition()
            Dim adornerLayer As AdornerLayer = Me.Parent
            If Not adornerLayer Is Nothing Then
                adornerLayer.Update(AdornedElement)
            End If
        End Sub

        'Finally, adorners are always placed relative To the element they adorn. You can Then place them relative To the corners, the middle, off To the side, within, etc. We'll just offset the adorner to where the user would like to drop the dragged element, by adding a translate transform to whatever was necessary to get to the adorned element.

        Public Overrides Function GetDesiredTransform(ByVal transform As System.Windows.Media.GeneralTransform) As System.Windows.Media.GeneralTransform
            Dim result As GeneralTransformGroup = New GeneralTransformGroup()
            result.Children.Add(MyBase.GetDesiredTransform(transform))
            result.Children.Add(New TranslateTransform(LeftOffset - _startoffset.X, TopOffset - _startoffset.Y))
            Return result
        End Function
    End Class

End Class

Public Class Tile

    Sub New()

    End Sub

    Sub New(Name As String)
        Me.Name = Name
    End Sub

    Public Property Name As String

    Public ReadOnly Property Icon As BitmapImage
        Get
            Return New BitmapImage(New Uri("pack://application:,,,/images/folder.png"))
        End Get
    End Property

    Public Property Description As String

End Class

