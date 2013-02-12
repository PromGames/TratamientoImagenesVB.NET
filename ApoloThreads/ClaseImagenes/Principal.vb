﻿Imports ClaseImagenes.Apolo
Imports System.ComponentModel
Imports System.IO

Public Class Principal
    Dim WithEvents objetoTratamiento As New TratamientoImagenes 'Objeto para todo el formulario así no se inicializan las variables de la clase en cada instancia
    Dim transformacion As String 'Transformación a aplicar

#Region "control de excepciones"
    Private Shared Sub Application_ThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        Dim objetoTra As New TratamientoImagenes
        Dim captura As Bitmap = objetoTra.capturarPantalla()
        Dim excepc As Exception = e.Exception
        'Hay que crear la instancia con constructor y el valor del color
        Dim frmError As New NotificacionError(captura, excepc)
        frmError.Show()
    End Sub
#End Region

    Private Sub Principal_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        'Al cerrar el formulario borramos todo lo acumulador por el programa
        Dim folder As New DirectoryInfo(System.IO.Directory.GetCurrentDirectory().ToString & "\ImagenesCloud\") 'Directorio
        Dim listaDearchivos As New ArrayList
        For Each file As FileInfo In folder.GetFiles() 'Comprobamos si los archivos xml
            Try
                Kill(folder.ToString & file.ToString)
            Catch
            End Try
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'borramos todo lo acumulador por el programa
        Dim folder As New DirectoryInfo(System.IO.Directory.GetCurrentDirectory().ToString & "\ImagenesCloud\") 'Directorio
        Dim listaDearchivos As New ArrayList
        For Each file As FileInfo In folder.GetFiles() 'Comprobamos si los archivos xml
            Try
                Kill(folder.ToString & file.ToString)
            Catch
            End Try
        Next
        'Manejamos cualquier excepción no controlada
        AddHandler Application.ThreadException, AddressOf Application_ThreadException

        'Establecemos el control del botón derecho  
        Me.ContextMenuStrip = ContextMenuStrip1
        'Habilitamos el arrastre para el control PictureBox1 (No lo tiene permitido en tiempo de diseño)
        PictureBox1.AllowDrop = True
        'Asignamos el gestor que controle cuando sale imagen
        AddHandler objetoTratamiento.actualizaBMP, New ActualizamosImagen(AddressOf actualizarPicture)
        'Asignamos el gestor que controle cuando se abre una imagen nueva
        AddHandler objetoTratamiento.actualizaNombreImagen, New ActualizamosNombreImagen(AddressOf actualizarNombrePicture)

        'ACtualizamos la imagen Lena
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        Me.PictureBox1.Image = objetoTratamiento.ActualizarImagen(bmp, True)
        'ACtualizamos de forma manual el histograma
        actualizarHistrograma()
        tiempo = 0 'Para que el contador se pare
        Button1.Text = "Actualizar histograma"
    End Sub


#Region "Archivo"
    'Abrir imagen desde archivo
    Private Sub AbrirImagenToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AbrirImagenToolStripMenuItem1.Click
        Dim bmp As Bitmap
        bmp = objetoTratamiento.abrirImagen()
        If bmp IsNot Nothing Then
            PictureBox1.Image = bmp
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    'Abrir imagen como recurso web
    Private Sub CargarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CargarToolStripMenuItem.Click
        AbrirRecurso.Show()
    End Sub

    'Abrir imágenes con BING
    Private Sub BuscarImágenesEnLaWebToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuscarImágenesEnLaWebToolStripMenuItem.Click
        AbrirBing.Show()
    End Sub
    'Abrir imágenes con FB
    Private Sub BuscarImágenesEnFacebookToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuscarImágenesEnFacebookToolStripMenuItem.Click
        AbrirFacebook.Show()
    End Sub
    'Creamos nuevo tapiz
    Private Sub CrearTapizToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CrearTapizToolStripMenuItem.Click
        Tapiz.Show()
    End Sub
    'Guardamos imagen
    Private Sub GuardarComoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuardarComoToolStripMenuItem.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        objetoTratamiento.guardarcomo(bmp, 4)
    End Sub
    Private Sub AbrirCompiladorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AbrirCompiladorToolStripMenuItem.Click
        'Abrimos el exe y guardamos la imagen actual
        Dim bmpComp As New Bitmap(PictureBox1.Image)
        bmpComp.Save(System.IO.Directory.GetCurrentDirectory().ToString & "\Compilador\imgforCompilador.png", System.Drawing.Imaging.ImageFormat.Png)
        Dim Process1 As New Process
        Process1.StartInfo.RedirectStandardOutput = True
        Process1.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory().ToString & "\Compilador\FastSharp.exe"
        Process1.StartInfo.UseShellExecute = False
        Process1.StartInfo.CreateNoWindow = True
        Process1.Start()
        'A la espera de que se cierre...
        Process1.WaitForExit()
        'Aquí listar todas las imágenes que se han creado
        ImgCompilador.ShowDialog()
    End Sub

    Private Sub CompartirImagenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CompartirImagenToolStripMenuItem.Click
        Dim form As New Compartir()
        form.Show()
    End Sub
#End Region

#Region "Editar"
    Private Sub RefrescarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefrescarToolStripMenuItem.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    'Deshacer
    Private Sub DeshacerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeshacerToolStripMenuItem.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            PictureBox1.Image = objetoTratamiento.ListadoImagenesAtras
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox2.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    'Rehacer
    Private Sub RehacerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RehacerToolStripMenuItem.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            PictureBox1.Image = objetoTratamiento.ListadoImagenesAdelante
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox2.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    Private Sub RegistroDeCambiosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RegistroDeCambiosToolStripMenuItem.Click
        RegistroCambio.Show()
    End Sub

    'Actualizar imagen
    Private Sub ActualizarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ActualizarToolStripMenuItem.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        Me.PictureBox1.Image = objetoTratamiento.ActualizarImagen(bmp)
        'Actualizamos el Panel1
        Panel1.AutoScrollMinSize = PictureBox2.Image.Size
        Panel1.AutoScroll = True
    End Sub

    'Deshacer imagen original
    Private Sub ImagenOriginalToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImagenOriginalToolStripMenuItem.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            PictureBox1.Image = objetoTratamiento.ImagenOriginalGuardada
        End If
    End Sub
    'Propiedades de la imagen
    Private Sub PropiedadesDeLaImagenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PropiedadesDeLaImagenToolStripMenuItem.Click
        PropImagen.Show()
    End Sub
#End Region



#Region "OperacionesBasicas"
    Private Sub EscalaDeGrisesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EscalaDeGrisesToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "blancoNegro"
        transformar()
    End Sub

    Private Sub EscalaDeGrisesToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles EscalaDeGrisesToolStripMenuItem1.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "escalaGrises"
        transformar()
    End Sub


    Private Sub RGBToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RGBToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "invertir"
        transformar()
    End Sub

    Private Sub RojoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RojoToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "invertirRojo"
        transformar()
    End Sub

    Private Sub VerdeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VerdeToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "invertirVerde"
        transformar()
    End Sub

    Private Sub AzulToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AzulToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "invertirAzul"
        transformar()
    End Sub

    Private Sub SepiaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SepiaToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "sepia"
        transformar()
    End Sub

    Private Sub FiltroAzulToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FiltroAzulToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "FiltroAzul"
        transformar()
    End Sub
    Private Sub FiltroVerdeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FiltroVerdeToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "FiltroVerde"
        transformar()
    End Sub
    Private Sub FiltroRojoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FiltroRojoToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "FiltroRojo"
        transformar()
    End Sub

    Private Sub BGRToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BGRToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "RGBtoBGR"
        transformar()
    End Sub

    Private Sub GRBToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GRBToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "RGBtoGRB"
        transformar()
    End Sub

    Private Sub RBGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RBGToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "RGBtoRBG"
        transformar()
    End Sub

    Private Sub HorizontalToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HorizontalToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "ReflexionHori"
        transformar()
    End Sub

    Private Sub VerticalToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VerticalToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "ReflexionVert"
        transformar()
    End Sub
    'Histograma detallada
    Private Sub DetalladoToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles DetalladoToolStripMenuItem1.Click
        'Hay que crear la instancia con constructor y el valor del color
        Dim frmHisto As New Histogramas(Color.Red)
        frmHisto.Show()
    End Sub
    'Todos los histogramas
    Private Sub TodosToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles TodosToolStripMenuItem1.Click
        TodosHistogramas.Show()
    End Sub
    'Redimensionar imagen
    Private Sub RedimensionarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RedimensionarToolStripMenuItem.Click
        Redimensionar.Show()
    End Sub
#End Region

#Region "Operaciones básicos personalizadas"
    Private Sub BlancoYNegroToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BlancoYNegroToolStripMenuItem.Click
        BlancoYnegro.Show()
    End Sub

    Private Sub EscalaDeGrisesToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles EscalaDeGrisesToolStripMenuItem2.Click
        EscalaDeGrises.Show()
    End Sub

    Private Sub BrilloToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles BrilloToolStripMenuItem1.Click
        Brillo.Show()
    End Sub
    Private Sub Contraste1ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Contraste1ToolStripMenuItem.Click
        Contraste1.Show()
    End Sub

    Private Sub Contraste2ToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles Contraste2ToolStripMenuItem.Click
        Contraste2.Show()
    End Sub
    Private Sub GamaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GamaToolStripMenuItem.Click
        Gamma.Show()
    End Sub
    Private Sub ExposiciónToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ExposiciónToolStripMenuItem1.Click
        Exposicion.Show()
    End Sub

    Private Sub ModificarCanalesToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ModificarCanalesToolStripMenuItem1.Click
        Canales.Show()
    End Sub
    Private Sub ReducirColoresToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReducirColoresToolStripMenuItem.Click
        ReducirColores.Show()
    End Sub

    Private Sub FiltrarColoresToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FiltrarColoresToolStripMenuItem.Click
        FiltrarColores.Show()
    End Sub

    Private Sub MatrizToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MatrizToolStripMenuItem.Click
        Matriz.Show()
    End Sub

    Private Sub DetectarContornosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DetectarContornosToolStripMenuItem.Click
        Contornos.Show()
    End Sub
#End Region

#Region "Operaciones aritmeticas 1 imagen"

    Private Sub OperacionesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OperacionesToolStripMenuItem.Click
        OperacionesAritmeticas.Show()
    End Sub
    Private Sub OperacionesLógicasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OperacionesLógicasToolStripMenuItem.Click
        OperacionesLogicas.Show()
    End Sub
    Private Sub OperacionesMorfológicasbetaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OperacionesMorfológicasbetaToolStripMenuItem.Click
        OperadoresMorfologicos.Show()
    End Sub
#End Region

#Region "Máscaras"
    Private Sub PasoAltoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PasoAltoToolStripMenuItem.Click
        PasoAlto.Show()
    End Sub

    Private Sub PasoBajoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PasoBajoToolStripMenuItem.Click
        PasoBajo.Show()
    End Sub

    Private Sub BordesYContornosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BordesYContornosToolStripMenuItem.Click
        BordesYContornos.Show()
    End Sub

    Private Sub MáscaraManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MáscaraManualToolStripMenuItem.Click
        MascaraManual.Show()
    End Sub
    Private Sub SobelTotalToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SobelTotalToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "SobelTotal"
        transformar()
    End Sub
#End Region

#Region "Efectos"

    Private Sub DesenfoqueMovimientoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DesenfoqueMovimientoToolStripMenuItem.Click
        Desenfocar.Show()
    End Sub
    Private Sub DesenfonqueDistorsiónToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DesenfonqueDistorsiónToolStripMenuItem.Click
        Distorsion.Show()
    End Sub
    Private Sub DesenfoqueBLURToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DesenfoqueBLURToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "blur"
        transformar()
    End Sub

    Private Sub PixeladoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PixeladoToolStripMenuItem.Click
        Pixelar.Show()
    End Sub

    Private Sub CuadrículaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CuadrículaToolStripMenuItem.Click
        Cuadricula.Show()
    End Sub

    Private Sub SombraDeVidrioToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SombraDeVidrioToolStripMenuItem.Click
        SombraVidrio.Show()
    End Sub
    Private Sub TresPartesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TresPartesToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "3partes"
        transformar()
    End Sub

    Private Sub SeisPartesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SeisPartesToolStripMenuItem.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "6partes"
        transformar()
    End Sub
    Private Sub PonerLosDosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PonerLosDosToolStripMenuItem.Click
        Ruido.Show()
    End Sub

    Private Sub SadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SadToolStripMenuItem.Click
        RuidoDe.Show()
    End Sub

    Private Sub ÓleoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ÓleoToolStripMenuItem.Click
        Oleo.Show()
    End Sub

#End Region

#Region "Operaciones con dos imágenes"
    Private Sub SumaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SumaToolStripMenuItem.Click
        OperacionesAritmeticasDosImagenes.Show()
    End Sub

    Private Sub OperacionesLógicasToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles OperacionesLógicasToolStripMenuItem1.Click
        OperacionesLogicasDosImagenes.Show()
    End Sub

    Private Sub LocalToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LocalToolStripMenuItem.Click
        CompararImagenes.Show()
    End Sub

    Private Sub VecinosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VecinosToolStripMenuItem.Click
        CompararImagenesVecinos.Show()
    End Sub
#End Region

#Region "Cloud"
    Private Sub GuardarImágenesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuardarImágenesToolStripMenuItem.Click
        Dim form As New Compartir()
        form.Show()
    End Sub
    Private Sub CrearToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CrearToolStripMenuItem.Click
        CrearCarpetaPrivada.Show()
    End Sub

    Private Sub AccesoCarpetaPrivadaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AccesoCarpetaPrivadaToolStripMenuItem.Click
        AccesoPrivado.Show()
    End Sub
#End Region

#Region "Herramientas"
    Dim HistogramasAutomáticos As Boolean = True
    'Activa/desactiva histogramas automáticos
    Private Sub HistogramasAutomáticosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HistogramasAutomáticosToolStripMenuItem.Click
        If HistogramasAutomáticosToolStripMenuItem.Checked = False Then
            HistogramasAutomáticosToolStripMenuItem.Checked = True
            tiempo = 0 'Para que el contador se pare
            Button1.Text = "Actualizar histograma"
            TabPage1.Text = "Histograma"
            TabPage2.Text = "Registro cambios"
            HistogramasAutomáticos = True
        Else
            HistogramasAutomáticosToolStripMenuItem.Checked = False
            tiempo = 0 'Para que el contador se pare
            Button1.Text = "Actualizar histograma"
            TabPage1.Text = "Histograma"
            TabPage2.Text = "Registro cambios"
            HistogramasAutomáticos = False
        End If
    End Sub
    'Liberar memoria (libera todas las imágenes guardadas para hacer retroceso)
    Private Sub LiberarMemoriaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LiberarMemoriaToolStripMenuItem.Click
        Dim resultado = MessageBox.Show("Esta opción borrará todo el historial de imágenes y no podrá ser recuperado. Además, es posible que durante unos segundos se ralentice el sistema. ¿Está seguro de querer hacerlo?", "Apolo threads", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation)
        If resultado = Windows.Forms.DialogResult.Yes Then
            Dim objeto As New TratamientoImagenes
            objeto.LiberarImagenes()
            'forzamos la actualización del tabpage 2 (registro imágenes). Los histogramas no son necesarios puesto que nos quedamos con la imagen actual
            TabControl1_SelectedIndexChanged(sender, e)
            ClearMemory()
        End If
    End Sub

    'Código para liberar RAM disponible en************************
    'http://gdev.wordpress.com/2005/11/30/liberar-memoria-con-vb-net/
    'Declaración de la API
    Private Declare Auto Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal procHandle As IntPtr, ByVal min As Int32, ByVal max As Int32) As Boolean
    Public Sub ClearMemory()

        Try
            Dim Mem As Process
            Mem = Process.GetCurrentProcess()
            SetProcessWorkingSetSize(Mem.Handle, -1, -1)
        Catch ex As Exception
            'Control de errores
        End Try
    End Sub
#End Region

#Region "Menú ayda"
    Private Sub NotificarUnErrorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NotificarUnErrorToolStripMenuItem.Click
        NotificarErrorAyuda.Show()
    End Sub

    Private Sub AyúdanosAMejorarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AyúdanosAMejorarToolStripMenuItem.Click
        AyudanosAmejorar.Show()
    End Sub
    Private Sub ColaboraConElProyectoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ColaboraConElProyectoToolStripMenuItem.Click
        Colabora.Show()
    End Sub

#End Region

#Region "Crear proceso con thread"
    'ACtualizamos el estado del proceso
    Private Sub Timer1_Tick_1(sender As Object, e As EventArgs) Handles Timer1.Tick
        barraestado.Value = CInt(TratamientoImagenes.porcentaje(0))
        Estadoactual.Text = TratamientoImagenes.porcentaje(1)
        PorcentajeActual.Text = CInt(TratamientoImagenes.porcentaje(0)) & " %"
    End Sub


    'Si no está ocupado activamos el control BackgroundWorker1
    Sub transformar()
        If BackgroundWorker1.IsBusy = False Then
            BackgroundWorker1.RunWorkerAsync()
        End If
    End Sub


    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        'Obtener el objeto BackgroundWorker que provocó este evento
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
        Dim bmp As New Bitmap(PictureBox1.Image)

        Select Case transformacion
            Case "escalaGrises"
                PictureBox1.Image = objetoTratamiento.EscalaGrises(bmp)
            Case "blancoNegro"
                PictureBox1.Image = objetoTratamiento.BlancoNegro(bmp)
            Case "invertir"
                PictureBox1.Image = objetoTratamiento.Invertir(bmp)
            Case "invertirRojo"
                PictureBox1.Image = objetoTratamiento.Invertir(bmp, True, False, False)
            Case "invertirVerde"
                PictureBox1.Image = objetoTratamiento.Invertir(bmp, False, True, False)
            Case "invertirAzul"
                PictureBox1.Image = objetoTratamiento.Invertir(bmp, False, False, True)
            Case "sepia"
                PictureBox1.Image = objetoTratamiento.sepia(bmp)
            Case "FiltroRojo"
                PictureBox1.Image = objetoTratamiento.filtrosBasicos(bmp, True, False, False)
            Case "FiltroVerde"
                PictureBox1.Image = objetoTratamiento.filtrosBasicos(bmp, False, True, False)
            Case "FiltroAzul"
                PictureBox1.Image = objetoTratamiento.filtrosBasicos(bmp, False, False, True)
            Case "RGBtoBGR"
                PictureBox1.Image = objetoTratamiento.RGBto(bmp, True, False, False)
            Case "RGBtoGRB"
                PictureBox1.Image = objetoTratamiento.RGBto(bmp, False, True, False)
            Case "RGBtoRBG"
                PictureBox1.Image = objetoTratamiento.RGBto(bmp, False, False, True)
            Case "SobelTotal"
                PictureBox1.Image = objetoTratamiento.sobelTotal(bmp)
            Case "ReflexionHori"
                PictureBox1.Image = objetoTratamiento.Reflexion(bmp, True, False)
            Case "ReflexionVert"
                PictureBox1.Image = objetoTratamiento.Reflexion(bmp, False, True)
            Case "3partes"
                PictureBox1.Image = objetoTratamiento.ImagenTresPartes(bmp)
            Case "6partes"
                PictureBox1.Image = objetoTratamiento.ImagenSeisPartes(bmp)
            Case "blur"
                Dim objetoMascara As New TratamientoImagenes.mascaras
                Dim mascara = objetoMascara.LOW9
                PictureBox1.Image = objetoTratamiento.mascara3x3RGB(bmp, mascara, , )
        End Select
    End Sub

    'Cuando acaba el hilo..
    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        'Aprovechamos y actualizamos el Panel1
        Panel1.AutoScrollMinSize = PictureBox1.Image.Size
        Panel1.AutoScroll = True
        BackgroundWorker1.Dispose()
    End Sub

#End Region

#Region "Actualizar histograma"
    Dim tiempo As Integer = 3 'Variable que controla el tiempo de actualización

    Sub actualizarHistrograma() 'Función que recibe y dibuja el histograma
        Try
            'Los ponesmos del colores correspondiente
            Chart1.Series("Rojo").Color = Color.Red
            Chart2.Series("Verde").Color = Color.Green
            Chart3.Series("Azul").Color = Color.Blue
            'Borramos el contenido
            Chart1.Series("Rojo").Points.Clear()
            Chart2.Series("Verde").Points.Clear()
            Chart3.Series("Azul").Points.Clear()
            Dim bmpHisto As New Bitmap(PictureBox1.Image, New Size(New Point(100, 100)))
            Dim histoAcumulado = objetoTratamiento.histogramaAcumulado(bmpHisto)
            For i = 0 To UBound(histoAcumulado)
                Chart1.Series("Rojo").Points.AddXY(i + 1, histoAcumulado(i, 0))
                Chart2.Series("Verde").Points.AddXY(i + 1, histoAcumulado(i, 1))
                Chart3.Series("Azul").Points.AddXY(i + 1, histoAcumulado(i, 2))
            Next

        Catch
            MessageBox.Show("Lo sentimos, algo ha ocurrido. Pruebe a deshacer los cambios y desactivar el histograma automático (Herramientas/Histograma automático)", "Apolo threads", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        actualizarHistrograma()
        tiempo = 0 'Para que el contador se pare
        Button1.Text = "Actualizar histograma"
        TabPage1.Text = "Histograma"
        TabPage2.Text = "Registro cambios"
    End Sub 'Botón para actualizar histograma manualmente

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        If HistogramasAutomáticos = True Then 'Variable global que controla si está activado o desactivado
            If tiempo > 1 Then 'Si es mayor que uno vamos mostrando la cuenta atrás en el botón
                tiempo -= 1
                Button1.Text = "Actualizando en (" & tiempo & ")"
                TabPage1.Text = "Actualizando(" & tiempo & ")"
                TabPage2.Text = "Actualizando(" & tiempo & ")"
            ElseIf tiempo = 1 Then 'Cuando llega a uno actualizamos
                TabPage1.Text = "Histograma"
                TabPage2.Text = "Registro de cambios"
                Button1.Text = "Actualizar histograma"
                actualizarHistrograma()
                TabControl1_SelectedIndexChanged(sender, e)
                tiempo = 0 'Pasamos el tiempo a cero para que no siga descontando y no estre en esta sentencia de control
            End If
        End If
    End Sub
#End Region
#Region "Abrir histogramas"
    'Botón de mostrar todos los histográmas
    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        TodosHistogramas.Show()
    End Sub

    Private Sub Chart3_Click_1(sender As Object, e As EventArgs) Handles Chart3.Click
        'Hay que crear la instancia con constructor y el valor del color
        Dim frmHisto As New Histogramas(Color.Blue)
        frmHisto.Show()
    End Sub

    Private Sub Chart2_Click_1(sender As Object, e As EventArgs) Handles Chart2.Click
        'Hay que crear la instancia con constructor y el valor del color
        Dim frmHisto As New Histogramas(Color.Green)
        frmHisto.Show()

    End Sub

    Private Sub Chart1_Click_1(sender As Object, e As EventArgs) Handles Chart1.Click
        'Hay que crear la instancia con constructor y el valor del color
        Dim frmHisto As New Histogramas(Color.Red)
        frmHisto.Show()
    End Sub

    Private Sub Chart1_MouseEnter(sender As Object, e As EventArgs) Handles Chart1.MouseEnter
        Me.Cursor = Cursors.Hand
    End Sub

    Private Sub Chart1_MouseLeave(sender As Object, e As EventArgs) Handles Chart1.MouseLeave
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub Chart3_MouseEnter(sender As Object, e As EventArgs) Handles Chart3.MouseEnter
        Me.Cursor = Cursors.Hand
    End Sub

    Private Sub Chart3_MouseLeave(sender As Object, e As EventArgs) Handles Chart3.MouseLeave
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub Chart2_MouseEnter(sender As Object, e As EventArgs) Handles Chart2.MouseEnter
        Me.Cursor = Cursors.Hand
    End Sub

    Private Sub Chart2_MouseLeave(sender As Object, e As EventArgs) Handles Chart2.MouseLeave
        Me.Cursor = Cursors.Default
    End Sub

#End Region


#Region "Actualizar registro cambios tabcontrol"


    Sub RefrescarTab()
        Dim ancho As Integer = TabPage2.Width / 3 * 2.5
        For Each c As Control In TabPage2.Controls
            If TypeOf c Is PictureBox Then
                c.Width = ancho
            End If
        Next
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        Try
            If TabControl1.SelectedIndex = 1 Then
                TabPage2.Controls.Clear() 'Borramos todos los controles creados anteriormente
                Dim listaCompletaInfo = objetoTratamiento.ListadoTotalDeInfo
                Dim labelInfo(listaCompletaInfo.Count - 1) As Label
                Dim alto As Integer = 5

                For i = 0 To listaCompletaInfo.Count - 1 'Creamos los labls
                    'Labels
                    labelInfo(i) = New Label
                    TabPage2.Controls.Add(labelInfo(i))
                    labelInfo(i).Text = listaCompletaInfo(i)
                    labelInfo(i).Size = New Size(TabPage2.Width - 5, 12)
                    labelInfo(i).Location = New Size(5, alto)
                    labelInfo(i).Font = New Font("Segoe UI", 7, FontStyle.Bold)
                    alto += 115
                    '-----
                Next

                Dim listaCompletaImagenes = objetoTratamiento.ListadoTotalDeImagenes
                Dim picImagenes(listaCompletaImagenes.Count - 1) As PictureBox
                alto = 20
                Dim ancho As Integer = TabPage2.Width / 3 * 2.5
                For i = 0 To listaCompletaImagenes.Count - 1 'Creamos los picturebox
                    'Labels
                    picImagenes(i) = New PictureBox
                    TabPage2.Controls.Add(picImagenes(i))
                    picImagenes(i).Image = listaCompletaImagenes(i)
                    picImagenes(i).Size = New Size(ancho, 100)
                    picImagenes(i).Location = New Size(5, alto)
                    picImagenes(i).SizeMode = PictureBoxSizeMode.StretchImage
                    picImagenes(i).BorderStyle = BorderStyle.FixedSingle
                    alto += 115
                    '-----
                Next

                '-- Scroll Vertical
                Me.TabPage2.VerticalScroll.Visible = True
                Me.TabPage2.AutoScroll = True


                'Recorremos los picturebox del tabcontrol para crear el evento que gestione todo
                For Each c As Object In TabPage2.Controls
                    If c.GetType Is GetType(PictureBox) Then
                        AddHandler DirectCast(c, PictureBox).MouseEnter, AddressOf conFoco
                        AddHandler DirectCast(c, PictureBox).MouseLeave, AddressOf sinFoco
                        AddHandler DirectCast(c, PictureBox).MouseClick, AddressOf Pulsa
                    End If
                Next
            End If
        Catch
        End Try
    End Sub

    Private Sub conFoco(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.Cursor = Cursors.Hand
    End Sub


    Private Sub sinFoco(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub Pulsa(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim bmp As New Bitmap(DirectCast(sender, PictureBox).Image)
        Me.Cursor = Cursors.Default
        PictureBox1.Image = objetoTratamiento.ActualizarImagen(bmp) 'La imagen seleccionado la actualizamos
        'Actualizamos el Panel1
        Panel1.AutoScrollMinSize = PictureBox1.Image.Size
        Panel1.AutoScroll = True
        If HistogramasAutomáticos = False Then 'Si no se actualiza automáticamente (está deshabiltado), forzamos la actualización
            TabControl1_SelectedIndexChanged(sender, e)
        End If
    End Sub


#End Region

#Region "Actualizar imagen secundaria/ actualizar hacer y deshacer."
    'Realizamos esto cuando recibimos el evento
    Sub actualizarPicture(ByVal bmp As Bitmap)
        Try
            PictureBox1.Image = bmp
            PictureBox2.Image = bmp
            'ACtualizamos el nombre del menú hacer/rehacer
            Timer2.Enabled = True
            'Con esto actualizamos el histograma
            tiempo = 3 '3 segundos para actualización
            Timer3.Enabled = True
        Catch
        End Try
    End Sub

    'Actualizar deshacer/hacer
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        DeshacerToolStripMenuItem.Text = "Deshacer - " & objetoTratamiento.ListadoInfoAtras
        RehacerToolStripMenuItem.Text = "Rehacer - " & objetoTratamiento.ListadoInfoAdelante
        ImagenOriginalToolStripMenuItem.Text = objetoTratamiento.imagenOriginalInfo
    End Sub


    'FIN de actualizar imagen secundaria
#End Region

#Region "Actualizar nombre imagen"
    'Realizamos esto cuando recibimos el evento
    Sub actualizarNombrePicture(ByVal nombre() As String)
        Me.Text = "[" & nombre(0) & "]  " & "(" & nombre(1) & " x " & nombre(2) & ")   " & nombre(3)
        Try 'Actualizamos panel
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        Catch ex As Exception

        End Try
    End Sub
    'FIN de actualizar imagen secundaria
#End Region



#Region "DRAG&DROP"



    Private Sub PictureBox1_DragEnter(sender As Object, e As DragEventArgs) Handles PictureBox1.DragEnter
        'DataFormats.FileDrop nos devuelve el array de rutas de archivos
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            'Los archivos son externos a nuestra aplicación por lo que de indicaremos All ya que dará lo mismo.
            e.Effect = DragDropEffects.All
        End If
    End Sub
    Private Sub PictureBox1_DragDrop(sender As Object, e As DragEventArgs) Handles PictureBox1.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim rutaImagen As String
            'Asignamos la primera posición del array de ruta de archivos a la variable de tipo string
            'declarada anteriormente ya que en este caso sólo mostraremos una imagen en el control.
            rutaImagen = e.Data.GetData(DataFormats.FileDrop)(0)
            'La cargamos al control
            Dim bmp As Bitmap
            bmp = objetoTratamiento.abrirDragDrop(rutaImagen)
            If bmp IsNot Nothing Then
                PictureBox1.Image = bmp
            End If
        End If
    End Sub
#End Region



#Region "Adaptar panel secundario y formuPrincipal"
    Private Sub SplitContainer1_SplitterMoved(sender As Object, e As SplitterEventArgs) Handles SplitContainer1.SplitterMoved
        PictureBox2.Width = SplitContainer1.Panel2.Width - 5 'Imagen general
        Chart1.Width = SplitContainer1.Panel2.Width 'Chart-->histogramas
        Chart2.Width = SplitContainer1.Panel2.Width
        Chart3.Width = SplitContainer1.Panel2.Width
        Button1.Width = SplitContainer1.Panel2.Width - 20 'Botón de actualizar histograma
        Button2.Width = SplitContainer1.Panel2.Width - 20 'Botón de actualizar histograma
        'Adaptamos label --> imagen general
        Label1.Location = New Size((SplitContainer1.Panel2.Width / 2) - (Label1.Width / 2), PictureBox2.Location.Y - 20)
        'Adaptamos el tabcontrol
        TabControl1.Size = New Size(SplitContainer1.Panel2.Width, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + 30))
        RefrescarTab() 'Actualizamos el registro de cambios
    End Sub
    Private Sub Principal_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        'Colocamos la imagen secundaria en la parte inferior
        PictureBox2.Location = New Size(PictureBox2.Location.X, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + 5))
        'Colocamos label imagen general
        Label1.Location = New Size((SplitContainer1.Panel2.Width / 2) - (Label1.Width / 2), PictureBox2.Location.Y - 20)
        'Colocamos los chart y el botón
        Chart1.Location = New Size(-7, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + Chart1.Size.Height + 100))
        Chart2.Location = New Size(-7, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + (Chart1.Size.Height * 2) + 100))
        Chart3.Location = New Size(-7, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + (Chart1.Size.Height * 3) + 100))
        Button1.Location = New Size((TabControl1.Width / 2) - (Button1.Width / 2) - 3, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + 102))
        'Botón de todos los histogramas
        Button2.Location = New Size((TabControl1.Width / 2) - (Button2.Width / 2) - 3, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + 130) - ((Chart1.Size.Height * 3)))
        'Adaptamos el tabcontrol
        TabControl1.Size = New Size(SplitContainer1.Panel2.Width, SplitContainer1.Panel2.Height - (PictureBox2.Size.Height + 30))

    End Sub
#End Region

#Region "Barra de herramientas con imágenes"
    'ABrir imagen
    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Dim bmp As Bitmap
        bmp = objetoTratamiento.abrirImagen()
        If bmp IsNot Nothing Then
            PictureBox1.Image = bmp
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub
    'ABrir imagen (como recurso)
    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        AbrirRecurso.Show()
    End Sub
    'Guardar como...
    Private Sub ToolStripButton8_Click(sender As Object, e As EventArgs) Handles ToolStripButton8.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        objetoTratamiento.guardarcomo(bmp, 4)
    End Sub
    'ABrir imagen desde bing
    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        AbrirBing.Show()
    End Sub
    'ABrir imagen desde facebook
    Private Sub ToolStripButton4_Click(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        AbrirFacebook.Show()
    End Sub
    'Deshacer
    Private Sub ToolStripButton5_Click(sender As Object, e As EventArgs) Handles ToolStripButton5.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            PictureBox1.Image = objetoTratamiento.ListadoImagenesAtras
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox2.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub
    'Rehacer
    Private Sub ToolStripButton6_Click_1(sender As Object, e As EventArgs) Handles ToolStripButton6.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            PictureBox1.Image = objetoTratamiento.ListadoImagenesAdelante
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox2.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub
    'Refrescar
    Private Sub ToolStripButton7_Click(sender As Object, e As EventArgs) Handles ToolStripButton7.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub
    'Actualizar
    Private Sub ToolStripButton9_Click(sender As Object, e As EventArgs) Handles ToolStripButton9.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        Me.PictureBox1.Image = objetoTratamiento.ActualizarImagen(bmp)
        'Actualizamos el Panel1
        Panel1.AutoScrollMinSize = PictureBox1.Image.Size
        Panel1.AutoScroll = True
    End Sub
    'Blanco y negro
    Private Sub ToolStripButton10_Click(sender As Object, e As EventArgs) Handles ToolStripButton10.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "blancoNegro"
        transformar()
    End Sub
    'Escala de grises
    Private Sub ToolStripButton11_Click(sender As Object, e As EventArgs) Handles ToolStripButton11.Click
        Dim bmp As New Bitmap(PictureBox1.Image)
        transformacion = "escalaGrises"
        transformar()
    End Sub
#End Region

#Region "ContextMenuStrip (Botón derecho)"

    Private Sub AbrirImagenToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles AbrirImagenToolStripMenuItem3.Click
        Dim bmp As Bitmap
        bmp = objetoTratamiento.abrirImagen()
        If bmp IsNot Nothing Then
            PictureBox1.Image = bmp
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    Private Sub GuardarImagenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuardarImagenToolStripMenuItem.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        objetoTratamiento.guardarcomo(bmp, 4)
    End Sub

    Private Sub RefrescarToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles RefrescarToolStripMenuItem2.Click
        If BackgroundWorker1.IsBusy = False Then 'Si el hilo no está en uso
            'Actualizamos el Panel1
            Panel1.AutoScrollMinSize = PictureBox1.Image.Size
            Panel1.AutoScroll = True
        End If
    End Sub

    Private Sub ActualizarToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ActualizarToolStripMenuItem2.Click
        Dim bmp As New Bitmap(Me.PictureBox1.Image)
        Me.PictureBox1.Image = objetoTratamiento.ActualizarImagen(bmp)
        'Actualizamos el Panel1
        Panel1.AutoScrollMinSize = PictureBox1.Image.Size
        Panel1.AutoScroll = True
    End Sub
#End Region


#Region "Posición puntero en Picturebox//Color picturebox"

    'Calculamos la posición del puntero dentro del picturebox
    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove

        Dim dpiH, dpiV As Integer 'Puntos por pulgada
        Dim Resolucion As Size 'Resolución de pantalla
        Dim gr As Graphics
        gr = Me.CreateGraphics
        dpiH = gr.DpiX 'Puntos por pulgada
        dpiV = gr.DpiY
        Resolucion = System.Windows.Forms.SystemInformation.PrimaryMonitorSize 'Resolución de pantalla
        Dim ResHeight As Integer = Resolucion.Height
        Dim ResWidth As Integer = Resolucion.Width


        Dim mouseDownLocation As New Point(e.X, e.Y) 'Situación del puntero

        If PíxelesToolStripMenuItem.Checked = True Then 'Si están selecionado píxeles
            ToolStripStatusLabel2.Text = "(" & mouseDownLocation.X & "," & mouseDownLocation.Y & ") px"
        ElseIf CentímetrosToolStripMenuItem.Checked = True Then 'Centímetros
            ToolStripStatusLabel2.Text = "(" & FormatNumber((mouseDownLocation.X / dpiH) * 2.54, 2) & "," & FormatNumber((mouseDownLocation.Y / dpiV) * 2.54, 2) & ") cm"
        ElseIf MilímetrosToolStripMenuItem.Checked = True Then 'Centímetros
            ToolStripStatusLabel2.Text = "(" & FormatNumber((mouseDownLocation.X / dpiH) * 254, 0) & "," & FormatNumber((mouseDownLocation.Y / dpiV) * 254, 0) & ") mm"
        ElseIf PulgadasToolStripMenuItem.Checked = True Then 'Pulgadas
            ToolStripStatusLabel2.Text = "(" & FormatNumber((mouseDownLocation.X / dpiH), 2) & "," & FormatNumber((mouseDownLocation.Y / dpiV), 2) & ") in"
        End If

    End Sub

    Sub quitarCheck()
        PulgadasToolStripMenuItem.Checked = False
        MilímetrosToolStripMenuItem.Checked = False
        PíxelesToolStripMenuItem.Checked = False
        CentímetrosToolStripMenuItem.Checked = False
    End Sub

    Private Sub PulgadasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PulgadasToolStripMenuItem.Click
        quitarCheck()
        PulgadasToolStripMenuItem.Checked = True
    End Sub

    Private Sub PíxelesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PíxelesToolStripMenuItem.Click
        quitarCheck()
        PíxelesToolStripMenuItem.Checked = True
    End Sub

    Private Sub CentímetrosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CentímetrosToolStripMenuItem.Click
        quitarCheck()
        CentímetrosToolStripMenuItem.Checked = True
    End Sub

    Private Sub MilímetrosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MilímetrosToolStripMenuItem.Click
        quitarCheck()
        MilímetrosToolStripMenuItem.Checked = True
    End Sub

    '-------------------------------------
    'Extraemos el color
    Private Sub PictureBox1_MouseClick(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseClick
        If ModifierKeys = Keys.Control Then 'Si pulsa control al hacer clic
            Dim bmp As Bitmap
            bmp = PictureBox1.Image
            Dim rojo, verde, azul, alfa As Byte
            Try
                'Obetentemos color
                rojo = bmp.GetPixel(e.X, e.Y).R
                verde = bmp.GetPixel(e.X, e.Y).G
                azul = bmp.GetPixel(e.X, e.Y).B
                alfa = bmp.GetPixel(e.X, e.Y).A
                'Creamos un bitmap para ponerlo como imagen (con el color obtenido)
                Dim bmpAux As New Bitmap(16, 16)
                For i = 0 To 15
                    For j = 0 To 15
                        bmpAux.SetPixel(i, j, Color.FromArgb(alfa, rojo, verde, azul))
                    Next
                Next
                'Asignamos la imagen
                ToolStripStatusImagen.Image = bmpAux
                'Escribimos los valores
                ToolStripStatusColor.Text = bmp.GetPixel(e.X, e.Y).ToString()
            Catch ex As Exception
            End Try
        End If
    End Sub

#End Region


End Class
