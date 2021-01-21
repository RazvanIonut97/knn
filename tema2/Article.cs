using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

namespace tema2
{   static class Global
    {
        public static List<string> VectorGlobal = new List<string>();
        public static double nrTotal = 0;
        public static List<string> TipVectori=new List<string>();
    }
    public class Article
    {
        private string title;
        private string text;
        public List<string> ClassCodes;
        public string tip;



        public Article(string title, string text, List<string> ClassCodes,string tip)
        {
            this.title =title;
            this.text = text;
            this.ClassCodes = ClassCodes;
            this.tip = tip;
       
        }

        public Article(){}

        public string GetXmlNodeContentByName(XmlDocument document ,string nodeName)
        {
            XmlNodeList list = document.GetElementsByTagName(nodeName);
            return list[0].InnerText.ToLower(); 
        }
        public List<string> GetClassCodesFromXml(XmlDocument document)
        {
            XmlNodeList list = document.SelectNodes("newsitem/metadata/codes");
            XmlNodeList CodesList = list;
            List<string> ret = new List<string>();

            foreach (XmlNode node in list)
            {
                if (node.Attributes[0].Value == "bip:topics:1.0")
                {
                    CodesList = node.SelectNodes("code");
                }
            }
            foreach (XmlNode node in CodesList)
            {
                ret.Add(node.Attributes[0].Value);
            }
            return ret;
        }
        public List<Article> GetArticleList(string path)
        {
            List<Article> lista = new List<Article>();
           string[] a= Directory.GetFiles(path, "*.xml",SearchOption.AllDirectories);
            for(int i = 0; i < a.Length; i++)
            {
                
                XmlDocument document = new XmlDocument();
                document.Load(a[i]);
                string title = GetXmlNodeContentByName(document, "title");
                string text = GetXmlNodeContentByName(document, "text");
                string tip= new DirectoryInfo(a[i].Substring(0,a[i].Length-12)).Name;
                lista.Add(new Article(title,text,GetClassCodesFromXml(document),tip));

            }
            return lista;
        }
        public List<SortedDictionary<int,int>> GetDictList(List<Article> lista)
        {
            List<SortedDictionary<int, int>> list = new List<SortedDictionary<int, int>>();
            foreach (Article art in lista)
            {
                SortedDictionary<int, int> VectorRar = new SortedDictionary<int, int>();
                MakeGlobalVector(art.title);
                MakeGlobalVector(art.text);
                list.Add(MakeRareVector(VectorRar,art.title+" "+art.text));
                Global.TipVectori.Add(art.tip);
            }
            return list;
        }
        private SortedDictionary<int, int> MakeRareVector(SortedDictionary<int, int> vectorRar, string text)
        {
            PorterStemmer p = new PorterStemmer();
            string t = text;
            string[] del = { "\t", " ", ",", ".", "%", "/", ":", ";", "?", "!", "\'s", "'re", "\"", ")", "(", "\'t", "--", "-", "'", "&", "$", "@", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };


            foreach (string s in t.Split(del, StringSplitOptions.RemoveEmptyEntries))
            {
        
                if (StopWord(s) != false)
                {
                    string ceva=p.StemWord(s);
                    if (vectorRar.ContainsKey(Global.VectorGlobal.IndexOf(ceva)) == false)
                    {
                        vectorRar.Add(Global.VectorGlobal.IndexOf(ceva), 1);
                    }
                    else
                    {
                        vectorRar[Global.VectorGlobal.IndexOf(ceva)]++;
                    }

                }
            }
          
            
            return vectorRar;
        }
        private void MakeGlobalVector(string text)
        {
            PorterStemmer p = new PorterStemmer();
            string[] del = { "\t"," ", ",", ".","%","/", ":", ";", "?", "!", "\'s", "'re","\"", ")", "(", "\'t", "--", "-","'","&","$","@","0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (string s in text.Split(del, StringSplitOptions.RemoveEmptyEntries))
            {
                if (StopWord(s) != false)
                {

                    CheckVectors(p.StemWord(s));
                }
            }
        }
        private void CheckVectors(string s)
        {
            if (Global.VectorGlobal.Contains(s) == false)
            {
                Global.VectorGlobal.Add(s);
            }
            
        }
        private bool StopWord(string word)
        {
            string line;
            StreamReader s = new StreamReader(@"..\..\stopwords.txt");
            while ((line = s.ReadLine()) != null)
            {
                if (word == line)
                {
                    return false;
                }
            }

            return true;
        }
        public int CalculeazaNr(string cuvant,List<Clasa> repartitia)
        {
            int rez = 0;
            foreach (Clasa cls in repartitia)
            {
                if (cls.cuvinte.Contains(cuvant))
                {
                    rez++;
                }
            }
            return rez;
        }
        public List<Clasa> RepartitiePeClase(List<Article> listaArticole) 
        {
            List < Clasa > repartitie=new List<Clasa>();
            foreach (Article art in listaArticole)
            {
                List<string> words = new List<string>();
                PorterStemmer p = new PorterStemmer();
                string[] del = { "\t", " ", ",", ".", "%", "/", ":", ";", "?", "!", "\'s", "'re", "\"", ")", "(", "\'t", "--", "-", "'", "&", "$", "@", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

                foreach (string s in art.title.Split(del, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (StopWord(s) != false)
                    {
                        words.Add(p.StemWord(s));
                        
                    }
                }
                foreach (string s in art.text.Split(del, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (StopWord(s) != false)
                    {
                        words.Add(p.StemWord(s));

                    }
                }
                // repartitie.Add(new Clasa(words, art.ClassCodes));
               // List<string> list = new List<string>();
                //list.Add(art.ClassCodes[0]);
                repartitie.Add(new Clasa(words, art.ClassCodes[0]));

            }
            return repartitie;
        }
        public double EntropieCuvant(string cuvant,List<Clasa> clase)
        {
            Dictionary<string, int> repartitiaPeClase = new Dictionary<string, int>();
            double entropie = 0;
            foreach(Clasa cls in clase)
            {
                if (cls.cuvinte.Contains(cuvant))
                 {
                   
                        if (repartitiaPeClase.ContainsKey(cls.clasa))
                        {
                        repartitiaPeClase[cls.clasa]++;//=nr;
                        }
                        else
                        {
                            repartitiaPeClase.Add(cls.clasa, 1);
                        }  
                }
            }
            double nrTotalElemente = Global.nrTotal;

            entropie = CalculeazaEntropia(repartitiaPeClase, nrTotalElemente);
            
            return entropie;

        }
        public double EntropieNuCuvant(string cuvant, List<Clasa> clase)
        {
            Dictionary<string, int> repartitiaPeClase = new Dictionary<string, int>();
            double entropie = 0;
            foreach (Clasa cls in clase)
            {
                if (!cls.cuvinte.Contains(cuvant))
                {
                    if (repartitiaPeClase.ContainsKey(cls.clasa))
                    {
                        repartitiaPeClase[cls.clasa]++;
                    }
                    else
                    {
                        repartitiaPeClase.Add(cls.clasa, 1);
                    }
                }
            }
            double nrTotalElemente = Global.nrTotal;
           
            entropie = CalculeazaEntropia(repartitiaPeClase, nrTotalElemente);

            return entropie;

        }
        public double CalculeazaEntropia(Dictionary<string, int> repartitiaPeClase, double nrTotalElemente)
        {
            double entropia = 0;
            foreach (KeyValuePair<string, int> pair in repartitiaPeClase)
            {
                double valoare = pair.Value / nrTotalElemente;
                entropia -= (valoare) * Math.Log(valoare, 2);
            }

            return entropia;
        }
        public double CalculeazaEntropiaGlobala(List<Article> listaArticole)
        {
            double entropie = 0;
            Dictionary<string, int> repartitiaPeClase = new Dictionary<string, int>();

            foreach (Article art in listaArticole)
            {

                    if (repartitiaPeClase.ContainsKey(art.ClassCodes[0]))
                    {
                        repartitiaPeClase[art.ClassCodes[0]]++;
                    }
                    else
                    {
                        repartitiaPeClase.Add(art.ClassCodes[0], 1);
                    }
                
            }
            double nrTotalElemente = 0;
            foreach (KeyValuePair<string, int> pair in repartitiaPeClase)
            {
                nrTotalElemente += pair.Value;
            }
            Global.nrTotal = nrTotalElemente;
            entropie = CalculeazaEntropia(repartitiaPeClase, nrTotalElemente);
            return entropie;
        }
        public List<double> CalculeazaCastigulInformational(List<string> vectorGlobal, List<Article> listaArticole, List<Clasa> rep)
        {
            List<double> listaCastig = new List<double>();
            double entTot = CalculeazaEntropiaGlobala(listaArticole);

            foreach (string s in vectorGlobal)
            {
                double nr = CalculeazaNr(s, rep);
                double nrN = Global.nrTotal - nr;
                double entCuv = EntropieCuvant(s, rep);
                double entNuCuv = EntropieNuCuvant(s, rep);
                double rez = entTot - ((nr / Global.nrTotal) * entCuv) - ((nrN / Global.nrTotal) * entNuCuv);
                listaCastig.Add(rez);
            }

            return listaCastig;
        }
        public double CalculeazaDistantaManhatten(Dictionary<int, int> vectorRar1, Dictionary<int, int> vectorRar2)
        {
            double distanta = 0;
            if (vectorRar1.Count > vectorRar2.Count)
            {
                int diferenta = vectorRar1.Count - vectorRar2.Count;
                for (int i = 0; i < diferenta; i++)
                {
                    vectorRar2.Add(vectorRar2.Count(), 0);
                }
            }
            else if (vectorRar1.Count < vectorRar2.Count)
            {
                int diferenta = vectorRar2.Count - vectorRar1.Count;
                for (int i = 0; i < diferenta; i++)
                {
                    vectorRar1.Add(vectorRar1.Count(), 0);
                }
            }
            double val = 0;
            for (int i = 0; i < vectorRar1.Count; i++)
            {
                val += Math.Abs(vectorRar1[i] - vectorRar2[i]);
            }
            return distanta;
        }
        public double CalculeazaDistantaEuclidiana(Dictionary<int, int> vectorRar1, Dictionary<int, int> vectorRar2)
        {
            double distantaEuclidiana = 0;
            if (vectorRar1.Count > vectorRar2.Count)
            {
                int diferenta = vectorRar1.Count - vectorRar2.Count;
                for (int i = 0; i < diferenta; i++)
                {
                    vectorRar2.Add(vectorRar2.Count(), 0);
                }
            }
            else if (vectorRar1.Count < vectorRar2.Count)
            {
                int diferenta = vectorRar2.Count - vectorRar1.Count;
                for (int i = 0; i < diferenta; i++)
                {
                    vectorRar1.Add(vectorRar1.Count(), 0);
                }
            }
            double val = 0;
            for (int i = 0; i < vectorRar1.Count; i++)
            {
                val += Math.Pow((vectorRar1[i] - vectorRar2[i]), 2);
            }
            distantaEuclidiana = Math.Sqrt(val);
            return distantaEuclidiana;
        }
        public Dictionary<int, double> NormalizareCornel(Dictionary<int, int> VectorRar)
        {
            Dictionary<int, double> VectorNormalizat = new Dictionary<int, double>();

            foreach (KeyValuePair<int, int> pair in VectorRar)
            {
                if (pair.Value == 0)
                {
                    VectorNormalizat.Add(pair.Key,0);
                }
                else 
                {
                    double val = 1 + Math.Log10(1 + Math.Log10(pair.Value));
                    VectorNormalizat.Add(pair.Key,val);
                }
               
            }
            return VectorNormalizat;
        }
        public Dictionary<int, double> NormalizareSuma1(Dictionary<int, int> VectorRar)
        {
            Dictionary<int, double> VectorNormalizat = new Dictionary<int, double>();

            double suma = VectorRar.Values.Sum();
            foreach (KeyValuePair<int, int> pair in VectorRar)
            {
                VectorNormalizat.Add(pair.Key, (pair.Value / suma));
            }
            return VectorNormalizat;
        }
        public Dictionary<int, double> NormalizareNominala(Dictionary<int, int> VectorRar)
        {
            Dictionary<int, double> VectorNormalizat = new Dictionary<int, double>();

            int max = VectorRar.Values.Max();
            foreach (KeyValuePair<int, int> pair in VectorRar)
            {
                VectorNormalizat.Add(pair.Key, (pair.Value / max));
            }
            return VectorNormalizat;
        }
        public Dictionary<int, double> NormalizareBinara(Dictionary<int, int> VectorRar)
        {
            Dictionary<int, double> VectorNormalizat = new Dictionary<int, double>();
            foreach (KeyValuePair<int, int> pair in VectorRar)
            {
                if (pair.Value > 0)
                {
                    VectorNormalizat.Add(pair.Key, 1);
                }
                else { VectorNormalizat.Add(pair.Key, 0); }
            }
            return VectorNormalizat;
        }
        //
        internal List<Dictionary<int, int>> ImparteSetul(List<SortedDictionary<int, int>> listaVectoriRari, int tip)
        {
            List<Dictionary<int, int>> set = new List<Dictionary<int, int>>();
            for(int i = 0; i < listaVectoriRari.Count; i++)
            {
                if (tip == 1)
                {
                    if (Global.TipVectori[i] == "Testing")
                    {
                        set.Add(listaVectoriRari[i].ToDictionary(x => x.Key, x => x.Value));
                    }
                }
                else
                {
                    if (Global.TipVectori[i] == "Training")
                    {
                        set.Add(listaVectoriRari[i].ToDictionary(x => x.Key, x => x.Value));
                    }
                }
            }
            return set;
        }


    }

}