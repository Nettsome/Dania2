using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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


/// <summary>
/// Не знаю, что этот класс делает
/// </summary>
class Program
{

    static void Main(string[] args)
    {
        Bound A = new Bound(-1, 0, 100_000_000, 1); //1 тред = 0 тредов
        var threads = new List<Thread>();

        double resultSumM = 0.0;
        double resultSumL = 0.0;
        double resultSumT = 0.0;

        Console.WriteLine("Интегрирование x*e^x на [-1,0] с 1 тредом");
        var sw = new Stopwatch();
        //на медианных суммах
        sw.Start();
        for (int tID = 1; tID <= A.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= A.precision + 1; i += A.ThreadCount)
                {
                    PartialSum += Function((double)i * A.Delta - (0.5) * A.Delta + A.Left) * A.Delta;
                }
                lock (threads) { resultSumM += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} медианными суммами за {1}мс", resultSumM, sw.ElapsedMilliseconds);
        //на медианных суммах

        //на левых суммах
        sw.Restart();
        threads = new List<Thread>();
        for (int tID = 1; tID <= A.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= A.precision + 1; i += A.ThreadCount)
                {
                    PartialSum += Function((i - 1) * A.Delta + A.Left) * A.Delta;
                }
                lock (threads) { resultSumL += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} левыми суммами за {1}мс", resultSumL, sw.ElapsedMilliseconds);
        //на левых суммах

        //на трапезоидных суммах
        sw.Restart();
        threads = new List<Thread>();
        for (int tID = 1; tID <= A.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= A.precision + 1; i += A.ThreadCount)
                {
                    PartialSum += (Function((i - 1) * A.Delta + A.Left) + Function(i * A.Delta + A.Left)) / 2 * A.Delta;
                }
                lock (threads) { resultSumT += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} трапезоидными суммами за {1}мс", resultSumT, sw.ElapsedMilliseconds);
        //на трапезоидных суммах

        Bound B = new Bound(-100, 0, 100_000_000, 10);

        resultSumM = 0.0;
        resultSumL = 0.0;
        resultSumT = 0.0;

        Console.WriteLine("Интегрирование x*e^x на [-100,0] с 10 тредами");
        //на медианных суммах
        sw.Restart();
        for (int tID = 1; tID <= B.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= B.precision + 1; i += B.ThreadCount)
                {
                    PartialSum += Function((double)i * B.Delta - (0.5) * B.Delta + B.Left) * B.Delta;
                }
                lock (threads) { resultSumM += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} медианными суммами за {1}мс", resultSumM, sw.ElapsedMilliseconds);
        //на медианных суммах

        //на левых суммах
        sw.Restart();
        threads = new List<Thread>();
        for (int tID = 1; tID <= B.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= B.precision + 1; i += B.ThreadCount)
                {
                    PartialSum += Function((i - 1) * B.Delta + B.Left) * B.Delta;
                }
                lock (threads) { resultSumL += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} левыми суммами за {1}мс", resultSumL, sw.ElapsedMilliseconds);
        //на левых суммах

        //на трапезоидных суммах
        sw.Restart();
        threads = new List<Thread>();
        for (int tID = 1; tID <= B.ThreadCount; tID++)
        {
            threads.Add(new Thread((tID) =>
            {
                double PartialSum = 0.0;

                for (int i = (int)tID; i <= B.precision + 1; i += B.ThreadCount)
                {
                    PartialSum += (Function((i - 1) * B.Delta + B.Left) + Function(i * B.Delta + B.Left)) / 2 * B.Delta;
                }
                lock (threads) { resultSumT += PartialSum; }
            }
            ));
            threads.Last().Start(tID);
        }
        foreach (var thread in threads) thread.Join();
        sw.Stop();
        Console.WriteLine("Результат {0} трапезоидными суммами за {1}мс", resultSumT, sw.ElapsedMilliseconds);
        //на трапезоидных суммах
    }

    public static double Function(double x)
    {
        return x * Math.Exp(x); //x*e^x
    }


}