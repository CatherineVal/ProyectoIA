using System;
using System.Collections.Generic;
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

namespace ProyectoIA_ScanLetras
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 63; i++)
            {
                letra.Children.Add(new RadioButton() {Name="rb"+i});
            }
            for (int i = 0; i < 9; i++)
            {
                botonesd.Children.Add(new Button() { Name = "btd" + i, Height=14 });
            }
            for (int i = 0; i < 7; i++)
            {
                botonesi.Children.Add(new Button() { Name = "bti" + i, Width=15});
            }
            for (int i = 0; i < 3; i++)
            {
                tipos.Items.Add("Letra A Tipo " + (i+1));
                tipos.Items.Add("Letra B Tipo " + (i+1));
                tipos.Items.Add("Letra C Tipo " + (i+1));
            }
        }




        ////////////////////////////////Metodos/////////////////////////////////


        //////////////////////definicion de variables//////////////////////////
        private double alfa = 0.001;
    }


    class Capa
    {
        public void InicializaPesos(int umbral)
        {
            foreach (var neurona in neuronas)
            {
                neurona.InicializaPesos(umbral, entradas.Count);
            }
        }

        public void CalcularY()
        {
            foreach (var neurona in neuronas)
            {
                neurona.CalcularY(entradas);
            }
        }

        public void CalcularFy()
        {
            foreach (var neurona in neuronas)
            {
                neurona.CalcularFy(funcion);
            }
        }

        public List<double> ENTRADAS { set => entradas = value; get => entradas; }
        public List<Neurona> NEURONAS { set => neuronas = value; get => neuronas; }
        public FuncionActivacion FUNCION { set => funcion = value; get => funcion; }
        private List<Neurona> neuronas;
        private List<double> entradas;
        private FuncionActivacion funcion;
}

    class Neurona
    {
        public void InicializaPesos(int umbral, int tamaño)
        {
            pesos = new List<double>();
            Random r = new Random();
            b = r.Next(-umbral, umbral);
            for (int i = 0; i < tamaño; i++)
            {
                pesos.Add(r.Next(-umbral, umbral));
            }         
        }

        public double CalcularY(List<double> entradas)
        {
            double sum = 0;
            for (int i = 0; i < pesos.Count; i++)
            {
                sum += pesos.ElementAt(i) * entradas.ElementAt(i);
            }
            sum += b;
            y = sum;
            return sum;
        }

        public double CalcularFy(FuncionActivacion funcion)
        {
            fy=funcion.evaluar(y);
            return fy;
        }

        ////////////////////////////Propieties////////////////////////////
        public List<double> PESOS { set => pesos = value; get => pesos; }
        public double Y { set => y = value; get => y; }
        public double FY { set => fy = value; get => fy; }
        public double B { set => b = value; get => b; }
        //////////////////////Definicion de variables/////////////////////
        private double y;
        private double fy;
        private double b;
        private List<double> pesos;
    }

    class FuncionActivacion
    {
        public double evaluar(double y)
        {
            return ((Math.Exp(y) - Math.Exp(-y)) / (Math.Exp(y) + Math.Exp(-y)));
        }
    }
}
