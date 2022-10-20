using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Project_app__Sheet_6_
{
    [Serializable]
    public class Project {
        public string name;
        public string description;
        public string time;
        public string state;
        public string notes;

     
        public Project(string name, string description, string time, string state, string notes) {
            this.name = name;
            this.description = description;
            this.time = time;
            this.state = state;
            this.notes = notes;
        }
        
    }
    public class ProjectInformation
    {

        private static ProjectInformation projectInformation;

        private Dictionary<string, Project> projectsDictionary;
        private BinaryFormatter formatter;

        private const string DATA_FILENAME = "ProjectInformation.dat";

        public static ProjectInformation Instance()
        {
            if (projectInformation == null)
            {
                projectInformation = new ProjectInformation();
            } // end if

            return projectInformation;
        } // end public static ProjectInformation Instance()

        private ProjectInformation()
        {
            // Create a Dictionary to store friends at runtime
            this.projectsDictionary = new Dictionary<string, Project>();
            this.formatter = new BinaryFormatter();
        } // end private ProjectInformation()

        public int AddProject(string name, string description, string time, string state, string notes)
        {
            if (this.projectsDictionary.ContainsKey(name))
            {
                MessageBox.Show("You had already added " + name + " before.");
                return 0;
            }
 
            // in our dictionary
            else
            {
                // Add it to the dictionary
                this.projectsDictionary.Add(name, new Project(name, description, time, state, notes));
                return 1;
            } // end if
        } 

        public void RemoveProject(string name)
        {
            // If we do not have a project with this name
            if (!this.projectsDictionary.ContainsKey(name))
            {
                MessageBox.Show("ERROR 404 \"" + name + "\"" + " was not found.");
            }
            
            else
            {
                if (this.projectsDictionary.Remove(name))
                {
                    MessageBox.Show(name + " had been removed successfully.");
                }
                else
                {
                    MessageBox.Show("Unable to remove " + name);
                } 
            } 
        }

        public int EditProject(string name, string description, string time, string state, string notes)
        {
            if (this.projectsDictionary.ContainsKey(name) == false)
            {
                MessageBox.Show("ERROR, project not found");
                return 0;
            }

            else
                {
                try {
                    Project projectToEdit = this.projectsDictionary[name];
                    projectToEdit.description = description;
                    projectToEdit.time = time;
                    projectToEdit.state = state;
                    projectToEdit.notes = notes;
                    return 1;
                }

                catch (Exception)
                {
                    return 0;
                }
            }
            
        }

        public void Save()
        {
            // Gain code access to the file that we are going
            // to write to
            try
            {
                // Create a FileStream that will write data to file.
                FileStream writerFileStream = new FileStream(DATA_FILENAME, FileMode.Create, FileAccess.Write);
                // Save our dictionary of friends to file
                this.formatter.Serialize(writerFileStream, this.projectsDictionary);

                // Close the writerFileStream when we are done.
                writerFileStream.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to save project information" + "\n"+ e.Message);
            } // end try-catch
        } // end public bool Load()

        public void Load()
        {

            // Check if we had previously Save information of the projects
            if (File.Exists(DATA_FILENAME))
            {

                try
                {
                    // Create a FileStream will gain read access to the 
                    // data file.
                    FileStream readerFileStream = new FileStream(DATA_FILENAME,
                        FileMode.Open, FileAccess.Read);
                    // Reconstruct information of our friends from file.
                    this.projectsDictionary = (Dictionary<String, Project>)
                        this.formatter.Deserialize(readerFileStream);
                    // Close the readerFileStream when we are done
                    readerFileStream.Close();

                }
                catch (Exception)
                {
                    Console.WriteLine("There seems to be a file that contains the project information but a problem has occured.");
                } // end try-catch

            } // end if

        }

        public string[] GetProjectNames() {
            string[] projectNames = this.projectsDictionary.Keys.ToArray();
            return projectNames;
        }

        public Project GetProject(string name) {
            return this.projectsDictionary[name];
        }

        public double[] GetProjectPercent() { 
            
            double[] percentages = new double[4];
            int amountStarted = 0;
            int amountInProg = 0;
            int amountCom = 0;
            int amountAband = 0;

            if (this.projectsDictionary.Count == 0)
            {
                return new double[] { 0, 0, 0, 0};
            }

            foreach (KeyValuePair<string, Project> project in this.projectsDictionary) {

                if (project.Value.state == "not-started")
                {
                    amountStarted += 1;
                }
                else if (project.Value.state == "in-progress")
                {
                    amountInProg += 1;
                }
                else if (project.Value.state == "completed")
                {
                    amountCom += 1;
                }
                else {
                    amountAband += 1;
                }
            }

            int total = this.projectsDictionary.Count;

            percentages[0] = Math.Ceiling((double)amountStarted / total * 100);
            percentages[1] = Math.Ceiling((double)(amountInProg * 100 / total));
            percentages[2] = Math.Ceiling((double)(amountCom * 100 / total));
            percentages[3] = Math.Ceiling((double)(amountAband * 100 / total)); ;

            return percentages;
        }
    }
        public partial class MainWindow : Window
    {
        Canvas rootCanvas = new Canvas();
        Canvas mainContent = new Canvas();
        ProjectInformation pi;
        List<Button> projectButtonList = new List<Button>();
        StackPanel buttons;
        BrushConverter bc = new BrushConverter();
        ScrollViewer projectButtons;
        int curretButtonIndex;

        public MainWindow()
        {
            InitializeComponent();
            pi = ProjectInformation.Instance();
            pi.Load();
            Viewbox dynamicViewbox = new Viewbox();
            // Set StretchDirection and Stretch properties  
            dynamicViewbox.StretchDirection = StretchDirection.Both;
            dynamicViewbox.Stretch = Stretch.Fill;

            rootCanvas.Height = Root.Height;
            rootCanvas.Width = Root.Width;
            rootCanvas.Background = Brushes.Black;

            mainContent.Height = rootCanvas.Height;
            mainContent.Width = (3 * rootCanvas.Width) / 4;
            mainContent.Background = Brushes.Black;

            rootCanvas.Children.Add(mainContent);
            Canvas.SetLeft(mainContent, (rootCanvas.Width)/4);

            Root.Content = dynamicViewbox;
            dynamicViewbox.Child = rootCanvas;
            Root.Arrange(new Rect(0, 0, Width, Height));

            loadbuttons();
            HomePage();
        }

        void loadbuttons() {
            

            Button newProject = createNewButton("New Project...", Brushes.White, bc.ConvertFrom("#333333"), rootCanvas.Height / 6, (rootCanvas.Width / 4), 28, 0);
            projectButtonList.Add(newProject);
            Canvas.SetTop(newProject, 0);
            Canvas.SetLeft(newProject, 0);
            rootCanvas.Children.Add(newProject);

            projectButtons = new ScrollViewer();
            projectButtons.Width = rootCanvas.Width / 4;
            projectButtons.Height = rootCanvas.Height - newProject.Height;
            projectButtons.Background = Brushes.LightGray;
            projectButtons.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            buttons = new StackPanel();
            projectButtons.Content = buttons;

            string[] projects = pi.GetProjectNames();

            for (int i = 0; i < projects.Length; i++) {
                Button tempbutton = createNewButton(projects[i], Brushes.White, (Brush)bc.ConvertFrom("#6B6B6B"), rootCanvas.Height / 6, (rootCanvas.Width / 4) - 20, 23, i + 1);
                projectButtonList.Add(tempbutton);
                buttons.Children.Add(tempbutton);
            }
            rootCanvas.Children.Add(projectButtons);
            projectButtons.ScrollToTop();
            Canvas.SetTop(projectButtons, newProject.Height);

        }

        void ButtonClick(object sender, EventArgs e) {
            if (curretButtonIndex == 0)
            {
                projectButtonList[curretButtonIndex].Background = (Brush)bc.ConvertFrom("#333333");
            }
            else {
                projectButtonList[curretButtonIndex].Background = (Brush)bc.ConvertFrom("#6B6B6B");
            }
            
            curretButtonIndex = (int)((Button)sender).Tag;
            Button currentButton = projectButtonList[(int)((Button)sender).Tag];
            if ((int)currentButton.Tag == 0)
            {
                currentButton.Background = Brushes.Blue;
                NewProjectPage();
            }
            else {
                currentButton.Background = Brushes.Blue;
                ProjectInformationPage(pi.GetProject(currentButton.Content.ToString()));
            }
        }

        void HomePage() {
            mainContent.Children.Clear();

            // Welcome text 
            TextBlock Welcome = createTextBlock("Welcome!", 36, Brushes.White);
            Canvas.SetTop(Welcome, 20);
            Canvas.SetLeft(Welcome, 222);
            mainContent.Children.Add(Welcome);

            // Projects text

            TextBlock States = createTextBlock("Projects:", 24, Brushes.White);
            Canvas.SetTop(States, 110);
            Canvas.SetLeft(States, 256);
            mainContent.Children.Add(States);
            
            // Percentages of projects

            double[] percentages = pi.GetProjectPercent();
            TextBlock StatesValues = new TextBlock();
            StatesValues.Text = percentages[0].ToString() + "% Not started\n" + percentages[1].ToString() + "% In-progress\n" + percentages[2].ToString() + "% Completed\n" + percentages[3].ToString() + "% Abandonded\n";
            StatesValues.FontSize = 20;
            StatesValues.Foreground = Brushes.White;
            Canvas.SetTop(StatesValues, 162);
            Canvas.SetLeft(StatesValues, 229);
            mainContent.Children.Add(StatesValues);
        }

        void NewProjectPage() {
            mainContent.Children.Clear();

            // Name label
            TextBlock nameLabel = createTextBlock("Project name:", 22, Brushes.White);
            Canvas.SetLeft(nameLabel, 50);
            Canvas.SetTop(nameLabel, 37);
            mainContent.Children.Add(nameLabel);

            // name text box
            TextBox nameTextBox = createNewTextBox(37, 157, 16);
            Canvas.SetLeft(nameTextBox, 47);
            Canvas.SetTop(nameTextBox, 77);
            mainContent.Children.Add(nameTextBox);

            // State Label
            TextBlock StateLabel = createTextBlock("State:", 22, Brushes.White);
            Canvas.SetLeft(StateLabel, 276);
            Canvas.SetTop(StateLabel, 37);
            mainContent.Children.Add(StateLabel);

            //radio buttons
            RadioButton r1 = new RadioButton();
            r1.IsChecked = true;
            r1.Content = "Not started";
            r1.FontSize = 15;
            r1.Foreground = Brushes.White;
            Canvas.SetLeft(r1, 269);
            Canvas.SetTop(r1, 87);
            mainContent.Children.Add(r1);

            RadioButton r2 = new RadioButton();
            r2.IsChecked = false;
            r2.Content = "In-Progress";
            r2.Foreground = Brushes.White;
            r2.FontSize = 15;
            Canvas.SetLeft(r2, 269);
            Canvas.SetTop(r2, 107);
            mainContent.Children.Add(r2);


            RadioButton r3 = new RadioButton();
            r3.IsChecked = false;
            r3.Content = "Finsihed";
            r3.FontSize = 15;
            r3.Foreground = Brushes.White;
            Canvas.SetLeft(r3, 269);
            Canvas.SetTop(r3, 127);
            mainContent.Children.Add(r3);

            RadioButton r4 = new RadioButton();
            r4.IsChecked = false;
            r4.Content = "Abandoned";
            r4.FontSize = 15;
            r4.Foreground = Brushes.White;
            Canvas.SetLeft(r4, 269);
            Canvas.SetTop(r4, 147);
            mainContent.Children.Add(r4);

            //Deadline Label
            TextBlock DeadLabel = createTextBlock("Deadline:", 22, Brushes.White);
            Canvas.SetLeft(DeadLabel, 418);
            Canvas.SetTop(DeadLabel, 37);
            mainContent.Children.Add(DeadLabel);


            //Date picker

            DatePicker deadline = new DatePicker();
            deadline.FontSize = 18;
            Canvas.SetLeft(deadline, 418);
            Canvas.SetTop(deadline, 86);
            mainContent.Children.Add(deadline);

            //Description Label
            TextBlock DesLabel = createTextBlock("Description:", 22, Brushes.White);
            Canvas.SetLeft(DesLabel, 50);
            Canvas.SetTop(DesLabel, 175);
            mainContent.Children.Add(DesLabel);

            //Description Textbox

            TextBox descTextBox = createNewTextBox(120, 250, 16);
            descTextBox.TextWrapping = TextWrapping.Wrap;
            descTextBox.KeyDown += new KeyEventHandler(keypressed);
            Canvas.SetLeft(descTextBox, 47);
            Canvas.SetTop(descTextBox, 221);
            mainContent.Children.Add(descTextBox);

            // Notes Label
            TextBlock notesLabel = createTextBlock("Notes:", 22, Brushes.White);
            Canvas.SetLeft(notesLabel, 366);
            Canvas.SetTop(notesLabel, 175);
            mainContent.Children.Add(notesLabel);


            //Notes textbox

            TextBox notesTextBox = createNewTextBox(125, 216, 16);
            notesTextBox.TextWrapping = TextWrapping.Wrap;
            notesTextBox.KeyDown += new KeyEventHandler(keypressed);
            Canvas.SetLeft(notesTextBox, 365);
            Canvas.SetTop(notesTextBox, 216);
            mainContent.Children.Add(notesTextBox);

            // Save Button
            Button save = createNewButton("Save", Brushes.White, (Brush)bc.ConvertFrom("#01C8F1"), 32, 97, 22, -1);
            save.Click -=  new RoutedEventHandler(ButtonClick);
            save.Click += (sender, EventArgs) => { SavePress(sender, EventArgs, nameTextBox.Text, r1, r2, r3, deadline.Text, descTextBox.Text, notesTextBox.Text); };
            Canvas.SetLeft(save, 482);
            Canvas.SetTop(save, 404);
            mainContent.Children.Add(save);
        }

        void ProjectInformationPage(Project project) {
            mainContent.Children.Clear();
            TextBlock title = createTextBlock(project.name, 30, Brushes.White);
            title.Width = (3 * rootCanvas.Width) / 4;
            title.HorizontalAlignment = HorizontalAlignment.Center;
            Canvas.SetLeft(title, rootCanvas.Width / 4);
            Canvas.SetTop(title, 15);
            mainContent.Children.Add(title);

            // State Label
            TextBlock StateLabel = createTextBlock("State:", 22, Brushes.White);
            Canvas.SetLeft(StateLabel, 398);
            Canvas.SetTop(StateLabel, 63);
            mainContent.Children.Add(StateLabel);

            //radio buttons
            RadioButton r1 = new RadioButton();
            r1.Content = "Not started";
            r1.FontSize = 15;
            r1.Foreground = Brushes.White;
            Canvas.SetLeft(r1, 381);
            Canvas.SetTop(r1, 104);
            mainContent.Children.Add(r1);

            RadioButton r2 = new RadioButton();
            r2.Content = "In-Progress";
            r2.Foreground = Brushes.White;
            r2.FontSize = 15;
            Canvas.SetLeft(r2, 381);
            Canvas.SetTop(r2, 124);
            mainContent.Children.Add(r2);


            RadioButton r3 = new RadioButton();
            r3.Content = "Finsihed";
            r3.FontSize = 15;
            r3.Foreground = Brushes.White;
            Canvas.SetLeft(r3, 381);
            Canvas.SetTop(r3, 144);
            mainContent.Children.Add(r3);

            RadioButton r4 = new RadioButton();
            r4.Content = "Adandoned";
            r4.FontSize = 15;
            r4.Foreground = Brushes.White;
            Canvas.SetLeft(r4, 381);
            Canvas.SetTop(r4, 164);
            mainContent.Children.Add(r4);

            if (project.state == "not-started")
            {
                r1.IsChecked = true;
            }
            else if (project.state == "in-progress")
            {
                r2.IsChecked = true;
            }
            else if (project.state == "completed")
            {
                r3.IsChecked = true;
            }
            else {
                r4.IsChecked = true;
            }

            //Deadline Label
            TextBlock DeadLabel = createTextBlock("Deadline:", 22, Brushes.White);
            Canvas.SetLeft(DeadLabel, 380);
            Canvas.SetTop(DeadLabel, 205);
            mainContent.Children.Add(DeadLabel);


            //Date picker

            DatePicker deadline = new DatePicker();
            deadline.FontSize = 18;
            deadline.Text = project.time;
            Canvas.SetLeft(deadline, 370);
            Canvas.SetTop(deadline, 255);
            mainContent.Children.Add(deadline);

            //Description Label
            TextBlock DesLabel = createTextBlock("Description:", 22, Brushes.White);
            Canvas.SetLeft(DesLabel, 25);
            Canvas.SetTop(DesLabel, 63);
            mainContent.Children.Add(DesLabel);

            //Description Textbox

            TextBox descTextBox = createNewTextBox(135, 230, 16);
            descTextBox.TextWrapping = TextWrapping.Wrap;
            descTextBox.Text = project.description;
            descTextBox.KeyDown += new KeyEventHandler(keypressed);
            Canvas.SetLeft(descTextBox, 25);
            Canvas.SetTop(descTextBox, 98);
            mainContent.Children.Add(descTextBox);

            // Notes Label
            TextBlock notesLabel = createTextBlock("Notes:", 22, Brushes.White);
            Canvas.SetLeft(notesLabel, 25);
            Canvas.SetTop(notesLabel, 235);
            mainContent.Children.Add(notesLabel);


            //Notes textbox

            TextBox notesTextBox = createNewTextBox(128, 230, 16);
            notesTextBox.TextWrapping = TextWrapping.Wrap;
            notesTextBox.Text = project.notes;
            notesTextBox.KeyDown += new KeyEventHandler(keypressed);
            Canvas.SetLeft(notesTextBox, 25);
            Canvas.SetTop(notesTextBox, 275);
            mainContent.Children.Add(notesTextBox);

            // Save Button
            Button save = createNewButton("Save", Brushes.White, (Brush)bc.ConvertFrom("#01C8F1"), 32, 97, 22, -2);
            save.Click -= new RoutedEventHandler(ButtonClick);
            save.Click += (sender, EventArgs) => { SavePress(sender, EventArgs, project.name, r1, r2, r3, deadline.Text, descTextBox.Text, notesTextBox.Text); };
            Canvas.SetLeft(save, 482);
            Canvas.SetTop(save, 404);
            mainContent.Children.Add(save);

            // delete button
            Button del = createNewButton("Save", Brushes.White, Brushes.Black, 50, 45, 22, -3);
            del.Click -= new RoutedEventHandler(ButtonClick);
            del.Click += (sender, EventArgs) => { DeletePress(sender, EventArgs, project.name); };
            Image deleteico = new Image();
            string currentapth = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeleteIcon.png");
            deleteico.Source = new BitmapImage(new Uri(currentapth));
            deleteico.Height = del.Height;
            deleteico.Width = del.Width;
            del.Content = deleteico;
            Canvas.SetLeft(del, 542);
            Canvas.SetTop(del, 10);
            mainContent.Children.Add(del);

        }

        Button createNewButton(string name, object foreground, object background, double height, double width, int fontsize,int tag) {
            Button tempbutton = new Button();
            tempbutton.Content = name;
            tempbutton.Foreground = (Brush)foreground;
            tempbutton.Background =(Brush)background;
            tempbutton.Height = height;
            tempbutton.Width = width;
            tempbutton.FontSize = fontsize;
            tempbutton.Click += new RoutedEventHandler(ButtonClick);
            tempbutton.Tag = tag;
            return tempbutton;
        }

        TextBlock createTextBlock(string content, int fontsize, object foreground){
            TextBlock tempLabel = new TextBlock();
            tempLabel.Text = content;
            tempLabel.FontSize = fontsize;
            tempLabel.Foreground = (Brush)foreground;
            return tempLabel;
        }

        TextBox createNewTextBox(int height, int width, int fontsize) {
            TextBox tempTextBox = new TextBox();
            tempTextBox.Width = width;
            tempTextBox.Height = height;
            tempTextBox.FontSize = fontsize;
            return tempTextBox;
        }

        void DeletePress(object sender, EventArgs e, string name) {
            pi.RemoveProject(name);
            projectButtonList.Clear();
            loadbuttons();
            HomePage();
            curretButtonIndex = 0;
        }

        void SavePress(object sender, EventArgs e, string name, object r1, object r2, object r3, string date, string desc, string notes) {
            string state = "0";
            if (((RadioButton)r1).IsChecked == true)
            {
                state = "not-started";
            }
            else if (((RadioButton)r2).IsChecked == true)
            {
                state = "in-progress";
            }
            else if (((RadioButton)r3).IsChecked == true)
            {
                state = "completed";
            }
            else {
                state = "abandoned";
            }

            if ((int)((Button)sender).Tag == -1)
            {

                int statusCode = pi.AddProject(name, desc, date, state, notes);
                if (statusCode == 1)
                {
                    Button newButton = createNewButton(name, Brushes.White, (Brush)bc.ConvertFrom("#6B6B6B"), rootCanvas.Height / 6, (rootCanvas.Width / 4) - 20, 23, projectButtonList.Count);
                    projectButtonList.Add(newButton);
                    buttons.Children.Add(newButton);
                    ButtonClick(projectButtonList[projectButtonList.Count-1], new EventArgs());
                    projectButtons.ScrollToBottom();

                    //
                }
            }

            else if ((int)((Button)sender).Tag == -2)
            { 
                int statusCode = pi.EditProject(name, desc, date, state, notes);
            }
        }

        void keypressed(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                ((TextBox)sender).Text = ((TextBox)sender).Text + "\n";
                ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                ((TextBox)sender).SelectionLength = 0;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            pi.Save();
        }


    }
}
