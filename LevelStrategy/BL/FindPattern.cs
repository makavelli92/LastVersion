using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelStrategy.Model;
using System.Threading;


namespace LevelStrategy.BL
{
    public class FindPattern
    {
        public event EventHandler<string> EventSignal;
        public int sumCandleVolume;
        public int singleClasterVolume;
        public int singleClasterVolumeFor5Minut;
        public int neighborVolume;
        public int neighborVolForDensity;
        public string name;
        public DateTime passNCVS;
        public DateTime passSCV;
        public DateTime passVD;
        public DateTime passSVIC;

        public FindPattern(EventHandler<string> eventHandler, int sumCandleVolume, int singleClasterVolume, int singleClastVolFor5Min, int neighborVol, int neighborVolDensity, string name)
        {
            EventSignal = eventHandler;
            this.sumCandleVolume = sumCandleVolume;
            this.singleClasterVolume = singleClasterVolume;
            singleClasterVolumeFor5Minut = singleClastVolFor5Min;
            neighborVolume = neighborVol;
            neighborVolForDensity = neighborVolDensity;
            this.name = name;
        }
        public SortedDictionary<double, int> LastClaster(Ticks ticks, int timeFrame)
        {
            SortedDictionary<double, int> claster = new SortedDictionary<double, int>();
            DateTime tempTime = ticks.Time.Last().AddMinutes(timeFrame);
            int temp = ticks.Count - 1;
            while (tempTime < ticks.Time[temp] && temp > 0)
            {
                if (claster.ContainsKey(ticks.Close[temp]))
                    claster[ticks.Close[temp]] += (int)ticks.Volume[temp];
                else
                    claster.Add(ticks.Close[temp], (int)ticks.Volume[temp]);
                temp--;
            }
            return claster;
        }
        public void StartFind(Ticks ticks)
        {
            if (ticks.Count > 0)
            {
                /* Console.WriteLine("_______________111_______________________");
                   SortedDictionary<double, int> cluster = ticks.LastClaster(60);
                   foreach (var i in cluster)
                   {
                       Console.WriteLine("{0} || {1}", i.Key, i.Value);
                   }*/
                SumVolumeInCluster(LastClaster(ticks, 300), sumCandleVolume);
                SingleClusterVolume(LastClaster(ticks, 300), singleClasterVolume, 5);
                SingleClusterVolume(LastClaster(ticks, 1500), singleClasterVolumeFor5Minut, 15);
                NeighborClusterVolumeSum(LastClaster(ticks, 300), 2, neighborVolume);
                VolumeDensity(LastClaster(ticks, 300), 5, neighborVolForDensity);
              //  Console.WriteLine("{0} - {1}", ticks.date.Last(), ticks.Name);
            }
        }
        // Метод для Алерта по объему нескольких соседних кластеров:
        // * countNeighborCluster определяет кол-во кластеров, volumeLimit - объем кот-ый должны кластера наторговать
        public void NeighborClusterVolumeSum(SortedDictionary<double, int> cluster, int countNeighborCluster, int volumeLimit)
        {
            if (passNCVS == null || DateTime.Now > passNCVS.AddMinutes(5))
            {
                Array valueArray = cluster.Values.ToArray();
                Array keyArray = cluster.Keys.ToArray();
                for (int i = 0; i < valueArray.Length - countNeighborCluster; i++)
                {
                    int temp = (int)valueArray.GetValue(i);
                    for (int j = 0, k = i + 1; j < countNeighborCluster - 1; j++, k++)
                    {
                        temp += (int)valueArray.GetValue(k);
                    }
                    if (temp > volumeLimit)
                    {
                        string s = String.Format("{4} - Объем соседних кластеров - {0} > {1} Кластера с уровня цены {2} до {3}", temp, volumeLimit, keyArray.GetValue(i), keyArray.GetValue(i + countNeighborCluster - 1), name);
                    //  string s = String.Format("{4} - Cluster Volume {0} > {1} from {2} before {3}", temp, volumeLimit, keyArray.GetValue(i), keyArray.GetValue(i + countNeighborCluster - 1), name);
                        passNCVS = DateTime.Now;
                        EventSignal(this, s);
                        break;
                    }
                }
            }
        }
        // Объем одного кластера 
        public void SingleClusterVolume(SortedDictionary<double, int> cluster, int volumeLimit, int timeFrame)
        {
            if (passSCV == null || DateTime.Now > passSCV)
            {
                foreach (KeyValuePair<double, int> i in cluster)
                {
                    if (i.Value > volumeLimit)
                    {
                        string s = String.Format("{2} - Объем на уровне > {0} по цене {1} в течение {3} минут(ы)", volumeLimit, i.Key, name, timeFrame);
                    //  string s = String.Format("{2} - Cluster Volume > {0} in {1} during {3} minut", volumeLimit, i.Key, name, timeFrame);
                        passSCV = DateTime.Now.AddMinutes(timeFrame);
                        EventSignal(this, s);
                        break;
                    }
                }
            }
        }
        // Метод для определния плотности проторгованного объема:
        // * countNeighborCluster - кол-во соседних кластеров, volumeLimit - объем кот-ый должен превысить каждый из кластеров
        public void VolumeDensity(SortedDictionary<double, int> claster, int countNeighborCluster, int volumeLimit)
        {
            if (passVD == null || DateTime.Now > passVD.AddMinutes(5))
            {
                int temp = countNeighborCluster;
                foreach (KeyValuePair<double, int> i in claster)
                {
                    if (i.Value >= volumeLimit)
                        temp--;
                    if (temp == 0)
                    {
                        temp = countNeighborCluster;
                        string s = String.Format("{3} - Большая плотность в области цен {0} кластеров > {1}. Верхний кластер {2}", countNeighborCluster, volumeLimit, i, name);
                      //  string s = String.Format("{3} - {0} price cluster have volume > {1}. Upper cluster {2}", countNeighborCluster, volumeLimit, i, name);
                        passVD = DateTime.Now;
                        EventSignal(this, s);
                        break;
                    }
                }
            }
        }
        // Объем целого бара
        public void SumVolumeInCluster(SortedDictionary<double, int> cluster, int volumeLimit)
        {
            if (passSVIC == null || DateTime.Now > passSVIC.AddMinutes(5))
            {
                int sum = 0;
                foreach (KeyValuePair<double, int> i in cluster)
                {
                    sum += i.Value;
                }
                if (sum >= volumeLimit)
                {
                    string s = String.Format("{1} - Sum Volume in Cluster >= {0}", volumeLimit, name);
                    passSVIC = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Red;
                    EventSignal(this, s);
                    Console.ResetColor();
                }
            }
        }
    }
}
