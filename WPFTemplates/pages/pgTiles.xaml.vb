Imports System.Collections.ObjectModel
Imports System.ComponentModel

Class pgTiles

    Dim tiles As ObservableCollection(Of Tile)


    Private Sub pgTiles_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        tiles = New ObservableCollection(Of Tile)
        icTiles.ItemsSource = tiles

        For I As Integer = 0 To 10
            tiles.Add(New Tile("tile " & I, FindResource("TileLength"), I))
        Next
    End Sub



End Class

Public Class Tile
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Sub NotifyPropertyChanged(propertyName As String)
        Me.OnPropertyChanged(New PropertyChangedEventArgs(propertyName))
    End Sub

    Protected Overridable Sub OnPropertyChanged(e As PropertyChangedEventArgs)
        RaiseEvent PropertyChanged(Me, e)
    End Sub

    Sub New()

    End Sub

    Sub New(Name As String, Optional length As Double = 100, Optional index As Integer = 0)
        Me.Name = Name
        Me.Length = Me.Length
        Left = index * length
        Top = 10
    End Sub

    Public Property Name As String

    Public Length As Double
    Private _left As Double
    Public Property Left As Double
        Get
            Return _left
        End Get
        Set(value As Double)
            _left = value
            NotifyPropertyChanged("Left")
        End Set
    End Property

    Private _top As Double
    Public Property Top As Double
        Get
            Return _top
        End Get
        Set(value As Double)
            _top = value
            NotifyPropertyChanged("Top")
        End Set
    End Property

    Public ReadOnly Property Icon As BitmapImage
        Get
            Return New BitmapImage(New Uri("pack://application:,,,/images/folder.png"))
        End Get
    End Property

    Public Property Description As String

End Class

