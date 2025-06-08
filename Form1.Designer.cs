namespace RipMD
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            TxtDesCap = new TextBox();
            BtnDesCap = new Button();
            TxtMangas = new TextBox();
            label2 = new Label();
            TxtCapInicio = new TextBox();
            label3 = new Label();
            label4 = new Label();
            TxtCapFinal = new TextBox();
            PbrCapitulo = new ProgressBar();
            PbrManga = new ProgressBar();
            BtnCookies = new Button();
            PnlWeb = new Panel();
            label5 = new Label();
            TxtCola = new TextBox();
            label6 = new Label();
            LblCapDescar = new Label();
            PbrCola = new ProgressBar();
            BtnAbrirDes = new Button();
            BtnDesMan = new Button();
            BtnDesCola = new Button();
            BtnOrdenarCaps = new Button();
            SuspendLayout();
            // 
            // BtnDesMan
            // 
            BtnDesMan.Location = new Point(570, 119);
            BtnDesMan.Name = "BtnDesMan";
            BtnDesMan.Size = new Size(94, 29);
            BtnDesMan.TabIndex = 6;
            BtnDesMan.Text = "Descargar";
            BtnDesMan.UseVisualStyleBackColor = true;
            BtnDesMan.Click += BtnDesMan_Click;
            // 
            // BtnDesCola
            // 
            BtnDesCola.Location = new Point(570, 599);
            BtnDesCola.Name = "BtnDesCola";
            BtnDesCola.Size = new Size(94, 29);
            BtnDesCola.TabIndex = 19;
            BtnDesCola.Text = "Descargar";
            BtnDesCola.UseVisualStyleBackColor = true;
            BtnDesCola.Click += BtnDesCola_Click;
            // 
            // BtnOrdenarCaps
            // 
            BtnOrdenarCaps.Location = new Point(673, 599);
            BtnOrdenarCaps.Name = "BtnOrdenarCaps";
            BtnOrdenarCaps.Size = new Size(138, 29);
            BtnOrdenarCaps.TabIndex = 22;
            BtnOrdenarCaps.Text = "Ordenar Capítulos";
            BtnOrdenarCaps.UseVisualStyleBackColor = true;
            BtnOrdenarCaps.Click += BtnOrdenarCaps_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(136, 20);
            label1.TabIndex = 0;
            label1.Text = "Descargar Capítulo";
            // 
            // TxtDesCap
            // 
            TxtDesCap.Location = new Point(12, 32);
            TxtDesCap.Name = "TxtDesCap";
            TxtDesCap.Size = new Size(552, 27);
            TxtDesCap.TabIndex = 2;
            // 
            // BtnDesCap
            // 
            BtnDesCap.Location = new Point(570, 30);
            BtnDesCap.Name = "BtnDesCap";
            BtnDesCap.Size = new Size(94, 29);
            BtnDesCap.TabIndex = 3;
            BtnDesCap.Text = "Descargar";
            BtnDesCap.UseVisualStyleBackColor = true;
            BtnDesCap.Click += BtnDesCap_Click;
            // 
            // TxtMangas
            // 
            TxtMangas.Location = new Point(12, 121);
            TxtMangas.Name = "TxtMangas";
            TxtMangas.Size = new Size(552, 27);
            TxtMangas.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 98);
            label2.Name = "label2";
            label2.Size = new Size(142, 20);
            label2.TabIndex = 4;
            label2.Text = "Descargar Capítulos";
            // 
            // TxtCapInicio
            // 
            TxtCapInicio.Location = new Point(12, 180);
            TxtCapInicio.Name = "TxtCapInicio";
            TxtCapInicio.Size = new Size(55, 27);
            TxtCapInicio.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 157);
            label3.Name = "label3";
            label3.Size = new Size(48, 20);
            label3.TabIndex = 8;
            label3.Text = "Inicio:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(99, 157);
            label4.Name = "label4";
            label4.Size = new Size(43, 20);
            label4.TabIndex = 10;
            label4.Text = "Final:";
            // 
            // TxtCapFinal
            // 
            TxtCapFinal.Location = new Point(99, 180);
            TxtCapFinal.Name = "TxtCapFinal";
            TxtCapFinal.Size = new Size(55, 27);
            TxtCapFinal.TabIndex = 9;
            // 
            // PbrCapitulo
            // 
            PbrCapitulo.Location = new Point(160, 66);
            PbrCapitulo.Name = "PbrCapitulo";
            PbrCapitulo.Size = new Size(504, 29);
            PbrCapitulo.TabIndex = 11;
            // 
            // PbrManga
            // 
            PbrManga.Location = new Point(160, 154);
            PbrManga.Name = "PbrManga";
            PbrManga.Size = new Size(504, 29);
            PbrManga.TabIndex = 12;
            // 
            // BtnCookies
            // 
            BtnCookies.Location = new Point(864, 9);
            BtnCookies.Name = "BtnCookies";
            BtnCookies.Size = new Size(185, 47);
            BtnCookies.TabIndex = 13;
            BtnCookies.Text = "Recargar Cookies";
            BtnCookies.UseVisualStyleBackColor = true;
            BtnCookies.Click += BtnCookies_Click;
            // 
            // PnlWeb
            // 
            PnlWeb.Enabled = false;
            PnlWeb.Location = new Point(673, 58);
            PnlWeb.Name = "PnlWeb";
            PnlWeb.Size = new Size(376, 535);
            PnlWeb.TabIndex = 14;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 218);
            label5.Name = "label5";
            label5.Size = new Size(209, 20);
            label5.TabIndex = 16;
            label5.Text = "Cola de mangas y/o capítulos:";
            // 
            // TxtCola
            // 
            TxtCola.Location = new Point(12, 241);
            TxtCola.Multiline = true;
            TxtCola.Name = "TxtCola";
            TxtCola.Size = new Size(652, 352);
            TxtCola.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(160, 187);
            label6.Name = "label6";
            label6.Size = new Size(172, 20);
            label6.TabIndex = 17;
            label6.Text = "Capítulo descargándose:";
            // 
            // LblCapDescar
            // 
            LblCapDescar.AutoSize = true;
            LblCapDescar.Location = new Point(338, 186);
            LblCapDescar.MaximumSize = new Size(320, 50);
            LblCapDescar.Name = "LblCapDescar";
            LblCapDescar.Size = new Size(0, 20);
            LblCapDescar.TabIndex = 18;
            // 
            // PbrCola
            // 
            PbrCola.Location = new Point(12, 599);
            PbrCola.Name = "PbrCola";
            PbrCola.Size = new Size(552, 29);
            PbrCola.TabIndex = 20;
            // 
            // BtnAbrirDes
            // 
            BtnAbrirDes.Location = new Point(673, 9);
            BtnAbrirDes.Name = "BtnAbrirDes";
            BtnAbrirDes.Size = new Size(185, 47);
            BtnAbrirDes.TabIndex = 21;
            BtnAbrirDes.Text = "Abrir Descargas";
            BtnAbrirDes.UseVisualStyleBackColor = true;
            BtnAbrirDes.Click += BtnAbrirDes_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1054, 659);
            Controls.Add(BtnOrdenarCaps);
            Controls.Add(BtnAbrirDes);
            Controls.Add(PbrCola);
            Controls.Add(BtnDesCola);
            Controls.Add(LblCapDescar);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(TxtCola);
            Controls.Add(PnlWeb);
            Controls.Add(BtnCookies);
            Controls.Add(PbrManga);
            Controls.Add(PbrCapitulo);
            Controls.Add(label4);
            Controls.Add(TxtCapFinal);
            Controls.Add(label3);
            Controls.Add(TxtCapInicio);
            Controls.Add(BtnDesMan);
            Controls.Add(TxtMangas);
            Controls.Add(label2);
            Controls.Add(BtnDesCap);
            Controls.Add(TxtDesCap);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "RipMD";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TxtDesCap;
        private Button BtnDesCap;
        private Button BtnDesMan;
        private TextBox TxtMangas;
        private Label label2;
        private TextBox TxtCapInicio;
        private Label label3;
        private Label label4;
        private TextBox TxtCapFinal;
        private ProgressBar PbrCapitulo;
        private ProgressBar PbrManga;
        private Button BtnCookies;
        private Panel PnlWeb;
        private Label label5;
        private TextBox TxtCola;
        private Label label6;
        private Label LblCapDescar;
        private ProgressBar PbrCola;
        private Button BtnAbrirDes;
        private Button BtnDesCola;
        private Button BtnOrdenarCaps;
    }
}
