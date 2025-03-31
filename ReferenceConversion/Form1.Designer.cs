namespace ReferenceConversion
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
            Btn_GetFolder = new Button();
            Tb_ShowPath = new TextBox();
            Lb_ShowAllCsproj = new ListBox();
            Tb_SaveFolder = new TextBox();
            Btn_Convert = new Button();
            label1 = new Label();
            groupBox1 = new GroupBox();
            Lb_Allowlist = new ListBox();
            Cbx_Project_Allowlist = new ComboBox();
            groupBox2 = new GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // Btn_GetFolder
            // 
            Btn_GetFolder.Location = new Point(44, 27);
            Btn_GetFolder.Name = "Btn_GetFolder";
            Btn_GetFolder.Size = new Size(85, 29);
            Btn_GetFolder.TabIndex = 0;
            Btn_GetFolder.Text = "選取資料夾";
            Btn_GetFolder.UseVisualStyleBackColor = true;
            Btn_GetFolder.Click += Btn_GetFolder_Click;
            // 
            // Tb_ShowPath
            // 
            Tb_ShowPath.Location = new Point(135, 31);
            Tb_ShowPath.Name = "Tb_ShowPath";
            Tb_ShowPath.Size = new Size(433, 23);
            Tb_ShowPath.TabIndex = 1;
            // 
            // Lb_ShowAllCsproj
            // 
            Lb_ShowAllCsproj.FormattingEnabled = true;
            Lb_ShowAllCsproj.HorizontalScrollbar = true;
            Lb_ShowAllCsproj.ItemHeight = 15;
            Lb_ShowAllCsproj.Location = new Point(8, 22);
            Lb_ShowAllCsproj.Name = "Lb_ShowAllCsproj";
            Lb_ShowAllCsproj.Size = new Size(522, 214);
            Lb_ShowAllCsproj.TabIndex = 2;
            // 
            // Tb_SaveFolder
            // 
            Tb_SaveFolder.Location = new Point(181, 248);
            Tb_SaveFolder.Name = "Tb_SaveFolder";
            Tb_SaveFolder.Size = new Size(349, 23);
            Tb_SaveFolder.TabIndex = 3;
            // 
            // Btn_Convert
            // 
            Btn_Convert.Location = new Point(344, 357);
            Btn_Convert.Name = "Btn_Convert";
            Btn_Convert.Size = new Size(70, 25);
            Btn_Convert.TabIndex = 4;
            Btn_Convert.Text = "Convert";
            Btn_Convert.UseVisualStyleBackColor = true;
            Btn_Convert.Click += Btn_Convert_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 253);
            label1.Name = "label1";
            label1.Size = new Size(169, 15);
            label1.TabIndex = 5;
            label1.Text = "Project Reference 指定資料夾";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Lb_Allowlist);
            groupBox1.Controls.Add(Cbx_Project_Allowlist);
            groupBox1.Location = new Point(44, 72);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5);
            groupBox1.Size = new Size(275, 279);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "專案 白名單";
            // 
            // Lb_Allowlist
            // 
            Lb_Allowlist.FormattingEnabled = true;
            Lb_Allowlist.ItemHeight = 15;
            Lb_Allowlist.Location = new Point(8, 51);
            Lb_Allowlist.Name = "Lb_Allowlist";
            Lb_Allowlist.Size = new Size(259, 214);
            Lb_Allowlist.TabIndex = 8;
            // 
            // Cbx_Project_Allowlist
            // 
            Cbx_Project_Allowlist.FormattingEnabled = true;
            Cbx_Project_Allowlist.Location = new Point(6, 19);
            Cbx_Project_Allowlist.Name = "Cbx_Project_Allowlist";
            Cbx_Project_Allowlist.Size = new Size(263, 23);
            Cbx_Project_Allowlist.TabIndex = 7;
            Cbx_Project_Allowlist.Text = "請選取白名單專案";
            Cbx_Project_Allowlist.SelectedIndexChanged += Cbx_Project_Allowlist_SelectedIndexChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(Lb_ShowAllCsproj);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(Tb_SaveFolder);
            groupBox2.Location = new Point(344, 72);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(5);
            groupBox2.Size = new Size(538, 279);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "目前所有 csproj 檔案";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(936, 413);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(Btn_Convert);
            Controls.Add(Tb_ShowPath);
            Controls.Add(Btn_GetFolder);
            MaximumSize = new Size(1000, 1000);
            MinimumSize = new Size(635, 431);
            Name = "Form1";
            Padding = new Padding(5);
            Text = "Reference Convert";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Btn_GetFolder;
        private TextBox Tb_ShowPath;
        private ListBox Lb_ShowAllCsproj;
        private TextBox Tb_SaveFolder;
        private Button Btn_Convert;
        private Label label1;
        private GroupBox groupBox1;
        private ListBox Lb_Allowlist;
        private ComboBox Cbx_Project_Allowlist;
        private GroupBox groupBox2;
    }
}
