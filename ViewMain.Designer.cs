namespace DesktopWeightViewer
{
    partial class ViewMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewMain));
            txtTrama = new TextBox();
            statusStrip1 = new StatusStrip();
            toolStripSplitButton1 = new ToolStripDropDownButton();
            toolStripTextBox2 = new ToolStripMenuItem();
            txtUsuario = new ToolStripTextBox();
            toolStripMenuItem1 = new ToolStripMenuItem();
            txtContrasena = new ToolStripTextBox();
            btnIngresar = new ToolStripMenuItem();
            menuConfiguracion = new ToolStripDropDownButton();
            toolStripMenuItem3 = new ToolStripMenuItem();
            cbxTramas = new ToolStripComboBox();
            btnAbrirTrama = new ToolStripMenuItem();
            btnCerrarTrama = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            cbxComBalanza = new ToolStripComboBox();
            abrirBalanza = new ToolStripMenuItem();
            cerrarBalanza = new ToolStripMenuItem();
            btnGuardarConfiguracion = new ToolStripMenuItem();
            timer1 = new System.Windows.Forms.Timer(components);
            timer2 = new System.Windows.Forms.Timer(components);
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // txtTrama
            // 
            txtTrama.BackColor = SystemColors.MenuText;
            txtTrama.BorderStyle = BorderStyle.None;
            txtTrama.Font = new Font("Segoe UI Semibold", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtTrama.ForeColor = Color.OrangeRed;
            txtTrama.Location = new Point(0, -3);
            txtTrama.Name = "txtTrama";
            txtTrama.Size = new Size(256, 86);
            txtTrama.TabIndex = 0;
            txtTrama.TextAlign = HorizontalAlignment.Center;
            txtTrama.Enter += txtTrama_Enter;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = SystemColors.InfoText;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripSplitButton1, menuConfiguracion });
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripSplitButton1
            // 
            toolStripSplitButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripSplitButton1.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox2, toolStripMenuItem1, btnIngresar });
            toolStripSplitButton1.Image = (Image)resources.GetObject("toolStripSplitButton1.Image");
            toolStripSplitButton1.ImageTransparentColor = Color.Magenta;
            toolStripSplitButton1.Name = "toolStripSplitButton1";
            toolStripSplitButton1.Size = new Size(50, 20);
            toolStripSplitButton1.Text = "Login";
            toolStripSplitButton1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // toolStripTextBox2
            // 
            toolStripTextBox2.DropDownItems.AddRange(new ToolStripItem[] { txtUsuario });
            toolStripTextBox2.Name = "toolStripTextBox2";
            toolStripTextBox2.Size = new Size(180, 22);
            toolStripTextBox2.Text = "Usuario";
            // 
            // txtUsuario
            // 
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(100, 23);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { txtContrasena });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(180, 22);
            toolStripMenuItem1.Text = "Contraseña";
            // 
            // txtContrasena
            // 
            txtContrasena.Name = "txtContrasena";
            txtContrasena.Size = new Size(100, 23);
            txtContrasena.TextChanged += txtContrasena_TextChanged;
            // 
            // btnIngresar
            // 
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(180, 22);
            btnIngresar.Text = "Ingresar";
            btnIngresar.Click += btnIngresar_Click;
            // 
            // menuConfiguracion
            // 
            menuConfiguracion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuConfiguracion.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem3, toolStripMenuItem2, btnGuardarConfiguracion });
            menuConfiguracion.Image = (Image)resources.GetObject("menuConfiguracion.Image");
            menuConfiguracion.ImageScaling = ToolStripItemImageScaling.None;
            menuConfiguracion.ImageTransparentColor = Color.Magenta;
            menuConfiguracion.Name = "menuConfiguracion";
            menuConfiguracion.Size = new Size(96, 20);
            menuConfiguracion.Text = "Configuración";
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] { cbxTramas, btnAbrirTrama, btnCerrarTrama });
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(195, 22);
            toolStripMenuItem3.Text = "Trama";
            // 
            // cbxTramas
            // 
            cbxTramas.Name = "cbxTramas";
            cbxTramas.Size = new Size(121, 23);
            // 
            // btnAbrirTrama
            // 
            btnAbrirTrama.Name = "btnAbrirTrama";
            btnAbrirTrama.Size = new Size(181, 22);
            btnAbrirTrama.Text = "Abrir";
            btnAbrirTrama.Click += btnAbrirTrama_Click;
            // 
            // btnCerrarTrama
            // 
            btnCerrarTrama.Name = "btnCerrarTrama";
            btnCerrarTrama.Size = new Size(181, 22);
            btnCerrarTrama.Text = "Cerrar";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { cbxComBalanza, abrirBalanza, cerrarBalanza });
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(195, 22);
            toolStripMenuItem2.Text = "Balanza";
            // 
            // cbxComBalanza
            // 
            cbxComBalanza.Name = "cbxComBalanza";
            cbxComBalanza.Size = new Size(121, 23);
            // 
            // abrirBalanza
            // 
            abrirBalanza.Name = "abrirBalanza";
            abrirBalanza.Size = new Size(181, 22);
            abrirBalanza.Text = "Abrir";
            abrirBalanza.Click += abrirBalanza_Click;
            // 
            // cerrarBalanza
            // 
            cerrarBalanza.Name = "cerrarBalanza";
            cerrarBalanza.Size = new Size(181, 22);
            cerrarBalanza.Text = "Cerrar";
            // 
            // btnGuardarConfiguracion
            // 
            btnGuardarConfiguracion.Name = "btnGuardarConfiguracion";
            btnGuardarConfiguracion.Size = new Size(195, 22);
            btnGuardarConfiguracion.Text = "Guardar Configuración";
            btnGuardarConfiguracion.Click += btnGuardarConfiguracion_Click;
            // 
            // ViewMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.MenuText;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Controls.Add(statusStrip1);
            Controls.Add(txtTrama);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ViewMain";
            Text = "PRINCIPAL";
            Load += ViewMain_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTrama;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuItemLogin;
        private ToolStripMenuItem toolStripTextBox1;
        private ToolStripTextBox tstbUsuario;
        private ToolStripTextBox tstbContrasena;
        //private ToolStripTextBox btnIngresar;
        private ToolStripMenuItem menuItemConfiguracion;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripTextBox abrirTrama;
        private ToolStripTextBox cerrarTrama;
        private StatusStrip statusStrip1;
        private ToolStripTextBox toolStripTextBox3;
        private ToolStripDropDownButton menuConfiguracion;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripComboBox cbxTramas;
        private ToolStripMenuItem btnAbrirTrama;
        private ToolStripMenuItem btnCerrarTrama;
        private ToolStripMenuItem btnGuardarConfiguracion;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripComboBox cbxComBalanza;
        private ToolStripMenuItem abrirBalanza;
        private ToolStripMenuItem cerrarBalanza;
        private ToolStripDropDownButton toolStripSplitButton1;
        private ToolStripMenuItem toolStripTextBox2;
        private ToolStripTextBox txtUsuario;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripTextBox txtContrasena;
        private ToolStripMenuItem btnIngresar;
    }
}
