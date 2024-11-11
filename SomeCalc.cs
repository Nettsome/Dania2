using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General;

/// <summary>
/// # Кратко о классах в этом файле
/// 
/// SimpsonRuleCalc -- класс для вычисления интеграла по методу симпсона (без использования потоков и с использованием потоков)
///     
/// 
/// </summary>
/// 


public class SimpsonRuleCalc
{
    ushort numOfThreads = 2;

    public long N = 1000;                       // кол-во разбиений                     
    public ushort NumOfThreads                     // кол-во потоков
    {
        get { return numOfThreads; }
        set
        {
            if (value < 16)
                numOfThreads = value;
            else
                numOfThreads = 15;
        }
    }

    public SimpsonRuleCalc(long n, ushort numofthreads = 2)
    {
        N = n;
        NumOfThreads = numofthreads;
    }


    /// without async
    public double CalcWithoutAsync(double a, double b, Func<double, double> function, long n = 1000)
    {
        N = n;
        double step = Math.Abs(a - b) / N;

        double sum = 0;
        for (double i = a + step; i <= b; i += step)
        {
            sum += function(i - step) + 4 * function(i - step / 2) + function(i);
        }

        return sum * step / 6;
    }

    /// with async
    public double CalcWithAsync(double a, double b, Func<double, double> function, long n = 1000)
    {
        N = n;
        double sum = 0;
        int done = 0;                           // может переделать в byte тип? Но тогда придется поменять тип у NumOfThreads
        double step = Math.Abs(a - b) / N;
        object lockObject = new object();

        Thread[] threads = new Thread[NumOfThreads];

        for (int i = 0; i < NumOfThreads; ++i)
        {
            threads[i] = new Thread((object? index) =>
            {
                double inThreadSum = 0;
                for (double j = a + ((int)index + 1) * step; j <= b; j += NumOfThreads * step)
                {
                    inThreadSum += function(j - NumOfThreads * step) + 4 * function(j - NumOfThreads * step / 2) + function(j); ;
                }
                lock (lockObject)
                {
                    sum += inThreadSum;
                    done++;
                }
            });
            threads[i].Start(i);
        }

        while (done != NumOfThreads) { }        // аналог await, Task.Wait или как там


        return sum * step / 6;
    }
}