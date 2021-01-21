using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace tema2
{
    public partial class Form1 : Form
    {
        int k;
        int normalizare;
        int distanta;
        string cale;
        string numeFolder;
        Article art = new Article();
        List<Article> listaArticole = new List<Article>();
        List<double> ListaCastig = new List<double>();
        List<double> ListaCastig2 = new List<double>();
        string[] vectorGlobal=new string[10000];
        List<Clasa> reprezentarePeClase = new List<Clasa>();
        string final = "";
        List <SortedDictionary<int, int>> ListaVectoriRari = new List<SortedDictionary<int, int>>();
        public Form1()
        {
            InitializeComponent();
            textPrag.Increment = 0.1m;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            listaArticole = art.GetArticleList(cale);//Ia lista de articole
            ListaVectoriRari = art.GetDictList(listaArticole);//Face lista de vectori rari si vectorul global
            Global.VectorGlobal.CopyTo(vectorGlobal);
            string path= Directory.GetCurrentDirectory();
             final = Path.GetFullPath(Path.Combine(path, @"..\..\"));//face calea pt folderul Input

            if (!Directory.Exists(final))
            {
                Directory.CreateDirectory(final);//creaza folderul
            }

            FileStream fs = new FileStream(final+ @"\Input\rezultat.txt", FileMode.Create);
            fs.Close();
            StreamWriter sw = new StreamWriter(final + @"\Input\rezultat.txt");

            //Afisare
            int i = 0;
            foreach (SortedDictionary<int, int> dict in ListaVectoriRari)
            {
                foreach (KeyValuePair<int, int> ab in dict)
                {
                  listBox1.Items.Add((ab.Key+1) + ":" + ab.Value );
                  sw.Write((ab.Key + 1) + ":" + ab.Value + " ");
                }
                sw.Write("#");
                foreach (string li in listaArticole[i].ClassCodes)
                {
                    sw.Write(li + " ");
                }
                sw.Write("#"+Global.TipVectori[i]+"\n");
                i++;
                sw.Flush();
            }


            foreach (string s in Global.VectorGlobal) {
                listBox2.Items.Add(s); 
            }
            listBox2.Items.Add(Global.VectorGlobal.Count);
            //Afisare
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog BtnBrowse = new FolderBrowserDialog();
            
            if (BtnBrowse.ShowDialog() == DialogResult.OK)
            {
                cale = BtnBrowse.SelectedPath;


                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(cale);
                numeFolder = info.Name.ToString().ToLower();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            reprezentarePeClase = art.RepartitiePeClase(listaArticole);
            ListaCastig = art.CalculeazaCastigulInformational(Global.VectorGlobal, listaArticole, reprezentarePeClase);
            ListaCastig2 = art.CalculeazaCastigulInformational(Global.VectorGlobal, listaArticole, reprezentarePeClase);
            double prag = Convert.ToDouble(textPrag.Value);
            List<int> elimina = new List<int>();
            List<string> eliminaDict = new List<string>();
        
           
           
            for (int i=0;i<ListaCastig.Count;i++)
            {
                double d = ListaCastig[i];
                if (ListaCastig[i] <prag)
                {
                    
                    elimina.Add(i);
                    eliminaDict.Add(Global.VectorGlobal[i]);
                }
            }
            foreach (int i in elimina)
            {
            foreach (SortedDictionary<int, int> dict in ListaVectoriRari)
            {
                    if (dict.ContainsKey(i))
                    {
                        int nr = ListaVectoriRari.IndexOf(dict);
                        ListaVectoriRari[nr].Remove(i);
                    }
              
            }
                ListaCastig.RemoveAll(v => v < prag);
            }

            foreach(string s in eliminaDict)
            {
                Global.VectorGlobal.RemoveAll(x => x == s);
            }
            foreach (double d in ListaCastig)
            {
                listBox3.Items.Add(d);
            }
            //int i = 0;
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            foreach (SortedDictionary<int, int> dict in ListaVectoriRari)
            {
                foreach (KeyValuePair<int, int> ab in dict)
                {
                    listBox1.Items.Add((ab.Key + 1) + ":" + ab.Value);
                   
                }
            }

            foreach (string s in Global.VectorGlobal)
            {
                listBox2.Items.Add(s);
            }


            FileStream fs = new FileStream(final + @"\Input\FisierDeTest.txt", FileMode.Create);
            fs.Close();
            StreamWriter sw = new StreamWriter(final + @"\Input\FisierDeTest.txt");
            fs= new FileStream(final + @"\Input\FisierDeAntrenare.txt", FileMode.Create);
            fs.Close();
            StreamWriter sw2 = new StreamWriter(final + @"\Input\FisierDeAntrenare.txt");


            Dictionary<string, double> testing = new Dictionary<string, double>();
            Dictionary<string, double> training = new Dictionary<string, double>();
            List<Tuple<int, string, string>> testing2 = new List<Tuple<int, string, string>>();
            List<Tuple<int, string, string>> training2 = new List<Tuple<int, string, string>>();
            for (int i = 0; i < Global.TipVectori.Count; i++)
            {
                if (Global.TipVectori[i] == "Testing")
                {
                    testing2.Add(Tuple.Create(i, Global.TipVectori[i], reprezentarePeClase[i].clasa));
                    foreach (KeyValuePair<int,int> pair in ListaVectoriRari[i])
                    {
                        if (!testing.ContainsKey(vectorGlobal[pair.Key]))
                        {   
                           
                            testing.Add(vectorGlobal[pair.Key], ListaCastig2[pair.Key]);
                        }
                    }
                }else if (Global.TipVectori[i] == "Training")
                {
                    training2.Add(Tuple.Create(i, Global.TipVectori[i], reprezentarePeClase[i].clasa));
                    foreach (KeyValuePair<int, int> pair in ListaVectoriRari[i])
                    {
                        if (!training.ContainsKey(vectorGlobal[pair.Key]))
                        {
                            training.Add(vectorGlobal[pair.Key], ListaCastig2[pair.Key]);
                        }
                    }
                }
            }
            foreach(KeyValuePair<string,double> p in testing)
            {
                sw.WriteLine("@attribute " + p.Key + " " + p.Value);
            }
            
            foreach (KeyValuePair<string, double> p in training)
            {
                sw2.WriteLine("@attribute " + p.Key + " " + p.Value);
            }
            
            sw.WriteLine("\n@data");
            sw2.WriteLine("\n@data");
            foreach(Tuple<int, string, string> t in testing2)
            {
                foreach(KeyValuePair<int,int> pair in ListaVectoriRari[t.Item1])
                {
                    sw.Write(pair.Key + ":" + pair.Value + " ");
                }
                sw.Write("#" + t.Item3 + "\n");
            }
            foreach (Tuple<int, string, string> t in training2)
            {
                foreach (KeyValuePair<int, int> pair in ListaVectoriRari[t.Item1])
                {
                    sw2.Write(pair.Key + ":" + pair.Value + " ");
                }
                sw2.Write("#" + t.Item3 + "\n");
            }
            sw2.Flush();
            sw.Flush();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
                if (ix != e.Index) checkedListBox1.SetItemChecked(ix, false);
            normalizare = checkedListBox1.SelectedIndex;

        }

        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int ix = 0; ix < checkedListBox2.Items.Count; ++ix)
                if (ix != e.Index) checkedListBox2.SetItemChecked(ix, false);
            distanta = checkedListBox2.SelectedIndex;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            k =(int) numericUpDown1.Value;
        }

        private void btnKNN_Click(object sender, EventArgs e)
        {
            List<Dictionary<int, int>> vectTest = new List<Dictionary<int, int>>();
            List<Dictionary<int, int>> vectTrain = new List<Dictionary<int, int>>();
            vectTest = art.ImparteSetul(ListaVectoriRari, 1);
            vectTrain = art.ImparteSetul(ListaVectoriRari, 2);
            List<Dictionary<int, double>>TestNorm = new List<Dictionary<int, double>>();
            foreach(Dictionary<int,int> pair in vectTest)
            {
                switch (normalizare)
                {
                    case 0:
                        TestNorm.Add(art.NormalizareBinara(pair));
                        break;
                    case 1:
                        TestNorm.Add(art.NormalizareNominala(pair));
                        break;
                    case 2:
                        TestNorm.Add(art.NormalizareSuma1(pair));
                        break;
                    case 3:
                        TestNorm.Add(art.NormalizareCornel(pair)); ;
                        break;
                }
                
            }
            List<Dictionary<int, double>> TrainNorm = new List<Dictionary<int, double>>();
            foreach (Dictionary<int, int> pair in vectTrain)
            {
                switch (normalizare)
                {
                    case 0:
                        TrainNorm.Add(art.NormalizareBinara(pair));
                        break;
                    case 1:
                        TrainNorm.Add(art.NormalizareNominala(pair));
                        break;
                    case 2:
                        TrainNorm.Add(art.NormalizareSuma1(pair));
                        break;
                    case 3:
                        TrainNorm.Add(art.NormalizareCornel(pair)); ;
                        break;
                }

            }

        }
    }
}
