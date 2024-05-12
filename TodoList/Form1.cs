using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TodoList
{
    public partial class Form1 : Form
    {
        private bool dateChanged = false;
        public Form1()
        {
            InitializeComponent();
        }

        private bool alert(string message, string title) {
            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) {
                MessageBox.Show(title + " cannot be empty!", "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return false;
        }

        private void search(string keyword)
        {

            using (var context = new SimpleTodoListEntities())
            {

                var query = from item in context.tb_task
                            where item.title.Contains(keyword)
                            select new
                            {
                                ID = item.id,
                                Title = item.title,
                                Task = item.task,
                                Status = item.status,
                                Created_At = item.created_at
                            };

                table.Columns.Clear();
                table.DataSource = query.ToArray();

                DataGridViewButtonColumn delete = new DataGridViewButtonColumn();
                delete.HeaderText = "";
                delete.Text = "Delete";
                delete.UseColumnTextForButtonValue = true;

                DataGridViewButtonColumn update = new DataGridViewButtonColumn();
                update.HeaderText = "";
                update.Text = "Update";
                update.UseColumnTextForButtonValue = true;

                table.Columns.AddRange(delete, update);

            }
        }

        private void filter()
        {

            using (var context = new SimpleTodoListEntities())
            {

                var status = cbStatus.SelectedItem;
                var query = from item in context.tb_task
                            where (cbStatus.SelectedIndex == 0 || item.status == status.ToString())
                            && (!dateChanged || DbFunctions.TruncateTime(item.created_at) >= DbFunctions.TruncateTime(date1.Value) && DbFunctions.TruncateTime(item.created_at) <= DbFunctions.TruncateTime(date2.Value))
                            select new
                            {
                                ID = item.id,
                                Title = item.title,
                                Task = item.task,
                                Status = item.status,
                                Created_At = item.created_at
                            };

                table.Columns.Clear();
                table.DataSource = query.ToArray();

                DataGridViewButtonColumn delete = new DataGridViewButtonColumn();
                delete.HeaderText = "";
                delete.Text = "Delete";
                delete.UseColumnTextForButtonValue = true;

                DataGridViewButtonColumn update = new DataGridViewButtonColumn();
                update.HeaderText = "";
                update.Text = "Update";
                update.UseColumnTextForButtonValue = true;

                table.Columns.AddRange(delete, update);

            }

            cbStatus.SelectedIndex = 0;
            date1.ResetText();
            date2.ResetText();
            dateChanged = false;
            btnFilter.Enabled = false;
        }

        private void refresh()
        {
            dateChanged = false;
            inputTask.Clear();
            inputTitle.Clear();
            r1.Checked = true;
            r2.Checked = false;
            btnCancel.Enabled = false;
            btnSubmit.Text = "Tambah";
            btnSubmit.Tag = "";

            using (var context = new SimpleTodoListEntities())
            {

                var query = from item in context.tb_task
                            select new { 
                                ID = item.id,
                                Title = item.title,
                                Task = item.task,
                                Status = item.status,
                                Created_At = item.created_at
                            };

                table.Columns.Clear();
                table.DataSource = query.ToArray();

                DataGridViewButtonColumn delete = new DataGridViewButtonColumn();
                delete.HeaderText = "";
                delete.Text = "Delete";
                delete.UseColumnTextForButtonValue = true;

                DataGridViewButtonColumn update = new DataGridViewButtonColumn();
                update.HeaderText = "";
                update.Text = "Update";
                update.UseColumnTextForButtonValue = true;

                table.Columns.AddRange(delete, update);

            }
        }

        private void add()
        {
            if (alert(inputTitle.Text, "Title")) { return; }
            if (alert(inputTask.Text, "Task")) { return; }

            using (var context = new SimpleTodoListEntities())
            {
                var selected = r1.Checked ? r1.Text : r2.Text;
                var newData = new tb_task();
                newData.title = inputTitle.Text;
                newData.task = inputTask.Text;
                newData.status = selected;
                newData.created_at = DateTime.Now;

                context.tb_task.Add(newData);
                context.SaveChanges();
                refresh();

            }
        }

        private void update()
        {
            if (alert(inputTitle.Text, "Title")) { return; }
            if (alert(inputTask.Text, "Task")) { return; }

            using (var context = new SimpleTodoListEntities())
            {
                var selected = r1.Checked ? r1.Text : r2.Text;
                var id = int.Parse(btnSubmit.Tag.ToString());
                var newData = context.tb_task.SingleOrDefault(item => item.id == id);
                newData.title = inputTitle.Text;
                newData.task = inputTask.Text;
                newData.status = selected;
                newData.update_at = DateTime.Now;
                context.SaveChanges();
                refresh();

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            refresh();
            cbStatus.Items.AddRange(new string[] {"Choose Status", "Ongoing", "Finished"});
            cbStatus.SelectedIndex = 0;
            btnFilter.Enabled = false;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (btnSubmit.Text == "Tambah")
            {
                add();
            } else
            {
                update();
            }
        }

        private void table_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1) { return; }
            DataGridViewRow row = table.Rows[e.RowIndex];
            var id = int.Parse(row.Cells[0].Value.ToString());
            if (e.ColumnIndex == 5)
            {
                DialogResult dialog = MessageBox.Show("Are you sure delete, the data?", "Delete Option", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes)
                {
                    using (var context = new SimpleTodoListEntities())
                    {
                        var data = context.tb_task.SingleOrDefault(item => item.id == id);
                        context.tb_task.Remove(data);
                        context.SaveChanges();
                        refresh();
                    }
                }
                return;
            }

            if (e.ColumnIndex == 6)
            {
                inputTask.Text = row.Cells[2].Value.ToString();
                inputTitle.Text = row.Cells[1].Value.ToString();
                r1.Checked = row.Cells[3].Value.ToString().Equals("Ongoing");
                r2.Checked = !row.Cells[3].Value.ToString().Equals("Ongoing");
                btnCancel.Enabled = true;
                btnSubmit.Text = "Update";
                btnSubmit.Tag = id;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            inputTask.Clear();
            inputTitle.Clear();
            r1.Checked = true;
            r2.Checked = false;
            btnCancel.Enabled = false;
            btnSubmit.Text = "Tambah";
            btnSubmit.Tag = "";
            dateChanged = false;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputSearch.Text) || string.IsNullOrWhiteSpace(inputSearch.Text))
            {
                refresh();
                return;
            }
            search(inputSearch.Text);
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            filter();
        }

        private void date1_ValueChanged(object sender, EventArgs e)
        {
            dateChanged = true;
            btnFilter.Enabled = true;
        }

        private void date2_ValueChanged(object sender, EventArgs e)
        {
            dateChanged = true;
            btnFilter.Enabled = true;
        }

        private void cbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnFilter.Enabled = true;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}
