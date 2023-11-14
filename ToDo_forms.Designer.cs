using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ToDo_forms.Properties;

namespace ToDo_forms
{
    public partial class MainForm : Form
    {
        // Определение компонентов формы
        private List<TaskItem> tasks;
        private TextBox taskTextBox;
        private ListBox taskListBox;
        private ToolTip toolTip;
        private Button addButton;
        private Button deleteButton;
        private Button editButton;
        private Button clearButton;

        public MainForm()
        {
            // Инициализация компонентов в конструкторе
            LoadComponents();

            // Другая инициализация
            tasks = new List<TaskItem>();
            LoadTasks();
            UpdateFormLocalization();

            // Подписываемся на событие изменения языка
            TaskItem.LanguageChanged += TaskItem_LanguageChanged;
        }

        private void RussianButton_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            UpdateFormLocalization();
        }

        private void EnglishButton_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            UpdateFormLocalization();
        }

        private void TaskItem_LanguageChanged(object sender, EventArgs e)
        {
            // Обновление формы при изменении языка
            // ...
        }

        private void UpdateFormLocalization()
        {
            // Очистка предыдущих подсказок
            toolTip.RemoveAll();

            Text = Resources.TaskApp;
            addButton.Text = Resources.AddTask;
            deleteButton.Text = Resources.DeleteTask;
            editButton.Text = Resources.Edittask;
            clearButton.Text = Resources.ClearTasks;

            toolTip.SetToolTip(addButton, Resources.Addanewtask);
            toolTip.SetToolTip(deleteButton, Resources.Deletetheselectedtask);
            toolTip.SetToolTip(editButton, Resources.Edittheselectedtask);
            toolTip.SetToolTip(clearButton, Resources.Clearalltasks);

            // Обновляем текст в ListBox для каждой задачи
            for (int i = 0; i < tasks.Count; i++)
            {
                string taskDisplayString = GetTaskDisplayString(tasks[i]);
                taskListBox.Items[i] = taskDisplayString;
            }

            // Перерисовываем элементы управления, чтобы обновить текст на кнопках
            Invalidate(true);
        }

        private void LoadComponents()
        {
            if (taskListBox != null)
            {
                // Если taskListBox уже инициализирован, необходимо просто выйти из метода
                return;
            }

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            taskTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 280
            };

            addButton = new Button
            {
                Text = Resources.AddTask,
                Location = new System.Drawing.Point(290, 10),
                Width = 70
            };
            addButton.Click += AddButton_Click;

            taskListBox = new ListBox
            {
                Location = new System.Drawing.Point(10, 40),
                Width = 350,
                Height = 300
            };

            deleteButton = new Button
            {
                Text = Resources.DeleteTask,
                Location = new System.Drawing.Point(10, 350),
                Width = 350
            };
            deleteButton.Click += DeleteButton_Click;

            editButton = new Button
            {
                Text = Resources.Edittask,
                Location = new System.Drawing.Point(10, 330),
                Width = 350
            };
            editButton.Click += EditButton_Click;

            clearButton = new Button
            {
                Text = Resources.ClearTasks,
                Location = new System.Drawing.Point(10, 370),
                Width = 350
            };
            clearButton.Click += ClearButton_Click;

            Button languageButton = new Button
            {
                Text = "RU",
                Location = new System.Drawing.Point(10, 390),
                Width = 30
            };
            languageButton.Click += RussianButton_Click;
            Controls.Add(languageButton);

            Button languageButton2 = new Button
            {
                Text = "EN",
                Location = new System.Drawing.Point(40, 390),
                Width = 30
            };
            languageButton2.Click += EnglishButton_Click;
            Controls.Add(languageButton2);

            Controls.Add(taskTextBox);
            Controls.Add(addButton);
            Controls.Add(taskListBox);
            Controls.Add(deleteButton);
            Controls.Add(editButton);
            Controls.Add(clearButton);
            Controls.Add(languageButton);
            Controls.Add(languageButton2);

            FormClosing += MainForm_FormClosing;

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            toolTip.SetToolTip(addButton, Resources.Addanewtask);
            toolTip.SetToolTip(deleteButton, Resources.Deletetheselectedtask);
            toolTip.SetToolTip(editButton, Resources.Edittheselectedtask);
            toolTip.SetToolTip(clearButton, Resources.Clearalltasks);

            Size = new Size(385, 455); // Замените значениями, которые соответствуют вашим требованиям

        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string taskDescription = taskTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(taskDescription))
            {
                DateTime? dueTime = GetDueTimeFromUser();
                string category = PromptUserForDescription(Resources.EnterCategory, Resources.EnterTaskCategory, "");
                TaskItem task = new TaskItem { Description = taskDescription, DueTime = dueTime, Category = category };
                tasks.Add(task);
                taskListBox.Items.Add(GetTaskDisplayString(task));
                SaveTasks();
                taskTextBox.Text = string.Empty;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            List<int> selectedIndices = new List<int>();

            // Собираем все выбранные индексы в список
            foreach (int index in taskListBox.SelectedIndices)
            {
                selectedIndices.Add(index);
            }

            // Удаляем задачи начиная с конца списка, чтобы не нарушить индексы
            for (int i = selectedIndices.Count - 1; i >= 0; i--)
            {
                int selectedIndex = selectedIndices[i];
                tasks.RemoveAt(selectedIndex);
                taskListBox.Items.RemoveAt(selectedIndex);
            }

            SaveTasks();
            UpdateFormLocalization();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTasks();
            UpdateFormLocalization();
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists("tasks.json"))
                {
                    string json = File.ReadAllText("tasks.json");
                    if (!string.IsNullOrEmpty(json))
                    {
                        tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);

                        foreach (TaskItem task in tasks)
                        {
                            taskListBox.Items.Add(GetTaskDisplayString(task));
                        }
                    }
                    else
                    {
                        tasks = new List<TaskItem>();
                    }
                }
                else
                {
                    tasks = new List<TaskItem>();
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine(Resources.ErrorLoadingTasks + ex.Message);
                MessageBox.Show(Resources.ErrorLoadingTasks + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTasks()
        {
            try
            {
                string json = JsonConvert.SerializeObject(tasks);
                File.WriteAllText("tasks.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ErrorSavingTasks + ex.Message);
                MessageBox.Show(Resources.ErrorSavingTasks2);
            }
        }

        private DateTime? GetDueTimeFromUser()
        {
            DateTimePicker dateTimePicker = new DateTimePicker();
            dateTimePicker.Format = DateTimePickerFormat.Custom;
            dateTimePicker.CustomFormat = "HH:mm dd-MM-yyyy";

            Form promptForm = new Form();
            promptForm.Text = Resources.EnterTime;
            promptForm.Width = 300;
            promptForm.Height = 100;
            promptForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            promptForm.StartPosition = FormStartPosition.CenterScreen;

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new Point(120, 10);
            okButton.Click += (sender, e) => { promptForm.DialogResult = DialogResult.OK; };

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(180, 10);
            cancelButton.Click += (sender, e) => { promptForm.DialogResult = DialogResult.Cancel; };

            promptForm.Controls.Add(dateTimePicker);
            promptForm.Controls.Add(okButton);
            promptForm.Controls.Add(cancelButton);

            if (promptForm.ShowDialog() == DialogResult.OK)
            {
                return dateTimePicker.Value;
            }

            return null;
        }

        private string GetTaskDisplayString(TaskItem task)
        {
            if (task.DueTime.HasValue)
            {
                string dueText = Resources.Due;
                string formattedDueTime = task.DueTime.Value.ToString("mm:HH dd-MM-yyyy");
                return $"{task.Description} ({dueText}: {formattedDueTime}, {Resources.Category}: {task.Category})";
            }
            else
            {
                return $"{task.Description} ({Resources.Category}: {task.Category})";
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.ConfirmClearTasks, Resources.Confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                tasks.Clear();
                taskListBox.Items.Clear();
                SaveTasks();
                UpdateFormLocalization();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (taskListBox.SelectedIndex != -1)
            {
                int selectedIndex = taskListBox.SelectedIndex;
                TaskItem selectedTask = tasks[selectedIndex];

                string newDescription = PromptUserForDescription(Resources.Edittask, Resources.EnterNewTaskDescription, selectedTask.Description);
                if (newDescription != null)
                {
                    selectedTask.Description = newDescription;

                    DateTime? newDueTime = GetDueTimeFromUser();
                    if (newDueTime != null)
                    {
                        selectedTask.DueTime = newDueTime;
                    }

                    string newCategory = PromptUserForDescription(Resources.Edittask, Resources.EnterNewTaskCategory, selectedTask.Category);
                    selectedTask.Category = newCategory;

                    taskListBox.Items[selectedIndex] = GetTaskDisplayString(selectedTask);
                    SaveTasks();
                    UpdateFormLocalization();
                }
            }
        }

        private string PromptUserForDescription(string title, string prompt, string defaultValue)
        {
            string userInput = Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultValue);
            return string.IsNullOrEmpty(userInput) ? null : userInput.Trim();
        }

        public class TaskItem
        {
            public static event EventHandler LanguageChanged;

            private static CultureInfo currentLanguage = CultureInfo.InvariantCulture;

            public static CultureInfo CurrentLanguage
            {
                get { return currentLanguage; }
                set
                {
                    if (currentLanguage != value)
                    {
                        LanguageChanged?.Invoke(null, EventArgs.Empty);
                        currentLanguage = value;
                    }
                }
            }

            public string Description { get; set; }
            public DateTime? DueTime { get; set; }
            public string Category { get; set; }
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}