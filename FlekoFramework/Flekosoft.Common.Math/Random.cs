namespace Flekosoft.Common.Math
{
    public class Random
    {
        private const int MaxIntValue = 0x7fff;

        private readonly System.Random _rnd;

        bool _randGaussianUseLast;
        private double _randGaussianY2;

        public Random(int seed)
        {
            _rnd = new System.Random(seed);
        }

        public int Next(int minValue, int maxValue)
        {
            return _rnd.Next(minValue, maxValue);
        }

        public int Next()
        {
            return _rnd.Next(MaxIntValue);
        }

        //returns a random double between zero and 1
        public double RandFloat()
        {
            return ((Next()) / (MaxIntValue + 1.0));
        }

        public double RandGaussian(double minValue, double maxValue, double step)
        {
            var mean = (maxValue + minValue) * 0.5;
            var deviation = (maxValue - mean) / 3;
            //int n = 0;
            //for (float j = minValue; j < maxValue; j += step)
            //{
            //    deviation += (j - mean) * (j - mean);
            //    n++;
            //}
            //deviation = deviation / n;
            //deviation = (float)System.Math.Sqrt(deviation);
            var rnd = RandGaussian(mean, deviation);
            if (rnd > maxValue) rnd = maxValue;
            if (rnd < minValue) rnd = minValue;
            return rnd;
        }


        /// <summary>
        /// Возвращает случайную величинунормального распределенияя
        /// http://www.taygeta.com/random/gaussian.html
        /// </summary>
        /// <param name="mean">Мат ожидание</param>
        /// <param name="standardDeviation">Дисперсия</param>
        /// <returns></returns>
        public double RandGaussian(double mean = 0.0, double standardDeviation = 1.0)
        {
            // ReSharper disable TooWideLocalVariableScope
            double x1;
            double x2;
            double w;
            double y1;
            // ReSharper restore TooWideLocalVariableScope

            if (_randGaussianUseLast)		        /* use value from previous call */
            {
                y1 = _randGaussianY2;
                _randGaussianUseLast = false;
            }
            else
            {
                do
                {
                    x1 = 2.0 * RandFloat() - 1.0;
                    x2 = 2.0 * RandFloat() - 1.0;
                    w = x1 * x1 + x2 * x2;
                }
                while (w >= 1.0);

                w = System.Math.Sqrt((-2.0 * System.Math.Log(w)) / w);
                y1 = x1 * w;
                _randGaussianY2 = x2 * w;
                _randGaussianUseLast = true;
            }

            return (mean + y1 * standardDeviation);
        }

    }
}
