﻿Imports System.Collections.ObjectModel

Class pgTreeView

    Public Shared containers As ObservableCollection(Of TreeViewContainer)

    Private Sub pgTreeView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        containers = New ObservableCollection(Of TreeViewContainer)

        Dim c As TreeViewContainer
        Dim cc As TreeViewContainer

        c = New TreeViewContainer("Root 1")
        c.Children.Add(New TreeViewContainer("Child 1-1", c))
        c.Children.Add(New TreeViewContainer("Child 1-2", c))
        c.Children.Add(New TreeViewContainer("Child 1-3", c))
        containers.Add(c)

        c = New TreeViewContainer("Root 2")
        c.Children.Add(New TreeViewContainer("Child 2-1", c))
        c.Children.Add(New TreeViewContainer("Child 2-2", c))
        c.Children.Add(New TreeViewContainer("Child 2-3", c))
        containers.Add(c)

        c = New TreeViewContainer("Root 3")
        cc = New TreeViewContainer("Child 3-3", c)
        cc.Children.Add(New TreeViewContainer("Child 3-3-1", cc))
        cc.Children.Add(New TreeViewContainer("Child 3-3-2", cc))
        cc.Children.Add(New TreeViewContainer("Child 3-3-3", cc))

        c.Children.Add(New TreeViewContainer("Child 3-1", c))
        c.Children.Add(New TreeViewContainer("Child 3-2", c))
        c.Children.Add(cc)
        containers.Add(c)



        tvMain.ItemsSource = containers
    End Sub


    Private Sub TreeViewItemBorder_DragEnter(sender As Object, e As DragEventArgs)
        DragDropHelper.SetIsDragOver(sender, True)
    End Sub

    Private Sub TreeViewItemBorder_PreviewDragLeave(sender As Object, e As DragEventArgs)
        DragDropHelper.SetIsDragOver(sender, False)
        Dim tvi = FindVisualParent(Of TreeViewItem)(sender)
        If tvi IsNot Nothing Then tvi.IsSelected = False
    End Sub

    Private Sub TreeViewItemBorder_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        Dim tvi = FindVisualParent(Of TreeViewItem)(sender)
        If tvi IsNot Nothing Then tvi.IsSelected = True
    End Sub

    Private Sub TreeViewItemExpander_DragEnter(sender As Object, e As DragEventArgs)
        CType(sender, Primitives.ToggleButton).IsChecked = True
    End Sub

    Dim startmouseposition As Point

    Private Sub TreeView_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
        startmouseposition = e.GetPosition(sender)
    End Sub

    Private Sub TreeView_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
        startmouseposition = Nothing
    End Sub

    Private Sub TreeView_MouseMove(sender As Object, e As MouseEventArgs)
        Dim treeview As TreeView = TryCast(sender, TreeView)

        If e.LeftButton = MouseButtonState.Pressed And e.GetPosition(sender).X < treeview.ActualWidth - SystemParameters.VerticalScrollBarWidth And
            Math.Abs(e.GetPosition(sender).X - startmouseposition.X) < 10 And
            Math.Abs(e.GetPosition(sender).Y - startmouseposition.Y) < 10 And
            (Math.Abs(e.GetPosition(sender).X - startmouseposition.X) > 5 Or
            Math.Abs(e.GetPosition(sender).Y - startmouseposition.Y) > 5) Then

            Dim tvi = FindVisualParent(Of TreeViewItem)(e.OriginalSource)

            If tvi IsNot Nothing AndAlso tvi.DataContext IsNot Nothing Then
                Dim dragData As New DataObject(tvi.DataContext)

                DragDrop.DoDragDrop(treeview, dragData, DragDropEffects.Move)
            End If
        End If
    End Sub

    Private Sub TreeView_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(GetType(TreeViewContainer)) Then
            Dim tvidestination = FindVisualParent(Of TreeViewItem)(e.OriginalSource)
            Dim destination As TreeViewContainer = If(tvidestination IsNot Nothing AndAlso TypeOf (tvidestination.DataContext) Is TreeViewContainer, CType(tvidestination.DataContext, TreeViewContainer), Nothing)
            Dim source As TreeViewContainer = e.Data.GetData(GetType(TreeViewContainer))

            e.Effects = DragDropEffects.Move

            If source Is destination Then e.Effects = DragDropEffects.None
            If source IsNot Nothing And destination IsNot Nothing Then
                Dim parent = destination.Parent
                While parent IsNot Nothing
                    If source Is parent Then e.Effects = DragDropEffects.None : Exit While
                    parent = parent.Parent
                End While
            End If
        Else
            e.Effects = DragDropEffects.None
        End If

        e.Handled = True
    End Sub

    Private Sub TreeView_Drop(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(GetType(TreeViewContainer)) Then
            Dim tvidestination = FindVisualParent(Of TreeViewItem)(e.OriginalSource)
            Dim objdestination As TreeViewContainer = If(tvidestination IsNot Nothing AndAlso TypeOf (tvidestination.DataContext) Is TreeViewContainer, CType(tvidestination.DataContext, TreeViewContainer), Nothing)
            Dim objsource As TreeViewContainer = e.Data.GetData(GetType(TreeViewContainer))
            Dim tvisource As TreeViewItem = CType(e.Source, TreeView).ItemContainerGenerator.ContainerFromItem(objsource)

            If objsource Is objdestination Then Exit Sub
            If objsource IsNot Nothing And objdestination IsNot Nothing Then
                Dim parent = objdestination.Parent
                While parent IsNot Nothing
                    If objsource Is parent Then Exit Sub
                    parent = parent.Parent
                End While

                objdestination.Children.Add(objsource)
                If objsource.Parent IsNot Nothing Then
                    objsource.Parent.Children.Remove(objsource)
                Else
                    containers.Remove(objsource)
                End If
                objsource.Parent = objdestination
                tvidestination.IsExpanded = True
            ElseIf objsource IsNot Nothing And objdestination Is Nothing Then
                containers.Add(objsource)
                If objsource.Parent IsNot Nothing Then
                    objsource.Parent.Children.Remove(objsource)
                Else
                    containers.Remove(objsource)
                End If
                objsource.Parent = objdestination
            End If
        End If
    End Sub

End Class

Public Class DragDropHelper
    Public Shared ReadOnly IsDragOverProperty As DependencyProperty = DependencyProperty.RegisterAttached("IsDragOver", GetType(Boolean), GetType(DragDropHelper), New PropertyMetadata(False))

    Public Shared Sub SetIsDragOver(element As DependencyObject, value As Boolean)
        element.SetValue(IsDragOverProperty, value)
    End Sub

    Public Shared Function GetIsDragOver(element As DependencyObject) As Boolean
        Return element.GetValue(IsDragOverProperty)
    End Function
End Class

Public Class ConverterLeftMarginMultiplier
    Implements IValueConverter

    Public Property LeftMargin As Double

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Dim item As TreeViewItem = value
        Dim depth As Integer = GetDepth(Of TreeViewItem)(item, 0)
        If item Is Nothing Then Return New Thickness(0)
        Return New Thickness(depth * LeftMargin, 0, 0, 0)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

    Public Function GetDepth(Of T As DependencyObject)(ByVal child As Object, depth As Integer) As Integer
        Dim parent As DependencyObject = If(child.Parent IsNot Nothing, child.Parent, VisualTreeHelper.GetParent(child))
        If parent IsNot Nothing Then
            If TypeOf parent Is T Then
                Return GetDepth(Of T)(parent, depth + 1)
            Else
                Return GetDepth(Of T)(parent, depth)
            End If
        Else
            Return depth
        End If
    End Function

End Class

Public Class TreeViewContainer

    Sub New()

    End Sub

    Sub New(Name As String, Optional Parent As TreeViewContainer = Nothing)
        Me.Name = Name
        Me.Parent = Parent
    End Sub

    Public Property Name As String

    Public ReadOnly Property Icon As BitmapImage
        Get
            Return New BitmapImage(New Uri("pack://application:,,,/images/folder.png"))
        End Get
    End Property

    Public Property Description As String
    Public Property Children As New ObservableCollection(Of TreeViewContainer)
    Public Property Parent As TreeViewContainer

End Class