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
            textBox1 = new TextBox();
            menuStrip1 = new MenuStrip();
            menuItemLogin = new ToolStripMenuItem();
            toolStripTextBox1 = new ToolStripMenuItem();
            tstbUsuario = new ToolStripTextBox();
            toolStripMenuItem2 = new ToolStripMenuItem();
            tstbContrasena = new ToolStripTextBox();
            btnIngresar = new ToolStripTextBox();
            menuItemConfiguracion = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            cbxTrama = new ToolStripComboBox();
            abrirTrama = new ToolStripTextBox();
            cerrarTrama = new ToolStripTextBox();
            btnGuardarConfiguracion = new ToolStripTextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI Semibold", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(33, 53);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(511, 93);
            textBox1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuItemLogin, menuItemConfiguracion });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuItemLogin
            // 
            menuItemLogin.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox1, toolStripMenuItem2, btnIngresar });
            menuItemLogin.Name = "menuItemLogin";
            menuItemLogin.Size = new Size(49, 20);
            menuItemLogin.Text = "Login";
            // 
            // toolStripTextBox1
            // 
            toolStripTextBox1.DropDownItems.AddRange(new ToolStripItem[] { tstbUsuario });
            toolStripTextBox1.Name = "toolStripTextBox1";
            toolStripTextBox1.Size = new Size(180, 22);
            toolStripTextBox1.Text = "Usuario";
            // 
            // tstbUsuario
            // 
            tstbUsuario.Name = "tstbUsuario";
            tstbUsuario.Size = new Size(100, 23);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { tstbContrasena });
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(180, 22);
            toolStripMenuItem2.Text = "Contraseña";
            // 
            // tstbContrasena
            // 
            tstbContrasena.Name = "tstbContrasena";
            tstbContrasena.Size = new Size(100, 23);
            // 
            // btnIngresar
            // 
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(100, 23);
            btnIngresar.Text = "Ingresar";
            // 
            // menuItemConfiguracion
            // 
            menuItemConfiguracion.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem4, btnGuardarConfiguracion });
            menuItemConfiguracion.Name = "menuItemConfiguracion";
            menuItemConfiguracion.Size = new Size(95, 20);
            menuItemConfiguracion.Text = "Configuración";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.DropDownItems.AddRange(new ToolStripItem[] { cbxTrama, abrirTrama, cerrarTrama });
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(210, 22);
            toolStripMenuItem4.Text = "Trama";
            // 
            // cbxTrama
            // 
            cbxTrama.Name = "cbxTrama";
            cbxTrama.Size = new Size(121, 23);
            // 
            // abrirTrama
            // 
            abrirTrama.Name = "abrirTrama";
            abrirTrama.Size = new Size(100, 23);
            abrirTrama.Text = "Abrir";
            // 
            // cerrarTrama
            // 
            cerrarTrama.Name = "cerrarTrama";
            cerrarTrama.Size = new Size(100, 23);
            cerrarTrama.Text = "Cerrar";
            // 
            // btnGuardarConfiguracion
            // 
            btnGuardarConfiguracion.Name = "btnGuardarConfiguracion";
            btnGuardarConfiguracion.Size = new Size(150, 23);
            btnGuardarConfiguracion.Text = "Guardar configuración";
            // 
            // ViewMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "ViewMain";
            Text = "PRINCIPAL";
            Load += ViewMain_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuItemLogin;
        private ToolStripMenuItem toolStripTextBox1;
        private ToolStripTextBox tstbUsuario;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripTextBox tstbContrasena;
        private ToolStripTextBox btnIngresar;
        private ToolStripMenuItem menuItemConfiguracion;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripComboBox cbxTrama;
        private ToolStripTextBox abrirTrama;
        private ToolStripTextBox cerrarTrama;
        private ToolStripTextBox btnGuardarConfiguracion;
    }
}
