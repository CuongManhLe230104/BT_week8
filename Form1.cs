using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Lab05.BUS;
using Lab05.DAL.StudentModel;

namespace Quan_Ly_Sinh_Vien
{
    public partial class Form1 : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dtgSinhVien);
                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFacultyCombobox(listFacultys);
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFacultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty());
            this.cmbKhoa.DataSource = listFacultys;
            this.cmbKhoa.DisplayMember = "FacultyName";
            this.cmbKhoa.ValueMember = "FacultyID";
        }

        private void BindGrid(List<Student> listStudent)
        {
            dtgSinhVien.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dtgSinhVien.Rows.Add();
                dtgSinhVien.Rows[index].Cells[0].Value = item.StudentID;
                dtgSinhVien.Rows[index].Cells[1].Value = item.FullName;
                if (item.Faculty != null)
                    dtgSinhVien.Rows[index].Cells[2].Value = item.Faculty.FacultyName;
                dtgSinhVien.Rows[index].Cells[3].Value = item.AverageScore + "";
                if (item.MajorID != null)
                    dtgSinhVien.Rows[index].Cells[4].Value = item.Major.Name + "";
            }

        }

        private void ShowAvatar(string ImageName)
        {
            if (String.IsNullOrEmpty(ImageName))
            {
                pibAVT.Image = null;
            }
            else
            {
                string binDirectory = Path.Combine(Application.StartupPath, "Images");
                string imagePath = Path.Combine(binDirectory, ImageName);
                if (File.Exists(imagePath))
                {
                    pibAVT.Image = Image.FromFile(imagePath);
                    pibAVT.SizeMode = PictureBoxSizeMode.Zoom;
                    pibAVT.Refresh();
                }
                else
                {
                    MessageBox.Show("File does not exist at: " + imagePath);
                }
            }
        }

        public void setGridViewStyle(DataGridView dtg)
        {
            dtg.BorderStyle = BorderStyle.None;
            dtg.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dtg.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dtg.BackgroundColor = Color.White;
            dtg.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void dangKyChuyenNganhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();
            if (this.checkBox1.Checked)
                listStudents = studentService.GetAllHasNoMajor();  // Dữ liệu đã được tải đầy đủ
            else
                listStudents = studentService.GetAll();  // Dữ liệu đã được tải đầy đủ

            BindGrid(listStudents);
        }

        private void btnThem_CapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                // Tìm sinh viên theo ID
                var student = studentService.FindById(txtMSSV.Text);

                // Nếu không tồn tại, tạo mới
                if (student == null)
                {
                    student = new Student();
                    student.StudentID = txtMSSV.Text;
                }

                // Cập nhật thông tin sinh viên
                student.FullName = txtHoten.Text;
                student.AverageScore = double.Parse(txtDTB.Text);

                // Kiểm tra và xử lý trường hợp cmbKhoa.SelectedValue null
                if (cmbKhoa.SelectedValue != null)
                {
                    student.FacultyID = int.Parse(cmbKhoa.SelectedValue.ToString());
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn khoa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Dừng việc thực thi nếu không chọn được khoa
                }

                // Xử lý avatar
                if (!string.IsNullOrEmpty(avatarFilePath))
                {
                    string avatarFileName = SaveAvatar(avatarFilePath, student.StudentID);
                    if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        student.Avatar = avatarFileName;
                    }
                }

                // Cập nhật hoặc thêm mới sinh viên
                studentService.InsertUpdate(student);

                // Cập nhật lại DataGridView
                BindGrid(studentService.GetAll());

                // Xóa dữ liệu tạm thời
                ClearData();
                avatarFilePath = string.Empty;

                MessageBox.Show("Cập nhật danh sách thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm hoặc cập nhật dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dtgSinhVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    string studentID = dtgSinhVien.Rows[e.RowIndex].Cells[0].Value.ToString();
                    var student = studentService.FindById(studentID);
                    if (student != null)
                    {
                        txtMSSV.Text = student.StudentID;
                        txtHoten.Text = student.FullName;
                        txtDTB.Text = student.AverageScore.ToString();
                        if (student.Faculty != null)
                        {
                            cmbKhoa.SelectedValue = student.Faculty.FacultyID;
                        }
                        ShowAvatar(student.Avatar);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMSSV.Text))
                {
                    MessageBox.Show("Chưa chọn sinh viên", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var student = studentService.FindById(txtMSSV.Text);

                    if (student != null)
                    {
                        studentService.Delete(student.StudentID);
                        MessageBox.Show("Xóa sinh viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindGrid(studentService.GetAll());
                        ClearData();
                    }
                    else
                    {
                        MessageBox.Show("Sinh viên không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string avatarFilePath = string.Empty;
        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    avatarFilePath = openFileDialog.FileName;
                    pibAVT.Image = Image.FromFile(avatarFilePath);
                }
            }
        }

        private void LoadAvatar(string studentID)
        {
            string folderPath = Path.Combine(Application.StartupPath, "Images");
            var student = studentService.FindById(studentID);
            if (student != null && !string.IsNullOrEmpty(student.Avatar))
            {
                string avatarFilePath = Path.Combine(folderPath, student.Avatar);
                if (File.Exists(avatarFilePath))
                {
                    pibAVT.Image = Image.FromFile(avatarFilePath);
                }
                else
                {
                    pibAVT.Image = null;
                }
            }
        }

        private string SaveAvatar(string sourceFilePath, string studentID)
        {
            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string fileExtension = Path.GetExtension(sourceFilePath);
                string targetFilePath = Path.Combine(folderPath, $"{studentID}{fileExtension}");
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file: {sourceFilePath}");
                }
                File.Copy(sourceFilePath, targetFilePath, true);
                return $"{studentID}{fileExtension}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving avatar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void luuHinhAnhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                var student = studentService.FindById(txtMSSV.Text) ?? new Student();
                student.StudentID = txtMSSV.Text;
                student.FullName = txtHoten.Text;
                student.AverageScore = double.Parse(txtDTB.Text);
                student.FacultyID = int.Parse(cmbKhoa.SelectedValue.ToString());
                if (!string.IsNullOrEmpty(avatarFilePath))
                {
                    string avatarFileName = SaveAvatar(avatarFilePath, txtMSSV.Text);
                    if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        student.Avatar = avatarFileName;
                    }
                }
                studentService.InsertUpdate(student);
                BindGrid(studentService.GetAll());
                ClearData();
                avatarFilePath = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearData()
        {
            txtMSSV.Clear();
            txtHoten.Clear();
            txtDTB.Clear();
            cmbKhoa.SelectedIndex = -1;
            pibAVT.Image = null;
            avatarFilePath = string.Empty;
        }

        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dtgSinhVien_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    string studentID = dtgSinhVien.Rows[e.RowIndex].Cells[0].Value.ToString();
                    var student = studentService.FindById(studentID);
                    if (student != null)
                    {
                        txtMSSV.Text = student.StudentID;
                        txtHoten.Text = student.FullName;
                        txtDTB.Text = student.AverageScore.ToString();
                        if (student.Faculty != null)
                        {
                            cmbKhoa.SelectedValue = student.Faculty.FacultyID;
                        }
                        ShowAvatar(student.Avatar);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
