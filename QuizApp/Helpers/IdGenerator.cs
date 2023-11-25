using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace QuizApp.Helpers
{
    public class IdGenerator
    {
        private static object _locker = new object();

        private static char[] alphaNum = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private static char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static string[] combinationOf2 = Get2CartesianProduct();//36*36
        private static string[] combinationOf3 = Get3CartesianProduct();//36*36*36
        private static string[] combinationOf4 = Get4CartesianProduct();//36*36*36*36
        public static string GenerateLongId()
        {
            var id = string.Empty;
            lock (_locker)
            {
                Thread.Sleep(100);
                id = DateTime.Now.ToString("yyyyMMddHHmmssf");
            }
            var g1 = "ABCDEFGHIJ".ToCharArray();
            var g2 = "KLMNOPQRST".ToCharArray();
            var g3 = new string[] { "AU", "BV", "CW", "DX", "EY", "FZ", "GU", "HV", "IW", "JX" };
            var groupSelector = new string[] { "n", "g1", "g1", "g1", "g2", "g2", "n", "n", "g2", "g2", "g1", "g1", "g2", "g2", "g3" };
            var newId = string.Empty;
            for (var i = 0; i < id.Length; i++)
            {
                var g = groupSelector[i];
                if (g == "n")
                    newId += id[i];
                else if (g == "g1")
                    newId += g1[int.Parse(id[i].ToString())];
                else if (g == "g2")
                    newId += g2[int.Parse(id[i].ToString())];
                else if (g == "g3")
                    newId += g3[int.Parse(id[i].ToString())];
            }
            return newId;
        }
        public static string GetShortCode()
        {

            string code = "";
            var date = DateTime.UtcNow;
            lock (_locker)
            {
                Thread.Sleep(100);
                date = DateTime.UtcNow;
            }
            var year = date.Year - 2020;
            var dayOfYear = date.DayOfYear;
            var minuteOfYear = (dayOfYear * 1440) + (date.Hour * 60) + date.Minute;
            var second = date.Second.ToString();
            if (second.Length < 2)
                second = "0" + second;
            var ms = date.Millisecond;
            if (ms < 100)
                ms = 0;
            else
                ms = ms / 100;
            //3 letters representing year. Max is 36*36*36
            //4 letters representing minute of year.
            //2 letters representing second of minute.
            //1 letter representing first digit of ms.
            code = $"{combinationOf3[year]}{combinationOf4[minuteOfYear]}{alpha[int.Parse(second[0].ToString())]}{alpha[int.Parse(second[1].ToString())]}{alpha[ms]}";

            return code;
        }

        public static string[] Get2CartesianProduct()
        {
            var cartersian = new List<string>();
            foreach (var item1 in alphaNum)
            {
                foreach (var item2 in alphaNum)
                {
                    cartersian.Add((item1.ToString() + item2.ToString()));
                }
            }
            return cartersian.ToArray();
        }

        public static string[] Get3CartesianProduct()
        {
            var cartersian = new List<string>();
            foreach (var item1 in alphaNum)
            {
                foreach (var item2 in alphaNum)
                {
                    foreach (var item3 in alphaNum)
                    {
                        cartersian.Add((item1.ToString() + item2.ToString()) + item3.ToString());
                    }

                }
            }
            return cartersian.ToArray();
        }

        public static string[] Get4CartesianProduct()
        {
            var cartersian = new List<string>();
            foreach (var item1 in alphaNum)
            {
                foreach (var item2 in alphaNum)
                {
                    foreach (var item3 in alphaNum)
                    {
                        foreach (var item4 in alphaNum)
                        {
                            cartersian.Add((item1.ToString() + item2.ToString()) + item3.ToString() + item4.ToString());
                        }
                    }

                }
            }
            return cartersian.ToArray();
        }

    }
}