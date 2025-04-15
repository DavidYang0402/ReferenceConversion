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
            groupBox1 = new GroupBox();
            Lb_Allowlist = new ListBox();
            Cbx_Project_Allowlist = new ComboBox();
            groupBox2 = new GroupBox();
            Btn_ToReference = new Button();
            Btn_ToProjectReference = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // Btn_GetFolder
            // 
            Btn_GetFolder.Location = new Point(19, 21);
            Btn_GetFolder.Name = "Btn_GetFolder";
            Btn_GetFolder.Size = new Size(85, 29);
            Btn_GetFolder.TabIndex = 0;
            Btn_GetFolder.Text = "選取資料夾";
            Btn_GetFolder.UseVisualStyleBackColor = true;
            Btn_GetFolder.Click += Btn_GetFolder_Click;
            // 
            // Tb_ShowPath
            // 
            Tb_ShowPath.Location = new Point(110, 25);
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
            Lb_ShowAllCsproj.Size = new Size(522, 244);
            Lb_ShowAllCsproj.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Lb_Allowlist);
            groupBox1.Controls.Add(Cbx_Project_Allowlist);
            groupBox1.Location = new Point(19, 66);
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
            groupBox2.Location = new Point(319, 66);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(5);
            groupBox2.Size = new Size(538, 279);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "目前所有 csproj 檔案";
            // 
            // Btn_ToReference
            // 
            Btn_ToReference.Location = new Point(319, 351);
            Btn_ToReference.Name = "Btn_ToReference";
            Btn_ToReference.Size = new Size(97, 30);
            Btn_ToReference.TabIndex = 8;
            Btn_ToReference.Text = "轉成檔案參考";
            Btn_ToReference.UseVisualStyleBackColor = true;
            Btn_ToReference.Click += Btn_ToReference_Click;
            // 
            // Btn_ToProjectReference
            // 
            Btn_ToProjectReference.Location = new Point(422, 351);
            Btn_ToProjectReference.Name = "Btn_ToProjectReference";
            Btn_ToProjectReference.Size = new Size(96, 30);
            Btn_ToProjectReference.TabIndex = 9;
            Btn_ToProjectReference.Text = "轉成專案參考";
            Btn_ToProjectReference.UseVisualStyleBackColor = true;
            Btn_ToProjectReference.Click += Btn_ToProjectReference_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(881, 402);
            Controls.Add(Btn_ToProjectReference);
            Controls.Add(Btn_ToReference);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Btn_GetFolder;
        private TextBox Tb_ShowPath;
        private ListBox Lb_ShowAllCsproj;
        private GroupBox groupBox1;
        private ListBox Lb_Allowlist;
        private ComboBox Cbx_Project_Allowlist;
        private GroupBox groupBox2;
        private Button Btn_ToReference;
        private Button Btn_ToProjectReference;
    }
}
