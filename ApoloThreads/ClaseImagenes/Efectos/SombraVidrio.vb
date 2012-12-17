﻿Imports ClaseImagenes.Apolo

Public Class SombraVidrio

    Dim objetoTratamiento As New TratamientoImagenes 'Instancia a la clase TratamientoImagenes
    Dim bmpP As New Bitmap(Principal.PictureBox1.Image) 'Imagen de principal

    Private Sub SombraVidrio_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Configuramos el hscrollbar
        HScrollBar1.Minimum = 1
        HScrollBar1.Maximum = bmpP.Height
        HScrollBar1.Value = bmpP.Height / 3
        Label1.Text = HScrollBar1.Value
        'Activamos check
        CheckBox1.Checked = True
        'Asignamos el gestor que controle cuando sale imagen
        AddHandler objetoTratamiento.actualizaBMP, New ActualizamosImagen(AddressOf Principal.actualizarPicture)
        'Asignamos el gestor que controle cuando se abre una imagen nueva
        AddHandler objetoTratamiento.actualizaNombreImagen, New ActualizamosNombreImagen(AddressOf Principal.actualizarNombrePicture)
    End Sub

    Private Sub HScrollBar1_Scroll(sender As Object, e As ScrollEventArgs) Handles HScrollBar1.Scroll
        Label1.Text = HScrollBar1.Value
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If BackgroundWorker1.IsBusy = False Then
            BackgroundWorker1.RunWorkerAsync()
        End If
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        If CheckBox1.Checked = True Then
            Principal.PictureBox1.Image = objetoTratamiento.SombraVidrio(bmpP, HScrollBar1.Value, True)
        Else
            Principal.PictureBox1.Image=objetoTratamiento.SombraVidrio(bmpP,HScrollBar1.Value,False)
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class