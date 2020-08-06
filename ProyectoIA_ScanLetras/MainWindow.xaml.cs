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
        const int NEURONASCAPAOCULTA = 3;
        const int NEURONASSALIDA = 2;
        const int ENTRADAS = 2;
        const int EPOCAS = 20;
        const double ALFA = 0.001;

        public MainWindow()
        {
            // InitializeComponent();

            //carga de los patrones contenidos en el el archivo
            String[] patrones = System.IO.File.ReadAllLines("../../patrones.txt");
            int npatrones = patrones.Length;
            String[] targets = System.IO.File.ReadAllLines("../../targets.txt");

            /////////////////////////////////Inicializacion de objetos necesarios//////////////////////////////////
            ///            

            //Instanciacion de la capa oculta y la de salida
            Capa capaOculta = new Capa(NEURONASCAPAOCULTA);
            Capa capaSalida = new Capa(NEURONASSALIDA);

            //Funcion de activacion que se le pasa a la capa, para ser utilizado por todas las neuronas de esa capa
            FuncionActivacion funcion = new FuncionActivacion();
            capaOculta.FUNCION = funcion;
            capaSalida.FUNCION = funcion;

            //Inicializacion de los pesos de las capas
            Random r = new Random();
            capaOculta.InicializaPesos(1, ENTRADAS,r);
            capaSalida.InicializaPesos(1, NEURONASCAPAOCULTA,r);



                                        ////////////////////////////////////////////////
            ////////////////////////////////Inicio del Entrenamiento retropropagacion//////////////////////////////
                                        ////////////////////////////////////////////////
                                        
            //Ciclo externo, segun el numero de epocas
            for (int i = 0; i < EPOCAS; i++)
            {
                //Ciclo Interno, segun el numero de patrones de muestra
                for (int j = 0; j < npatrones; j++)
                {
     //<----Paso 1----> ///////Propagar hacia adelante////////////////////////////////////////////////////////////////////////////

                    //Le pasamos las entradas recolecadas del archivo a la lista de entradas de la capa oculta
                    List<double> entradas1 = new List<double>();                    
                    for (int k = 0; k < patrones[j].Length; k++)
                    {
                        entradas1.Add(Double.Parse(patrones[j].Substring(k,1)));
                    }
                    capaOculta.ENTRADAS = entradas1;

                    //Calculamos Y, Fy de la primera capa
                    capaOculta.CalcularY();
                    capaOculta.CalcularFy();

                    //Propagamos el vector Fy como entradas de la capa de salida
                    List<double> entradas2 = new List<double>();
                    foreach (var neurona in capaOculta.NEURONAS)
                    {
                        entradas2.Add(neurona.FY);
                    }
                    capaSalida.ENTRADAS = entradas2;

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
                    foreach (var neurona in capaSalida.NEURONAS)
                    {
                        neurona.CalcularSM(funcion);
                    }

                    


                }
               
            }


        }



        //    double sigw = sigCapa.neuronas.ElementAt(0).PESOS.ElementAt(0);
        //    double sigs = sigCapa.neuronas.ElementAt(0).S;
        //    //obtengo la neurona objetivo 
        //    for (int i = 0; i < neuronas.Count; i++)
        //    {
        //        neuronas.ElementAt(i).CalcularS(funcion, sigCapa.neuronas.ElementAt(i).PESOS.ElementAt(i), sigCapa.);
        //    }
        //}

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

        public void InicializaPesos(int umbral,int nPesos,Random r)
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

        public List<double> ENTRADAS { set => entradas = value; get => entradas; }
        public List<Neurona> NEURONAS { set => neuronas = value; get => neuronas; }
        public FuncionActivacion FUNCION { set => funcion = value; get => funcion; }
        private List<Neurona> neuronas;
        private List<double> entradas;
        private FuncionActivacion funcion;
}

    class Neurona
    {
        public void InicializaPesos(int umbral, int tamaño,Random r)
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
            return 1-(fy*fy);
        }
    }
}
