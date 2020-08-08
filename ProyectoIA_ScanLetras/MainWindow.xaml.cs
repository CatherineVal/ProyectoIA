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
        const int NEURONASCAPAOCULTA = 3;
        const int NEURONASSALIDA = 5;
        const int ENTRADAS = 63;
        const int EPOCAS = 10;
        const double UMBRAL = 1;
        const double ALFA = 0.0001;

        Capa capaOculta;
        Capa capaSalida;
        String[] patrones;
        int npatrones;
        String[] targets;
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
            ////////////////////////////////////

            //Para el llenado de combobox
            String letras = "ABCDEJK";
            for (int i = 0; i < letras.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    cboxLetras.Items.Add($"Letras {letras[i]} tipo {j+1}");
                }
            }

            patrones = System.IO.File.ReadAllLines("../../patrones.txt");
            npatrones = patrones.Length;
            targets = System.IO.File.ReadAllLines("../../targets.txt");
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
            capaOculta.InicializaPesos(UMBRAL, ENTRADAS,r);
            capaSalida.InicializaPesos(UMBRAL, NEURONASCAPAOCULTA,r);



            ////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////Inicio del Entrenamiento retropropagacion//////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////

            List<double> entradas;

            //Ciclo externo, segun el numero de epocas
            for (int i = 0; i < EPOCAS; i++)
            {
                if (i>0 && capaSalida.NEURONAS.First().E > -0.1 && capaSalida.NEURONAS.First().E < 0.1)
                {
                    break;
                }
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
                    List<double> t = new List<double>();
                    foreach (var item in targets.ElementAt(j))
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
                    capaSalida.CalcularE(t);

                    //Calcular la sensibilidad de la ultima capa
                    capaSalida.CalcularSM();

                    if (j == 0)
                    {
                        salida.Text = salida.Text + "\n" + capaSalida.NEURONAS.First().E;
                    }

                    //Calcular la sensibilidad de la capa oculta y de paso actualizamos pesos
                    Neurona neuronaTemp;
                    for (int l = 0; l < capaSalida.NEURONAS.Count; l++)
                    {
                        neuronaTemp = capaSalida.NEURONAS.ElementAt(l);
                        for (int k = 0; k < capaOculta.NEURONAS.Count; k++)
                        {
                            double sigw = neuronaTemp.PESOS.ElementAt(k);
                            double sigs = neuronaTemp.S;
                            capaOculta.NEURONAS.ElementAt(k).CalcularS(funcion, sigw, sigs);
                        }

                        //Actualizamos los pesos sinapticos de la ultima capa                        
                        List<double> wtemp = new List<double>();
                        for (int n = 0; n < neuronaTemp.PESOS.Count; n++)
                        {
                            wtemp.Add(neuronaTemp.PESOS.ElementAt(n) - ALFA * neuronaTemp.S * capaSalida.ENTRADAS.ElementAt(n));
                        }
                        neuronaTemp.PESOS = wtemp;
                        neuronaTemp.B -= ALFA * neuronaTemp.S;

                        //Actualizamos los pesos sinapticos de la capa oculta
                        wtemp = new List<double>();
                        foreach (var neurona in capaOculta.NEURONAS)
                        {
                            for (int m = 0; m < neurona.PESOS.Count; m++)
                            {
                                wtemp.Add(neurona.PESOS.ElementAt(m) - ALFA * neurona.S * capaOculta.ENTRADAS.ElementAt(m));
                            }
                            neurona.B -= ALFA * neurona.S;
                        }

                    }
                    if (capaSalida.NEURONAS.First().E < 0.1)
                    {
                        break;
                    }

                }

            }

            /////////////////////////////////////////Fin Entrenamiento/////////////////////////////////////////////////////////
        }

        private void Entrenar(object sender, RoutedEventArgs e)
        {
            salida.Text = "";
            List<double> entradas;

            //Ciclo externo, segun el numero de epocas
            for (int i = 0; i < EPOCAS; i++)
            {
                if (i > 0 && capaSalida.NEURONAS.First().E > -0.1 && capaSalida.NEURONAS.First().E<0.1)
                {
                    break;
                }
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
                    List<double> t = new List<double>();
                    foreach (var item in targets.ElementAt(j))
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
                    capaSalida.CalcularE(t);

                    //Calcular la sensibilidad de la ultima capa
                    capaSalida.CalcularSM();

                    if (j == 0)
                    {
                        salida.Text = salida.Text + "\n" + capaSalida.NEURONAS.First().E;
                    }

                    //Calcular la sensibilidad de la capa oculta y de paso actualizamos pesos
                    Neurona neuronaTemp;
                    for (int l = 0; l < capaSalida.NEURONAS.Count; l++)
                    {
                        neuronaTemp = capaSalida.NEURONAS.ElementAt(l);
                        for (int k = 0; k < capaOculta.NEURONAS.Count; k++)
                        {
                            double sigw = neuronaTemp.PESOS.ElementAt(k);
                            double sigs = neuronaTemp.S;
                            capaOculta.NEURONAS.ElementAt(k).CalcularS(funcion, sigw, sigs);
                        }

                        //Actualizamos los pesos sinapticos de la ultima capa                        
                        List<double> wtemp = new List<double>();
                        for (int n = 0; n < neuronaTemp.PESOS.Count; n++)
                        {
                            wtemp.Add(neuronaTemp.PESOS.ElementAt(n) - ALFA * neuronaTemp.S * capaSalida.ENTRADAS.ElementAt(n));
                        }
                        neuronaTemp.PESOS = wtemp;
                        neuronaTemp.B -= ALFA * neuronaTemp.S;

                        //Actualizamos los pesos sinapticos de la capa oculta
                        wtemp = new List<double>();
                        foreach (var neurona in capaOculta.NEURONAS)
                        {
                            for (int m = 0; m < neurona.PESOS.Count; m++)
                            {
                                wtemp.Add(neurona.PESOS.ElementAt(m) - ALFA * neurona.S * capaOculta.ENTRADAS.ElementAt(m));
                            }
                            neurona.B -= ALFA * neurona.S;
                        }

                    }

                    if (capaSalida.NEURONAS.First().E < 0.1)
                    {
                        break;
                    }
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
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
        }

        private void VerificarLetra()
        {

        }

        private void cboxLetras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboxLetras.SelectedIndex>-1)
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
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var item in puntos)
            {
                item.Fill = Brushes.White;
            }
            cboxLetras.SelectedIndex = -1;
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

        public void CalcularSM()
        {
            foreach (var neurona in neuronas)
            {
                neurona.CalcularSM(funcion);
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

        public double CalcularS(FuncionActivacion funcion, double wm, double sm)
        {
            s = funcion.evaluarF(y) * sm * wm;
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
