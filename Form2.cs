using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab05.BUS;
using Lab05.DAL.StudentModel;

namespace Quan_Ly_Sinh_Vien
{
    public partial class Form2 : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFacultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFacultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbKhoa.DataSource = listFacultys;
            this.cmbKhoa.DisplayMember = "Facultyname";
            this.cmbKhoa.ValueMember = "FacultyID";
        }

        private void FillMajorCombobox(List<Major> listMajors)
        {
            cmbChuyeNganh.DataSource = listMajors;
            cmbChuyeNganh.DisplayMember = "Name";
            cmbChuyeNganh.ValueMember = "FacultyID";
        }

        //private void cmbChuyeNganh_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    Faculty selectedFaculty = cmbKhoa.SelectedItem as Faculty;
        //    if (selectedFaculty != null)
        //    {
        //        var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
        //        FillMajorCombobox(listMajor);
        //        var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
        //        BindGrid(listStudents);
        //    }
        //}

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

        //private void dtgSinhVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex >= 0)
        //    {
        //        DataGridViewRow row = dtgSinhVien.Rows[e.RowIndex];
        //        string studentID = row.Cells[1].Value.ToString();
        //        Student selectedStudent = studentService.FindById(studentID);

        //        if (selectedStudent != null)
        //        {
        //            if (selectedStudent.FacultyID != null)
        //            {
        //                cmbKhoa.SelectedValue = selectedStudent.FacultyID;
        //                var listMajors = majorService.GetAllByFaculty((int)selectedStudent.FacultyID);
        //                FillMajorCombobox(listMajors);

        //                if (selectedStudent.MajorID != null)
        //                {
        //                    cmbChuyeNganh.SelectedValue = selectedStudent.MajorID;
        //                }
        //                else
        //                {
        //                    cmbChuyeNganh.SelectedIndex = -1;
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("Sinh viên không có khoa.");
        //            }
        //        }
        //    }
        //}

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            if (dtgSinhVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên.");
                return;
            }

            if (cmbChuyeNganh.SelectedItem != null && cmbKhoa.SelectedItem != null)
            {
                Faculty selectedFaculty = cmbKhoa.SelectedItem as Faculty;
                Major selectedMajor = cmbChuyeNganh.SelectedItem as Major;

                if (selectedFaculty != null && selectedMajor != null)
                {
                    string studentID = dtgSinhVien.SelectedRows[0].Cells[1].Value.ToString();
                    if (!string.IsNullOrEmpty(studentID))
                    {
                        Student student = studentService.FindById(studentID);

                        if (student != null)
                        {
                            student.MajorID = selectedMajor.MajorID;
                            student.FacultyID = selectedFaculty.FacultyID;
                            studentService.InsertUpdate(student);
                            MessageBox.Show("Đăng ký chuyên ngành thành công!");
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy sinh viên.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("ID sinh viên không hợp lệ.");
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn cả khoa và chuyên ngành.");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn chuyên ngành.");
            }
        }

        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbKhoa.SelectedItem as Faculty;
            if(selectedFaculty != null )
            {
                var listMajor = majorService.getAllMajorListByFacultyID(selectedFaculty.FacultyID);
                FillMajorCombobox(listMajor);

                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }
    }
}
