using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
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
        const int NEURONASCAPAOCULTA = 5;
        const int NEURONASSALIDA = 7;
        const int ENTRADAS = 63;
        const double UMBRAL = 0.1;
        const double ALFA = 0.001;
        const int NEUROACTIVASXNEURONAS = 3;

        //Variables Globales
        int EPOCAS = 1;

        Capa capaOculta;
        Capa capaSalida;
        String[] patrones;
        int npatrones; int totalpruebas;
        String[] targets;
        String[] letras;
        FuncionActivacion funcion;
        List<Rectangle> puntos;

        public MainWindow()
        {
            InitializeComponent();
          
            //Para el area de dibujo de la letra
            puntos = new List<Rectangle>();
            Rectangle temp;
            for (int i = 0; i < ENTRADAS; i++)
            {
                temp = new Rectangle() { Width = 20, Height = 20, Fill = Brushes.White };
                temp.MouseLeftButtonDown += cambio;
                puntos.Add(temp);
                puntosLetra.Children.Add(temp);
            }

            //Para el llenado de combobox
            letras = System.IO.File.ReadAllLines("../../cbox.txt");
            patrones = System.IO.File.ReadAllLines("../../patrones.txt");
            targets = System.IO.File.ReadAllLines("../../targets.txt");
            npatrones = patrones.Length;
            cboxLetras.ItemsSource = letras;

            /////////////////////////////////Inicializacion de objetos necesarios//////////////////////////////////          

            //Instanciacion de la capa oculta y la de salida
            capaOculta = new Capa(NEURONASCAPAOCULTA);
            capaSalida = new Capa(NEURONASSALIDA);

            //Funcion de activacion que se le pasa a la capa, para ser utilizado por todas las neuronas de esa capa
            funcion = new FuncionActivacion();
            capaOculta.FUNCION = funcion;
            capaSalida.FUNCION = funcion;

            //Inicializacion de los pesos de las capas
            Random r = new Random();
            capaOculta.InicializaPesos(UMBRAL, ENTRADAS, r);
            capaSalida.InicializaPesos(UMBRAL, NEURONASCAPAOCULTA, r);

            //mostrando pesos
            for (int i = 0; i < NEURONASCAPAOCULTA; i++)
            {
                cboxCapaOculta.Items.Add($"Neurona {i + 1}");
            }
            for (int i = 0; i < NEURONASSALIDA; i++)
            {
                cboxCapaSalida.Items.Add($"Neurona {i + 1}");
            }
            cboxCapaSalida.SelectedIndex = 0;
            cboxCapaOculta.SelectedIndex = 0;

            //Iniciamos Entrenamiento 

        }


        private void actualizarListEntradas()
        {
            gridEntradas.Children.Clear();            
            int t = 1;
            String txt;
            foreach (var item in puntos)
            {
                if (item.Fill.Equals(Brushes.Black))
                {
                    txt = $"Entrada #{t}:\t 1";
                }
                else
                {
                    txt = $"Entrada #{t}:\t 0";
                }
                gridEntradas.Children.Add(new TextBlock() { Foreground = Brushes.White, Text = txt });
                t++;
            }
        }
        private void actualizarFYCapaOculta()
        {
            gridCapaOculta.Children.Clear();                
                int t = 1;
                foreach (var item in capaOculta.NEURONAS)
                {
                    gridCapaOculta.Children.Add(new TextBlock() { Foreground = Brushes.White, Text = $"N{t} F(y)={item.FY}", Margin = new Thickness(5, 0, 5, 0) });
                    t++;
                }
        }
        private void actualizarFYCapaSalida()
        {
            gridCapaSalida.Children.Clear();           
            int t = 1;
            foreach (var item in capaSalida.NEURONAS)
            {
                gridCapaSalida.Children.Add(new TextBlock() { Foreground = Brushes.White, Text = $"N{t} F(y)={item.FY}", Margin = new Thickness(5, 0, 5, 0) });
                t++;
            }
        }
        private void actualizarSalidaEsperada()
        {
            gridEsperado.Children.Clear();            
            if (cboxLetras.SelectedIndex != -1)
            {
                int t = 1;
                String txt;
                foreach (var item in targets.ElementAt(cboxLetras.SelectedIndex))
                {
                    if (item == '1')
                    {
                        txt = $"N #{t}:\t 1";
                    }
                    else
                    {
                        txt = $"N #{t}:\t -1";
                    }
                    gridEsperado.Children.Add(new TextBlock() { Foreground = Brushes.White, Text = txt });
                    t++;
                }
            }
        }
        private void actualizarErrores()
        {
            int t = 0;
            gridError.Children.Add(new TextBlock() { Text = $"Prueba {totalpruebas}", HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.White });
            foreach (var neurona in capaSalida.NEURONAS)
            {
                gridError.Children.Add(new TextBlock() { Text = $"N #{t+1}:\t{neurona.E}", Foreground = Brushes.White });
                t++;
            }
        }
        private void actualizarListaPesos()
        {
            int c = 1;
            gridWCapaOculta.Children.Clear();
            foreach (var peso in capaOculta.NEURONAS.ElementAt(cboxCapaOculta.SelectedIndex).PESOS)
            {
                gridWCapaOculta.Children.Add(new TextBlock() { Text = $"W{cboxCapaOculta.SelectedIndex + 1}{c}:\t{peso}", Foreground = Brushes.White });
                c++;
            }
            c = 1;
            gridWCapaSalida.Children.Clear();
            foreach (var peso in capaSalida.NEURONAS.ElementAt(cboxCapaSalida.SelectedIndex).PESOS)
            {
                gridWCapaSalida.Children.Add(new TextBlock() { Text = $"W{cboxCapaSalida.SelectedIndex + 1} {c}:\t{peso}", Foreground = Brushes.White });
                c++;
            }
        }
        private void actualizarResultado()
        {
            List<double> mayores = new List<double>();
            var Lista = capaSalida.obtenerfys();           

            for (int i = 0; i < NEUROACTIVASXNEURONAS; i++)
            {                
                mayores.Add(Lista.Max());
                Lista.RemoveAt(Lista.IndexOf(mayores.Last()));
            }

            String t = "";
            String salidabinaria = "";
            foreach (var item in capaSalida.obtenerfys())
            {
                t = "0";
                foreach (var item2 in mayores)
                {
                    if (item2 == item)
                    {
                        t = "1";
                    }
                }
                salidabinaria += t;
            }
            int m = 0;
            foreach (var item in targets)
            {
                if (item.Equals(salidabinaria))
                {
                    gridResultado.Children.Add(new TextBlock() { Text = $"Prueba #{totalpruebas}:\t{letras.ElementAt(m)}" , Foreground = Brushes.White });
                }
                m++;
            }

        }
        private void VerificarLetra(object sender, RoutedEventArgs e)
        {
            totalpruebas++;
            List<double> entrada = new List<double>();
            foreach (var punto in puntos)
            {
                if (punto.Fill.Equals(Brushes.Black))
                {
                    entrada.Add(1);
                }
                else
                {
                    entrada.Add(0);
                }
            }
            capaOculta.ENTRADAS = entrada;
            capaOculta.CalcularY();
            capaOculta.CalcularFy();
            capaSalida.ENTRADAS = capaOculta.obtenerfys();
            capaSalida.CalcularY();
            capaSalida.CalcularFy();
            actualizarFYCapaOculta();
            actualizarFYCapaSalida();
            if (cboxLetras.SelectedIndex>-1)
            {
                capaSalida.CalcularE( getTargets(cboxLetras.SelectedIndex));
                actualizarErrores();
            }
            actualizarResultado();
            
        }
        private void Entrenar(object sender, RoutedEventArgs e)
        {
            List<double> entradas;
            if(!txtepocas.Text.Equals(""))EPOCAS = int.Parse(txtepocas.Text);

            //Ciclo externo, segun el numero de epocas
            for (int i = 0; i < EPOCAS; i++)
            {

                //Ciclo Interno, segun el numero de patrones de muestra
                for (int j = 0; j < npatrones; j++)
                {
                    //<----Paso 1----> ///////Propagar hacia adelante////////////////////////////////////////////////////////////////////////////

                    //Le pasamos las entradas recolecadas del archivo a la lista de entradas de la capa oculta
                    entradas = new List<double>();
                    for (int k = 0; k < patrones[j].Length; k++)
                    {
                        entradas.Add(Double.Parse(patrones[j].Substring(k, 1)));
                    }
                    capaOculta.ENTRADAS = entradas;

                    //Calculamos Y, Fy de la primera capa
                    capaOculta.CalcularY();
                    capaOculta.CalcularFy();

                    //Propagamos el vector Fy como entradas de la capa de salida                    
                    capaSalida.ENTRADAS = capaOculta.obtenerfys();

                    //Calculamos Y, Fy de la segunda capa
                    capaSalida.CalcularY();
                    capaSalida.CalcularFy();

                    //<----Paso 2----> ///////Propagar hacia atras////////////////////////////////////////////////////////////////////////////////

                    //Calculo del error, hay que transformar la cadena de estring a numeros, y los ceros a -1                    
                    capaSalida.CalcularE(getTargets(j));

                    //Calcular la sensibilidad de la ultima capa
                    capaSalida.CalcularSM();

                    //Calcular la sensibilidad de la capa oculta y de paso actualizamos pesos
                    capaOculta.CalcularS(capaSalida.WtS());

                    //actualizamos pesos
                    capaSalida.ActualizarPesos(ALFA);
                    capaOculta.ActualizarPesos(ALFA);
                    actualizarListaPesos();
                    

                }

            }
            //actualizar lls fy
            actualizarListaPesos();
        }
        private List<double> getTargets(int n)
        {
            List<double> t = new List<double>();
            foreach (var item in targets.ElementAt(n))
            {
                if (item.Equals('0'))
                {
                    t.Add(-1);
                }
                else
                {
                    t.Add(1);
                }
            }
            return t;
        }
        private void cambio(object sender, RoutedEventArgs e)
        {
            var temp = (Rectangle)e.Source;
            if (temp.Fill.Equals(Brushes.White))
            {
                temp.Fill = Brushes.Black;
            }
            else
            {
                temp.Fill = Brushes.White;
            }
            actualizarListEntradas();
            actualizarSalidaEsperada();
            cboxLetras.SelectedItem = null;
        }
        private void Limpiar(object sender, RoutedEventArgs e)
        {
            foreach (var item in puntos)
            {
                item.Fill = Brushes.White;
            }
            cboxLetras.SelectedIndex = -1;
        }
        private void cboxCapaOculta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int c = 1;
            gridWCapaOculta.Children.Clear();
            foreach (var peso in capaOculta.NEURONAS.ElementAt((sender as ComboBox).SelectedIndex).PESOS)
            {
                gridWCapaOculta.Children.Add(new TextBlock() { Text = $"W{(sender as ComboBox).SelectedIndex + 1}{c}:\t{peso}", Foreground = Brushes.White });
                c++;
            }
        }
        private void cboxCapaSalida_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int c = 1;
            gridWCapaSalida.Children.Clear();
            foreach (var peso in capaSalida.NEURONAS.ElementAt((sender as ComboBox).SelectedIndex).PESOS)
            {
                gridWCapaSalida.Children.Add(new TextBlock() { Text = $"W{(sender as ComboBox).SelectedIndex + 1} {c}:\t{peso}", Foreground = Brushes.White });
                c++;
            }
        }
        private void cboxLetras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboxLetras.SelectedIndex > -1)
            {
                for (int i = 0; i < patrones.ElementAt(cboxLetras.SelectedIndex).Length; i++)
                {
                    if (patrones.ElementAt(cboxLetras.SelectedIndex).ElementAt(i) == '1')
                    {
                        puntos.ElementAt(i).Fill = Brushes.Black;
                    }
                    else
                    {
                        puntos.ElementAt(i).Fill = Brushes.White;
                    }
                }
            }
            actualizarListEntradas();
            actualizarSalidaEsperada();
        }
        private void txtepocas_TextChanged(object sender, TextChangedEventArgs e)
        {
            String numero = "";
            var caja = sender as TextBox;
            foreach (var item in caja.Text)
            {
                if (item >= '0' && item <= '9')
                {
                    numero += item;
                }
            }
            caja.Text = numero;
        }
    }

    class Capa
    {
        public Capa(int s)
        {
            neuronas = new List<Neurona>();
            for (int i = 0; i < s; i++)
            {
                neuronas.Add(new Neurona());
            }
        }

        public void InicializaPesos(double umbral,int nPesos,Random r)
        {
            foreach (var neurona in neuronas)
            {
                neurona.InicializaPesos(umbral,nPesos, r);
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

        public void CalcularE(List<double> target)
        {
            for (int i = 0; i < neuronas.Count; i++)
            {
                neuronas.ElementAt(i).CalcularE(target.ElementAt(i));
            }
        }

        public List<double> obtenerfys()
        {
            List<double> s = new List<double>();
            foreach (var neurona in neuronas)
            {
                s.Add(neurona.FY);
            }
            return s;
        }

        public List<double> WtS()
        {
            List<double> r = new List<double>();
            for (int i = 0; i < entradas.Count; i++)
            {
                r.Add(0);
            }
            int temp = 0;
            foreach (var neurona in neuronas)
            {
                foreach (var peso in neurona.PESOS)
                {
                    r[temp] += peso* neurona.S;
                    temp++;
                }
                temp = 0;
            }
            return r;
        }

        public void CalcularSM()
        {
            foreach (var neurona in neuronas)
            {
                neurona.CalcularSM(funcion);
            }
        }

        public void CalcularS(List<double> WtS)
        {
            int t = 0;
            foreach (var neurona in neuronas)
            {
                neurona.CalcularS(funcion, WtS[t]);
                t++;
            }
        }

        public void ActualizarPesos(double alfa)
        {
            for (int i = 0; i < neuronas.Count; i++)
            {
                for (int j = 0; j < neuronas[i].PESOS.Count; j++)
                {
                    neuronas[i].PESOS[j] -= alfa * neuronas[i].S * entradas[j];
                }
                neuronas[i].B -= alfa * neuronas[i].S;
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
        public void InicializaPesos(double umbral, int tamaño,Random r)
        {
            pesos = new List<double>();
            b = 2*umbral*r.NextDouble()-umbral;
            for (int i = 0; i < tamaño; i++)
            {
                pesos.Add(2 *umbral*r.NextDouble() - umbral);
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
            return y;
        }

        public double CalcularFy(FuncionActivacion funcion)
        {
            fy=funcion.evaluar(y);
            return fy;
        }

        public double CalcularE(double target)
        {
            e = target - fy;
            return e;
        }

        public double CalcularSM(FuncionActivacion funcion)
        {
            s = -2 * funcion.evaluarF(y) * e;
            return s;
        }

        public double CalcularS(FuncionActivacion funcion, double wts)
        {
            s = funcion.evaluarF(y) * wts;
            return s;
        }


        ////////////////////////////Propieties////////////////////////////
        public List<double> PESOS { set => pesos = value; get => pesos; }
        public double Y { set => y = value; get => y; }
        public double FY { set => fy = value; get => fy; }
        public double B { set => b = value; get => b; }
        public double E { set => e = value; get => e; }
        public double S { set => s = value; get => s; }
        //////////////////////Definicion de variables/////////////////////
        private double y;
        private double fy;
        private double b;
        private double e;
        private double s;
        private List<double> pesos;
    }

    class FuncionActivacion
    {
        public double evaluar(double y)
        {
            return ((Math.Exp(y) - Math.Exp(-y)) / (Math.Exp(y) + Math.Exp(-y)));
        }

        public double evaluarF(double fy)
        {
            return (1-(fy*fy));
        }
    }

}
