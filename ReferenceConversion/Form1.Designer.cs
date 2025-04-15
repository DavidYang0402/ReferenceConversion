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
            Cbx_solution_file = new ComboBox();
            label2 = new Label();
            Btn_ToReference = new Button();
            Btn_ToProjectReference = new Button();
            label1 = new Label();
            Tb_Ref_Path = new TextBox();
            groupBox3 = new GroupBox();
            Bt_Clean_DllRef_Path = new Button();
            Bt_Dll_Rel_Path = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // Btn_GetFolder
            // 
            Btn_GetFolder.Location = new Point(478, 8);
            Btn_GetFolder.Name = "Btn_GetFolder";
            Btn_GetFolder.Size = new Size(85, 29);
            Btn_GetFolder.TabIndex = 0;
            Btn_GetFolder.Text = "選取資料夾";
            Btn_GetFolder.UseVisualStyleBackColor = true;
            Btn_GetFolder.Click += Btn_GetFolder_Click;
            // 
            // Tb_ShowPath
            // 
            Tb_ShowPath.Location = new Point(110, 11);
            Tb_ShowPath.Name = "Tb_ShowPath";
            Tb_ShowPath.Size = new Size(362, 23);
            Tb_ShowPath.TabIndex = 1;
            // 
            // Lb_ShowAllCsproj
            // 
            Lb_ShowAllCsproj.FormattingEnabled = true;
            Lb_ShowAllCsproj.HorizontalScrollbar = true;
            Lb_ShowAllCsproj.ItemHeight = 15;
            Lb_ShowAllCsproj.Location = new Point(6, 51);
            Lb_ShowAllCsproj.Name = "Lb_ShowAllCsproj";
            Lb_ShowAllCsproj.Size = new Size(522, 154);
            Lb_ShowAllCsproj.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Lb_Allowlist);
            groupBox1.Controls.Add(Cbx_Project_Allowlist);
            groupBox1.Location = new Point(25, 39);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5);
            groupBox1.Size = new Size(275, 221);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "專案 白名單";
            // 
            // Lb_Allowlist
            // 
            Lb_Allowlist.FormattingEnabled = true;
            Lb_Allowlist.HorizontalScrollbar = true;
            Lb_Allowlist.ItemHeight = 15;
            Lb_Allowlist.Location = new Point(8, 51);
            Lb_Allowlist.Name = "Lb_Allowlist";
            Lb_Allowlist.Size = new Size(259, 154);
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
            groupBox2.Controls.Add(Cbx_solution_file);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(Lb_ShowAllCsproj);
            groupBox2.Location = new Point(306, 39);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(5);
            groupBox2.Size = new Size(538, 221);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "目前所有 csproj 檔案";
            // 
            // Cbx_solution_file
            // 
            Cbx_solution_file.FormattingEnabled = true;
            Cbx_solution_file.Location = new Point(120, 19);
            Cbx_solution_file.Name = "Cbx_solution_file";
            Cbx_solution_file.Size = new Size(406, 23);
            Cbx_solution_file.TabIndex = 4;
            Cbx_solution_file.Text = "請選取 Solution File";
            Cbx_solution_file.SelectedIndexChanged += Cbx_solution_file_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 22);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 3;
            label2.Text = "選取 solution file : ";
            // 
            // Btn_ToReference
            // 
            Btn_ToReference.Location = new Point(438, 289);
            Btn_ToReference.Name = "Btn_ToReference";
            Btn_ToReference.Size = new Size(200, 50);
            Btn_ToReference.TabIndex = 8;
            Btn_ToReference.Text = "轉成 檔案參考";
            Btn_ToReference.UseVisualStyleBackColor = true;
            Btn_ToReference.Click += Btn_ToReference_Click;
            // 
            // Btn_ToProjectReference
            // 
            Btn_ToProjectReference.Location = new Point(644, 289);
            Btn_ToProjectReference.Name = "Btn_ToProjectReference";
            Btn_ToProjectReference.Size = new Size(200, 50);
            Btn_ToProjectReference.TabIndex = 9;
            Btn_ToProjectReference.Text = "轉成 專案參考";
            Btn_ToProjectReference.UseVisualStyleBackColor = true;
            Btn_ToProjectReference.Click += Btn_ToProjectReference_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 15);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 10;
            label1.Text = "選取資料夾：";
            // 
            // Tb_Ref_Path
            // 
            Tb_Ref_Path.Location = new Point(6, 17);
            Tb_Ref_Path.Name = "Tb_Ref_Path";
            Tb_Ref_Path.PlaceholderText = "預設路徑： App_Data/example.dll";
            Tb_Ref_Path.ReadOnly = true;
            Tb_Ref_Path.Size = new Size(385, 23);
            Tb_Ref_Path.TabIndex = 12;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(Bt_Clean_DllRef_Path);
            groupBox3.Controls.Add(Bt_Dll_Rel_Path);
            groupBox3.Controls.Add(Tb_Ref_Path);
            groupBox3.Location = new Point(25, 272);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(397, 73);
            groupBox3.TabIndex = 13;
            groupBox3.TabStop = false;
            groupBox3.Text = "DLL Reference 路徑：";
            // 
            // Bt_Clean_DllRef_Path
            // 
            Bt_Clean_DllRef_Path.Location = new Point(233, 44);
            Bt_Clean_DllRef_Path.Name = "Bt_Clean_DllRef_Path";
            Bt_Clean_DllRef_Path.Size = new Size(75, 23);
            Bt_Clean_DllRef_Path.TabIndex = 15;
            Bt_Clean_DllRef_Path.Text = "清除路徑";
            Bt_Clean_DllRef_Path.UseVisualStyleBackColor = true;
            Bt_Clean_DllRef_Path.Click += Bt_Clean_DllRef_Path_Click;
            // 
            // Bt_Dll_Rel_Path
            // 
            Bt_Dll_Rel_Path.Location = new Point(316, 44);
            Bt_Dll_Rel_Path.Name = "Bt_Dll_Rel_Path";
            Bt_Dll_Rel_Path.Size = new Size(75, 23);
            Bt_Dll_Rel_Path.TabIndex = 14;
            Bt_Dll_Rel_Path.Text = "選取資料夾";
            Bt_Dll_Rel_Path.UseVisualStyleBackColor = true;
            Bt_Dll_Rel_Path.Click += Bt_Dll_Rel_Path_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(864, 353);
            Controls.Add(groupBox3);
            Controls.Add(label1);
            Controls.Add(Btn_ToProjectReference);
            Controls.Add(Btn_ToReference);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(Tb_ShowPath);
            Controls.Add(Btn_GetFolder);
            MaximumSize = new Size(1000, 1000);
            MinimumSize = new Size(880, 380);
            Name = "Form1";
            Padding = new Padding(5);
            Text = "Reference Convert";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
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
        private Label label1;
        private ComboBox Cbx_solution_file;
        private Label label2;
        private TextBox Tb_Ref_Path;
        private GroupBox groupBox3;
        private Button Bt_Dll_Rel_Path;
        private Button Bt_Clean_DllRef_Path;
    }
}
