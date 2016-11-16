using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

/*
Critical Tattoo Data Input Utility
Program checks for multi or single input radio clicks. Allows
user to type in serial number range. Checkbox is checked and automatically
adds prefix to the serial number. 
Adds data into a excel document that has sheets per type of product.
1.00 : initial release to employees
1.01 : added progress bar
1.02 : removed successfully added prompt if there was a failure

*/

namespace CTSerialLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int serialInputStart = 0;
        int serialInputEnd = 0;
        int serialLength = 0;
        string prodInputRevision = "";
        string productSerialPrfix = "";
        string prodSheetSelect = "";
        string mfgDate = "";
        int numOfProd = 0;
        string prodNotes = "";
        int isSuccesfull = 0;


        //string dbPath = @"C:\temp\CriticalProdDBtemp.xlsx";
        string dbPath = @"p:\criticalos\seriallookup\CriticalProdDB.xlsx";


        public MainWindow()
        {
            InitializeComponent();
           
            
            prodCompleteDatePicker.SelectedDate = DateTime.Today; //sets date picker to current date

        }

        private void insertDataButton_Click(object sender, RoutedEventArgs e)
        {
            prodNotes = NotestextBox.Text;
            isSuccesfull = 0;
            if (singleItemInputRadio.IsChecked == true)
            {
                singleItemInputMethod();
            }
            else if (multiItemInputRadio.IsChecked == true)
            {
                multiItemInputMethod();
            }

        }

        private void multiItemInputMethod()
        {
            int.TryParse(serialInputStartBox.Text, out serialInputStart);
            int.TryParse(serialInputEndBox.Text, out serialInputEnd);
            prodInputRevision = prodInputRevisionTextBox.Text;

            mfgDate = prodCompleteDatePicker.SelectedDate.Value.ToString("d");


            if (prodSelectComboBox.Text == "Choose Product") //verifies user input is a number
            {
                MessageBox.Show("Choose Product Type.");
            }
            else if (serialInputStart == 0 || serialInputEnd == 0)
            {
                MessageBox.Show("Enter a valid serial number.");
            }
            else if (prodInputRevisionTextBox.Text == "")
            {
                MessageBox.Show("Enter product revision.");
            }
            else if (serialInputStart > serialInputEnd)
            {
                MessageBox.Show("Serial number start is larger than serial end.", "Check serial range!");
            }
            else
            {
                numOfProd = (serialInputEnd - serialInputStart)+1;
                MessageBoxResult dialogResult = MessageBox.Show("About to add " + numOfProd + " " + prodSelectComboBox.Text + "s to Database. \n" + productSerialPrfix + serialInputStart.ToString("00000") + " - " + productSerialPrfix + serialInputEnd.ToString("00000"),"Are you sure?", MessageBoxButton.YesNo);

                if (dialogResult == MessageBoxResult.Yes)
                {            
                    insertMultiDataIntoTable(serialInputStart);
                }
                else if (dialogResult == MessageBoxResult.No)
                {
                     //TODO: make something happen when user selects NO
                }
            }
        }

        private void singleItemInputMethod()
        {
            int.TryParse(serialInputStartBox.Text, out serialInputStart);
            
            prodInputRevision = prodInputRevisionTextBox.Text;

            mfgDate = prodCompleteDatePicker.SelectedDate.Value.ToString("d");


            if (prodSelectComboBox.Text == "Choose Product") //verifies user input is a number
            {
                MessageBox.Show("Choose Product Type.");
            }
            else if (serialInputStart == 0)
            {
                MessageBox.Show("Enter a valid serial number.");
            }
            else if (prodInputRevisionTextBox.Text == "")
            {
                MessageBox.Show("Enter product revision.");
            }
            else
            {
                MessageBoxResult dialogResult = MessageBox.Show("About to add 1 " + prodSelectComboBox.Text + ": " + productSerialPrfix + serialInputStart.ToString("00000") + " to Database.", "Are you sure?", MessageBoxButton.YesNo);
     
                if (dialogResult == MessageBoxResult.Yes)
                {
                    insertSingleDataIntoTable();
                }
                else if (dialogResult == MessageBoxResult.No)
                {
                    //TODO: make something happen when user selects NO
                }
            }
        }

        private void prefixSet_comboBoxClosed(object sender, EventArgs e) //when item selected in combobox, it sets prefix
        {
            switch (prodSelectComboBox.Text)
            {
                case "Choose Product":
                    productSerialPrfix = "";
                    break;
                case "CX-1":
                    productSerialPrfix = "1X";
                    prodSheetSelect = "[cx1$]";
                    break;
                case "CX-2":
                    productSerialPrfix = "2B";
                    prodSheetSelect = "[cx2$]";
                    break;
                case "CX-2R":
                    productSerialPrfix = "2R";
                    prodSheetSelect = "[cx2r$]";
                    break;
                case "Atom":
                    productSerialPrfix = "AT";
                    prodSheetSelect = "[atom$]";
                    break;
                case "CXP (wireless)":
                    productSerialPrfix = "CXP";
                    prodSheetSelect = "[cxp$]";
                    break;
                case "CXP (wired)":
                    productSerialPrfix = "CXW";
                    prodSheetSelect = "[cxw$]";
                    break;

            }
            serialPrefixLabel1.Content = productSerialPrfix;
            serialPrefixLabel2.Content = productSerialPrfix;
        }

        private void singleUnitRadio_click(object sender, RoutedEventArgs e)
        {
            serialInputEndBox.Visibility = Visibility.Hidden;
            serialEndLabel.Visibility = Visibility.Hidden;
            serialPrefixLabel2.Visibility = Visibility.Hidden;
            serialStartLabel.Visibility = Visibility.Hidden;
            inputDataLabel.Content = "Enter Serial Number: ";
            serialInputEnd = 1;
            label1.Visibility = Visibility.Hidden;
            
        }

        private void multiUnitRadio_click(object sender, RoutedEventArgs e)
        {
            serialInputEndBox.Visibility = Visibility.Visible;
            serialEndLabel.Visibility = Visibility.Visible;
            serialPrefixLabel2.Visibility = Visibility.Visible;
            serialStartLabel.Visibility = Visibility.Visible;
            inputDataLabel.Content = "Enter Serial Range: ";
            serialInputEnd = 0;
            label1.Visibility = Visibility.Visible;
        }

        private void windowDrag_event(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void insertMultiDataIntoTable(int serialNum) //adds unser input data into excel table
        {

            //multi serial input
            serialLength = (serialInputStartBox.Text.Length);
            progBar.Maximum = numOfProd-1;
            string strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + dbPath + ";Extended Properties='Excel 12.0;'";
            using (OleDbConnection oleConn = new OleDbConnection(strConn))
            {
                try
                {
                    oleConn.Open();
                    OleDbCommand command = new OleDbCommand();
                    command.Connection = oleConn;
                    for (int i = 0; i < numOfProd; i++)
               
                    {                //add notes section to input  into database        
                        command.CommandText = string.Format("INSERT INTO {0} ([Date], [Serial], [Rev], [Notes]) VALUES ('{1}', '{2}', '{3}', '{4}')", prodSheetSelect, mfgDate, productSerialPrfix + serialNum.ToString("00000"), prodInputRevision.ToUpper(), prodNotes);
                        command.ExecuteNonQuery();
                        serialNum++;
                        //MessageBox.Show(".");
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                        {
                            progBar.Value = i;
                        }));
                        isSuccesfull = 1;
                    }
                    
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
            if (isSuccesfull == 1)
            {
                MessageBox.Show("Serials successfully added to Database!");
                clearAllInputData();
            }
            else
            {
                MessageBox.Show("Please check data and try again.");
            }
        }

        private void insertSingleDataIntoTable() //adds unser input data into excel table
        {

            //SINGLE ITEM INPUT
            serialLength = (serialInputStartBox.Text.Length);
            string strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + dbPath + ";Extended Properties='Excel 12.0;'";
            using (OleDbConnection oleConn = new OleDbConnection(strConn))
            {
                try
                {
                    oleConn.Open();
                    OleDbCommand command = new OleDbCommand();
                    command.Connection = oleConn;

                    command.CommandText = string.Format("INSERT INTO {0} ([Date], [Serial], [Rev], [Notes]) VALUES ('{1}', '{2}', '{3}', '{4}')", prodSheetSelect, mfgDate, productSerialPrfix + serialInputStart.ToString("00000"), prodInputRevision.ToUpper(), prodNotes);
                    command.ExecuteNonQuery();
                    isSuccesfull = 1;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    //TODO: make a bool switch true or false on successfull or exception
                }

            }
            if (isSuccesfull == 1) {
                MessageBox.Show("Serial successfully added to Database!");
                clearAllInputData();
            }
            else
            {
                MessageBox.Show("Please check data and try again.");
            }
        }

        private void clearAllInputData()
        {
            serialInputStart = 0;
            serialInputEnd = 0;
            prodInputRevision = "";
            //productSerialPrfix = "";
            //prodSheetSelect = "";
            mfgDate = "";
            numOfProd = 0;
            serialLength = 0;

            serialInputStartBox.Text = "";
            serialInputEndBox.Text = "";
            prodInputRevisionTextBox.Text = "";
            progBar.Value = 0;

        }

        private void button_Click(object sender, RoutedEventArgs e) //closes application
        {
            Application.Current.Shutdown();
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e) //minimize
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Serial Number Logger\nVersion: 1.02\nCreated by: Chris Bryant\n©2016", "About");
        }
    }

    
}
