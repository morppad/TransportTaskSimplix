using System;
using System.Collections.Generic;
using System.Data;
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

namespace TransportTask2
{
    public partial class MainWindow : Window
    {
        DataTable data;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btCreate_Click(object sender, RoutedEventArgs e)
        {
            if (xCell.Text == "" | yCell.Text == "") MessageBox.Show("Введите размерность матрицы!", "Внимание");
            else CreateTable(int.Parse(xCell.Text), int.Parse(yCell.Text));
        }
        private void btCalc_Click(object sender, RoutedEventArgs e)
        {
            int a = 1;
            if (!int.TryParse(xCell.Text, out a) | !int.TryParse(yCell.Text, out a))
            {
                MessageBox.Show("Введите размерность матрицы!", "Внимание");

                return;
            }
            DataTable answer = mainMethod(data);
            DataGridResult.ItemsSource = answer.DefaultView;
             lbAnsw.Content = "F= " + Math.Round(PrintAnswer(answer), 3).ToString();
        }
        private void CreateTable(int x, int y)
        {
            data = new DataTable();
            int nbColumns = x + 1;
            int nbRows = y + 1;
            for (int i = 0; i < nbColumns; i++)
            {
                data.Columns.Add(i.ToString(), typeof(double));
            }
            for (int row = 0; row < nbRows; row++)
            {
                DataRow dr = data.NewRow();
                for (int col = 0; col < nbColumns; col++)
                {
                    dr[col] = 0;
                }
                data.Rows.Add(dr);
            }
            DataGridStart.ItemsSource = data.DefaultView;
        }
        private DataTable AddVal(DataTable result)
        {
            int count = 0;
            for (int i = 0; i < int.Parse(yCell.Text); i++)
            {
                DataColumn col = result.Columns.Add((int.Parse(xCell.Text) + i + 1).ToString(), typeof(double));
                col.SetOrdinal(result.Columns.Count - 2);
                for (int j = 0; j < int.Parse(yCell.Text) + 1; j++)
                {
                    result.Rows[j][result.Columns.Count - 2] = 0;
                }
                result.Rows[i][result.Columns.Count - 2] = 1;
                count++;
            }
            return result;
        }
        private double[] KeyColumn(DataTable result)
        {
            double[] max = new double[] { 0, 0, double.MinValue };
            double min = double.MaxValue;
            int[] buf = new int[2];
            for (int i = 0; i < result.Columns.Count - 1; i++)
            {
                if ((double)result.Rows[result.Rows.Count - 1][i] <= 0)
                {
                    continue;
                }
                min = double.MaxValue;
                for (int j = 0; j < result.Rows.Count - 1; j++)
                {
                    if ((double)result.Rows[j][i] > 0 && (double)result.Rows[j][result.Columns.Count - 1] / (double)result.Rows[j][i] < min)
                    {
                        min = (double)result.Rows[j][result.Columns.Count - 1] / (double)result.Rows[j][i];
                        buf[0] = i; buf[1] = j;
                    }
                }
                if (min * (double)result.Rows[result.Rows.Count - 1][i] > max[2])
                {
                    max[0] = buf[0]; max[1] = buf[1]; max[2] = min * (double)result.Rows[result.Rows.Count - 1][i];
                }
            }
            return max;
        }
        private DataTable Algorithm(double[] coord, DataTable result)
        {
            int row = (int)coord[1];
            int col = (int)coord[0];
            double start;
            double perm;
            if ((double)result.Rows[row][col] != 1)
            {
                start = (double)result.Rows[row][col];
                for (int x = 0; x < result.Columns.Count; x++)
                {
                    result.Rows[row][x] = (double)result.Rows[row][x] / start;
                }
            }
            for (int y = 0; y < result.Rows.Count; y++)
            {
                if (y == row | (double)result.Rows[y][col] == 0)
                {
                    continue;
                }
                perm = (double)result.Rows[y][col];
                if (y != result.Rows.Count - 1)
                {
                    for (int i = 0; i < result.Columns.Count; i++)
                    {
                        result.Rows[y][i] = (double)result.Rows[y][i] - (double)result.Rows[row][i] * perm;
                    }
                }
                else
                {
                    for (int i = 0; i < result.Columns.Count - 1; i++)
                    {
                        result.Rows[y][i] = (double)result.Rows[y][i] - (double)result.Rows[row][i] * perm;
                    }
                }
            }
            return result;
        }
        private Boolean CheckNull(DataTable result)
        {
            for (int x = 0; x < result.Columns.Count; x++)
            {
                if ((double)result.Rows[result.Rows.Count - 1][x] > 0)
                {
                    return true;
                }
            }
            return false;
        }
        private double PrintAnswer(DataTable result)
        {
            double answ = 0;
            int xi = 0;
            for (int x = 0; x < int.Parse(xCell.Text); x++)
            {
                if ((double)result.Rows[result.Rows.Count - 1][x] < 0)
                {
                    xi += 1;
                    tbRes.Text += $"x{xi} = 0\n";
                }
                if ((double)result.Rows[result.Rows.Count - 1][x] == 0)
                {
                    for (int y = 0; y < result.Rows.Count - 1; y++)
                    {
                        if ((double)result.Rows[y][x] == 1)
                        {
                            answ += (double)result.Rows[y][result.Columns.Count - 1] * (double)data.Rows[result.Rows.Count - 1][x];
                            xi += 1;
                            tbRes.Text += $"x{xi} = {(double)result.Rows[y][result.Columns.Count - 1]}\n";
                        }
                    }

                }

            }
            return answ;
        }
        private DataTable mainMethod(DataTable data)
        {
            DataTable result = data.Copy();
            AddVal(result);
            double[] coord = new double[3];
            while (CheckNull(result))
            {
                coord = KeyColumn(result);
                Algorithm(coord, result);
            }
            return result;
        }
    }
}
