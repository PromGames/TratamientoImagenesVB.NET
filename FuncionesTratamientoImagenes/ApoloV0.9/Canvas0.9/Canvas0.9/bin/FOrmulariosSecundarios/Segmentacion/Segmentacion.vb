﻿Public Class Segmentacion

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        If CheckBox1.Checked = True Then
            My.Settings.MensajeSegmentacion = False
        Else
            My.Settings.MensajeSegmentacion = True
        End If
        My.Settings.Save()
        Me.Close()
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
End Class